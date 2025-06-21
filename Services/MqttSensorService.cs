using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class MqttSensorService : BackgroundService
{
    private readonly ILogger<MqttSensorService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private HiveMQClient _mqttClient;
    private int _readingId = 4000; // Example sensor ID, adjust as needed
    private readonly string[] _topics = {
        "sensorData/dht22/temperature",
        "sensorData/dht22/humidity",
        "sensorData/ldr"
    };

    private readonly TimeSpan _reconnectDelay = TimeSpan.FromSeconds(5);
    private readonly TimeSpan _keepAliveInterval = TimeSpan.FromSeconds(30);
    private readonly AlertService alertService;

    public MqttSensorService(
        ILogger<MqttSensorService> logger,
        IConfiguration configuration,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConnectAndRunAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("MQTT service stopping due to cancellation request");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error in MQTT service. Restarting in {Delay} seconds...", _reconnectDelay.TotalSeconds);
                
                // Cleanup before retry
                await CleanupConnectionAsync();
                
                try
                {
                    await Task.Delay(_reconnectDelay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        await CleanupConnectionAsync();
    }

    private async Task ConnectAndRunAsync(CancellationToken stoppingToken)
    {
        // Create MQTT client options
        var options = new HiveMQClientOptions
        {
            Host = "20ade4c354a947ceb0f0e19b78249e92.s1.eu.hivemq.cloud",
            Port = 8883,
            UseTLS = true,
            UserName = "hivemq.webclient.1747841822728",
            Password = "us2kd&?*ztU49!B1FVAN",
            ClientId = $"sensor-service-{Environment.MachineName}-{DateTime.UtcNow.Ticks}"
        };

        _mqttClient = new HiveMQClient(options);

        // Set up event handlers
        _mqttClient.OnMessageReceived += OnMessageReceived;

        // Connect to broker
        _logger.LogInformation("Attempting to connect to HiveMQ broker...");
        var connectResult = await _mqttClient.ConnectAsync();
        
        if (connectResult == null || !connectResult.ReasonCode.Equals(HiveMQtt.MQTT5.ReasonCodes.ConnAckReasonCode.Success))
        {
            _logger.LogError("Failed to connect to HiveMQ broker. Reason: {Reason}", connectResult?.ReasonCode);
            throw new InvalidOperationException("Failed to connect to MQTT broker");
        }

        _logger.LogInformation("Successfully connected to HiveMQ broker");

        // Subscribe to topics
        await SubscribeToTopicsAsync(stoppingToken);

        // Keep the connection alive and monitor status
        await MonitorConnectionAsync(stoppingToken);
    }

    private async Task SubscribeToTopicsAsync(CancellationToken stoppingToken)
    {
        foreach (var topic in _topics)
        {
            try
            {
                var subscribeResult = await _mqttClient.SubscribeAsync(
                    topic, 
                    HiveMQtt.MQTT5.Types.QualityOfService.AtMostOnceDelivery);
                
                if (subscribeResult != null && subscribeResult.Subscriptions.Count > 0)
                {
                    _logger.LogInformation("Successfully subscribed to topic: {Topic}", topic);
                }
                else
                {
                    _logger.LogWarning("Failed to subscribe to topic: {Topic}", topic);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing to topic: {Topic}", topic);
                throw;
            }
        }
    }

    private async Task MonitorConnectionAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested && _mqttClient?.IsConnected() == true)
        {
            try
            {
                await Task.Delay(_keepAliveInterval, stoppingToken);
                
                // Optional: Send ping or heartbeat here if needed
                _logger.LogDebug("MQTT connection is alive");
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        if (!stoppingToken.IsCancellationRequested && _mqttClient?.IsConnected() != true)
        {
            _logger.LogWarning("MQTT connection lost, triggering reconnection");
            throw new InvalidOperationException("MQTT connection lost");
        }
    }

    private async void OnMessageReceived(object sender, HiveMQtt.Client.Events.OnMessageReceivedEventArgs args)
    {
        try
        {
            var topic = args.PublishMessage.Topic;
            var payload = Encoding.UTF8.GetString(args.PublishMessage.Payload);
            
            _logger.LogInformation("Received message on topic {Topic}: {Payload}", topic, payload);

            await ProcessSensorDataAsync(topic, payload, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling received message");
        }
    }

    private async Task ProcessSensorDataAsync(string topic, string payload, CancellationToken stoppingToken)
    {
        try
        {
            // Map topic to SensorId, SensorType, and validate payload
            var (sensorId, sensorTypeName, value) = topic switch
            {
                "sensorData/dht22/temperature" => (10, "Temperature", ParseFloat(payload, -40, 80)),
                "sensorData/dht22/humidity" => (11, "Humidity", ParseFloat(payload, 0, 100)),
                "sensorData/ldr" => (12, "Luminosity", ParseInteger(payload, 0, 4095)),
                _ => ((int?)null, (string)null, (double?)null)
            };

            if (sensorId == null || sensorTypeName == null || value == null)
            {
                _logger.LogWarning("Invalid topic or payload: {Topic}, {Payload}", topic, payload);
                return;
            }

            // Use a new scope to resolve DbContext
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Optional: Validate Sensor exists (uncomment if needed)
            /*
            var sensor = await dbContext.Sensors
                .FirstOrDefaultAsync(s => s.Id == sensorId, stoppingToken);
            if (sensor == null)
            {
                _logger.LogWarning("Sensor not found: SensorId={SensorId}", sensorId);
                return;
            }

            // Validate SensorType matches
            var sensorType = await dbContext.SensorTypes
                .FirstOrDefaultAsync(st => st.Name == sensorTypeName, stoppingToken);
            if (sensorType == null || sensor.SensorTypeId != sensorType.Id)
            {
                _logger.LogWarning("Invalid SensorType for SensorId={SensorId}: {SensorTypeName}", sensorId, sensorTypeName);
                return;
            }
            */

            // Create and save SensorReading
            var sensorReading = new SensorReading
            {
                Id = _readingId++, // Increment reading ID for each new reading
                SensorId = sensorId.Value,
                Value = value.Value,
                Timestamp = DateTime.UtcNow
            };

            // await alertService.CreateAlertAsync(sensorId.Value, (float)value.Value, "Sensor reading alert").ContinueWith(task =>
            // {
            //     if (task.IsFaulted)
            //     {
            //         _logger.LogError(task.Exception, "Error creating alert for SensorId={SensorId}", sensorId);
            //     }
            // });

            dbContext.SensorReadings.Add(sensorReading);
            await dbContext.SaveChangesAsync(stoppingToken);
            
            _logger.LogInformation("Saved SensorReading: SensorId={SensorId}, Value={Value}, Timestamp={Timestamp}", 
                sensorId, value, sensorReading.Timestamp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing sensor data for topic {Topic}: {Payload}", topic, payload);
        }
    }

    private double? ParseFloat(string payload, double min, double max)
    {
        if (double.TryParse(payload, System.Globalization.NumberStyles.Float, 
            System.Globalization.CultureInfo.InvariantCulture, out double result))
        {
            if (result >= min && result <= max)
                return result;
        }
        return null;
    }

    private double? ParseInteger(string payload, int min, int max)
    {
        if (int.TryParse(payload, out int result))
        {
            if (result >= min && result <= max)
                return result;
        }
        return null;
    }

    private async Task CleanupConnectionAsync()
    {
        if (_mqttClient != null)
        {
            try
            {
                if (_mqttClient.IsConnected())
                {
                    await _mqttClient.DisconnectAsync();
                    _logger.LogInformation("Disconnected from HiveMQ broker");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during MQTT client disconnect");
            }
            finally
            {
                _mqttClient.Dispose();
                _mqttClient = null;
            }
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MQTT service is stopping...");
        await CleanupConnectionAsync();
        await base.StopAsync(stoppingToken);
    }
}
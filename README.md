# Plant Monitoring API Documentation

This document provides an overview of the models and controllers for the Plant Monitoring API, a system designed to manage users, plants, sensors, and sensor readings for plant care and monitoring.

## Table of Contents
1. [Models](#models)
   - [User](#user)
   - [SensorType](#sensortype)
   - [SensorReading](#sensorreading)
   - [Sensor](#sensor)
   - [Plant](#plant)
   - [UserRegisterDTO](#userregisterdto)
   - [UserLoginDTO](#userlogindto)
   - [SensorsForReportDTO](#sensorsforreportdto)
2. [Controllers](#controllers)
   - [AuthController](#authcontroller)
   - [HomeController](#homecontroller)
3. [Background Services](#background-services)
   - [MqttSensorService](#mqttsensorservice)
4. [Application Services](#application-services)
   - [AlertService](#alertservice) 

## Models

### User
Represents a user in the system.

**Properties:**
- `Id` (int): Unique identifier for the user.
- `Name` (string, nullable): User's name.
- `Email` (string, nullable): User's email address.
- `Password` (string, nullable): User's hashed password.

### SensorType
Defines the type of sensor used for monitoring.

**Properties:**
- `Id` (int): Unique identifier for the sensor type.
- `Name` (string, nullable): Name of the sensor type (e.g., temperature, humidity).

### SensorReading
Stores sensor measurement data.

**Properties:**
- `Id` (int): Unique identifier for the reading.
- `SensorId` (int): ID of the associated sensor.
- `Sensor` (Sensor, nullable): Navigation property to the associated sensor.
- `Value` (double): Measured value from the sensor.
- `Timestamp` (DateTime): Time the reading was recorded.

### Sensor
Represents a physical sensor device associated with a user and sensor type.

**Properties:**
- `Id` (int): Unique identifier for the sensor.
- `UserId` (int, nullable): ID of the user who owns the sensor.
- `User` (User, nullable): Navigation property to the associated user.
- `SensorTypeId` (int): ID of the sensor type.
- `SensorType` (SensorType, nullable): Navigation property to the associated sensor type.

### Plant
Represents a plant being monitored, associated with a user and sensors.

**Properties:**
- `Id` (int): Unique identifier for the plant.
- `Name` (string, nullable): Name of the plant.
- `Description` (string, nullable): Description of the plant.
- `UserId` (int): ID of the user who owns the plant.
- `User` (User, nullable): Navigation property to the associated user.
- `SensorIds` (List<int>): List of sensor IDs associated with the plant.

### UserRegisterDTO
Data Transfer Object for user registration.

**Properties:**
- `Name` (string, nullable): User's name.
- `Email` (string, nullable): User's email address.
- `Password` (string, nullable): User's password.

### UserLoginDTO
Data Transfer Object for user login.

**Properties:**
- `Email` (string, nullable): User's email address.
- `Password` (string, nullable): User's password.

### SensorsForReportDTO
Data Transfer Object for requesting sensor reading reports.

**Properties:**
- `SensorIds` (List<int>): List of sensor IDs to include in the report.
- `Days` (int): Number of days to look back for sensor readings.

## Controllers

### AuthController
Handles user authentication and registration.

**Endpoints:**

1. **POST /Auth/register**
   - **Description**: Registers a new user.
   - **Input**: `UserRegisterDTO` in JSON format:
     ```json
     {
       "Name": "string",
       "Email": "string",
       "Password": "string"
     }
     ```
   - **Validation**:
     - Checks for valid user data (non-null, non-empty email and password).
     - Password is hashed using BCrypt.
   - **Response**:
     - `200 OK`:
       ```json
       {
         "Message": "User registered successfully."
       }
       ```
     - `400 BadRequest`: If user data is invalid.

2. **POST /Auth/login**
   - **Description**: Authenticates a user and returns a JWT token.
   - **Input**: `UserLoginDTO` in JSON format:
     ```json
     {
       "Email": "string",
       "Password": "string"
     }
     ```
   - **Validation**:
     - Checks for valid login data (non-null, non-empty email and password).
     - Verifies email and password using BCrypt.
   - **Response**:
     - `200 OK`:
       ```json
       {
         "Token": "<JWT_token>"
       }
       ```
     - `400 BadRequest`: If login data is invalid.
     - `401 Unauthorized`: If email or password is incorrect.

3. **GET /Auth/getAll** (Authorized)
   - **Description**: Retrieves a list of all users.
   - **Response**:
     - `200 OK`:
       ```json
       [
         {
           "Id": int,
           "Name": "string",
           "Email": "string",
           "Password": "string"
         },
         ...
       ]
       ```
   - **Authorization**: Requires a valid JWT token.

### HomeController
Manages plant and sensor data retrieval.

**Endpoints:**

1. **GET /Home/{userId}**
   - **Description**: Retrieves all plants for a specific user.
   - **Parameters**: `userId` (int): ID of the user.
   - **Validation**:
     - Checks if `userId` is valid and exists.
   - **Response**:
     - `200 OK`:
       ```json
       [
         {
           "Id": int,
           "Name": "string",
           "Description": "string",
           "UserId": int,
           "User": null,
           "SensorIds": [int]
         },
         ...
       ]
       ```
     - `400 BadRequest`: If `userId` is invalid.
     - `404 NotFound`: If no plants are found.

2. **GET /Home/getPlant/{plantId}**
   - **Description**: Retrieves details of a specific plant.
   - **Parameters**: `plantId` (int): ID of the plant.
   - **Validation**:
     - Checks if `plantId` is valid and exists.
   - **Response**:
     - `200 OK`:
       ```json
       {
         "Id": int,
         "Name": "string",
         "Description": "string",
         "UserId": int,
         "User": null,
         "SensorIds": [int]
       }
       ```
     - `400 BadRequest`: If `plantId` is invalid.
     - `404 NotFound`: If plant is not found.

3. **GET /Home/getPlant/report/{specificSensor}/{period}**
   - **Description**: Retrieves sensor readings for a specific sensor over a period.
   - **Parameters**:
     - `specificSensor` (int): ID of the sensor.
     - `period` (int): Number of days to look back.
   - **Validation**:
     - Checks if `specificSensor` is valid and exists.
   - **Response**:
     - `200 OK`:
       ```json
       [
         {
           "Id": int,
           "SensorId": int,
           "Sensor": null,
           "Value": double,
           "Timestamp": "YYYY-MM-DDTHH:MM:SSZ"
         },
         ...
       ]
       ```
     - `400 BadRequest`: If `specificSensor` is invalid.
     - `404 NotFound`: If no readings are found.

4. **POST /Home/getPlant/report/allSensors**
   - **Description**: Retrieves sensor readings for multiple sensors over a period.
   - **Input**: `SensorsForReportDTO` in JSON format:
     ```json
     {
       "SensorIds": [int],
       "Days": int
     }
     ```
   - **Validation**:
     - Checks if input data is valid.
   - **Response**:
     - `200 OK`:
       ```json
       [
         {
           "Id": int,
           "SensorId": int,
           "Sensor": null,
           "Value": double,
           "Timestamp": "YYYY-MM-DDTHH:MM:SSZ"
         },
         ...
       ]
       ```
     - `400 BadRequest`: If input data is invalid.
     - `404 NotFound`: If no readings are found.

5. **PUT /Home/updatePlant**
   - **Description**: Updates a plant's name, description, associated sensors, and user.
   - **Input**: `PlantDTO` in JSON format.
   - **Validation**:
     - Checks if the plant exists.
     - Validates that the user exists.
   - **Response**:
     - `200 OK`: Plant updated successfully.
     - `400 BadRequest`: If the input is invalid.
     - `404 NotFound`: If the plant is not found.

6. **POST /Home/addSensor**
   - **Description**: Adds a new sensor to the system.
   - **Input**: `Sensor` object in JSON format.
   - **Validation**:
     - Ensures the `UserId` and `SensorTypeId` are valid and exist.
   - **Response**:
     - `200 OK`: Sensor added successfully.
     - `400 BadRequest`: If the input data is invalid.

7. **GET /Home/{userId}/getPlants**
   - **Description**: Retrieves all plants for a specific user (duplicate of `/Home/{userId}` for flexible routing).
   - **Parameters**: `userId` (int): ID of the user.
   - **Response**:
     - Same as `/Home/{userId}`.

8. **GET /Home/getAlerts/{userId}**
   - **Description**: Retrieves all alerts associated with a specific user.
   - **Parameters**: `userId` (int): ID of the user.
   - **Validation**:
     - Validates that the user exists.
   - **Response**:
     - `200 OK`: Returns a list of alerts.
     - `400 BadRequest`: If user ID is invalid.
     - `404 NotFound`: If no alerts are found.

9. **POST /Home/addAlert**
   - **Description**: Creates a new alert associated with a sensor.
   - **Input**: `Alert` object in JSON format.
   - **Validation**:
     - Validates presence of required fields (`SensorId`, `Message`, `UserId`).
     - Checks if the sensor and user exist.
   - **Response**:
     - `200 OK`: Alert added successfully.
     - `400 BadRequest`: If the data is invalid.

## Background Services

### MqttSensorService

**Description**:  
The `MqttSensorService` is a background worker that connects to a HiveMQ broker and listens to MQTT messages published by environmental sensors (e.g., temperature, humidity, luminosity). When a message is received, it processes and stores sensor readings into the database and optionally triggers alerts.

**Responsibilities**:
- Establish and maintain a secure MQTT connection using TLS.
- Subscribe to predefined topics such as:
  - `sensorData/dht22/temperature`
  - `sensorData/dht22/humidity`
  - `sensorData/ldr`
- Parse incoming MQTT messages, validate payloads, and map them to appropriate sensors.
- Use dependency-injected `AppDbContext` to store sensor readings.
- Automatically create alerts using the `AlertService` if thresholds or anomalies are detected.
- Implement reconnection logic to handle network or broker failures robustly.

**Key Features**:
- **Resilience**: Reconnects automatically on connection loss.
- **Validation**: Ensures sensor readings are within valid ranges before saving.
- **Scope-Aware**: Resolves `DbContext` per message using a DI scope.
- **Performance**: Asynchronous and non-blocking implementation.
- **Monitoring**: Includes detailed logging for broker communication, message processing, and exception handling.

**Configuration**:
- Broker: HiveMQ Cloud
- Port: 8883 (secure MQTT)
- Credentials and client ID configured via `HiveMQClientOptions`

**Logging Examples**:
- Successful connection and subscription
- Payload parsing and data processing
- Alerts creation outcomes
- Error handling on unexpected payloads or broker issues

**Use Case**:  
Allows the system to autonomously receive real-time environmental data from physical IoT sensors and transform it into actionable insights for plant monitoring and alerting.

## Application Services

### AlertService

**Description**:  
The `AlertService` is responsible for generating context-aware alerts based on incoming sensor readings. It encapsulates business logic that determines when an alert condition is met and saves the alert to the database.

**Responsibilities**:
- Analyze sensor readings and determine if values are outside safe thresholds.
- Generate appropriate alert messages specific to sensor type and reading.
- Persist alerts in the `Alerts` table with timestamps and descriptive messages.

**Core Logic**:
- For **Temperature sensors** (`sensorId == 10`):
  - If value < 17°C → triggers "Temperature is too low".
  - If value > 30°C → triggers "Temperature is too high".
- For **Humidity sensors** (`sensorId == 11`):
  - If value < 65% → triggers "Humidity is too low".
  - If value > 85% → triggers "Humidity is too high".
- The alert message is prefixed with the sensor type for clarity.

**Method Summary**:

#### `CreateAlertAsync(int sensorId, float value, string message)`
- **Input**:  
  - `sensorId`: ID of the sensor.
  - `value`: The latest sensor reading.
  - `message`: Initial or fallback message.
- **Output**:  
  - Returns an `Alert` object after saving it to the database.
- **Exceptions**:
  - Throws `ArgumentException` if `sensorId` is invalid.
- **Behavior**:
  - Evaluates thresholds and updates `message` accordingly.
  - Adds the alert to the database and commits asynchronously.

**Planned Features**:
- (Commented) `GetAlertsByUserIdAsync`: Intended to retrieve all alerts by user ID, supporting future expansion of alert history and UI integration.

**Use Case**:  
This service is called directly by `MqttSensorService` whenever a new reading is received. It ensures the system can proactively notify users about abnormal conditions affecting their plants, helping maintain plant health through timely intervention.


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
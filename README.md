# MyWellnessSync

MyWellnessSync is a vendor-neutral web API designed to synchronize and store health data from multiple wearable device vendors (e.g., Garmin, Fitbit) in a unified format. This project is extensible and supports custom plugins for data transformation and storage.

## Features
- **Vendor-Neutral Plugins**: Easily add plugins for new data providers.
- **Database Flexibility**: Output data to a variety of databases, currently supporting InfluxDB.
- **Background Synchronization**: Schedule periodic sync tasks using CRON expressions.
- **Manual Sync**: Trigger synchronization manually via API endpoints.

## Setup Instructions

### Prerequisites
- Docker installed on your system.
- Access to your desired database (e.g., InfluxDB).
- Vendor-specific credentials (e.g., Garmin API login).

### Building and Running
1. Clone the repository:
   ```bash
   git clone <repository-url>
   cd healthsync
   ```

2. Build the Docker image:
   ```bash
   docker build -t healthsync .
   ```

3. Run the Docker container:
   ```bash
   docker run -d -p 8080:8080 --name healthsync-container healthsync
   ```

4. Access the application at `http://localhost:8080/swagger`.

### Configuration
- Edit the `sync-config.example.yml` file to define your sync tasks.
- Use environment variables to supply sensitive data like API keys.

## API Endpoints
- **POST /api/sync/manual**: Trigger manual data synchronization.
- **GET /api/sync/{taskId}/lastSync**: Retrieve the last synchronization timestamp.

## Extending the Project
1. Create a new plugin by implementing the `IProviderPlugin` and `IRepositoryPlugin` interfaces.
2. Register your plugin in `ServiceCollectionExtensions`.

## License
This project is licensed under the MIT License. See the LICENSE file for details.

## Contributions
Feel free to fork this project, submit issues, and create pull requests. Contributions are welcome!

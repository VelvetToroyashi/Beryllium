# Beryllium
A Discord bot template for Discord moderation and management built with Remora.Discord.

## Features

- Built with [Remora.Discord](https://github.com/Remora/Remora.Discord) - A modern Discord bot framework
- SQLite database with Entity Framework Core
- .NET 10 support
- Dependency injection and service configuration
- Example ping command

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- A Discord Bot Token (create one at [Discord Developer Portal](https://discord.com/developers/applications))

## Getting Started

1. Clone the repository:
   ```bash
   git clone https://github.com/VelvetToroyashi/Beryllium.git
   cd Beryllium
   ```

2. Configure the bot:
   - Open `Beryllium.Bot/appsettings.json`
   - Replace `YOUR_DISCORD_BOT_TOKEN_HERE` with your Discord bot token
   - Alternatively, set the environment variable `BERYLLIUM_Bot__Token`

3. Build and run:
   ```bash
   dotnet build
   dotnet run --project Beryllium.Bot
   ```

## Configuration

The bot can be configured through `appsettings.json` or environment variables with the prefix `BERYLLIUM_`.

Example environment variable:
```bash
export BERYLLIUM_Bot__Token="your-token-here"
export BERYLLIUM_Bot__DatabaseConnection="Data Source=beryllium.db"
```

## Database

The bot uses SQLite with Entity Framework Core. The database is automatically created on first run.

### Adding Migrations

To add a new migration after changing the database models:

```bash
cd Beryllium.Bot
dotnet ef migrations add YourMigrationName
dotnet ef database update
```

## Adding Commands

Commands are added by creating classes that inherit from `CommandGroup`. See `Commands/PingCommands.cs` for an example.

## Project Structure

- `Commands/` - Bot command implementations
- `Data/` - Database context
- `Models/` - Data models
- `Services/` - Service classes and configuration
- `Program.cs` - Application entry point

## License

This project is open source and available under the MIT License.

# Contributing to Beryllium

Thank you for considering contributing to Beryllium! This guide will help you get started.

## Development Setup

1. **Prerequisites**
   - [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
   - A code editor (Visual Studio, VS Code, or Rider recommended)
   - Git

2. **Clone and Build**
   ```bash
   git clone https://github.com/VelvetToroyashi/Beryllium.git
   cd Beryllium
   dotnet build
   ```

3. **Configuration**
   - Copy `appsettings.example.json` to `appsettings.json` in the `Beryllium.Bot` directory
   - Add your Discord bot token to `appsettings.json`
   - Alternatively, use environment variables (see `.env.example`)

## Project Structure

- **Commands/** - Discord bot commands using Remora.Discord command system
- **Data/** - Database context and factory for EF Core
- **Models/** - Data models representing database entities
- **Services/** - Service classes and configuration
- **Migrations/** - EF Core database migrations

## Adding New Commands

1. Create a new class in the `Commands/` directory
2. Inherit from `CommandGroup`
3. Use the `[Command]` attribute on methods
4. Register the command in `Program.cs`:
   ```csharp
   services.AddCommandTree()
       .WithCommandGroup<YourNewCommandGroup>();
   ```

## Working with the Database

### Adding a New Model

1. Create your model class in `Models/`
2. Add a `DbSet` property in `BotDbContext.cs`
3. Configure the entity in `OnModelCreating` if needed
4. Create a migration:
   ```bash
   cd Beryllium.Bot
   dotnet ef migrations add YourMigrationName
   ```

### Updating the Database

```bash
cd Beryllium.Bot
dotnet ef database update
```

## Code Style

- Follow standard C# naming conventions
- Use XML documentation comments for public APIs
- Keep methods focused and single-purpose
- Use dependency injection for services

## Testing

Before submitting a pull request:

1. Ensure the project builds without errors:
   ```bash
   dotnet build
   ```

2. Test your changes with a real Discord bot token

3. Verify database migrations work correctly

## Submitting Changes

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## Questions?

Feel free to open an issue if you have questions or need help!

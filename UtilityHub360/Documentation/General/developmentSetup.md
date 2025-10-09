# Development Setup Guide

## 🛠️ Prerequisites

### Required Software
- **.NET 8.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **SQL Server** - [Download SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- **Visual Studio 2022** or **VS Code** - [Download VS 2022](https://visualstudio.microsoft.com/downloads/)

### Optional Tools
- **SQL Server Management Studio (SSMS)** - For database management
- **Postman** - For API testing
- **Git** - For version control

## 🚀 Initial Setup

### 1. Clone Repository
```bash
git clone <repository-url>
cd UtilityHub360
```

### 2. Database Configuration
Update connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=UtilityHub360;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

### 3. Install Dependencies
```bash
dotnet restore
```

### 4. Run Database Migrations
```bash
dotnet ef database update
```

### 5. Build and Run
```bash
dotnet build
dotnet run
```

## 🔧 Development Environment

### Visual Studio Setup
1. Open `UtilityHub360.sln` in Visual Studio 2022
2. Set startup project to `UtilityHub360`
3. Configure launch settings (already created in `Properties/launchSettings.json`)
4. Press F5 to run in debug mode

### VS Code Setup
1. Open project folder in VS Code
2. Install C# extension
3. Open terminal and run:
   ```bash
   dotnet restore
   dotnet build
   dotnet run
   ```

## 🌐 Development URLs

When running in development mode:
- **API Base**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger
- **HTTPS**: https://localhost:5001
- **Swagger HTTPS**: https://localhost:5001/swagger

## 🗄️ Database Setup

### SQL Server Configuration
1. Install SQL Server Express
2. Enable TCP/IP protocol
3. Create database: `UtilityHub360`
4. Update connection string in `appsettings.json`

### Connection String Options
```json
// Windows Authentication
"Server=localhost;Database=UtilityHub360;Trusted_Connection=true;TrustServerCertificate=true;"

// SQL Server Authentication
"Server=localhost;Database=UtilityHub360;User Id=sa;Password=YourPassword;TrustServerCertificate=true;"

// Azure SQL Database
"Server=tcp:yourserver.database.windows.net,1433;Database=UtilityHub360;User Id=username;Password=password;Encrypt=true;"
```

## 🔐 Environment Configuration

### Development Settings
```json
{
  "ASPNETCORE_ENVIRONMENT": "Development",
  "JwtSettings": {
    "SecretKey": "development-secret-key-must-be-long-enough-for-security",
    "Issuer": "UtilityHub360",
    "Audience": "UtilityHub360-Users",
    "ExpirationMinutes": 60
  }
}
```

### Production Settings
```json
{
  "ASPNETCORE_ENVIRONMENT": "Production",
  "JwtSettings": {
    "SecretKey": "production-secret-key-from-secure-storage",
    "Issuer": "UtilityHub360",
    "Audience": "UtilityHub360-Users",
    "ExpirationMinutes": 60
  }
}
```

## 🧪 Testing Setup

### Unit Testing
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test Tests/UtilityHub360.Tests

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### API Testing with Swagger
1. Navigate to `/swagger`
2. Click "Authorize" button
3. Login via `/api/auth/login` endpoint
4. Copy JWT token from response
5. Enter token in format: `Bearer <token>`
6. Test authenticated endpoints

### Postman Collection
Import the provided Postman collection for comprehensive API testing.

## 🔍 Debugging

### Visual Studio Debugging
1. Set breakpoints in code
2. Press F5 to start debugging
3. Use Debug Console to inspect variables
4. Step through code with F10/F11

### VS Code Debugging
1. Create `.vscode/launch.json`:
```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Core Launch (web)",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/bin/Debug/net8.0/UtilityHub360.dll",
      "args": [],
      "cwd": "${workspaceFolder}",
      "stopAtEntry": false,
      "serverReadyAction": {
        "action": "openExternally",
        "pattern": "\\bNow listening on:\\s+(https?://\\S+)"
      },
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      },
      "sourceFileMap": {
        "/Views": "${workspaceFolder}/Views"
      }
    }
  ]
}
```

## 📝 Code Style Guidelines

### C# Conventions
- Use PascalCase for public members
- Use camelCase for private members
- Use meaningful variable names
- Add XML documentation for public methods

### API Conventions
- Use RESTful URL patterns
- Return consistent response format
- Use appropriate HTTP status codes
- Include proper error handling

## 🚀 Build and Deployment

### Build Commands
```bash
# Debug build
dotnet build

# Release build
dotnet build -c Release

# Publish for deployment
dotnet publish -c Release -o ./publish
```

### Docker Support
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["UtilityHub360.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UtilityHub360.dll"]
```

## 🐛 Troubleshooting

### Common Issues

#### Database Connection Issues
- Verify SQL Server is running
- Check connection string format
- Ensure database exists
- Verify firewall settings

#### JWT Token Issues
- Check secret key length (minimum 32 characters)
- Verify token expiration settings
- Ensure HTTPS in production

#### Entity Framework Issues
- Run `dotnet ef migrations add InitialCreate`
- Run `dotnet ef database update`
- Check database permissions

#### Port Already in Use
- Change ports in `launchSettings.json`
- Kill process using port: `netstat -ano | findstr :5000`

### Performance Optimization
- Enable query logging in development
- Use async/await patterns
- Implement proper indexing
- Use connection pooling

## 📚 Additional Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [JWT Authentication](https://jwt.io/)
- [SQL Server Documentation](https://docs.microsoft.com/en-us/sql/)

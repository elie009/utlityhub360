# Environment Switching Guide - UtilityHub360

## 🔄 How to Switch Between Development and Production Databases

This guide shows you multiple ways to switch between your development and production database configurations.

## 📊 Current Database Configurations

### **Development Database** (`appsettings.json`)
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=DBUTILS;User Id=myadmin;Password=p@$$w0rdBAH;TrustServerCertificate=true;MultipleActiveResultSets=true"
}
```

### **Production Database** (`appsettings.Production.json`)
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=174.138.185.18;Database=DBUTILS;User Id=sa01;Password=iSTc0#T3tw~noz2r;TrustServerCertificate=true;MultipleActiveResultSets=true;Encrypt=true;"
}
```

## 🚀 Method 1: Environment Variables (Recommended)

### **Windows PowerShell:**
```powershell
# Switch to Development
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run

# Switch to Production
$env:ASPNETCORE_ENVIRONMENT="Production"
dotnet run
```

### **Windows Command Prompt:**
```cmd
# Switch to Development
set ASPNETCORE_ENVIRONMENT=Development
dotnet run

# Switch to Production
set ASPNETCORE_ENVIRONMENT=Production
dotnet run
```

### **Linux/Mac Terminal:**
```bash
# Switch to Development
export ASPNETCORE_ENVIRONMENT=Development
dotnet run

# Switch to Production
export ASPNETCORE_ENVIRONMENT=Production
dotnet run
```

## 🎯 Method 2: Command Line Arguments

```bash
# Run with Development settings
dotnet run --environment Development

# Run with Production settings
dotnet run --environment Production
```

## 🖥️ Method 3: Visual Studio/IDE

### **Visual Studio:**
1. Right-click on the project in Solution Explorer
2. Select **Properties**
3. Go to **Debug** → **General**
4. In **Environment variables** section:
   - Add: `ASPNETCORE_ENVIRONMENT` = `Development` or `Production`
5. Save and run

### **Visual Studio Code:**
1. Open `.vscode/launch.json`
2. Add environment variable to your configuration:
```json
{
  "name": "Launch Development",
  "type": "coreclr",
  "request": "launch",
  "program": "${workspaceFolder}/bin/Debug/net8.0/UtilityHub360.dll",
  "args": [],
  "cwd": "${workspaceFolder}",
  "stopAtEntry": false,
  "env": {
    "ASPNETCORE_ENVIRONMENT": "Development"
  }
}
```

## 🏃‍♂️ Method 4: Launch Profiles (Updated)

I've added a **Production** profile to your `launchSettings.json`. Now you can:

### **In Visual Studio:**
1. Click the dropdown next to the **Run** button
2. Select **Production** profile to use production database
3. Select **http** or **https** profile to use development database

### **Command Line with Profile:**
```bash
# Run with specific launch profile
dotnet run --launch-profile Production
dotnet run --launch-profile https
```

## 🔧 Method 5: Environment-Specific Scripts

Create batch/shell scripts for easy switching:

### **Windows Scripts:**

**`run-dev.bat`:**
```batch
@echo off
set ASPNETCORE_ENVIRONMENT=Development
echo Running in Development mode...
dotnet run
```

**`run-prod.bat`:**
```batch
@echo off
set ASPNETCORE_ENVIRONMENT=Production
echo Running in Production mode...
dotnet run
```

### **Linux/Mac Scripts:**

**`run-dev.sh`:**
```bash
#!/bin/bash
export ASPNETCORE_ENVIRONMENT=Development
echo "Running in Development mode..."
dotnet run
```

**`run-prod.sh`:**
```bash
#!/bin/bash
export ASPNETCORE_ENVIRONMENT=Production
echo "Running in Production mode..."
dotnet run
```

Make them executable:
```bash
chmod +x run-dev.sh run-prod.sh
```

## 🗄️ Database Migrations

When switching environments, you might need to run migrations:

### **Development Migrations:**
```bash
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet ef database update
```

### **Production Migrations:**
```bash
$env:ASPNETCORE_ENVIRONMENT="Production"
dotnet ef database update
```

## 🔍 How to Verify Current Environment

### **Check in Application:**
Add this to your controller to see current environment:
```csharp
[HttpGet("environment")]
public IActionResult GetEnvironment()
{
    return Ok(new { 
        Environment = _environment.EnvironmentName,
        ConnectionString = _configuration.GetConnectionString("DefaultConnection")
    });
}
```

### **Check in Logs:**
The application will log which environment it's running in at startup:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Development  // or Production
```

## ⚠️ Important Notes

### **Security Considerations:**
1. **Never commit** production credentials to version control
2. **Use environment variables** or secure configuration for production
3. **Test thoroughly** in development before switching to production

### **Database Safety:**
1. **Backup production database** before running migrations
2. **Test migrations** in development first
3. **Use transactions** for critical database operations

### **Connection String Priority:**
ASP.NET Core loads configuration in this order:
1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. Environment variables
4. Command line arguments

## 🎯 Quick Commands Reference

| Action | Command |
|--------|---------|
| **Development** | `$env:ASPNETCORE_ENVIRONMENT="Development"; dotnet run` |
| **Production** | `$env:ASPNETCORE_ENVIRONMENT="Production"; dotnet run` |
| **Dev Migrations** | `$env:ASPNETCORE_ENVIRONMENT="Development"; dotnet ef database update` |
| **Prod Migrations** | `$env:ASPNETCORE_ENVIRONMENT="Production"; dotnet ef database update` |
| **Build for Prod** | `dotnet publish -c Release -o ./publish` |

## 🔄 Switching Checklist

- [ ] Set environment variable
- [ ] Verify connection string in logs
- [ ] Run database migrations if needed
- [ ] Test API endpoints
- [ ] Check application logs for errors
- [ ] Verify database connectivity

## 🆘 Troubleshooting

### **Connection Issues:**
```bash
# Test database connection
dotnet ef database update --verbose
```

### **Wrong Environment:**
```bash
# Check current environment
echo $env:ASPNETCORE_ENVIRONMENT  # Windows
echo $ASPNETCORE_ENVIRONMENT      # Linux/Mac
```

### **Configuration Not Loading:**
1. Ensure `appsettings.{Environment}.json` exists
2. Check file naming (case-sensitive)
3. Verify environment variable is set correctly
4. Restart the application after changing environment

---

**Pro Tip**: Always test your database connections in development before switching to production! 🚀

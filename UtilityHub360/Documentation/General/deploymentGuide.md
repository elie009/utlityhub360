# Deployment Guide

## 🚀 Production Deployment

This guide covers deploying UtilityHub360 to production environments.

## 🏗️ Deployment Options

### 1. IIS Deployment (Windows Server)

#### Prerequisites
- Windows Server with IIS installed
- .NET 8.0 Runtime installed
- SQL Server (local or remote)

#### Steps
1. **Publish Application**
   ```bash
   dotnet publish -c Release -o ./publish
   ```

2. **Configure IIS**
   - Create new Application Pool (.NET CLR Version: No Managed Code)
   - Create new Website pointing to publish folder
   - Set appropriate permissions

3. **Update Configuration**
   ```json
   {
     "ASPNETCORE_ENVIRONMENT": "Production",
     "ConnectionStrings": {
       "DefaultConnection": "Server=prod-server;Database=UtilityHub360;User Id=prod-user;Password=secure-password;"
     }
   }
   ```

### 2. Docker Deployment

#### Dockerfile
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

#### Docker Commands
```bash
# Build image
docker build -t utilityhub360 .

# Run container
docker run -d -p 8080:80 --name utilityhub360 utilityhub360

# With environment variables
docker run -d -p 8080:80 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="Server=host.docker.internal;Database=UtilityHub360;User Id=sa;Password=password;" \
  --name utilityhub360 utilityhub360
```

### 3. Azure App Service

#### Deployment Steps
1. **Create App Service**
   - Create new App Service in Azure Portal
   - Select .NET 8.0 runtime stack

2. **Configure Application Settings**
   ```
   ASPNETCORE_ENVIRONMENT = Production
   ConnectionStrings__DefaultConnection = [Azure SQL Connection String]
   JwtSettings__SecretKey = [Secure Secret Key]
   ```

3. **Deploy Code**
   - Use Visual Studio publish profile
   - Or Azure CLI: `az webapp deployment source config`

### 4. AWS Elastic Beanstalk

#### Deployment Steps
1. **Package Application**
   ```bash
   dotnet publish -c Release
   zip -r utilityhub360.zip ./publish/*
   ```

2. **Create Environment**
   - Create new Elastic Beanstalk application
   - Select .NET Core platform
   - Upload deployment package

3. **Configure Environment Variables**
   - Add in Elastic Beanstalk console
   - Or use `.ebextensions` configuration

## 🔧 Production Configuration

### appsettings.Production.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-server;Database=UtilityHub360;User Id=prod-user;Password=secure-password;TrustServerCertificate=true;"
  },
  "JwtSettings": {
    "SecretKey": "production-secret-key-from-secure-storage",
    "Issuer": "UtilityHub360",
    "Audience": "UtilityHub360-Users",
    "ExpirationMinutes": 60
  }
}
```

### Security Configuration
```json
{
  "HttpsRedirection": {
    "HttpsPort": 443,
    "RedirectStatusCode": 301
  },
  "SecurityHeaders": {
    "X-Content-Type-Options": "nosniff",
    "X-Frame-Options": "DENY",
    "X-XSS-Protection": "1; mode=block"
  }
}
```

## 🗄️ Database Deployment

### SQL Server Setup
1. **Create Production Database**
   ```sql
   CREATE DATABASE UtilityHub360;
   CREATE LOGIN [utilityhub360_user] WITH PASSWORD = 'SecurePassword123!';
   USE UtilityHub360;
   CREATE USER [utilityhub360_user] FOR LOGIN [utilityhub360_user];
   ALTER ROLE db_owner ADD MEMBER [utilityhub360_user];
   ```

2. **Run Migrations**
   ```bash
   dotnet ef database update --connection "Server=prod-server;Database=UtilityHub360;User Id=utilityhub360_user;Password=SecurePassword123!;"
   ```

3. **Seed Initial Data**
   ```sql
   INSERT INTO Users (Id, Name, Email, Phone, Role, IsActive, CreatedAt, UpdatedAt)
   VALUES ('admin-guid', 'System Admin', 'admin@utilityhub360.com', '+1234567890', 'ADMIN', 1, GETDATE(), GETDATE());
   ```

### Azure SQL Database
1. **Create Azure SQL Server**
2. **Create Database**
3. **Configure Firewall Rules**
4. **Update Connection String**
   ```
   Server=tcp:your-server.database.windows.net,1433;Database=UtilityHub360;User ID=your-user;Password=your-password;Encrypt=true;TrustServerCertificate=false;Connection Timeout=30;
   ```

## 🔐 Security Considerations

### JWT Security
- Use strong secret keys (minimum 256 bits)
- Store secrets in secure configuration (Azure Key Vault, AWS Secrets Manager)
- Implement token refresh mechanism
- Set appropriate expiration times

### Database Security
- Use dedicated database users with minimal permissions
- Enable SSL/TLS for database connections
- Implement connection string encryption
- Regular security updates

### API Security
- Enable HTTPS only in production
- Implement rate limiting
- Add request/response logging
- Use security headers

### Environment Variables
```bash
# Production environment variables
export ASPNETCORE_ENVIRONMENT=Production
export ConnectionStrings__DefaultConnection="secure-connection-string"
export JwtSettings__SecretKey="secure-jwt-secret"
```

## 📊 Monitoring & Logging

### Application Insights (Azure)
```csharp
// Program.cs
builder.Services.AddApplicationInsightsTelemetry();
```

### Serilog Configuration
```csharp
// Program.cs
builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
    config.WriteTo.File("logs/utilityhub360-.txt", rollingInterval: RollingInterval.Day);
    config.WriteTo.Console();
});
```

### Health Checks
```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddSqlServer(connectionString)
    .AddDbContext<ApplicationDbContext>();

app.MapHealthChecks("/health");
```

## 🚀 CI/CD Pipeline

### GitHub Actions
```yaml
name: Deploy to Production

on:
  push:
    branches: [ main ]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v2
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v1
      with:
        dotnet-version: 8.0.x
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Test
      run: dotnet test --no-build --verbosity normal
    
    - name: Publish
      run: dotnet publish -c Release -o ./publish
    
    - name: Deploy to Azure
      uses: azure/webapps-deploy@v2
      with:
        app-name: 'utilityhub360'
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: ./publish
```

### Azure DevOps
```yaml
trigger:
- main

pool:
  vmImage: 'ubuntu-latest'

variables:
  buildConfiguration: 'Release'

steps:
- task: DotNetCoreCLI@2
  displayName: 'Restore packages'
  inputs:
    command: 'restore'
    projects: '**/*.csproj'

- task: DotNetCoreCLI@2
  displayName: 'Build'
  inputs:
    command: 'build'
    projects: '**/*.csproj'
    arguments: '--configuration $(buildConfiguration)'

- task: DotNetCoreCLI@2
  displayName: 'Test'
  inputs:
    command: 'test'
    projects: '**/*Tests/*.csproj'

- task: DotNetCoreCLI@2
  displayName: 'Publish'
  inputs:
    command: 'publish'
    projects: '**/*.csproj'
    arguments: '--configuration $(buildConfiguration) --output $(Build.ArtifactStagingDirectory)'

- task: PublishBuildArtifacts@1
  displayName: 'Publish artifacts'
  inputs:
    PathtoPublish: '$(Build.ArtifactStagingDirectory)'
    ArtifactName: 'drop'
```

## 🔄 Backup & Recovery

### Database Backup
```sql
-- Full backup
BACKUP DATABASE UtilityHub360 
TO DISK = 'C:\Backups\UtilityHub360_Full.bak'

-- Transaction log backup
BACKUP LOG UtilityHub360 
TO DISK = 'C:\Backups\UtilityHub360_Log.trn'
```

### Application Backup
- Regular file system backups
- Configuration backup
- SSL certificate backup
- Custom application data backup

## 📈 Performance Optimization

### Production Optimizations
- Enable response compression
- Configure caching
- Optimize database queries
- Use CDN for static content
- Implement connection pooling

### Monitoring
- Application performance monitoring
- Database performance monitoring
- Error tracking and alerting
- Resource utilization monitoring

## 🚨 Troubleshooting

### Common Issues
1. **Connection String Issues**
   - Verify server accessibility
   - Check credentials
   - Test connection string format

2. **JWT Token Issues**
   - Verify secret key configuration
   - Check token expiration settings
   - Validate issuer/audience settings

3. **Database Migration Issues**
   - Check database permissions
   - Verify connection string
   - Run migrations manually if needed

### Log Analysis
```bash
# View application logs
docker logs utilityhub360

# View IIS logs
Get-Content "C:\inetpub\logs\LogFiles\W3SVC1\*.log" -Tail 100
```

This deployment guide ensures a secure, scalable, and maintainable production deployment of UtilityHub360.

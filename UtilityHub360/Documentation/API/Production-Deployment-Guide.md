# Production Deployment Guide - UtilityHub360

## 🚀 Production Configuration

Your application has been configured with production settings for the following environment:

### 📊 Production Database Configuration
- **Host**: `174.138.185.18`
- **Database**: `DBUTILS`
- **Username**: `sa01`
- **Password**: `iSTc0#T3tw~noz2r`

### 📁 Configuration Files Created/Updated

#### ✅ `appsettings.Production.json` (NEW)
- Production database connection string
- Production JWT secret key
- Optimized logging levels for production
- Production-specific allowed hosts
- Kestrel endpoint configuration for production server

#### ✅ `appsettings.json` (UPDATED)
- Added Entity Framework logging configuration
- Added Kestrel endpoint configuration for localhost development

## 🔧 Deployment Steps

### 1. Build for Production
```bash
dotnet publish -c Release -o ./publish
```

### 2. Deploy to Production Server
```bash
# Copy files to production server
scp -r ./publish/* user@174.138.185.18:/var/www/utilityhub360/

# Or using rsync
rsync -avz ./publish/ user@174.138.185.18:/var/www/utilityhub360/
```

### 3. Run Database Migrations
```bash
# On production server
cd /var/www/utilityhub360
dotnet ef database update --environment Production
```

### 4. Set Environment Variables
```bash
# Set production environment
export ASPNETCORE_ENVIRONMENT=Production

# Or add to systemd service file
Environment=ASPNETCORE_ENVIRONMENT=Production
```

### 5. Start the Application
```bash
# Run directly
dotnet UtilityHub360.dll --environment Production

# Or as a service (recommended)
sudo systemctl start utilityhub360
sudo systemctl enable utilityhub360
```

## 🔒 Security Considerations

### JWT Secret Key
- **IMPORTANT**: Change the JWT secret key in production
- Generate a strong, unique secret key (at least 32 characters)
- Keep it secure and never commit to version control

### Database Security
- Connection string includes `TrustServerCertificate=true` for development
- Consider using SSL certificates in production
- Ensure firewall rules restrict database access

### HTTPS Configuration
- Production endpoints configured for both HTTP (5000) and HTTPS (5001)
- Configure SSL certificates for HTTPS
- Consider using reverse proxy (nginx/Apache) for better security

## 📋 Production Checklist

- [ ] **Database Connection**: Test connection to production database
- [ ] **JWT Secret**: Update JWT secret key to a secure value
- [ ] **SSL Certificates**: Configure HTTPS certificates
- [ ] **Firewall**: Configure server firewall rules
- [ ] **Logging**: Verify log files are being written correctly
- [ ] **Performance**: Monitor application performance
- [ ] **Backup**: Set up database backup procedures
- [ ] **Monitoring**: Implement application monitoring
- [ ] **Domain**: Configure domain name and DNS
- [ ] **Load Balancer**: Configure if using multiple instances

## 🐛 Troubleshooting

### Database Connection Issues
```bash
# Test database connection
dotnet ef database update --environment Production --verbose
```

### Application Won't Start
```bash
# Check logs
journalctl -u utilityhub360 -f

# Check application logs
tail -f /var/log/utilityhub360/app.log
```

### Port Conflicts
```bash
# Check if ports are in use
netstat -tlnp | grep :5000
netstat -tlnp | grep :5001
```

## 📊 Monitoring

### Application Health
- Monitor CPU and memory usage
- Check database connection pool
- Monitor response times
- Set up alerts for errors

### Database Health
- Monitor database performance
- Check connection limits
- Monitor disk space
- Set up automated backups

## 🔄 Environment Variables (Alternative)

Instead of using `appsettings.Production.json`, you can use environment variables:

```bash
export ConnectionStrings__DefaultConnection="Server=174.138.185.18;Database=DBUTILS;User Id=sa01;Password=iSTc0#T3tw~noz2r;TrustServerCertificate=true;MultipleActiveResultSets=true;Encrypt=true;"
export JwtSettings__SecretKey="YourSuperSecretProductionKeyThatIsAtLeast32CharactersLongAndSecure!"
export ASPNETCORE_ENVIRONMENT=Production
```

## 🎯 Next Steps

1. **Test the configuration** locally with production settings
2. **Deploy to staging** environment first
3. **Run integration tests** against production database
4. **Deploy to production** during maintenance window
5. **Monitor** application health and performance
6. **Set up** automated backups and monitoring

## 📞 Support

If you encounter any issues during deployment:
1. Check the application logs
2. Verify database connectivity
3. Ensure all environment variables are set correctly
4. Contact your system administrator for server-related issues

---

**Note**: This configuration assumes you have the necessary permissions and access to the production server. Always test in a staging environment first before deploying to production.

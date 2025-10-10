# Environment Setup Guide

## Quick Start - Single Place Configuration

You can now switch environments easily from **ONE location**!

## Method 1: Using PowerShell Script (⭐ Recommended)

### Step 1: Edit the Script
Open `set-environment.ps1` and change line 5:

```powershell
$Environment = "Development"  # Options: Development, Staging, Production
```

### Step 2: Run the Script
```powershell
.\set-environment.ps1
```

This will automatically update:
- ✅ `launchSettings.json` (for Visual Studio debugging)
- ✅ `web.config` (for IIS deployment)

### Step 3: Run Your App
Press **F5** in Visual Studio or run:
```powershell
dotnet run
```

---

## Method 2: Manual Configuration

If you prefer to edit manually, change the environment in these locations:

### For Visual Studio Debugging:
Edit `Properties/launchSettings.json` line 18 and 26:
```json
"ASPNETCORE_ENVIRONMENT": "Development"
```

### For IIS Deployment:
Edit `web.config` line 13:
```xml
<environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Development" />
```

---

## Environment Options

| Environment | Database Server | Database | Use Case |
|-------------|-----------------|----------|----------|
| **Development** | `localhost\SQLEXPRESS` | `DBUTILS` | Local development & testing |
| **Staging** | `174.138.185.18` | `DBUTILS` | Testing before production (⚠️ Currently uses LIVE DB) |
| **Production** | `174.138.185.18` | `DBUTILS` | Live production deployment |

---

## Configuration Files

Each environment has its own configuration file:

- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Local development settings
- `appsettings.Staging.json` - Staging/testing settings
- `appsettings.Production.json` - Production settings

The app automatically loads the correct file based on `ASPNETCORE_ENVIRONMENT`.

---

## Visual Studio Profile Selection

After using the script, you'll see a simplified profile dropdown in Visual Studio:

- **UtilityHub360** - Main profile (uses environment from launchSettings.json)
- **IIS Express** - IIS Express profile (uses environment from launchSettings.json)

Both profiles now use the same environment setting for consistency.

---

## Current Setup Status

✅ **Simplified Configuration** - Only 2 profiles (removed confusing multiple profiles)  
✅ **Single Environment Variable** - Change once, applies everywhere  
✅ **PowerShell Script** - Automated switching with visual feedback  
✅ **Visual Studio Compatible** - Works seamlessly with F5 debugging  
✅ **IIS Compatible** - Works for production deployment  

---

## Troubleshooting

### Issue: Still connecting to wrong database
**Solution:** 
1. Stop your running application
2. Run `.\set-environment.ps1` again
3. Restart Visual Studio
4. Press F5 to debug

### Issue: PowerShell script can't run
**Solution:** Enable script execution:
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Issue: Want to verify which database I'm using
**Solution:** Check the `/health` endpoint:
```
GET http://localhost:5000/health
```

---

## Security Recommendation

⚠️ **Important:** Your Staging environment currently uses the same database as Production.

**Recommended Fix:**
Create a separate test database and update `appsettings.Staging.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=174.138.185.18;Database=DBUTILS_TEST;..."
  }
}
```

---

## Last Updated
October 10, 2025


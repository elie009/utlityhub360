# How to Run Subscription Tables Migration

## Quick Steps

1. **Open SQL Server Management Studio (SSMS)** or **Azure Data Studio**

2. **Connect to your database:**
   - Server: `174.138.185.18`
   - Database: `DBUTILS`
   - Authentication: SQL Server Authentication
   - Username: `sa01`
   - Password: `iSTc0#T3tw~noz2r`

3. **Open the SQL script:**
   - File location: `utlityhub360-backend\UtilityHub360\Documentation\Database\Scripts\create_subscription_tables.sql`

4. **Execute the script** (Press F5 or click Execute)

5. **Verify the tables were created:**
   ```sql
   SELECT TABLE_NAME 
   FROM INFORMATION_SCHEMA.TABLES 
   WHERE TABLE_NAME IN ('SubscriptionPlans', 'UserSubscriptions');
   ```

6. **Verify the default plans were seeded:**
   ```sql
   SELECT Name, DisplayName, MonthlyPrice, YearlyPrice 
   FROM SubscriptionPlans;
   ```

7. **Restart your backend application**

## What This Creates

- ✅ `SubscriptionPlans` table
- ✅ `UserSubscriptions` table  
- ✅ Three default subscription plans:
  - **Starter** (Free) - $0/month
  - **Professional** (Premium) - $9.99/month or $99/year
  - **Enterprise** (Premium Plus) - $29.99/month or $299/year

## Alternative: Using Command Line

If you have `sqlcmd` installed and want to use command line:

```powershell
cd utlityhub360-backend\UtilityHub360
sqlcmd -S 174.138.185.18 -d DBUTILS -U sa01 -P "iSTc0#T3tw~noz2r" -i "Documentation\Database\Scripts\create_subscription_tables.sql"
```

**Note:** Make sure to escape the password properly if it contains special characters.


# 🚀 Allocation Planner Database Migration

## ⚠️ **IMPORTANT: Run the SQL Script, NOT the C# File**

You're getting SQL syntax errors because you tried to run the **C# entity file** (`Allocation.cs`) as SQL. 

**The C# file contains C# code like:**
```csharp
public string Id { get; set; } = Guid.NewGuid().ToString();
```

**SQL Server cannot understand C# syntax!**

---

## ✅ **Solution: Run the SQL Migration Script**

### **Step 1: Open SQL Server Management Studio (SSMS)**
- Or use Azure Data Studio, or any SQL tool
- Connect to your database

### **Step 2: Open the SQL Script**
- File location: `UtilityHub360/create_allocation_tables.sql`
- **NOT** `UtilityHub360/Entities/Allocation.cs` (that's C# code!)

### **Step 3: Execute the Script**
- Click "Execute" or press F5
- The script will:
  - Create 6 new tables for allocation planning
  - Add indexes for performance
  - Seed 4 system templates (50/30/20, Zero-Based Budget, etc.)

---

## 📋 **What the Script Creates**

### **Tables Created:**
1. **AllocationTemplates** - Budget templates (50/30/20 rule, etc.)
2. **AllocationTemplateCategories** - Categories within templates
3. **AllocationPlans** - User's active allocation plans
4. **AllocationCategories** - Categories within user plans
5. **AllocationHistories** - Historical tracking of performance
6. **AllocationRecommendations** - AI-generated recommendations

### **System Templates Seeded:**
- ✅ 50/30/20 Rule (Needs 50%, Wants 30%, Savings 20%)
- ✅ Zero-Based Budget (6 categories, 100% allocation)
- ✅ 60/20/20 Rule (Conservative - 60% Needs)
- ✅ 70/20/10 Rule (Aggressive Savings - 70% Needs)

---

## 🔍 **Verification**

After running the script, verify the tables exist:

```sql
-- Check if tables were created
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME LIKE 'Allocation%'
ORDER BY TABLE_NAME;

-- Check if templates were seeded
SELECT Name, Description, IsSystemTemplate 
FROM [dbo].[AllocationTemplates];
```

You should see:
- 6 tables
- 4 system templates

---

## ✅ **After Migration**

Once the tables are created:
1. **Restart your backend application**
2. The Allocation Planner API endpoints will be available
3. Users can create allocation plans using templates
4. The system will track allocation performance over time

---

## 🐛 **Troubleshooting**

### **Error: "Invalid column name 'UserId'"**
- Make sure the `Users` table exists first
- The script references `[dbo].[Users](Id)` as a foreign key

### **Error: "Table already exists"**
- The script uses `IF NOT EXISTS` checks, so it's safe to run multiple times
- If you need to recreate, drop the tables first:
  ```sql
  DROP TABLE IF EXISTS [dbo].[AllocationRecommendations];
  DROP TABLE IF EXISTS [dbo].[AllocationHistories];
  DROP TABLE IF EXISTS [dbo].[AllocationCategories];
  DROP TABLE IF EXISTS [dbo].[AllocationPlans];
  DROP TABLE IF EXISTS [dbo].[AllocationTemplateCategories];
  DROP TABLE IF EXISTS [dbo].[AllocationTemplates];
  ```

### **Error: "Permission denied"**
- Your SQL user needs `CREATE TABLE` and `ALTER TABLE` permissions
- Contact your database administrator

---

## 📝 **Files Reference**

- ✅ **SQL Script:** `create_allocation_tables.sql` ← **RUN THIS ONE**
- ❌ **C# Entities:** `Entities/Allocation.cs` ← **DO NOT RUN AS SQL**
- ✅ **Service:** `Services/AllocationService.cs`
- ✅ **Controller:** `Controllers/AllocationController.cs`
- ✅ **DTOs:** `DTOs/AllocationDto.cs`

---

**That's it! Once you run the SQL script, the Allocation Planner feature will be fully functional.**


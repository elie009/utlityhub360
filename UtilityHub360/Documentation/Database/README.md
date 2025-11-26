# Database Documentation

This directory contains database migration scripts and setup documentation.

## Structure

- **Scripts/** - SQL migration scripts for various features
- **Migrations/** - Entity Framework Core migration files
- **README_Database_Setup.md** - Database setup instructions

## Important Migration Scripts

### Month Closing Feature
- `create_closed_months_table_final.sql` - Creates the ClosedMonths table for month closing functionality

### Other Scripts
All other SQL scripts in the Scripts/ directory are historical migration scripts. Refer to the Migrations/ directory for current EF Core migrations.

## Usage

1. For new features, use Entity Framework Core migrations: `dotnet ef migrations add <MigrationName>`
2. For manual SQL scripts, review and test in a development environment first
3. Always backup your database before running migration scripts


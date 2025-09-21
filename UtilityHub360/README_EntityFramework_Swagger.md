# Entity Framework and Swagger Integration

This document explains how to use the newly added Entity Framework and Swagger features in the UtilityHub360 project.

## Entity Framework Setup

### Database Configuration
- **Connection String**: The project is configured to use LocalDB with the connection string name "DefaultConnection"
- **Database File**: `App_Data\UtilityHub360.mdf`
- **Provider**: SQL Server (LocalDB)

### Models
- **User Model**: Located in `Models/User.cs`
  - Properties: Id, FirstName, LastName, Email, CreatedDate, LastModifiedDate, IsActive
  - Includes data annotations for validation

### DbContext
- **UtilityHubDbContext**: Located in `Models/UtilityHubDbContext.cs`
  - Inherits from `DbContext`
  - Configured with Fluent API for entity mapping
  - Uses the "DefaultConnection" connection string

## Swagger Integration

### Configuration
- Swagger is configured in `App_Start/WebApiConfig.cs`
- API Documentation is available at: `/swagger`
- Swagger UI is available at: `/swagger/ui`

### API Documentation
- **API Version**: v1
- **Title**: UtilityHub360 API
- **Description**: A utility hub API for various tools and services

## Sample API Controller

### UsersController
Located in `Controllers/UsersController.cs`, provides the following endpoints:

- `GET /api/users` - Get all active users
- `GET /api/users/{id}` - Get a specific user by ID
- `POST /api/users` - Create a new user
- `PUT /api/users/{id}` - Update an existing user
- `DELETE /api/users/{id}` - Soft delete a user (sets IsActive to false)

## Getting Started

### 1. Install Packages
Run the following command in Package Manager Console:
```powershell
Update-Package -reinstall
```

### 2. Enable Migrations (Optional)
If you want to use Code First Migrations:
```powershell
Enable-Migrations
Add-Migration InitialCreate
Update-Database
```

### 3. Run the Application
1. Build and run the project
2. Navigate to `/swagger` to view the API documentation
3. Test the API endpoints using the Swagger UI

### 4. Database Initialization
The database will be created automatically when the first API call is made, or you can manually create it by calling the Users API endpoints.

## Testing the Setup

1. **Swagger Documentation**: Visit `http://localhost:[port]/swagger` to see the interactive API documentation
2. **API Testing**: Use the Swagger UI to test the Users API endpoints
3. **Database**: Check the `App_Data` folder for the created database file

## Next Steps

- Add more models and controllers as needed
- Implement authentication and authorization
- Add more sophisticated validation
- Configure logging and error handling
- Add unit tests for the API controllers


# UtilityHub360 - Loan Management API

## 📋 Project Overview

UtilityHub360 is a comprehensive ASP.NET Core Web API for managing loan applications, payments, and user accounts. It provides a complete backend solution for loan management systems with JWT authentication, role-based authorization, and RESTful API endpoints.

## 🏗️ Architecture

- **Framework**: ASP.NET Core 8.0
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: JWT Bearer Tokens
- **API Style**: RESTful
- **Documentation**: Swagger/OpenAPI

## 📁 Project Structure

```
UtilityHub360/
├── Controllers/          # API Controllers
├── Services/            # Business Logic Services
├── Entities/            # Database Models
├── DTOs/               # Data Transfer Objects
├── Data/               # Database Context
├── Models/             # Common Models & Enums
├── Migrations/         # Database Migrations
├── Documentation/      # Project Documentation
└── Properties/         # Launch Settings
```

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK
- SQL Server
- Visual Studio 2022 or VS Code

### Setup
1. Clone the repository
2. Update connection string in `appsettings.json`
3. Run migrations: `dotnet ef database update`
4. Build and run: `dotnet run`

### Development Mode
The project is configured to run in Development mode by default with Swagger UI available at:
- **HTTP**: http://localhost:5000/swagger
- **HTTPS**: https://localhost:5001/swagger

## 📚 Documentation Index

- [API Endpoints](./API-Endpoints.md) - Complete API reference
- [Authentication Guide](./Authentication-Guide.md) - JWT setup and usage
- [Database Schema](./Database-Schema.md) - Entity relationships
- [Deployment Guide](./Deployment-Guide.md) - Production deployment
- [Development Setup](./Development-Setup.md) - Local development
- [User Roles & Permissions](./User-Roles-Permissions.md) - Authorization guide

## 🔐 Key Features

- **User Authentication & Authorization**
- **Loan Application Management**
- **Payment Processing**
- **Admin Dashboard**
- **Repayment Scheduling**
- **Transaction History**
- **Notification System**
- **Role-based Access Control**

## 📞 Support

For questions or issues, please refer to the documentation files in this folder or contact the development team.

---

**Last Updated**: September 2025  
**Version**: 1.0.0

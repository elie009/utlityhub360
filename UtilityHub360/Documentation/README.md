# UtilityHub360 - Financial Management API

## 📋 Project Overview

UtilityHub360 is a comprehensive ASP.NET Core Web API for managing personal finances including loans, bills, bank accounts, savings, and income tracking. It provides a complete backend solution for financial management with JWT authentication, role-based authorization, and RESTful API endpoints.

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

### General
- [API Endpoints](./General/apiEndpoints.md) - Complete API reference
- [Authentication Guide](./General/authenticationGuide.md) - JWT setup and usage
- [Database Schema](./General/databaseSchema.md) - Entity relationships
- [Deployment Guide](./General/deploymentGuide.md) - Production deployment
- [Development Setup](./General/developmentSetup.md) - Local development

### Billing System
- [Billing API Documentation](./Billing/billingApiDocumentation.md) - Bill management endpoints
- [Variable Monthly Billing Flow](./Billing/variableMonthlyBillingFlow.md) - User guide for analytics & forecasting
- [Variable Monthly Billing Implementation](./Billing/variableMonthlyBillingImplementation.md) - Technical guide
- [Billing Flow Diagrams](./Billing/billingFlowDiagrams.md) - System flows
- [Billing Form Validation](./Billing/billingFormValidation.md) - Frontend validation

### Loan System
- [Loan Due Date Tracking](./Loan/loanDueDateTracking.md) - Due date system
- [Loan Update Flow](./Loan/loanUpdateFlow.md) - Update workflows
- [Principal Update Guide](./Loan/principalUpdateGuide.md) - Principal updates

### Bank Accounts
- [Bank Account Documentation](./BankAccount/bankAccountDocumentation.md) - Bank account management

### User Profile
- [User Profile System](./User/userProfileSystemDocumentation.md) - Profile and income tracking

### Savings
- [Savings Documentation](./Savings/savingsDocumentation.md) - Savings accounts management

### Payments
- [Payment Documentation](./Payment/paymentSystemDocumentation.md) - Payment processing

## 🔐 Key Features

### Core Features
- **User Authentication & Authorization** - JWT-based security
- **Role-based Access Control** - User and Admin roles

### Financial Management
- **Loan Management** - Application, approval, repayment scheduling
- **Bill Management** - Recurring bills, utilities, subscriptions
- **Payment Processing** - Multiple payment methods
- **Bank Account Integration** - Track accounts and transactions
- **Savings Management** - Savings accounts and goals
- **Income Tracking** - Multiple income sources

### Advanced Billing Features ⭐ NEW
- **Variable Monthly Billing Analytics** - Track bills with changing amounts
- **Bill Forecasting** - Predict future expenses (Simple, Weighted, Seasonal methods)
- **Variance Analysis** - Compare actual vs estimated bills
- **Budget Management** - Set budgets per provider/bill type
- **Smart Alerts** - Due date reminders, overdue notices, budget warnings
- **Trend Analysis** - Identify spending patterns
- **Provider Analytics** - Detailed spending analytics per provider
- **Automated Reminders** - Background service for notifications
- **Dashboard Integration** - Comprehensive financial overview

## 📞 Support

For questions or issues, please refer to the documentation files in this folder or contact the development team.

---

## 🎯 What's New in Latest Release

### Variable Monthly Billing System (October 2025)
A comprehensive analytics and forecasting system for bills with variable amounts:

- **17 new DTOs** for analytics, forecasting, variance, and budgets
- **3 new database tables** (BudgetSettings, BillAnalyticsCaches, BillAlerts)
- **20+ new API endpoints** for complete functionality
- **3 forecasting methods** (Simple, Weighted, Seasonal)
- **Intelligent alert system** with 6 alert types
- **Background service** for automated processing
- **Budget tracking** with real-time status
- **Provider analytics** with monthly trends
- **Comprehensive dashboard** integration

See [Variable Monthly Billing Flow](./Billing/variableMonthlyBillingFlow.md) for user guide and [Implementation Guide](./Billing/variableMonthlyBillingImplementation.md) for technical details.

---

**Last Updated**: October 11, 2025  
**Version**: 2.0.0

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

### Financial Dashboard ⭐ NEW
- [Financial Dashboard README](./Financial/README.md) - Complete documentation hub
- [Disposable Amount Flow](./Financial/disposableAmountFlow.md) - User guide & concepts
- [Dashboard API Documentation](./Financial/disposableAmountApiDocumentation.md) - API reference
- [Quick Start Guide](./Financial/DASHBOARD_QUICK_START.md) - Get started in 10 minutes
- [Dashboard Widgets Guide](./Financial/dashboardWidgetsGuide.md) - UI/UX implementation

### Loan System ⭐ **ENHANCED**
- [Loan Management System](./Loan/README.md) - Complete system overview and documentation hub
- [Payment Schedule Management](./Loan/PaymentScheduleManagement.md) - ⭐ **NEW** Add, extend, and manage monthly payment schedules
- [Loan Update Flow](./Loan/loanUpdateFlow.md) - Complete update workflows with smart calculations
- [Loan Due Date Tracking](./Loan/loanDueDateTracking.md) - Due date system and reminders
- [Principal Update Guide](./Loan/principalUpdateGuide.md) - Principal update functionality
- [Monthly Payment Totals](./Loan/loanMonthlyPaymentTotal.md) - Total payment obligations
- [Frontend Update Guide](./Loan/frontendLoanUpdateGuide.md) - Frontend implementation
- [Quick Reference](./Loan/loanUpdateQuickReference.md) - Quick API lookup

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
- **Loan Management** - Application, approval, flexible repayment scheduling ⭐ **ENHANCED**
- **Payment Schedule Management** - ⭐ **NEW** Add, extend, and regenerate monthly payment schedules
- **Bill Management** - Recurring bills, utilities, subscriptions with analytics
- **Payment Processing** - Multiple payment methods with tracking
- **Bank Account Integration** - Track accounts and transactions
- **Savings Management** - Savings accounts and goals
- **Income Tracking** - Multiple income sources

### Advanced Billing Features ⭐
- **Variable Monthly Billing Analytics** - Track bills with changing amounts
- **Bill Forecasting** - Predict future expenses (Simple, Weighted, Seasonal methods)
- **Variance Analysis** - Compare actual vs estimated bills
- **Budget Management** - Set budgets per provider/bill type
- **Smart Alerts** - Due date reminders, overdue notices, budget warnings
- **Trend Analysis** - Identify spending patterns
- **Provider Analytics** - Detailed spending analytics per provider
- **Automated Reminders** - Background service for notifications
- **Dashboard Integration** - Comprehensive financial overview

### Financial Dashboard Features ⭐ NEW
- **Disposable Amount Calculation** - Track money left after expenses
- **Variable Expense Tracking** - Log all discretionary spending
- **Financial Summary Dashboard** - Complete financial overview
- **Smart Insights** - AI-generated financial recommendations
- **Period Comparisons** - Track trends month-over-month
- **Savings Goal Integration** - Plan and track savings targets
- **Category Analytics** - Understand spending patterns
- **Beautiful UI Components** - Pre-built dashboard widgets

## 📞 Support

For questions or issues, please refer to the documentation files in this folder or contact the development team.

---

## 🎯 What's New in Latest Release

### Loan Payment Schedule Management System (October 2025) ⭐ **LATEST**
Complete monthly payment schedule flexibility for loan management:

- ✨ **Extend Loan Terms** - Add additional months to existing loans with ease
- ✨ **Add Custom Payment Schedules** - Insert specific monthly installments anywhere
- ✨ **Regenerate Payment Schedules** - Completely rebuild schedules with new terms
- ✨ **Delete Payment Installments** - Remove specific unpaid installments safely
- 🔧 **Smart Schedule Management** - Automatic interest calculations and conflict prevention
- 📅 **Flexible Due Dates** - Custom payment dates and amounts for any scenario
- 🔐 **Secure Access Control** - Users manage own loans, admins manage all
- 📚 **Comprehensive Documentation** - Complete guides and API references

Perfect for financial hardship situations, loan restructuring, and payment flexibility.

See [Payment Schedule Management Documentation](./Loan/PaymentScheduleManagement.md) for complete details.

### Financial Dashboard & Disposable Amount System (October 2025)
A complete financial health tracking system with disposable amount calculation:

- **New Entity**: VariableExpense for tracking all discretionary spending
- **3 new DTOs**: DisposableAmountDto, FinancialSummaryDto, VariableExpenseDto
- **New Service**: DisposableAmountService with intelligent calculations
- **2 new Controllers**: DashboardController & VariableExpensesController
- **10+ API endpoints** for complete financial tracking
- **Smart Insights**: AI-generated recommendations based on spending
- **Period Comparisons**: Automatic trend analysis
- **Category Analytics**: Understand where money goes
- **Complete UI Library**: Pre-built dashboard widgets
- **Mobile Responsive**: Beautiful UI components

See [Financial Dashboard README](./Financial/README.md) for complete documentation.

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

See [Variable Monthly Billing Flow](./Billing/variableMonthlyBillingFlow.md) for user guide.

---

**Last Updated**: October 12, 2025  
**Version**: 2.2.0 - Added Loan Payment Schedule Management System

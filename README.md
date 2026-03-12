# Leave Management System

A full stack web application for managing employee leave requests and approval workflows built with ASP.NET Core Web API Angular SQL Server and Entity Framework Core

---

## Tech Stack

Backend: ASP.NET Core Web API with Entity Framework Core and JWT Authentication
Frontend: Angular 17 with standalone components and HttpClient
Database: SQL Server
ORM: Entity Framework Core

---

## Features

Employees can register log in apply for different types of leave view their leave balances and cancel pending requests. Managers receive notifications for pending requests and can approve or reject them with optional review notes. Admins have full visibility over all leave requests across the system. Leave balances are automatically updated when a request is approved or cancelled. Date validation and overlap detection prevent conflicting requests from being submitted. JWT authentication protects all API endpoints and role based guards control access to manager and admin routes on the frontend

---

## Project Structure

```
LeaveManagement/
├── backend/
│   ├── Controllers/        API controllers for auth leave and notifications
│   ├── Models/             Entity models
│   ├── DTOs/               Data transfer objects for requests and responses
│   ├── Data/               AppDbContext and EF Core configuration
│   ├── Services/           Business logic for leave processing and authentication
│   ├── Interfaces/         Service interfaces
│   ├── Program.cs          App startup dependency injection and middleware
│   └── appsettings.json    Configuration and connection strings
├── frontend/
│   └── src/app/
│       ├── components/     Login Dashboard ApplyLeave LeaveList
│       ├── services/       AuthService LeaveService NotificationService
│       ├── models/         TypeScript interfaces
│       ├── guards/         Auth guard Manager guard JWT interceptor
│       └── app.routes.ts   Angular routing
└── sql/
    └── leave_management_db.sql   Full database schema triggers and seed data
```

---

## Getting Started

### Database Setup
1 Open SQL Server Management Studio
2 Run sql/leave_management_db.sql to create the database with all tables triggers procedures and seed data

### Backend Setup
1 Open the backend folder in Visual Studio or VS Code
2 Update the connection string in appsettings.json to match your SQL Server instance
3 Run the following commands:
```
dotnet restore
dotnet ef database update
dotnet run
```
4 The API will start at http://localhost:5000
5 Swagger UI is available at http://localhost:5000/swagger

### Frontend Setup
1 Open the frontend folder in a terminal
2 Install dependencies:
```
npm install
```
3 Start the development server:
```
ng serve
```
4 Open http://localhost:4200 in your browser

---

## API Endpoints

POST /api/auth/login - Login and receive JWT token
POST /api/auth/register - Register a new user

GET  /api/leave/my-requests - Get logged in users leave requests
GET  /api/leave/my-balance - Get leave balances for the current year
POST /api/leave/apply - Submit a new leave request
DELETE /api/leave/cancel/{id} - Cancel a leave request

GET /api/leave/pending-reviews - Get pending requests for manager review
PUT /api/leave/review/{id} - Approve or reject a leave request

GET /api/leave/all - Admin only view of all requests

GET /api/notification - Get all notifications for current user
PUT /api/notification/mark-read/{id} - Mark notification as read
PUT /api/notification/mark-all-read - Mark all notifications as read

---

## Default Test Accounts

Admin:    admin@company.com
Manager:  jane@company.com
Employee: john@company.com

Password for all accounts is set in the SQL seed data and should be updated before use

---

## Author
Update this section with your name and contact details


#  Full Stack Authentication System

A complete user authentication system with Registration, Login, JWT Token, and Forgot Password functionality.

## 🛠️ Tech Stack

| Category | Technologies |
|----------|--------------|
| **Frontend** | React.js, React Router, Axios, CSS3 |
| **Backend** | .NET Core 8 Web API, JWT, BCrypt |
| **Database** | MySQL |
| **Email** | SMTP (Gmail/Ethereal) |

## ✨ Features

- ✅ User Registration with Validation
- ✅ User Login with JWT Token
- ✅ Protected Dashboard Route
- ✅ Password Hashing (BCrypt)
- ✅ Forgot Password with Email OTP
- ✅ Password Show/Hide Toggle
- ✅ Remember Me Functionality
- ✅ Responsive Modern UI

## 📁 Project Structure

```
fullstack-auth-app/
├── backend/AuthAPI/
│   ├── Controllers/AuthController.cs
│   ├── Models/ (User.cs, PasswordReset.cs)
│   ├── Services/ (JwtService.cs, EmailService.cs)
│   └── Data/ApplicationDbContext.cs
└── frontend/src/
    ├── pages/ (Login, Register, Dashboard, ForgotPassword)
    ├── services/api.js
    ├── utils/auth.js
    └── styles/global.css
```

## 🚀 Setup Instructions

### Prerequisites
- Node.js (v18+)
- .NET SDK 8.0+
- MySQL Server

### Step 1: Database Setup
```sql
CREATE DATABASE AuthDB;
USE AuthDB;

CREATE TABLE Users (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(100) NOT NULL,
    Email VARCHAR(100) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    Role VARCHAR(50) DEFAULT 'User',
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE PasswordResets (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Email VARCHAR(100) NOT NULL,
    OtpCode VARCHAR(10) NOT NULL,
    ExpiryTime DATETIME NOT NULL,
    IsUsed BOOLEAN DEFAULT FALSE
);
```

### Step 2: Backend Setup
```bash
cd backend/AuthAPI

# Update appsettings.json with your MySQL connection
# "DefaultConnection": "Server=localhost;Port=3306;Database=AuthDB;User=root;Password=;"

dotnet restore
dotnet run
# Runs on: http://localhost:5254
```

### Step 3: Frontend Setup
```bash
cd frontend
npm install
npm start
# Runs on: http://localhost:3000
```

## 🔌 API Endpoints

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/auth/register` | Register new user | No |
| POST | `/api/auth/login` | Login user | No |
| GET | `/api/auth/profile` | Get user profile | Yes |
| POST | `/api/auth/forgot-password` | Send OTP | No |
| POST | `/api/auth/verify-otp` | Verify OTP | No |
| POST | `/api/auth/reset-password` | Reset password | No |

### Sample API Calls

**Register:**
```json
POST /api/auth/register
{
    "name": "John Doe",
    "email": "john@example.com",
    "password": "123456",
    "confirmPassword": "123456"
}
```

**Login:**
```json
POST /api/auth/login
{
    "email": "john@example.com",
    "password": "123456",
    "rememberMe": false
}
```

**Get Profile (Protected):**
```
GET /api/auth/profile
Authorization: Bearer <your_jwt_token>
```

## 🧪 Testing with Postman

1. **Register** → Get token
2. **Login** → Get new token
3. **Profile** → Use token in Authorization header
4. **Forgot Password** → Check email for OTP
5. **Reset Password** → Use OTP to set new password

## 🔐 Environment Variables (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=AuthDB;User=root;Password=;"
  },
  "Jwt": {
    "Key": "YourSuperSecretKeyAtLeast32Chars!",
    "Issuer": "AuthAPI",
    "Audience": "AuthClient",
    "ExpiryMinutes": 60
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "your-email@gmail.com",
    "SenderPassword": "your-app-password"
  }
}
```


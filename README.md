# 🚗 Car Dealership API

A comprehensive RESTful API for managing a car dealership system, built with .NET 9 and featuring role-based access control, OTP security, and complete CRUD operations.

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![API](https://img.shields.io/badge/API-REST-green.svg)](http://localhost:5050/swagger)

---

## 📋 Table of Contents

- [Features](#-features)
- [Tech Stack](#-tech-stack)
- [Getting Started](#-getting-started)
- [API Endpoints](#-api-endpoints)
- [Authentication](#-authentication)
- [Design Decisions](#-design-decisions)
- [Project Structure](#-project-structure)
- [Testing](#-testing)
- [Contributing](#-contributing)
- [License](#-license)

---

## ✨ Features

### Core Functionality
- **User Management**: Registration and login with OTP verification
- **Role-Based Access Control**: Admin and Customer roles with specific permissions
- **Vehicle Management**: Full CRUD operations for vehicle inventory
- **Purchase System**: Customer purchase requests with admin approval workflow
- **OTP Security**: Two-factor authentication for sensitive operations

### Technical Features
- **JWT Authentication**: Secure token-based authentication
- **Entity Framework Core**: In-memory database for development
- **Swagger/OpenAPI**: Interactive API documentation
- **Input Validation**: Comprehensive request validation
- **Error Handling**: Consistent error responses with appropriate HTTP status codes

---

## 🛠️ Tech Stack

| Technology | Purpose |
|------------|---------|
| .NET 9 | Framework |
| ASP.NET Core Web API | REST API development |
| Entity Framework Core | ORM and data access |
| JWT Bearer | Authentication |
| Swagger/OpenAPI | API documentation |
| SHA256 | Password hashing |

---

## 🚀 Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or later
- A code editor (Visual Studio, VS Code, or Rider)
- A REST client (Swagger UI included, or use Postman)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/car-dealership-api.git
   cd car-dealership-api
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the project**
   ```bash
   dotnet build
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Access Swagger UI**
   
   Open your browser and navigate to:
   ```
   http://localhost:5050/swagger
   ```

### Default Credentials

The application seeds two default users on startup:

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@cardealership.com | Admin@123 |
| Customer | customer@example.com | Customer@123 |

---

## 📡 API Endpoints

### Authentication

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/Auth/register/request` | Request user registration | No |
| POST | `/api/Auth/register/verify` | Verify registration with OTP | No |
| POST | `/api/Auth/login/request` | Request login | No |
| POST | `/api/Auth/login/verify` | Verify login with OTP | No |

### Vehicles

| Method | Endpoint | Description | Auth Required | Role |
|--------|----------|-------------|---------------|------|
| GET | `/api/Vehicles` | Get all vehicles (with filters) | Yes | Any |
| GET | `/api/Vehicles/{id}` | Get vehicle by ID | Yes | Any |
| POST | `/api/Vehicles` | Add new vehicle | Yes | Admin |
| POST | `/api/Vehicles/update/request` | Request vehicle update | Yes | Admin |
| PUT | `/api/Vehicles/update/verify` | Verify vehicle update with OTP | Yes | Admin |
| DELETE | `/api/Vehicles/{id}` | Delete vehicle | Yes | Admin |

**Vehicle Filters** (Query Parameters):
- `make`: Filter by manufacturer
- `model`: Filter by model
- `minYear` / `maxYear`: Filter by year range
- `minPrice` / `maxPrice`: Filter by price range
- `isAvailable`: Filter by availability

### Purchases

| Method | Endpoint | Description | Auth Required | Role |
|--------|----------|-------------|---------------|------|
| POST | `/api/Purchases/request` | Request vehicle purchase | Yes | Customer |
| POST | `/api/Purchases/verify` | Verify purchase with OTP | Yes | Customer |
| GET | `/api/Purchases/my-purchases` | Get customer's purchases | Yes | Customer |
| GET | `/api/Purchases` | Get all purchases | Yes | Admin |
| GET | `/api/Purchases/{id}` | Get purchase by ID | Yes | Any* |
| PUT | `/api/Purchases/{id}/process` | Process sale (approve/cancel) | Yes | Admin |

*Customers can only view their own purchases

### Users

| Method | Endpoint | Description | Auth Required | Role |
|--------|----------|-------------|---------------|------|
| GET | `/api/Users/customers` | Get all customers | Yes | Admin |
| GET | `/api/Users` | Get all users | Yes | Admin |
| GET | `/api/Users/{id}` | Get user details | Yes | Admin |

---

## 🔐 Authentication

This API uses JWT (JSON Web Tokens) for authentication with an additional OTP layer for sensitive operations.

### Login Flow

1. **Request Login**
   ```bash
   POST /api/Auth/login/request
   Content-Type: application/json

   {
     "email": "admin@cardealership.com",
     "password": "Admin@123"
   }
   ```

2. **Retrieve OTP**
   
   Check the console output where the API is running. You'll see:
   ```
   [OTP DELIVERY SIMULATION]
   To: admin@cardealership.com
   Purpose: Login
   OTP Code: 123456
   Valid for: 5 minutes
   ```

3. **Verify Login**
   ```bash
   POST /api/Auth/login/verify
   Content-Type: application/json

   {
     "email": "admin@cardealership.com",
     "otpCode": "123456"
   }
   ```

4. **Use Token**
   
   Include the received token in subsequent requests:
   ```bash
   Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
   ```

### OTP-Protected Operations

The following operations require OTP verification:
- User registration
- User login
- Vehicle updates (Admin)
- Purchase requests (Customer)

**OTP Specifications:**
- **Length**: 6 digits
- **Validity**: 5 minutes
- **Usage**: Single-use only
- **Delivery**: Console output (simulated)

---

## 🏗️ Design Decisions

### Architecture

**Clean Architecture Pattern**: The project follows a layered architecture with clear separation of concerns:
- **Controllers**: Handle HTTP requests and responses
- **Services**: Contain business logic
- **Models**: Define data structures
- **DTOs**: Transfer data between layers
- **Data**: Database context and access

### Security

1. **JWT Authentication**: Chosen for stateless authentication, scalability, and cross-platform compatibility
2. **OTP Verification**: Adds an extra security layer for sensitive operations
3. **Password Hashing**: SHA256 hashing for password storage (consider BCrypt for production)
4. **Role-Based Authorization**: Ensures users can only access permitted resources

### Database

**In-Memory Database**: Used for development and demonstration purposes
- **Pros**: Fast setup, no external dependencies, easy testing
- **Cons**: Data is lost on restart
- **Production**: Easily switchable to SQL Server, PostgreSQL, or MySQL

### OTP Delivery

**Console Simulation**: OTP codes are printed to console instead of sent via SMS/Email
- **Reason**: Simplifies development and avoids external service dependencies
- **Production**: Replace with actual SMS/Email service integration

### API Design

- **RESTful Principles**: Standard HTTP methods and status codes
- **Consistent Responses**: Uniform error and success response formats
- **Filtering Support**: Query parameters for flexible data retrieval
- **Swagger Documentation**: Interactive API exploration and testing

---

## 📁 Project Structure

```
CarDealershipAPI/
├── Controllers/
│   ├── AuthController.cs          # Authentication endpoints
│   ├── VehiclesController.cs      # Vehicle management
│   ├── PurchasesController.cs     # Purchase operations
│   └── UsersController.cs         # User management (Admin)
├── Models/
│   ├── User.cs                    # User entity
│   ├── Vehicle.cs                 # Vehicle entity
│   ├── Purchase.cs                # Purchase entity
│   └── OtpCode.cs                 # OTP entity
├── DTOs/
│   ├── AuthDTOs.cs                # Authentication DTOs
│   ├── VehicleDTOs.cs             # Vehicle DTOs
│   └── PurchaseDTOs.cs            # Purchase DTOs
├── Services/
│   ├── OtpService.cs              # OTP generation and validation
│   ├── TokenService.cs            # JWT token management
│   └── PasswordService.cs         # Password hashing
├── Data/
│   └── ApplicationDbContext.cs    # EF Core DbContext
├── Program.cs                     # Application entry point
├── appsettings.json              # Configuration
└── README.md                      # This file
```

---

## 🧪 Testing

### Using Swagger UI

1. Start the application
2. Navigate to `http://localhost:5050/swagger`
3. Click "Authorize" button
4. Login using the default credentials
5. Copy the received token
6. Paste in the authorization dialog with "Bearer " prefix
7. Try any endpoint!

### Using cURL

**Login:**
```bash
# Request login
curl -X POST http://localhost:5050/api/Auth/login/request \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@cardealership.com","password":"Admin@123"}'

# Verify login (replace OTP)
curl -X POST http://localhost:5050/api/Auth/login/verify \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@cardealership.com","otpCode":"123456"}'
```

**Get Vehicles:**
```bash
curl -X GET http://localhost:5050/api/Vehicles \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

**Add Vehicle (Admin):**
```bash
curl -X POST http://localhost:5050/api/Vehicles \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{
    "make": "Tesla",
    "model": "Model 3",
    "year": 2024,
    "price": 45000,
    "color": "White",
    "mileage": 0,
    "vin": "TESLA123456",
    "description": "Brand new Tesla Model 3"
  }'
```

### Automated Testing Script

A test script is included in the repository:
```bash
chmod +x test_api.sh
./test_api.sh
```

---

## 🔄 Assumptions & Limitations

### Assumptions

1. **Purchase Flow**: Customer requests → Admin approves/cancels
2. **No Payment Processing**: Payment integration is out of scope
3. **Single Currency**: All prices in USD
4. **VIN Uniqueness**: Each vehicle has a unique VIN
5. **Email Uniqueness**: Each user has a unique email
6. **Role Assignment**: User role is set during registration and cannot be changed

### Known Limitations

1. **In-Memory Database**: Data is lost on application restart
2. **No File Upload**: Cannot upload vehicle images
3. **Simulated OTP**: OTP is printed to console, not sent via SMS/Email
4. **No Pagination**: All results returned at once (suitable for small datasets)
5. **No Rate Limiting**: No protection against brute force attacks
6. **Single Instance**: Not designed for distributed deployment

### Future Enhancements

- [ ] Persistent database (SQL Server/PostgreSQL)
- [ ] Real SMS/Email integration for OTP
- [ ] Image upload for vehicles
- [ ] Pagination and sorting
- [ ] Advanced search and filters
- [ ] Rate limiting and throttling
- [ ] Unit and integration tests
- [ ] Docker containerization
- [ ] CI/CD pipeline
- [ ] Payment gateway integration
- [ ] Notification system
- [ ] Admin dashboard

---

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 📞 Contact

For questions or support, please open an issue in the GitHub repository.

---

## 🙏 Acknowledgments

- Built with [.NET 9](https://dotnet.microsoft.com/)
- Documentation powered by [Swagger/OpenAPI](https://swagger.io/)
- Inspired by real-world car dealership management systems

---



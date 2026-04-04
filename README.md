# E-Commerce System 🛒

A full-featured E-Commerce system built with ASP.NET API, implementing a scalable layered architecture with support for authentication, product management, Online Payment Integration, orders, and promotions.

---

## 🚀 Overview

This project is a complete e-commerce platform that allows users to browse products, manage their shopping cart, place orders, and apply promotions.

It also includes a powerful admin dashboard for managing all aspects of the system including products, categories, users, and orders.

---

## 🧩 Architecture

The project follows a **Layered Architecture** approach with clear separation of concerns:

- Controllers (Presentation Layer)
- Services (Business Logic Layer)
- Repositories (Data Access Layer)
- Models & DTOs

Additionally, the UI is organized using **ASP.NET Core Areas** to separate different modules:

- Admin Area
- Customer Area
- Identity Area

---

## 📂 Project Structure

- **Areas**
  - Admin → Manage products, categories, users, orders, promotions
  - Customer → Cart, checkout, browsing products
  - Identity → Authentication and account management

- **DataAccess**
  - ApplicationDbContext
  - Migrations

- **Models**
  - Product, Category, Brand
  - Cart, Order, OrderItem
  - Promotion, Review

- **DTOs**
  - Requests (Create, Update, Login, Register, etc.)
  - Responses (Products, Orders, Users, etc.)

- **Repositories**
  - Generic Repository Pattern
  - Custom repositories

- **Services**
  - Business logic implementation
  - AccountService and other services

- **Utilities**
  - Email Sender
  - DB Initializer & Seeders
  - Validation Attributes
  - SD

---

## ✨ Features

### 👤 Authentication & Authorization
- User Registration & Login
- Email Confirmation
- Password Reset
- Role-based Authorization (Super Admin /Admin /Employee/ User)

---

### 🛍️ Customer Features
- Browse Products
- Add to Cart
- Online Payment Integration (Stripe)
- Checkout System
- Place Orders
- Apply Promotions
---

### 🛠️ Admin Features
- Manage Products, Brands, Categories
- Manage Users & Roles
- Manage Orders
- Manage Promotions

---

## 🧰 Technologies Used

- ASP.NET Core MVC
- Entity Framework Core
- SQL Server
- Dependency Injection
- Repository Pattern
- Clean Code & SOLID Principles
- Scalar & PostMan
- Stripe Payment Gateway
---

## 🗄️ Database

- SQL Server
- Code First approach using Entity Framework Core
- Migrations included

---
## 🔐 Configuration

Update the following in appsettings.json:

- Database Connection String
- Update the following in Secret.json:
- Stripe Secret Key
- Stripe Publishable Key
## ⚙️ Installation

1. Clone the repository
   git clone(https://github.com/Ahmedweb22/E-Commerce.API-V9-)

3. Update connection string in `appsettings.json`

4. Apply migrations

5. Run the project

---

## 📈 Future Improvements

- Implement Microservices Architecture
- Add Docker support
- Integrate CI/CD pipeline
- Add caching (Redis)
- Improve performance optimization

---

## 👨‍💻 Author

Developed by Ahmed ElSaid


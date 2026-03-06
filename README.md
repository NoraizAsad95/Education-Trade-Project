# 🎓 Education Trade

Education Trade is a student-powered collaboration platform built with ASP.NET Core MVC.  
It allows students to exchange academic help using a virtual coin-based system — completely free.

---

## 🚀 Project Overview

Education Trade provides a secure and structured environment where:

- Students can post academic tasks
- Other students can accept and complete tasks
- Virtual coins are used instead of real money
- Users earn reputation and ratings
- Email verification ensures secure account creation

The goal is to create a transparent and fair academic collaboration ecosystem.

---

## 🛠️ Built With

- ASP.NET Core MVC (.NET 8)
- Entity Framework Core
- SQL Server
- Bootstrap 5
- Razor Views
- Session-based Authentication
- SMTP Email Verification (Gmail App Password)

---

## ✨ Features

### 👤 Authentication & Security
- User Registration
- Secure Login
- Email Verification (SMTP Integration)
- Session-based authorization

### 💰 Coin System
- Virtual coin balance for each user
- Coins spent when creating tasks
- Coins earned when completing tasks

### 📌 Task Management
- Create Tasks
- Accept Tasks
- Complete Tasks
- Task Status Tracking (Pending / Accepted / Completed)

### ⭐ Rating System
- Users can rate each other after task completion
- Average rating calculation

### 📊 Dashboard
- Coin balance overview
- Total tasks created
- Total tasks accepted
- Task activity summary

---

## 📧 Email Verification

When a new user registers:

1. A verification email is sent.
2. The user must verify their email before accessing the platform.
3. SMTP integration is configured using Gmail App Password.

This ensures secure and validated user accounts.

---

## 🏗️ Project Structure

The project follows a clean layered architecture:

- Controllers → Handle HTTP Requests
- Services → Business Logic
- Repositories → Data Access
- ViewModels → UI Data Mapping




# 📖 BlogProject - Comprehensive Documentation

A modern and scalable **blog platform** built with **.NET 8** and **Razor Pages**.  
It includes categories, posts, user management, and an admin panel with a **responsive UI** and improved **user experience**.  

---

## 📑 Table of Contents
- [Overview](#-overview)
- [Technologies and Architecture](#-technologies-and-architecture)
- [Installation and Setup](#-installation-and-setup)
- [Configuration](#-configuration)
- [Main Modules and Features](#-main-modules-and-features)
- [Database Structure](#-database-structure)
- [Security](#-security)
- [Development and Contribution](#-development-and-contribution)
- [Deployment](#-deployment)
- [Troubleshooting](#-troubleshooting)
- [License](#-license)

---

## 🔎 Overview
**BlogProject** is designed to be a **lightweight yet powerful blogging system**.  
It can be deployed to IIS, Azure, or Linux servers and supports **scalability and modularity**.

---

## 🛠 Technologies and Architecture
| Layer         | Stack                                                                 |
|---------------|----------------------------------------------------------------------|
| **Backend**   | .NET 8, Razor Pages, Entity Framework Core                           |
| **Frontend**  | HTML5, CSS3 (Bootstrap), JavaScript                                  |
| **Database**  | SQL Server (or any RDBMS compatible with EF Core)                    |
| **Session**   | ASP.NET Core Session                                                 |
| **Auth**      | ASP.NET Core Identity (optional)                                     |
| **Email**     | SMTP                                                                 |
| **Security**  | ASP.NET Core Data Protection, HTTPS/HSTS                             |

---

## ⚙️ Installation and Setup

### 1. Requirements
- .NET 8 SDK  
- Visual Studio 2022  
- SQL Server (or compatible DB)  

### 2. Restore Dependencies
```bash
dotnet restore

3. Apply Database Migrations

dotnet ef database update

4. Run the Application

dotnet run

⚡ Configuration
SMTP Settings

Edit appsettings.json:

"EmailSettings": {
  "Host": "smtp.example.com",
  "Port": 587,
  "Username": "your@email.com",
  "Password": "yourpassword"
}

Data Protection

    Keys are stored in App_Data/keys.

    Use SetApplicationName to isolate across environments.

Session

    Add app.UseSession() in Program.cs.

📦 Main Modules and Features
🗂 Category Management

    List categories & posts per category.

    Supports grid, list, and masonry layouts.

✍️ Post Management

    Add posts with drag-drop cover image upload (preview).

    Draft/publish toggle & preview option.

    Edit & delete posts.

👤 User Management (Admin)

    List, delete, and activate users.

    Modal confirmations & tooltip support.

💬 Comments (Optional)

    Users can add comments to posts.

    Admin can manage comments.

⚠️ Error Management

    Dev mode: detailed error page.

    Prod mode: custom error page + HSTS.

    Centralized ExceptionMiddleware.

🔐 Session & Authentication

    User sessions & authorization.

    Role-based access for admin panel.

🗄 Database Structure
Tables

    Users → User info & roles

    Posts → Title, content, cover image, status

    Categories → Name & description

    Comments (optional) → Comment text, linked to posts/users

Relationships

    Category ↔ Posts: 1 → Many

    Post ↔ Comments: 1 → Many

    User ↔ Posts/Comments: 1 → Many

🔒 Security

    Data Protection → Secure key storage

    Authentication → Identity, roles, and session handling

    HTTPS & HSTS enforced in production

    OWASP Top 10 secure coding guidelines followed

🤝 Development and Contribution
Contributing Workflow

    Fork the repo & create a new branch

    Make changes & test thoroughly

    Submit a Pull Request

Code Standards

    Follow C# 12 and .NET 8 standards

    Razor Pages architecture

    Add comments & inline documentation

🚀 Deployment
Build for Production

dotnet publish -c Release

Deploy To

    IIS

    Azure

    Linux server

Environment Variables

    Manage SMTP & DB connections via environment variables

🛠 Troubleshooting

    Migration Errors → Check dependencies & connection string

    SMTP Errors → Verify host, port, and credentials

    Session Errors → Ensure app.UseSession() is configured

    Static Files Issues → Add app.UseStaticFiles()

📬 Contact & Support

For questions, suggestions, or issues → please use GitHub Issues.

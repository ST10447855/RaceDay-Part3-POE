# RaceDay - Event Management System (PROG6212 Part 3 POE)

RaceDay is a full-stack event management solution built with ASP.NET Core API and MVC, leveraging SQL Server, Azure Blob Storage, Docker containerization, and automated GitHub Actions CI/CD workflows.

---

## 📌 Features & Architecture

* **RaceDay.API**: RESTful API backend handling events, participant registrations, and organizer endpoints.
* **RaceDay.MVC**: Frontend web portal with responsive views for event listing, registration, and management.
* **Azure Blob Storage**: Cloud-based media storage for high-resolution event banner images.
* **Docker Support**: Containerized configuration for easy cross-platform deployment[cite: 2].
* **CI/CD Automation**: GitHub Actions pipeline for automated builds and testing[cite: 2].

---

## 🚀 Build & CI/CD Status

The automated CI/CD pipeline builds and tests both API and MVC projects upon every push to the `main` branch[cite: 2].

![CI Build Status](docs/ci-green.png)

---

## 📺 Demonstration Video

Watch the walkthrough of the running application, database/Azure integration, Docker execution, and CI/CD setup:

* **YouTube Video Link**: [https://youtu.be/nUQAUiSqmj0]

---

## ⚙️ How to Run Locally

### Prerequisites
* .NET 8.0 SDK
* SQL Server
* Docker Desktop (optional for container run)

### Steps
1. **Clone the repository**:
   ```bash
   git clone [https://github.com/ST10447855/RaceDay-Part3-POE.git](https://github.com/ST10447855/RaceDay-Part3-POE.git)
   cd RaceDay-Part3-POE

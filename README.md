# Qonote - Your Intelligent Note-Taking Assistant for Videos

## 🚀 About The Project

Qonote is a modern, Notion-like note-taking application designed specifically for educational content on YouTube. The primary goal is to transform passive video watching into an active learning experience by providing structured, searchable, and organized notes for any video.

> **Note:** This project is under active development. The current features are focused on building a robust Minimum Viable Product (MVP), with an exciting roadmap for future enhancements.

---

## ✨ Core Features

### MVP Features (Current Focus)

- 🔐 **Secure User Authentication:** Full lifecycle management including registration, login, token refresh, and logout.
- 🔑 **Modern Authentication Scheme:** Built with JWT Access Tokens and secure `HttpOnly` Refresh Tokens.
- 🔗 **Note Creation from URL:** Automatically fetch video metadata (title, thumbnail, channel) directly from a YouTube link.
- 📑 **Automated Sectioning:** Parse timestamps from the video description to create structured, time-linked note sections automatically.
- 📂 **Workspace Organization:** Organize notes into user-defined groups/folders.
- ✒️ **Rich Text Editing:** Core text formatting capabilities for each note section.

### Post-MVP Roadmap (Future Enhancements)

- 🤖 **AI-Powered Sectioning:** For videos without timestamps, utilize Speech-to-Text (e.g., OpenAI Whisper) and LLMs (e.g., GPT-4) to generate logical sections and titles automatically via background jobs (Hangfire).
- 🧩 **Chrome Extension:** A browser extension to add notes to Qonote with a single click while watching videos on YouTube.
- 🔍 **Advanced Full-Text Search:** Implement a powerful, instant search across all note content using **Elasticsearch**.
- 🎨 **Advanced Editor:** Introduce drag-and-drop reordering for sections and individual content blocks.
- 💰 **Subscription Tiers:** Integrate a payment provider (e.g., Stripe) to offer premium features.
- 🤝 **Shareable Notes:** Generate read-only links to share notes publicly.

---

## 🛠️ Tech Stack & Architecture

This project is built with a modern, full-stack architecture, separating backend and frontend concerns for maximum scalability and maintainability.

### **Backend**

- **Framework:** .NET 8, ASP.NET Core Web API
- **Architecture:** Clean Architecture, Domain-Driven Design (DDD) Principles, CQRS with MediatR, Vertical Slice Architecture
- **Data Persistence:** PostgreSQL with Entity Framework Core, Repository & Unit of Work Patterns
- **Authentication:** ASP.NET Core Identity, JWT (Access + Refresh Tokens), OAuth 2.0 (Google Login - planned)
- **Validation:** FluentValidation
- **Caching:** Redis (planned for caching and rate limiting)
- **API Documentation:** Swagger / OpenAPI

### **Frontend (Planned)**

- **Framework:** **Nuxt 3** (built on **Vue 3**) for Server-Side Rendering (SSR) and a great developer experience.
- **Language:** **TypeScript** for type safety and scalability.
- **Styling:** **TailwindCSS** for a utility-first, modern UI.
- **State Management:** Pinia

### **DevOps & Infrastructure**

- **Containerization:** Docker / Docker Compose for a consistent development environment.
- **CI/CD:** Automated build and test workflows using GitHub Actions.
- **Cloud Provider:** The infrastructure is designed to be deployed on **Microsoft Azure**, utilizing services like Azure App Service, Azure Database for PostgreSQL, and Azure Key Vault for secret management.

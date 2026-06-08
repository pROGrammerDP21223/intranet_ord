# One Rank Digital — Intranet Platform: Complete Feature Documentation

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [System Architecture](#2-system-architecture)
3. [Backend API (.NET 8)](#3-backend-api-net-8)
   - 3.1 [Authentication & Authorization](#31-authentication--authorization)
   - 3.2 [User Management](#32-user-management)
   - 3.3 [Role & Permission Management](#33-role--permission-management)
   - 3.4 [Client Management](#34-client-management)
   - 3.5 [Enquiry Management](#35-enquiry-management)
   - 3.6 [Ticket / Support System](#36-ticket--support-system)
   - 3.7 [Product Catalog](#37-product-catalog)
   - 3.8 [Categories & Industries](#38-categories--industries)
   - 3.9 [Services](#39-services)
   - 3.10 [Financial Transactions](#310-financial-transactions)
   - 3.11 [Sales Pipeline & Hierarchy](#311-sales-pipeline--hierarchy)
   - 3.12 [API Key Management](#312-api-key-management)
   - 3.13 [Document Management](#313-document-management)
   - 3.14 [Image Upload](#314-image-upload)
   - 3.15 [Email Templates](#315-email-templates)
   - 3.16 [Internal Messaging](#316-internal-messaging)
   - 3.17 [Workflows & Automation](#317-workflows--automation)
   - 3.18 [Background Jobs](#318-background-jobs)
   - 3.19 [Analytics & Reporting](#319-analytics--reporting)
   - 3.20 [Data Export](#320-data-export)
   - 3.21 [Audit Logging](#321-audit-logging)
   - 3.22 [Backup & Restore](#322-backup--restore)
   - 3.23 [Data Archival](#323-data-archival)
   - 3.24 [Cache Management](#324-cache-management)
   - 3.25 [Security Features](#325-security-features)
   - 3.26 [Health Checks](#326-health-checks)
   - 3.27 [Contact Forms](#327-contact-forms)
   - 3.28 [Free Registrations](#328-free-registrations)
   - 3.29 [Notifications (Email & WhatsApp)](#329-notifications-email--whatsapp)
4. [Frontend Dashboard (React 19)](#4-frontend-dashboard-react-19)
5. [Database Schema](#5-database-schema)
6. [Infrastructure & Deployment](#6-infrastructure--deployment)
7. [Security Architecture](#7-security-architecture)
8. [All API Endpoints Reference](#8-all-api-endpoints-reference)

---

## 1. Project Overview

**One Rank Digital Intranet** is an enterprise-grade digital agency management system built for internal operations of One Rank Digital. It consists of two tightly integrated components running on a shared SQL Server database:

| Component | Technology | Purpose |
|---|---|---|
| Backend API | ASP.NET Core 8 / C# | Business logic, REST API |
| Internal Dashboard | React 19 + Vite | Staff-facing admin panel |

**Domain setup:**
- `intranet.ordbusinesshub.com` — React dashboard + API

---

## 2. System Architecture

```
┌─────────────────────────────────────────────────────────┐
│                     Single Docker Container             │
│                                                         │
│  ┌─────────┐   ┌──────────────┐                        │
│  │  Nginx  │──▶│  React SPA   │                        │
│  │  :80    │   │  /dashboard  │                        │
│  └────┬────┘   └──────────────┘                        │
│       │                                                  │
│       │        ┌──────────────────────────┐             │
│       └───────▶│  .NET Kestrel :5000       │             │
│                │  REST API                 │             │
│                └────────────┬─────────────┘             │
└─────────────────────────────┼───────────────────────────┘
                              │
                    ┌─────────▼────────┐
                    │  SQL Server 2022 │
                    │  backend_net_db  │
                    └──────────────────┘
```

### Middleware Pipeline (in order)

1. CORS (environment-aware origin policy)
2. `SecurityHeadersMiddleware` — HSTS, X-Frame-Options, CSP headers
3. `RequestLoggingMiddleware` — Structured request/response logging (Serilog)
4. `IpRateLimitingMiddleware` — Per-IP rate limiting (disabled in dev)
5. `ExceptionHandlerMiddleware` — Global exception → structured error response
6. Authentication / Authorization (JWT Bearer)

---

## 3. Backend API (.NET 8)

**Location:** `backend_net/`  
**Framework:** ASP.NET Core 8.0  
**ORM:** Entity Framework Core (SQL Server)  
**Auth:** JWT Bearer tokens  
**Validation:** FluentValidation  
**Logging:** Serilog (structured)  
**Jobs:** Hangfire (SQL Server storage)  
**Cache:** Redis → in-memory fallback  
**Email:** SMTP (Gmail)  

---

### 3.1 Authentication & Authorization

**Controller:** `AuthController`  
**Service:** `AuthService`, `JwtService`

| Endpoint | Method | Auth | Description |
|---|---|---|---|
| `/api/auth/register` | POST | Public | Register a new user |
| `/api/auth/login` | POST | Public | Login, receive JWT |
| `/api/auth/me` | GET | JWT | Get current user profile |
| `/api/auth/forgot-password` | POST | Public | Request password reset email |
| `/api/auth/reset-password` | POST | Public | Reset password with token |

**Features:**
- JWT access token generation and validation (`JwtService`)
- BCrypt password hashing
- Password reset via tokenized email link (`PasswordResetToken` entity)
- Two-Factor Authentication (TOTP + Email OTP)
- Role claims embedded in JWT payload
- `[Authorize]`, `[AllowAnonymous]`, `[AuthorizeRole]`, `[RequireApiKey]` attribute-level security

---

### 3.2 User Management

**Controller:** `UsersController`  
**Service:** `UserService`

| Endpoint | Method | Auth | Description |
|---|---|---|---|
| `/api/users` | GET | Admin/Owner | List all users |
| `/api/users/{id}` | GET | JWT | Get user by ID |
| `/api/users` | POST | Admin/Owner | Create user |
| `/api/users/{id}` | PUT | Admin/Owner | Update user |
| `/api/users/{id}` | DELETE | Admin/Owner | Soft-delete user |

**User Entity Fields:**
- Id, Name, Email, Phone, PasswordHash
- RoleId (linked to Roles table)
- IsActive, IsDeleted, CreatedAt, UpdatedAt
- TwoFactorEnabled, TwoFactorSecret
- Profile image, Company reference

---

### 3.3 Role & Permission Management

**Controllers:** `RolesController`, `PermissionsController`  
**Services:** `RoleService`, `PermissionService`, `AccessControlService`

#### Roles

| Endpoint | Method | Auth | Description |
|---|---|---|---|
| `/api/roles` | GET | JWT | List all roles |
| `/api/roles/{id}` | GET | JWT | Get role with permissions |
| `/api/roles/permissions` | GET | JWT | Get all role permissions |
| `/api/roles` | POST | Admin/Owner | Create role |
| `/api/roles/{id}` | PUT | Admin/Owner | Update role |
| `/api/roles/{id}` | DELETE | Admin/Owner | Delete role |

#### Permissions

| Endpoint | Method | Auth | Description |
|---|---|---|---|
| `/api/permissions` | GET | JWT | List all permissions |
| `/api/permissions/by-category` | GET | JWT | Permissions grouped by category |
| `/api/permissions/{id}` | GET | JWT | Get permission detail |
| `/api/permissions` | POST | Admin/Owner | Create permission |
| `/api/permissions/{id}` | PUT | Admin/Owner | Update permission |
| `/api/permissions/{id}` | DELETE | Admin/Owner | Delete permission |

**Built-in Roles:**
- Owner
- Admin
- HOD (Head of Department)
- Sales Manager
- Sales Person
- Calling Staff
- Employee
- Client

**Features:**
- Many-to-many Roles ↔ Permissions via `RolePermission` join table
- Dynamic permission lookup from database (not hard-coded)
- `AccessControlService` enforces role-based visibility across all queries
- `[AuthorizeRoleAttribute]` for controller-level role enforcement

---

### 3.4 Client Management

**Controller:** `ClientsController`  
**Service:** `ClientService`

| Endpoint | Method | Auth | Description |
|---|---|---|---|
| `/api/clients` | GET | JWT | List clients (role-filtered) |
| `/api/clients/{id}` | GET | JWT | Get client details |
| `/api/clients` | POST | JWT | Create client |
| `/api/clients/{id}` | PUT | JWT | Update client |
| `/api/clients/{id}` | DELETE | JWT | Soft-delete client |
| `/api/clients/{id}/toggle-premium` | PATCH | JWT | Toggle premium status |
| `/api/clients/{id}/email-info` | GET | JWT | Get email info for form dialog |
| `/api/clients/{id}/send-form-email` | POST | JWT | Email a form to client |
| `/api/clients/{id}/approve` | POST | JWT | Approve pending client |

**Client Entity Fields:**
- Company name, website, industry
- Contact person, email, phone
- Address (full)
- Status (Pending, Active, Inactive)
- IsPremium flag
- Company logo
- Linked services: SEO details (`ClientSeoDetail`), AdWords details (`ClientAdwordsDetail`)
- Email service subscriptions (`ClientEmailService`)
- Products assigned (`ClientProduct`)
- Soft delete (IsDeleted)

**Client Sub-Entities:**
- `ClientService` — which services the client is subscribed to
- `ClientEmailService` — email campaign subscriptions
- `ClientSeoDetail` — SEO service configuration
- `ClientAdwordsDetail` — Google Ads configuration

---

### 3.5 Enquiry Management

**Controller:** `EnquiriesController`  
**Service:** `EnquiryService`  
**Security:** `SecurityService` (CAPTCHA, CSRF, honeypot)

| Endpoint | Method | Auth | Description |
|---|---|---|---|
| `/api/enquiries/captcha-challenge` | GET | API Key | Get CAPTCHA image for website |
| `/api/security/captcha` | GET | API Key | Legacy CAPTCHA endpoint |
| `/api/security/csrf` | GET | API Key | Get CSRF token |
| `/api/enquiries` | POST | API Key | Submit enquiry from public site |
| `/api/enquiries` | GET | JWT | List all enquiries (role-filtered) |
| `/api/enquiries/{id}` | GET | JWT | Get enquiry by ID |
| `/api/enquiries/status/{status}` | GET | JWT | Filter by status |
| `/api/enquiries/client/{clientId}` | GET | JWT | Enquiries for a client |
| `/api/enquiries/{id}` | PUT | JWT | Update enquiry |
| `/api/enquiries/{id}` | DELETE | JWT | Delete enquiry |
| `/api/enquiries/statistics` | GET | JWT | Enquiry statistics |

**Enquiry Statuses:** New → In Progress → Resolved → Closed

**Security on public submission:**
- API key required (site-specific key)
- CAPTCHA validation (image-based, 5-minute TTL)
- CSRF token validation
- Honeypot field for bot detection
- Rate limiting (per IP)

**Request DTO fields:** Name, Email, Phone, Message, Service interest, CAPTCHA ID + Answer, CSRF token, Honeypot field

---

### 3.6 Ticket / Support System

**Controller:** `TicketsController`  
**Service:** `TicketService`

| Endpoint | Method | Auth | Description |
|---|---|---|---|
| `/api/tickets` | POST | JWT | Create ticket |
| `/api/tickets` | GET | JWT | List tickets |
| `/api/tickets/{id}` | GET | JWT | Get ticket with comments |
| `/api/tickets/client/{clientId}` | GET | JWT | Tickets for client |
| `/api/tickets/status/{status}` | GET | JWT | Filter by status |
| `/api/tickets/{id}` | PUT | JWT | Update ticket |
| `/api/tickets/{id}/assign` | POST | JWT | Assign ticket to user |
| `/api/tickets/{id}/comments` | POST | JWT | Add comment to ticket |

**Ticket Entity Fields:**
- Title, Description
- Status (Open, In Progress, Resolved, Closed)
- Priority (Low, Medium, High, Critical)
- AssignedToUserId, ClientId
- Comments (`TicketComment` collection)
- Created/Updated timestamps

---

### 3.7 Product Catalog

**Controller:** `ProductsController`, `ClientProductsController`  
**Service:** `ProductService`

#### Products

| Endpoint | Method | Auth | Description |
|---|---|---|---|
| `/api/products` | GET | JWT | List all products |
| `/api/products/by-category/{categoryId}` | GET | JWT | Products in category |
| `/api/products/by-client/{clientId}` | GET | JWT | Products assigned to client |
| `/api/products/{id}` | GET | JWT | Get product detail |
| `/api/products` | POST | JWT | Create product |
| `/api/products/{id}` | PUT | JWT | Update product |
| `/api/products/{id}` | DELETE | JWT | Delete product |

**Product Entity Fields:**
- Name, Slug, Description
- CategoryId, IndustryId
- Price, DiscountedPrice
- Images (`ProductImage` collection)
- IsActive, IsDeleted

#### Client–Product Assignments

| Endpoint | Method | Auth | Description |
|---|---|---|---|
| `/api/clientproducts/client/{clientId}` | GET | JWT | Products for client |
| `/api/clientproducts/attach` | POST | JWT | Assign one product to client |
| `/api/clientproducts/attach-multiple` | POST | JWT | Bulk assign products |
| `/api/clientproducts/detach` | POST | JWT | Remove product from client |
| `/api/clientproducts/check/{clientId}/{productId}` | GET | JWT | Check if assigned |

---

### 3.8 Categories & Industries

**Controllers:** `CategoriesController`, `IndustriesController`  
**Services:** `CategoryService`, `IndustryService`

#### Categories

| Endpoint | Method | Auth | Description |
|---|---|---|---|
| `/api/categories` | GET | JWT | All categories |
| `/api/categories/by-industry/{industryId}` | GET | JWT | Categories for industry |
| `/api/categories/{id}` | GET | JWT | Get category |
| `/api/categories` | POST | JWT | Create category |
| `/api/categories/{id}` | PUT | JWT | Update category |
| `/api/categories/{id}` | DELETE | JWT | Delete category |

#### Industries

| Endpoint | Method | Auth | Description |
|---|---|---|---|
| `/api/industries` | GET | JWT | All industries |
| `/api/industries/{id}` | GET | JWT | Get industry |
| `/api/industries` | POST | JWT | Create industry |
| `/api/industries/{id}` | PUT | JWT | Update industry |
| `/api/industries/{id}` | DELETE | JWT | Delete industry |

---

### 3.9 Services

**Controller:** `ServicesController`  
**Service:** `ServiceService`

| Endpoint | Method | Auth | Description |
|---|---|---|---|
| `/api/services` | GET | JWT | All services |
| `/api/services/by-category` | GET | JWT | Services grouped by category |
| `/api/services` | POST | JWT | Create service |
| `/api/services/{id}` | PUT | JWT | Update service |
| `/api/services/{id}` | DELETE | JWT | Delete service |

---

### 3.10 Financial Transactions

**Controller:** `TransactionsController`  
**Service:** `TransactionService`

| Endpoint | Method | Auth | Description |
|---|---|---|---|
| `/api/transactions/client/{clientId}` | GET | JWT | All transactions for client |
| `/api/transactions/{id}` | GET | JWT | Get transaction |
| `/api/transactions/client/{clientId}/balance` | GET | JWT | Current balance for client |
| `/api/transactions` | POST | JWT | Create transaction |
| `/api/transactions/{id}` | PUT | JWT | Update transaction |
| `/api/transactions/{id}` | DELETE | JWT | Delete transaction |

**Transaction Entity Fields:**
- ClientId (FK)
- Amount, Type (Invoice / Payment / Credit / Debit)
- Description, Reference number
- GST details
- Date, Created/Updated timestamps

---

### 3.11 Sales Pipeline & Hierarchy

**Controllers:** `UserClientsController`, `SalesPersonClientsController`, `SalesManagerClientsController`, `SalesManagerSalesPersonsController`, `OwnerClientsController`

#### User–Client Relationships

| Endpoint | Method | Description |
|---|---|---|
| `/api/userclients/attach` | POST | Assign user to client |
| `/api/userclients/detach` | POST | Remove user from client |
| `/api/userclients/user/{userId}` | GET | Clients for a user |
| `/api/userclients/client/{clientId}` | GET | Users for a client |

#### Sales Person–Client

| Endpoint | Method | Description |
|---|---|---|
| `/api/salespersonclients/attach` | POST | Assign sales person to client |
| `/api/salespersonclients/attach-multiple` | POST | Bulk assign |
| `/api/salespersonclients/detach` | POST | Remove sales person |
| `/api/salespersonclients/salesperson/{id}` | GET | Clients for sales person |
| `/api/salespersonclients/client/{clientId}` | GET | Sales persons for client |

#### Sales Manager–Client

| Endpoint | Method | Description |
|---|---|---|
| `/api/salesmanagerclients/attach` | POST | Assign manager to client |
| `/api/salesmanagerclients/detach` | POST | Remove manager |
| `/api/salesmanagerclients/salesmanager/{id}` | GET | Clients for manager |
| `/api/salesmanagerclients/client/{clientId}` | GET | Managers for client |

#### Sales Manager–Sales Person Hierarchy

| Endpoint | Method | Description |
|---|---|---|
| `/api/salesmanagersalespersons/attach` | POST | Assign sales person under manager |
| `/api/salesmanagersalespersons/detach` | POST | Remove from hierarchy |
| `/api/salesmanagersalespersons/salesmanager/{id}` | GET | Sales persons under manager |
| `/api/salesmanagersalespersons/salesperson/{id}` | GET | Manager(s) for sales person |

#### Owner–Client

| Endpoint | Method | Description |
|---|---|---|
| `/api/ownerclients/attach` | POST | Assign owner to client |
| `/api/ownerclients/detach` | POST | Remove owner |
| `/api/ownerclients/owner/{ownerId}` | GET | Clients for owner |
| `/api/ownerclients/client/{clientId}` | GET | Owners for client |

**Hierarchy:** Owner → Sales Manager → Sales Person → Client

---

### 3.12 API Key Management

**Controller:** `ApiKeysController`  
**Service:** `ApiKeyService`  
**Attribute:** `[RequireApiKey]`

| Endpoint | Method | Auth | Description |
|---|---|---|---|
| `/api/apikeys` | GET | Admin/Owner | List all API keys |
| `/api/apikeys/client/{clientId}` | GET | JWT | Keys for client |
| `/api/apikeys/{id}` | GET | JWT | Get key detail |
| `/api/apikeys` | POST | Admin/Owner | Create API key |
| `/api/apikeys/{id}` | PUT | Admin/Owner | Update key |
| `/api/apikeys/{id}` | DELETE | Admin/Owner | Delete key |
| `/api/apikeys/{id}/test` | GET | JWT | Test key validity |

**Features:**
- API keys scope specific endpoints (e.g., public enquiry submission)
- Keys are hashed in the database
- Keys linked to specific clients/websites
- `[RequireApiKey]` attribute validates the `X-Api-Key` header

---

### 3.13 Document Management

**Controller:** `DocumentsController`  
**Service:** `DocumentService`

| Endpoint | Method | Auth | Description |
|---|---|---|---|
| `/api/documents/entity/{entityType}/{entityId}` | GET | JWT | Documents for entity |
| `/api/documents/{id}` | GET | JWT | Get document metadata |
| `/api/documents/upload` | POST | JWT | Upload document file |
| `/api/documents/{id}/download` | GET | JWT | Download file |
| `/api/documents/{id}` | DELETE | JWT | Delete document |

**Features:**
- Polymorphic document attachment (attach to Client, Ticket, Enquiry, etc.)
- 50MB file upload limit
- Stored on disk with metadata in DB
- EntityType + EntityId for association

---

### 3.14 Image Upload

**Controller:** `ImageUploadController`

| Endpoint | Method | Auth | Description |
|---|---|---|---|
| `/api/imageupload/upload` | POST | JWT | Upload single image |
| `/api/imageupload/upload-multiple` | POST | JWT | Upload multiple images |
| `/api/imageupload/delete` | DELETE | JWT | Delete image |

Used for: company logos, product gallery images, profile pictures.

---

### 3.15 Email Templates

**Controller:** `EmailTemplatesController`  
**Service:** `EmailTemplateService`

| Endpoint | Method | Auth | Description |
|---|---|---|---|
| `/api/emailtemplates` | GET | JWT | All templates |
| `/api/emailtemplates/{id}` | GET | JWT | Get template |
| `/api/emailtemplates/type/{templateType}` | GET | JWT | Templates by type |
| `/api/emailtemplates` | POST | JWT | Create template |
| `/api/emailtemplates/{id}` | PUT | JWT | Update template |

**Template types include:** Welcome, Password Reset, Enquiry Confirmation, Client Form, etc.  
**Features:** Variable substitution in HTML templates, used by NotificationService.

---

### 3.16 Internal Messaging

**Controller:** `MessagesController`  
**Service:** `MessageService`

| Endpoint | Method | Auth | Description |
|---|---|---|---|
| `/api/messages/inbox` | GET | JWT | Get inbox |
| `/api/messages/sent` | GET | JWT | Get sent messages |
| `/api/messages/{id}` | GET | JWT | Get message detail |
| `/api/messages` | POST | JWT | Send message |
| `/api/messages/{id}/read` | POST | JWT | Mark as read |

---

### 3.17 Workflows & Automation

**Controller:** `WorkflowsController`  
**Service:** `WorkflowService`

| Endpoint | Method | Auth | Description |
|---|---|---|---|
| `/api/workflows` | GET | JWT | All workflows |
| `/api/workflows/{id}` | GET | JWT | Get workflow |
| `/api/workflows` | POST | JWT | Create workflow |
| `/api/workflows/{id}` | PUT | JWT | Update workflow |
| `/api/workflows/{id}` | DELETE | JWT | Delete workflow |

**Features:**
- Workflow automation rules triggered by system events
- Linked to `Event` and `Task` entities
- Email notifications via `EmailTemplateService`
- WhatsApp notifications via `TwilioWhatsAppService`

---

### 3.18 Background Jobs

**Controller:** `BackgroundJobsController`  
**Service:** `BackgroundJobService`  
**Engine:** Hangfire (dashboard at `/hangfire`)

| Endpoint | Method | Auth | Description |
|---|---|---|---|
| `/api/backgroundjobs/schedule-task-reminder` | POST | JWT | Schedule a task reminder |
| `/api/backgroundjobs/schedule-backup` | POST | JWT | Schedule a DB backup |
| `/api/backgroundjobs/schedule-archiving` | POST | JWT | Schedule data archiving |
| `/api/backgroundjobs/{jobId}` | DELETE | JWT | Cancel scheduled job |

**Features:**
- Delayed job execution (fire at specific time)
- Recurring jobs (cron expressions)
- Job retry on failure
- Hangfire dashboard for monitoring

---

### 3.19 Analytics & Reporting

**Controller:** `AnalyticsController`  
**Service:** `AnalyticsService`

| Endpoint | Method | Auth | Description |
|---|---|---|---|
| `/api/analytics` | GET | JWT | Dashboard analytics (role-filtered) |

**Metrics available:**
- Total clients, active clients, premium clients
- Enquiry counts by status and date range
- Ticket statistics
- Transaction summaries
- Sales person performance
- Role-based scoping (sales person sees only their data)

---

### 3.20 Data Export

**Controller:** `ExportController`  
**Service:** `ExportService`

| Endpoint | Method | Auth | Description |
|---|---|---|---|
| `/api/export/clients/excel` | GET | JWT | Clients → Excel (.xlsx) |
| `/api/export/clients/csv` | GET | JWT | Clients → CSV |
| `/api/export/enquiries/excel` | GET | JWT | Enquiries → Excel |
| `/api/export/enquiries/csv` | GET | JWT | Enquiries → CSV |
| `/api/export/transactions/excel` | GET | JWT | Transactions → Excel |
| `/api/export/transactions/csv` | GET | JWT | Transactions → CSV |

---

### 3.21 Audit Logging

**Controller:** `AuditLogsController`  
**Service:** `AuditService`

| Endpoint | Method | Auth | Description |
|---|---|---|---|
| `/api/auditlogs` | GET | Admin/Owner | List all audit logs |
| `/api/auditlogs/{id}` | GET | Admin/Owner | Get audit log entry |

**Captures:** EntityType, EntityId, Action (Create/Update/Delete), OldValues (JSON), NewValues (JSON), UserId, Timestamp.  
Every create/update/delete operation across all entities is automatically logged.

---

### 3.22 Backup & Restore

**Controller:** `BackupController`  
**Service:** `BackupService`

| Endpoint | Method | Auth | Description |
|---|---|---|---|
| `/api/backup/create` | POST | Admin/Owner | Create DB backup |
| `/api/backup/restore` | POST | Admin/Owner | Restore from backup |
| `/api/backup/list` | GET | Admin/Owner | List available backups |
| `/api/backup` | DELETE | Admin/Owner | Delete a backup |

---

### 3.23 Data Archival

**Controller:** `ArchiveController`  
**Service:** `ArchiveService`

| Endpoint | Method | Auth | Description |
|---|---|---|---|
| `/api/archive/clients` | POST | Admin/Owner | Archive old client records |
| `/api/archive/enquiries` | POST | Admin/Owner | Archive old enquiries |
| `/api/archive/tickets` | POST | Admin/Owner | Archive old tickets |
| `/api/archive/transactions` | POST | Admin/Owner | Archive old transactions |
| `/api/archive/audit-logs` | POST | Admin/Owner | Archive old audit logs |

---

### 3.24 Cache Management

**Controller:** `CacheStatisticsController`  
**Service:** `RedisCacheService` / `CacheService` (fallback)

| Endpoint | Method | Auth | Description |
|---|---|---|---|
| `/api/cachestatistics` | GET | Admin/Owner | Get cache hit/miss stats |
| `/api/cachestatistics/clear` | POST | Admin/Owner | Clear entire cache |

**Configuration:** Redis primary; falls back to in-memory cache if Redis unavailable.

---

### 3.25 Security Features

**Service:** `SecurityService`  
**Middleware:** `SecurityHeadersMiddleware`, `IpRateLimitingMiddleware`

#### CAPTCHA

| Endpoint | Method | Auth | Description |
|---|---|---|---|
| `/api/enquiries/captcha-challenge` | GET | API Key | Get CAPTCHA image + ID |
| `/api/security/captcha` | GET | API Key | Legacy CAPTCHA endpoint |

- Image-based CAPTCHA generated server-side
- Session-linked with 5-minute expiry
- Validated on enquiry/form submission

#### CSRF Protection

| Endpoint | Method | Auth | Description |
|---|---|---|---|
| `/api/security/csrf` | GET | API Key | Obtain CSRF token |

- Token required on all mutating public endpoints
- Session-bound tokens, validated server-side

#### Other Security Layers
- **Honeypot fields** — Hidden form field, submission rejected if filled
- **Rate limiting** — Per-IP request throttling (`AspNetCoreRateLimit`)
- **Security headers** — HSTS, X-Frame-Options, X-Content-Type-Options, CSP
- **Input validation** — FluentValidation on all request DTOs
- **Password hashing** — BCrypt

---

### 3.26 Health Checks

| Endpoint | Description |
|---|---|
| `/health` | Overall application health |
| `/health/ready` | Readiness probe (DB connectivity) |
| `/health/live` | Liveness probe |

---

### 3.27 Contact Forms

**Controller:** `ContactFormsController`

| Endpoint | Method | Auth | Description |
|---|---|---|---|
| `/api/contactforms` | GET | JWT | All contact form submissions |
| `/api/contactforms/{id}` | GET | JWT | Get submission |
| `/api/contactforms/{id}/status` | PUT | JWT | Update status |
| `/api/contactforms/{id}` | DELETE | JWT | Delete submission |
| `/api/contactforms/stats` | GET | JWT | Submission statistics |

---

### 3.28 Free Registrations

**Controller:** `FreeRegistrationsController`

| Endpoint | Method | Auth | Description |
|---|---|---|---|
| `/api/freeregistrations` | GET | JWT | All free trial registrations |
| `/api/freeregistrations/{id}` | GET | JWT | Get registration |
| `/api/freeregistrations/{id}/approve` | POST | JWT | Approve registration |
| `/api/freeregistrations/{id}/reject` | POST | JWT | Reject registration |
| `/api/freeregistrations/{id}/notes` | PUT | JWT | Update internal notes |

---

### 3.29 Notifications (Email & WhatsApp)

**Service:** `NotificationService`, `EmailService`, `TwilioWhatsAppService`

**Email:**
- SMTP (Gmail) integration
- Template-based HTML emails via `EmailTemplateService`
- Triggered by: enquiry creation, ticket updates, client actions, workflow events

**WhatsApp:**
- Twilio WhatsApp Business API integration
- `TwilioWhatsAppService` sends messages on key events
- Notification handlers wired into the DI pipeline

---

## 4. Frontend Dashboard (React 19)

**Location:** `new-dashboard/`  
**Framework:** React 19.2.0  
**Build tool:** Vite 7.3.1  
**Routing:** React Router DOM 7.13.0  
**Charts:** Chart.js + react-chartjs-2  
**Rich Text:** Jodit React  
**HTTP:** Axios 1.13.5  
**Notifications:** react-toastify  

### Pages / Screens

| Area | Features |
|---|---|
| Login / Register | JWT-based auth, token storage |
| Dashboard | Summary stats, analytics charts |
| Clients | CRUD list with search/filter, approve flow, premium toggle |
| Client Detail | Services, products, transactions, documents, send-form email |
| Enquiries | List, status filters, statistics |
| Tickets | Create, assign, comment, status tracking |
| Products | Catalog management, category assignment, image gallery |
| Categories | CRUD |
| Industries | CRUD |
| Services | CRUD |
| Transactions | Per-client transaction list + balance |
| Users | User management (Admin/Owner only) |
| Roles & Permissions | RBAC configuration |
| API Keys | Create/manage keys per client |
| Documents | Upload/download per entity |
| Email Templates | Create/edit HTML templates with rich text editor |
| Internal Messages | Inbox, sent, compose |
| Workflows | Automation rule management |
| Analytics | Role-scoped metrics and charts |
| Export | Excel/CSV download triggers |
| Audit Logs | Immutable change history viewer |
| Backup | DB backup/restore triggers |
| Archive | Bulk archival actions |
| Cache | Statistics and cache clear |
| Background Jobs | Schedule and cancel jobs |
| Contact Forms | Review and manage website contact submissions |
| Free Registrations | Approve/reject trial signups |

### Global Context Providers

| Provider | Purpose |
|---|---|
| `AuthContext` | Current user, JWT token, login/logout |
| `ThemeContext` | Dark / light mode toggle |
| `LanguageContext` | i18n / localization |
| `LoadingContext` | Global loading spinner |
| `NotificationContext` | Toast notification dispatch |

### Key Reusable Components

- `Layout.jsx` — Sidebar + topbar shell
- `ProtectedRoute.jsx` — Redirect unauthenticated users
- `DataTable.jsx` — Reusable sortable/filterable table
- `Loader.jsx` — Spinner overlay
- `ClientFormPrintView.jsx` — Printable client form
- `ClientProducts.jsx` — Product assignment UI
- `ClientTransactions.jsx` — Inline transaction list
- `SendMailModal.jsx` — Send email dialog
- `RichTextEditor.jsx` — Jodit-based HTML editor wrapper

---

## 5. Database Schema

**Database:** `backend_net_db` (SQL Server 2022)  
All entities extend a base entity with: `Id`, `IsDeleted`, `CreatedAt`, `UpdatedAt`

### Core Entities (35+ tables)

| Table | Description |
|---|---|
| `Users` | System user accounts |
| `Roles` | Role definitions |
| `Permissions` | Permission definitions |
| `RolePermissions` | Role ↔ Permission mapping (many-to-many) |
| `Clients` | Client company records |
| `ClientServices` | Services assigned to clients |
| `ClientEmailServices` | Email subscriptions per client |
| `ClientSeoDetails` | SEO service configuration |
| `ClientAdwordsDetails` | Google Ads configuration |
| `ClientProducts` | Product assignments to clients |
| `Products` | Product definitions |
| `ProductImages` | Product gallery images |
| `Categories` | Product categories |
| `Industries` | Industry verticals |
| `Services` | Digital services offered |
| `Transactions` | Financial transactions per client |
| `Enquiries` | Lead enquiries from public site |
| `Tickets` | Support tickets |
| `TicketComments` | Comments on tickets |
| `Documents` | Uploaded files (polymorphic) |
| `EmailTemplates` | HTML email templates |
| `AuditLogs` | Change audit trail |
| `Logs` | Application log records |
| `Messages` | Internal staff messages |
| `Workflows` | Automation workflow rules |
| `Events` | System events |
| `Tasks` | To-do/reminder tasks |
| `ApiKeys` | API authentication keys |
| `Webhooks` | Webhook configurations |
| `WebhookLogs` | Webhook execution history |
| `ContactForms` | Public contact form submissions |
| `FreeRegistrations` | Free trial requests |
| `DashboardWidgets` | Per-user dashboard customisation |
| `PasswordResetTokens` | One-time password reset tokens |
| `UserClient` | User ↔ Client assignments |
| `SalesPersonClient` | Sales person ↔ Client |
| `SalesManagerClient` | Sales manager ↔ Client |
| `SalesManagerSalesPerson` | Manager ↔ Sales person hierarchy |
| `OwnerClient` | Owner ↔ Client |

### Database Migrations (15+)

1. Initial schema (Users, Roles, Permissions)
2. Transactions entity
3. Products, ProductImages, Categories
4. User-Client relationships
5. Enquiries table
6. Ticket system + TicketComments
7. Two-factor authentication fields
8. Sales Manager hierarchy tables
9. Contact forms
10. Client SEO + AdWords detail tables
11. Company logo support on Clients
12. FreeRegistrations
13. ApiKeys
14. Webhooks + WebhookLogs

---

## 6. Infrastructure & Deployment

### Docker Architecture

Single all-in-one container (`Dockerfile`):

```
Stage 1: Node builder      → Build React dashboard (dist/)
Stage 2: .NET builder      → Publish .NET API
Stage 3: Final runtime
    OS:        Ubuntu + Nginx + .NET Runtime 8
    Drivers:   SQL Server ODBC drivers
    Manager:   Supervisor (manages Nginx, PHP-FPM, Kestrel)
    Paths:
      /var/www/dashboard   → React SPA
      /app                 → .NET API runtime
```

### Nginx Routing (inside container)

| Host | Routes to |
|---|---|
| `intranet.ordbusinesshub.com` | React SPA + proxy `/api/*` → Kestrel :5000 |

### Production VPS

- **Host:** Ubuntu VPS (72.61.248.96)
- **Outer Nginx:** Listens on :80/:443, proxies to container on :8080
- **SSL:** Let's Encrypt certificates
- **Container name:** `ord-all-in-one`
- **Database:** SQL Server (external, connection string via env vars)
- **Backup strategy:** Docker image tagging with timestamps

### Environment Variables (key ones)

| Variable | Used for |
|---|---|
| `DB_SERVER` | SQL Server host |
| `DB_NAME` | Database name |
| `JWT_SECRET` | JWT signing key |
| `SMTP_*` | Email sending credentials |
| `TWILIO_*` | WhatsApp notifications |
| `REDIS_*` | Cache connection |
| `DASHBOARD_HOST` | Nginx virtual host for dashboard |

### Deploy Files (`deploy/`)

| File | Purpose |
|---|---|
| `PROD_UPDATE_REDEPLOY_RUNBOOK.md` | Step-by-step production deploy runbook |
| `check_api_key_prod.sql` | SQL to verify API key in production DB |
| nginx config files | Nginx server block templates |

---

## 7. Security Architecture

| Layer | Mechanism |
|---|---|
| **API Authentication** | JWT Bearer tokens (all internal endpoints) |
| **Public Endpoint Auth** | API Key (`X-Api-Key` header + `[RequireApiKey]`) |
| **Authorisation** | Role-based (`[AuthorizeRole]`) + Permission-based (DB-driven) |
| **CAPTCHA** | Server-generated image, 5-min TTL, session-linked |
| **CSRF** | Token per session, validated on public POST endpoints |
| **Honeypot** | Hidden form field, auto-reject if filled |
| **Rate Limiting** | Per-IP request throttling (`AspNetCoreRateLimit`) |
| **Password Security** | BCrypt hashing |
| **2FA** | TOTP authenticator app + Email OTP |
| **Transport** | HTTPS only (HSTS header, Let's Encrypt SSL) |
| **Security Headers** | HSTS, X-Frame-Options, X-Content-Type-Options, CSP |
| **Input Validation** | FluentValidation on all DTOs |
| **Audit Trail** | Full create/update/delete audit log per entity |
| **Soft Deletes** | Records never physically deleted (IsDeleted flag) |

---

## 8. All API Endpoints Reference

### Quick Reference by Controller

| Controller | Base Route | # Endpoints |
|---|---|---|
| AuthController | `/api/auth` | 5 |
| UsersController | `/api/users` | 5 |
| RolesController | `/api/roles` | 6 |
| PermissionsController | `/api/permissions` | 6 |
| ClientsController | `/api/clients` | 9 |
| EnquiriesController | `/api/enquiries`, `/api/security` | 11 |
| TicketsController | `/api/tickets` | 8 |
| TransactionsController | `/api/transactions` | 6 |
| ProductsController | `/api/products` | 7 |
| CategoriesController | `/api/categories` | 6 |
| IndustriesController | `/api/industries` | 5 |
| ServicesController | `/api/services` | 5 |
| ClientProductsController | `/api/clientproducts` | 5 |
| UserClientsController | `/api/userclients` | 4 |
| SalesPersonClientsController | `/api/salespersonclients` | 5 |
| SalesManagerClientsController | `/api/salesmanagerclients` | 4 |
| SalesManagerSalesPersonsController | `/api/salesmanagersalespersons` | 4 |
| OwnerClientsController | `/api/ownerclients` | 4 |
| ApiKeysController | `/api/apikeys` | 7 |
| DocumentsController | `/api/documents` | 5 |
| EmailTemplatesController | `/api/emailtemplates` | 5 |
| AuditLogsController | `/api/auditlogs` | 2 |
| AnalyticsController | `/api/analytics` | 1 |
| WorkflowsController | `/api/workflows` | 5 |
| MessagesController | `/api/messages` | 5 |
| BackgroundJobsController | `/api/backgroundjobs` | 4 |
| BackupController | `/api/backup` | 4 |
| ArchiveController | `/api/archive` | 5 |
| ExportController | `/api/export` | 6 |
| ImageUploadController | `/api/imageupload` | 3 |
| CacheStatisticsController | `/api/cachestatistics` | 2 |
| ContactFormsController | `/api/contactforms` | 5 |
| FreeRegistrationsController | `/api/freeregistrations` | 5 |
| Health | `/health`, `/health/ready`, `/health/live` | 3 |
| Hangfire Dashboard | `/hangfire` | UI |

**Total: ~175+ API endpoint actions across 34 controllers**

---

*Last updated: 2026-05-05*

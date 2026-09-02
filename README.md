# Argo

Argo is a lightweight portfolio and intake platform for Data Strategy & Delivery teams. It combines:

- **Argo Intake**: a guided, multi-step request intake form.
- **Argo Portfolio**: a portfolio board and detail view for tracking projects, work packages/waves, activities, and RAID items.

The application is a single ASP.NET Core web app that serves both static frontend pages and a minimal API backend, with data stored in SQLite.

## Credits

- **Frontend design**: Garrett Rowley
- **Backend design and implementation**: Eamonn Watson

## What Argo Does

Argo supports the lifecycle from idea intake to portfolio execution:

1. A business/user submits a structured request from **Argo Intake**.
2. Submission creates a new project in the **Waiting** queue in Argo Portfolio.
3. Portfolio users triage and manage the project, then break work into:
   - Work packages/waves
   - Activities
   - RAID records (Risks, Assumptions, Issues, Decisions)

## Tech Stack

- **.NET**: `net10.0`
- **Framework**: ASP.NET Core (Minimal APIs + static file hosting)
- **Data Access**: Entity Framework Core + SQLite
- **Auth**: Windows Authentication (Negotiate)
- **Result handling**: FluentResults
- **Frontend**: HTML/CSS/vanilla JavaScript in `wwwroot`

## Solution Structure

- `Program.cs` - App startup, exception handling, static file hosting, endpoint mapping
- `Extensions/` - Service registration, API endpoint mapping, result conversion, ID generation
- `Services/` - Business logic (`IArgoService`, `ArgoService`)
- `Data/` - EF Core DbContext (`ArgoDbContext`)
- `Models/` - Domain models (Project, WorkItem, Activity, RaidItem, User)
- `DTO/` - API DTO contracts
- `wwwroot/` - Frontend pages/assets/scripts/styles
  - `Index.html` - Argo Portfolio UI
  - `Intake.html` - Argo Intake UI

## Data Model (High Level)

- **Project**
  - Owns many **WorkItems**
  - Owns many **RaidItems**
  - Includes `SourceRequestId`, `SubmittedAt`, optional serialized `IntakeDetails`
- **WorkItem**
  - Belongs to one Project
  - Owns many **Activities**
- **Activity**
  - Belongs to one WorkItem (and carries `ProjectId`)
- **RaidItem**
  - Belongs to one Project
- **User**
  - Keyed by `DomainID`
  - Used for authorization and PM filtering (`IsProjectManager`)

Relationships are configured in `ArgoDbContext` with cascade delete for dependent records.

## Authentication & Authorization

Argo uses Windows Negotiate authentication.

- API access requires an authenticated Windows identity.
- The username (domain prefix removed) must exist in the `Users` table.
- If the user is not recognized, API returns **401 Access Denied**.

> Note: The app does not currently expose a user-management endpoint, so users are managed directly in the database.

## Database Behavior

On startup:

- Database schema is auto-created via `EnsureCreated()`.
- SQLite WAL mode is enabled (`PRAGMA journal_mode=WAL`).

Default database location:

- `argo.db` in `AppContext.BaseDirectory`

Optional connection string override (recommended for explicit environment configuration):

```json
{
  "ConnectionStrings": {
	"ArgoDb": "C:\\path\\to\\argo.db"
  }
}
```

## API Overview (`/api/v1`)

### Portfolio & ingestion

- `GET /portfolio` - Returns full portfolio payload (projects, work items, activities, RAID)
- `POST /portfolio/ingest` - Placeholder ingest endpoint (currently returns empty result)

### Users

- `GET /users?projectManagersOnly=true|false` - Returns users (optionally PM-only)

### Projects

- `POST /projects` - Create project
- `PUT /projects/{id}` - Update project
- `DELETE /projects/{id}` - Delete project

### Work items

- `POST /workitems` - Create work item
- `PUT /workitems/{id}` - Update work item

### Activities

- `POST /activities` - Create activity
- `PUT /activities/{id}` - Update activity

### RAID

- `POST /raid` - Create RAID item
- `PUT /raid/{id}` - Update RAID item

### Intake submissions

- `POST /intake-submissions` - Save submitted intake and create a queued project

## Frontend Behavior

### Argo Intake (`wwwroot/Intake.html` + `wwwroot/js/intake.js`)

- Multi-step request flow (People, Need, Impact, Details, Review)
- Auto-save draft to `localStorage`
- Import/export JSON request payload
- Submission posts to `/api/v1/intake-submissions`
- On success, request is marked submitted and queued in portfolio

### Argo Portfolio (`wwwroot/Index.html` + `wwwroot/js/portfolio.js`)

- Loads portfolio from `/api/v1/portfolio`
- Shows summary metrics, team workload chart, board, and detail panel
- CRUD flows for projects, work items, activities, and RAID items
- Pulls PM names from `/api/v1/users?projectManagersOnly=true`

## ID Conventions

Generated IDs use Crockford Base32 random suffixes:

- Project: `PRJ-XXXXXXXX`
- Work item: `WI-XXXXXXXX`
- Activity: `ACT-XXXXXXXX`
- RAID: `RAID-XXXXXXXX`
- Intake request: `REQ-XXXXXXXX`

## Error Handling

- Global exception handler returns JSON errors.
- In Development, server error message details are included.
- In non-Development, a generic message is returned.
- Result-to-HTTP mapping:
  - Success with payload: `200 OK`
  - Success without payload: `204 No Content`
  - Unauthorized: `401`
  - Not found: `404`
  - Validation/business errors: `400`


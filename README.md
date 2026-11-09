# RaceDay (PROG6212 POE)

RaceDay is a web-based event management system for South African road running, walking and cycling events.
It has three projects: **RaceDay.API** (ASP.NET Core Web API + EF Core + SQL Server), **RaceDay.API.Tests** (xUnit) and
**RaceDay.MVC** (ASP.NET Core MVC front end that talks to the API only).

## Roles
- **Organiser** – creates, edits and deletes events (with a banner image), adds age/distance categories, views all
  enrolments for their events and captures finish times and positions.
- **Participant** – browses and filters events, views event details and categories, enters an event by choosing a
  category, sees their enrolments (colour-coded status), their results history and edits their profile/picture.

Role is chosen at registration. Passwords are hashed (ASP.NET Core `PasswordHasher`) and the API issues a JWT; the MVC
app stores the token, user id and role in server-side **session** and enforces role-based navigation and page access.

## Run locally
1. SQL Server LocalDB (or change `ConnectionStrings:DefaultConnection` in `RaceDay.API/appsettings.json`).
2. In `RaceDay.API` run `dotnet ef database update` (first time only).
3. Visual Studio: set **multiple startup projects** (RaceDay.API then RaceDay.MVC) and press F5, or use two terminals:
   ```
   dotnet run --project RaceDay.API --launch-profile https
   dotnet run --project RaceDay.MVC --launch-profile https
   ```
   API Swagger: https://localhost:7139/swagger – MVC site: https://localhost:7116

## Azure Blob Storage
Uploads go MVC -> `POST /api/Uploads/image` -> Azure Blob Storage; the returned URL is saved on the event
(`BannerImageURL`) or profile (`ProfilePictureURL`). Set these in `RaceDay.API` (user-secrets recommended, never commit keys):
```
dotnet user-secrets set "AzureStorage:ConnectionString" "<your storage account connection string>" --project RaceDay.API
dotnet user-secrets set "AzureStorage:ContainerName" "raceday-images" --project RaceDay.API
```
The container is created automatically with public blob read access.

## Docker (MVC application)
Start the API so the container can reach it (listen on all interfaces):
```
dotnet run --project RaceDay.API --urls "http://0.0.0.0:5276"
```
Then build and run the MVC container:
```
docker build -t raceday-mvc ./RaceDay.MVC
docker run --rm -p 8080:8080 -e ApiSettings__BaseUrl=http://host.docker.internal:5276/ raceday-mvc
```
Open http://localhost:8080 .

## Key API endpoints (Swagger documents all of them)
| Area | Endpoints |
|---|---|
| Auth | `POST /api/Auth/register`, `POST /api/Auth/login` |
| Public | `GET /api/public/events`, `GET /api/public/events/{id}` |
| Organiser | `GET/POST /api/organiser/events`, `PUT/DELETE /api/organiser/events/{id}`, `GET/POST /api/organiser/events/{id}/categories`, `DELETE /api/organiser/categories/{id}`, `GET /api/organiser/events/{id}/enrolments`, `POST /api/organiser/enrolments/{id}/result` |
| Participant | `POST /api/participant/enrol`, `GET /api/participant/enrolments`, `GET /api/participant/results`, `GET/PUT /api/participant/profile` |
| Uploads | `POST /api/Uploads/image` |

## Part 1 documents
The ERD, API endpoint plan and SQL script are in [`docs/`](docs/).

## Differences between the Part 1 plan and the final implementation
The final system follows the Part 1 design, with these deliberate changes made while building Parts 2 and 3:
- **Naming:** the code uses `Event.Name` (ERD: `EventName`), `Result.FinishingTime` / `Result.Position` (SQL: `FinishTime` / `FinishPosition`) and `ParticipantProfile.ProfileID` (as in the SQL script).
- **Fields not implemented:** `Gender`, `EmergencyContactName`, `EmergencyContactPhone`, `EntryFee`, `Event.Status` and `CreatedAt` appear in the ERD but are not used, because the application does not need them. The SQL script already omits them.
- **Enrolment status:** new enrolments are created as `Confirmed` straight away (the SQL default is `Pending`), so a participant's entry is recorded immediately as the Part 3 brief requires.
- **Age limits:** a category with no age limit is stored with min/max age `0`.
- **Endpoints:** the generic CRUD endpoints from the plan are implemented. Role-specific endpoints were added for the MVC app: `/api/organiser/...`, `/api/participant/...`, `/api/public/...` and `/api/Uploads/image`.
- **Passwords and sessions:** passwords are hashed with ASP.NET Core's `PasswordHasher`. The API issues a JWT, and the MVC app keeps the token, user id and role in server-side session.

## CI/CD
GitHub Actions (`.github/workflows/ci-cd.yml`) restores, builds the API and MVC, runs the unit tests and builds the Docker image.

![CI build](docs/ci-green.png)  <!-- add your green-build screenshot here -->

## Video
YouTube (unlisted): <!-- paste link -->

## AI use disclosure
<!-- Required by the brief: state which AI tools you used and for what (e.g. debugging, scaffolding views) -->

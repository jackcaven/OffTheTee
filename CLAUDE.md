# CLAUDE.md — Golf Tournament Management App (Off The Tee)

This file provides Claude Code with full context about the project: its purpose, architecture,
conventions, domain rules, and development priorities. Read this before making any changes.

---

## Project Overview

A web-based golf tournament management platform targeting four audiences:
- **Casual golfers / social rounds** — simple setup, quick scoring, informal leaderboards
- **Charity golf events** — live leaderboards for spectators, easy organiser tools
- **Corporate golf days** — branded event pages, team/individual formats, polished results views
- **Golf club competitions** — full handicap support, multiple scoring formats, multi-round events

### Core Principles
- Responsive design — the app must work on both desktop and mobile; mobile-first for player scorecard entry, desktop-first for organiser dashboard
- Organiser and player experiences are distinct — design for both separately
- Correctness of scoring logic is critical — test all scoring and handicap calculations thoroughly
- Offline resilience — score entry must work without a reliable signal; scores queue locally and sync when connectivity returns
- Keep v1 scope tight — see the "Out of Scope for v1" section before adding features

---

## Tech Stack

### Frontend
- **React 18+ with TypeScript** (strict mode enabled)
- **Vite** as the build tool
- **React Router v6** for client-side routing
- **TanStack Query (React Query)** for server state, caching, and async data fetching
- **Zustand** for lightweight client-side state (UI state, current user session)
- **Tailwind CSS** for styling
- **shadcn/ui** as the component library (built on Radix UI primitives)
- **React Hook Form + Zod** for form handling and validation
- **Workbox** (via vite-plugin-pwa) for service worker and offline score queuing

### Backend
- **.NET 10** — ASP.NET Core Web API (minimal API style for performance and simplicity)
- **Entity Framework Core 10** as the ORM
- **MediatR** for CQRS pattern (Commands and Queries separated)
- **FluentValidation** for request validation
- **SignalR** for real-time live leaderboard updates
- **Serilog** for structured logging

### Database
- **PostgreSQL** as the primary database (scalable, strong JSON support, excellent EF Core support)
- **Redis** for caching leaderboards and session data (reduces DB load during live events)

### Authentication
- **ASP.NET Core Identity** with JWT bearer tokens
- Support for social login (Google OAuth) for player convenience — organisers may use email/password

### Testing
- **xUnit** for backend unit and integration tests
- **Vitest** for frontend unit tests
- **Playwright** for end-to-end tests
- All scoring engine logic MUST have unit test coverage before being considered complete

---

## Repository Structure

```
/
├── src/
│   ├── GolfTournament.Api/          # ASP.NET Core Web API project
│   │   ├── Endpoints/               # Minimal API endpoint definitions
│   │   ├── Middleware/              # Auth, error handling, logging middleware
│   │   └── Program.cs
│   ├── GolfTournament.Application/  # MediatR handlers, Commands, Queries, DTOs
│   │   ├── Tournaments/
│   │   ├── Players/
│   │   ├── Scoring/
│   │   ├── Leaderboard/
│   │   └── Handicap/
│   ├── GolfTournament.Domain/       # Core domain models, enums, domain logic
│   │   ├── Entities/
│   │   ├── Enums/
│   │   └── ValueObjects/
│   ├── GolfTournament.Infrastructure/ # EF Core, repositories, external services
│   │   ├── Persistence/
│   │   ├── Repositories/
│   │   └── ExternalServices/        # Handicap provider integrations
│   └── GolfTournament.Tests/        # xUnit test projects
│       ├── Unit/
│       └── Integration/
├── client/                          # React + TypeScript frontend
│   ├── src/
│   │   ├── components/              # Shared UI components
│   │   ├── features/                # Feature-based folders (tournament, scoring, leaderboard)
│   │   ├── hooks/                   # Custom React hooks
│   │   ├── lib/                     # API client, utilities, Zod schemas
│   │   ├── pages/                   # Route-level page components
│   │   ├── store/                   # Zustand stores
│   │   └── types/                   # Shared TypeScript types
│   └── vite.config.ts
├── tests/
│   └── e2e/                         # Playwright end-to-end tests
└── CLAUDE.md
```

---

## Domain Model

These are the core entities. Keep this section updated as the schema evolves.

### Key Entities

**Tournament**
- Id, Name, Description, OrganiserId
- Format: `Strokeplay | Stableford`
- Status: `Draft | Registration | Active | Completed`
- StartDate, EndDate (supports multi-round)
- CourseId, MaxPlayers, InviteCode (unique, used for player self-registration)

**Round**
- Id, TournamentId, RoundNumber, Date, Status
- A tournament has one or more rounds

**Course**
- Id, Name, Location
- HoleCount: `9 | 12 | 18` (default 18)
- SlopeRating (nullable), CourseRating (nullable) — used for WHS playing handicap calculation
- CourseDataSource: `Manual | API`
- ExternalCourseId (nullable) — reference ID from course data API provider
- Per-hole data (18 rows max, keyed by HoleNumber): Par, StrokeIndex — required for handicap calculations

### Course Data Entry
Two supported methods for populating course hole data:

**Manual entry** — organiser inputs Par and StrokeIndex for each hole via a hole-by-hole form in the app. Required fields per hole: HoleNumber, Par, StrokeIndex. Optional: Yardage. Organiser also optionally enters SlopeRating and CourseRating for the tees being played.

**API integration** — course details fetched from an external course database API (e.g. Golf Course API, or equivalent). On successful fetch, hole data is pre-populated and the organiser can review/edit before saving. `CourseDataSource` is set to `API` and `ExternalCourseId` stored for future re-sync.

- The `ICourseDataProvider` interface abstracts API vs manual — implement `GolfApiCourseDataProvider` and a stub `ManualCourseDataProvider`
- If API fetch fails or no API is configured, always fall back to manual entry gracefully
- 12-hole courses: HoleNumbers 1–12, StrokeIndex values must still be unique within 1–12

**Player**
- Id, UserId (nullable — guests may not have accounts), DisplayName, Email
- HandicapIndex (WHS format, e.g. 14.2)
- HandicapSource: `Manual | WHS_API | CONGU_API`

**TournamentEntry**
- Id, TournamentId, PlayerId, PlayingHandicap (calculated or manually entered at time of entry)
- HandicapCalculationMode: `Auto | Manual`
- FlightId (nullable — for future flight support)

**Score**
- Id, TournamentEntryId, RoundId, HoleNumber, GrossStrokes, GIR, Putts (optional)
- Computed: NetStrokes, StablefordPoints (stored for performance)
- Status: `Draft | Submitted | Verified`
- SubmittedAt (timestamp when player completes and submits round)
- PartnerVerifiedAt (timestamp when partner verification checkbox is confirmed)

**Scorecard Submission**
- Scores are auto-saved hole-by-hole as the player progresses (Status: `Draft`)
- At the end of the round the player reviews their full card, checks a partner verification prompt ("I confirm my playing partner has reviewed and agreed this scorecard"), then submits
- On submission all hole scores for that round move to Status: `Submitted` and the leaderboard reflects the final round total
- Partner verification is a checkbox consent — no separate partner action required in v1

**Leaderboard (read model)**
- Cached/materialised view — rebuilt on each score submission via SignalR broadcast
- Supports Gross and Net views

### Enums
```csharp
public enum TournamentFormat { Strokeplay, Stableford }
public enum TournamentStatus { Draft, Registration, Active, Completed }
public enum HandicapSource { Manual, WHS, CONGU }
public enum ScoreStatus { Draft, Submitted, Verified }
public enum CourseDataSource { Manual, API }
public enum HandicapCalculationMode { Auto, Manual }
```

---

## Scoring Rules

This section defines the exact business logic. Do not deviate from these rules without
updating this document first.

### Stableford Points
```
Score vs Par    Points
Eagle or better   4
Birdie            3 (some variants: 3 + bonus, but default is 3)
Par               2
Bogey             1
Double bogey+     0
```
Net score is used: `NetStrokes = GrossStrokes - HoleHandicapAllowance`
Hole handicap allowance derived from: `PlayingHandicap` and hole `StrokeIndex`

### Playing Handicap (WHS)

Two modes depending on available course data:

**Auto-calculation** (preferred) — used when the course has both `SlopeRating` and `CourseRating` set:
`PlayingHandicap = round(HandicapIndex × (SlopeRating / 113) + (CourseRating - Par))`

**Manual entry** — used when SlopeRating/CourseRating are not available. The organiser or player enters the PlayingHandicap directly at time of tournament entry. The entry form should detect whether course rating data is present and show the appropriate mode, with a clear label indicating which method is being used.

- Store `HandicapCalculationMode: Auto | Manual` on `TournamentEntry`
- Always store the final `PlayingHandicap` value on `TournamentEntry` regardless of calculation mode — never recalculate retroactively once the tournament is active

### Strokeplay
- Gross: total strokes, lowest wins
- Net: `GrossTotal - PlayingHandicap`, lowest wins

> **Out of scope for v1**: Skins and Match Play formats have been de-scoped. Do not implement these. They are candidates for v2.

---

## Handicap Integration

### Priority Order
1. **WHS API** (World Handicap System) — primary integration target
   - England Golf API: `https://api.golfgenius.com` (check for current endpoint)
   - Golf Ireland, Golf Australia use WHS
2. **CONGU** — used by some UK clubs, may have separate API
3. **Manual entry** — always available as fallback

### Integration Pattern
- `IHandicapProvider` interface with implementations: `WHSHandicapProvider`, `ManualHandicapProvider`
- Players link their handicap ID during registration; a background job syncs their index
- Organisers can override a player's handicap at time of entry (stores override flag)
- Always store the handicap value at time of tournament entry — do not recalculate retroactively

---

## API Design Conventions

- Use **minimal APIs** in .NET 10 — group endpoints by feature in `Endpoints/` folder
- **API versioning**: Use URL path versioning — all endpoints are prefixed with `/api/v{version}` (e.g. `/api/v1/tournaments`). Use `Asp.Versioning.Http` NuGet package. Default version is `1.0`. New versions are only introduced for breaking changes — additive changes (new fields, new endpoints) do not require a version bump.
- Follow RESTful conventions: `GET /api/v1/tournaments`, `POST /api/v1/tournaments/{id}/scores`
- All responses use a consistent envelope:
```json
{
  "data": { ... },
  "error": null
}
```
- Validation errors return HTTP 422 with field-level error details
- Use **cursor-based pagination** for lists
- SignalR hub at `/hubs/leaderboard` — clients subscribe to `tournament:{id}` group

### Key Endpoints (non-exhaustive)
```
POST   /api/v1/auth/register
POST   /api/v1/auth/login
GET    /api/v1/tournaments
POST   /api/v1/tournaments
GET    /api/v1/tournaments/{id}
POST   /api/v1/tournaments/{id}/invite             # Generate invite link
POST   /api/v1/tournaments/{id}/join               # Player joins via invite code
GET    /api/v1/tournaments/{id}/leaderboard        # Current leaderboard snapshot
PUT    /api/v1/tournaments/{id}/scores/hole        # Auto-save a single hole score (upsert)
POST   /api/v1/tournaments/{id}/scores/submit      # Submit full round with partner verification flag
GET    /api/v1/tournaments/{id}/scores/{playerId}  # Get player's scorecard for a round
GET    /api/v1/courses                             # List courses
POST   /api/v1/courses                             # Create course (manual entry)
GET    /api/v1/courses/{id}
PUT    /api/v1/courses/{id}
GET    /api/v1/courses/search?name={name}          # Search external course API by name
POST   /api/v1/courses/import                      # Import course from external API by ExternalCourseId
GET    /api/v1/players/{id}/handicap               # Fetch latest handicap from provider
```

---

## Frontend Conventions

- **Feature-based folder structure** — all files for a feature live together
  (`/features/scoring/ScoringPage.tsx`, `/features/scoring/useScoring.ts`, etc.)
- Use **TanStack Query** for all server data — no raw fetch calls in components
- API calls go through a typed client in `lib/api.ts` (use `ky` or `axios`)
- All forms use **React Hook Form + Zod** — define Zod schemas in `lib/schemas/`
- Use **named exports** for components, not default exports
- TypeScript strict mode — no `any` types; use `unknown` and type guards where needed
- **Responsive layout strategy**:
  - Player scorecard: mobile-first, must be fully usable at 375px; single hole entry per screen on mobile, same layout scales to desktop (no separate desktop layout)
  - Organiser dashboard: desktop-first with a usable (not pixel-perfect) mobile experience; use responsive Tailwind breakpoints (`md:`, `lg:`) to adapt layouts
- Sync status indicator component must be present on all scorecard pages

### Key Pages / Routes
```
/                          Landing / marketing page
/auth/login
/auth/register
/dashboard                 Organiser dashboard — their tournaments
/tournaments/new           Create tournament wizard
/tournaments/:id           Tournament overview (organiser view)
/tournaments/:id/leaderboard  Public live leaderboard
/tournaments/:id/score     Player scorecard entry (mobile-optimised)
/join/:inviteCode          Player self-registration via invite link
/courses                   Course management
```

---

## Real-Time Leaderboard

- Uses **SignalR** (WebSockets with fallback)
- Leaderboard updates in real time as players auto-save hole scores during their round — running totals are shown, not just completed rounds
- Each hole save triggers a leaderboard recalculation and SignalR broadcast to the `tournament:{id}` group
- Players with rounds in progress show their current hole count and running total (e.g. "Through 12")
- Players who have submitted and verified their card are marked as "Completed"
- Frontend subscribes on leaderboard page mount, unsubscribes on unmount
- Leaderboard is also cached in **Redis** — HTTP GET returns cached snapshot instantly
- Cache invalidated on every score update
- Public leaderboard page requires no authentication — designed for display on a screen at events

---

## Offline Score Entry

Golf courses frequently have poor or no mobile signal. The scoring experience must be resilient to this.

### Behaviour
- Hole scores are saved to **localStorage** immediately on entry (optimistic local save)
- A background sync queue (via Workbox / service worker) retries failed API saves when connectivity returns
- The UI shows a sync status indicator: "Saved locally", "Syncing...", "Synced ✓", or "Sync failed — tap to retry"
- Players can continue entering scores for subsequent holes even if earlier holes haven't synced yet
- On reconnection, the queue flushes in hole order to maintain data integrity

### Implementation Notes
- Use `navigator.onLine` and the `online` event to detect connectivity changes
- Store pending hole scores in IndexedDB (via `idb` library) — more reliable than localStorage for queuing
- Each queued item includes: tournamentEntryId, roundId, holeNumber, grossStrokes, timestamp
- Deduplicate by (roundId, holeNumber) — if the same hole is edited offline multiple times, only the latest is sent
- The service worker should be registered in `main.tsx` and configured via `vite-plugin-pwa`

---

- Tournaments can have multiple rounds (e.g. 36-hole club competition over 2 days)
- Leaderboard aggregates across all completed rounds
- Scores are entered per-round; cumulative totals shown on leaderboard
- Single-day events have exactly one round (default)

---

## Invite & Registration Flow

1. Organiser creates tournament → system generates unique `InviteCode`
2. Organiser shares link: `https://app.example.com/join/{inviteCode}`
3. Player visits link → prompted to log in or register → automatically joined to tournament
4. Organiser can also manually add players from their dashboard
5. Organiser can close registration (sets tournament status to `Active`)

---

## Non-Functional Requirements

- **Performance**: Leaderboard must load in < 500ms for up to 200 concurrent players
- **Scalability**: API should be stateless and horizontally scalable; use Redis for shared state
- **Security**: JWT expiry 1 hour, refresh tokens 7 days; all score mutations require auth
- **Availability**: Scoring must work on poor mobile connections — consider optimistic UI updates
- **Accessibility**: WCAG 2.1 AA compliance for all player-facing pages

---

## Out of Scope for v1

Do not implement these unless explicitly asked:

- Payment / entry fee processing
- Results export (PDF, CSV, email)
- Flight / division grouping
- Custom branding / white-labelling for corporate events
- Native mobile apps (iOS / Android)
- Push notifications
- Handicap committee review workflows
- GPS course mapping or shot tracking
- Integration with tee time booking systems
- **Skins format** — de-scoped from v1, candidate for v2
- **Match Play / Head-to-Head format** — de-scoped from v1, candidate for v2

---

## Development Priorities (Suggested Build Order)

1. **Project scaffolding** — solution structure, DB migrations, auth endpoints
2. **Course management** — manual hole-by-hole entry (par, stroke index, optional yardage), slope/course rating fields, 9/12/18 hole support
3. **Course API integration** — search and import course data from external provider; fall back to manual if unavailable
4. **Tournament creation** — create, configure format (Strokeplay / Stableford), generate invite code
5. **Player registration** — invite flow, manual add, handicap entry / WHS lookup
6. **Score entry (online)** — hole-by-hole scorecard UI with auto-save, running total display
7. **Scorecard submission** — end-of-round review screen, partner verification checkbox, submit action
8. **Scoring engine** — Stableford and Strokeplay calculations (gross + net) + unit tests
9. **Leaderboard** — REST snapshot + SignalR live running totals + Redis caching
10. **Offline score queuing** — IndexedDB queue, service worker sync, connectivity status indicator
11. **Multi-round support** — aggregate leaderboard across rounds
12. **Organiser dashboard** — tournament management, player management, status control
13. **Public leaderboard page** — unauthenticated, display-screen friendly

---

## v1 Build Plan

This section is the canonical reference for what each build phase delivers and when it is considered done. Read before starting any phase. Update status and notes as phases complete.

---

### Phase 1 — Project Scaffolding ✅ COMPLETE

**Delivers**: Solution skeleton, all domain entities, EF Core migration, JWT auth endpoints, React + Vite + Tailwind frontend scaffold.

**Done**: `dotnet test` passes 10/10. `npm run build` succeeds. `/auth/login` and `/auth/register` pages render.

---

### Phase 2+3 — Course Management + External API Integration

**Delivers**: Full course CRUD (manual entry) and search/import from external course data API (stub falls back gracefully to manual).

**Backend** (`src/GolfTournament.Application/Courses/`, `src/GolfTournament.Api/Endpoints/CourseEndpoints.cs`):
- `CreateCourseCommand` — creates course + hole rows; validates HoleCount ∈ {9,12,18}, hole count matches HoleCount, par ∈ {3,4,5}, StrokeIndex values unique within course
- `UpdateCourseCommand` — replaces all holes; validates same rules; guards against editing if tournament is Active/Completed
- `GetCourseQuery` — single course with holes eager-loaded
- `ListCoursesQuery` — cursor-paginated list
- `SearchExternalCoursesQuery` — delegates to `ICourseDataProvider.SearchCourseAsync`; returns empty list when stub
- `ImportExternalCourseCommand` — imports from `ICourseDataProvider.ImportCourseAsync`, sets `CourseDataSource = API`
- Endpoints: `GET/POST /api/v1/courses`, `GET/PUT /api/v1/courses/{id}`, `GET /api/v1/courses/search?name=`, `POST /api/v1/courses/import`
- No new EF migration needed — `Course` and `CourseHole` tables exist

**Frontend** (`client/src/pages/CoursesPage.tsx`, `CourseCreatePage.tsx`, `CourseDetailPage.tsx`, `client/src/features/courses/ExternalCourseSearch.tsx`):
- `/courses` — list of courses with name, location, hole count
- `/courses/new` — two-section form: course details + hole table (useFieldArray, auto-generates rows on holeCount change); inline external search panel
- `/courses/:id` — read-only view with edit toggle; same form pre-populated
- Zod schema validates all hole rules client-side before submit

**Tests**: `CreateCourseCommandValidatorTests` (all validation edge cases), `CreateCourseCommandHandlerTests` (happy path, duplicate protection)

**Done when**: Can create an 18-hole course manually via API and UI; can create a 9-hole course; `GET /api/v1/courses/search` returns empty array (stub); `POST /api/v1/courses/import` returns 400 with clear message (stub).

---

### Phase 4 — Tournament Creation

**Delivers**: Organiser can create a tournament, configure format and dates, and get a shareable invite link.

**Backend** (`src/GolfTournament.Application/Tournaments/`):
- `CreateTournamentCommand` — creates tournament + one default Round; generates unique `InviteCode` (8-char alphanumeric); validates CourseId exists, StartDate ≤ EndDate
- `GetTournamentQuery` — returns tournament with course name, round count, entry count
- `ListTournamentsQuery` — cursor-paginated; scoped to `OrganiserId` (the calling user)
- `GenerateInviteLinkQuery` — returns `https://{host}/join/{inviteCode}` (reads InviteCode from DB)
- Endpoints: `GET/POST /api/v1/tournaments`, `GET /api/v1/tournaments/{id}`, `POST /api/v1/tournaments/{id}/invite`
- All tournament mutations require `[Authorize]`

**Frontend** (`client/src/pages/TournamentsPage.tsx`, `TournamentCreatePage.tsx`, `TournamentDetailPage.tsx`):
- `/dashboard` — lists organiser's tournaments; status badges; "New tournament" CTA
- `/tournaments/new` — wizard: name/description → format + dates → course picker → review → create
- `/tournaments/:id` — tournament overview: status, course, rounds, invite link (copy button), player count

**Tests**: `CreateTournamentCommandValidatorTests`, `GenerateInviteCodeTests` (uniqueness, format)

**Done when**: Organiser can create a tournament and copy an invite link that resolves to `/join/{code}`.

---

### Phase 5 — Player Registration

**Delivers**: Players can join a tournament via invite link; organisers can manually add players; handicap index can be entered manually or fetched from WHS stub.

**Backend** (`src/GolfTournament.Application/Players/`):
- `JoinTournamentCommand` — looks up tournament by InviteCode; creates/finds `Player` record for the authenticated user; creates `TournamentEntry`; calculates `PlayingHandicap` (Auto if course has SlopeRating + CourseRating, else Manual); validates tournament is in `Registration` status
- `AddPlayerCommand` — organiser manually adds a player by email; creates Player if not found
- `GetPlayerHandicapQuery` — calls `IHandicapProvider.GetHandicapIndexAsync`; returns null for stub
- `ListTournamentEntriesQuery` — returns all entries for a tournament with player names and playing handicap
- `UpdateEntryHandicapCommand` — organiser overrides a player's playing handicap
- Endpoints: `POST /api/v1/tournaments/{id}/join`, `POST /api/v1/tournaments/{id}/players`, `GET /api/v1/tournaments/{id}/players`, `PUT /api/v1/tournaments/{id}/players/{entryId}`, `GET /api/v1/players/{id}/handicap`

**Frontend**:
- `/join/:inviteCode` — if not logged in: prompt register/login then auto-join; if logged in: one-tap join with handicap entry (show Auto mode if course ratings present, Manual input otherwise)
- Tournament detail page gains a "Players" tab showing entries + playing handicap
- Organiser can add a player by email from the Players tab

**Tests**: `JoinTournamentCommandTests` (status guard, auto vs manual handicap calc, duplicate entry prevention), `PlayingHandicapCalculationTests` (WHS formula with reference values)

**Done when**: Player visits invite link, logs in, enters handicap, and appears in the tournament entry list. Playing handicap calculated correctly for both Auto and Manual modes.

---

### Phase 6+7 — Score Entry + Scorecard Submission

**Delivers**: Player can enter hole-by-hole scores on mobile; scores auto-save as drafts; end-of-round review + partner verification checkbox submits the card.

**Backend** (`src/GolfTournament.Application/Scoring/`):
- `SaveHoleScoreCommand` — upsert a single hole score (Draft status); computes and stores `NetStrokes` and `StablefordPoints` immediately; validates HoleNumber in range, GrossStrokes > 0
- `SubmitScorecardCommand` — validates all holes for the round have a Draft score; sets all to `Submitted`; records `SubmittedAt` and `PartnerVerifiedAt` (if flag passed); returns final round totals
- `GetScorecardQuery` — returns player's full scorecard for a round (all holes, gross, net, stableford)
- Endpoints: `PUT /api/v1/tournaments/{id}/scores/hole`, `POST /api/v1/tournaments/{id}/scores/submit`, `GET /api/v1/tournaments/{id}/scores/{playerId}`

**Frontend** (`client/src/pages/ScorecardPage.tsx`, `client/src/features/scoring/`):
- `/tournaments/:id/score` — mobile-first scorecard; single hole entry: large number input, GIR toggle, putts input; prev/next navigation; running total displayed
- Sync status indicator on every hole: "Saved", "Syncing…", "Sync failed — tap to retry"
- Final screen: full scorecard table review + partner verification checkbox + Submit button
- Confirmed submit navigates to `/tournaments/:id/leaderboard`

**Tests**: `SaveHoleScoreCommandTests`, `SubmitScorecardCommandTests` (all holes required, already-submitted guard), `ScoringCalculationTests` (hole handicap allowance, StablefordPoints for all score-vs-par combinations)

**Done when**: Player can enter all 18 holes and submit; submitted card appears in DB with `Submitted` status; cannot resubmit.

---

### Phase 8 — Scoring Engine

**Delivers**: Domain-layer scoring logic with full unit test coverage — this is the correctness-critical heart of the application.

**Backend** (`src/GolfTournament.Domain/Services/ScoringEngine.cs`):
- `ScoringEngine` static class (pure functions, no DI, no EF):
  - `CalculateHoleHandicapAllowance(int playingHandicap, int strokeIndex, int holeCount)` — strokes received on a given hole
  - `CalculateNetStrokes(int grossStrokes, int holeHandicapAllowance)` → int
  - `CalculateStablefordPoints(int netStrokes, int par)` → int (0–4 per CLAUDE.md table)
  - `CalculateStrokeplayGrossTotal(IEnumerable<Score>)` → int
  - `CalculateStrokeplayNetTotal(IEnumerable<Score>)` → int
  - `CalculateStablefordTotal(IEnumerable<Score>)` → int
  - `CalculatePlayingHandicap(decimal handicapIndex, decimal slopeRating, decimal courseRating, int par)` → int (WHS formula)

- Wire `SaveHoleScoreCommand` handler to use `ScoringEngine` instead of inline calculation

**Tests** (`src/GolfTournament.Tests/Scoring/ScoringEngineTests.cs`):
- Stableford: eagle=4, birdie=3, par=2, bogey=1, double bogey+=0
- Hole handicap allowance: full stroke, extra strokes, no strokes (multiple playing handicap values)
- WHS playing handicap formula with reference values from the WHS handbook
- Net strokeplay total

**Done when**: All scoring unit tests pass; scoring engine has zero infrastructure dependencies; `SaveHoleScoreCommand` uses the engine.

---

### Phase 9 — Leaderboard

**Delivers**: Live leaderboard updating in real time as scores are saved; Redis-cached HTTP snapshot; public access (no auth required).

**Backend** (`src/GolfTournament.Application/Leaderboard/`):
- `GetLeaderboardQuery` — returns cached snapshot from Redis if available; falls back to DB query; builds leaderboard sorted by format (Stableford: highest points first, Strokeplay: lowest strokes first)
- `LeaderboardEntry` DTO: PlayerId, DisplayName, PlayingHandicap, RoundStatus ("In progress — through N", "Completed"), GrossTotal, NetTotal or StablefordTotal
- On every `SaveHoleScoreCommand` and `SubmitScorecardCommand`: publish `LeaderboardUpdatedEvent` via MediatR; notification handler rebuilds leaderboard, updates Redis, broadcasts via `IHubContext<LeaderboardHub>`
- Full `LeaderboardHub` implementation: `SubscribeToTournament` / `UnsubscribeFromTournament` groups
- Endpoint: `GET /api/v1/tournaments/{id}/leaderboard` — no `[Authorize]`; returns cached or computed leaderboard

**Frontend** (`client/src/pages/LeaderboardPage.tsx`):
- `/tournaments/:id/leaderboard` — table: position, name, handicap, score; Gross/Net toggle for Strokeplay; auto-refreshes via SignalR connection
- "Through N" row style for in-progress players; "Completed" badge
- Subscribes to `tournament:{id}` SignalR group on mount, unsubscribes on unmount

**Tests**: `LeaderboardCalculationTests` (sort order, tie handling, in-progress display)

**Done when**: Leaderboard updates within 1 second of a hole save; Redis cache hit on second GET; public page loads without auth token.

---

### Phase 10 — Offline Score Queuing

**Delivers**: Hole scores are saved locally first (IndexedDB); a service worker syncs them when connectivity returns; the UI shows clear sync status at all times.

**Frontend** (`client/src/features/scoring/offlineQueue.ts`, `client/src/features/scoring/useSyncStatus.ts`):
- `offlineQueue.ts` — IndexedDB queue using `idb` library; operations: `enqueue(item)`, `dequeue(roundId, holeNumber)`, `getAll()`, deduplicate by `(roundId, holeNumber)`
- Queue item shape: `{ tournamentEntryId, roundId, holeNumber, grossStrokes, gir, putts, timestamp }`
- `useSyncStatus` hook — monitors `navigator.onLine`; flushes queue in hole-number order when online; exposes per-hole status: `'local' | 'syncing' | 'synced' | 'error'`
- `SyncStatusIndicator` component — shown on every scorecard page; top-bar pill: "Saved locally / Syncing… / Synced ✓ / Sync failed — tap to retry"
- Workbox background sync registered in service worker as fallback if tab is closed during sync

**Tests** (Vitest): `offlineQueue.test.ts` — enqueue, deduplication (same hole overwritten), flush order, retry on failure

**Done when**: Disconnecting network mid-round still allows score entry; reconnecting triggers automatic sync; all holes reach "Synced ✓" status.

---

### Phase 11 — Multi-Round Support

**Delivers**: Tournaments can span multiple rounds; leaderboard aggregates across all completed rounds.

**Backend**:
- `CreateRoundCommand` — adds a new Round to an existing tournament (organiser only; tournament must be Active)
- `GetLeaderboardQuery` updated — aggregates scores across all rounds; cumulative totals per player
- `LeaderboardEntry` updated — shows per-round breakdown + cumulative total
- Endpoint: `POST /api/v1/tournaments/{id}/rounds`

**Frontend**:
- Tournament detail page gains a Rounds tab; organiser can add rounds
- Leaderboard shows cumulative column + expandable per-round detail
- Scorecard page uses `roundId` query param to target a specific round

**Done when**: A 2-round tournament accumulates correctly; leaderboard shows correct cumulative totals after both rounds submitted.

---

### Phase 12 — Organiser Dashboard

**Delivers**: Full-featured organiser home screen; tournament lifecycle management; player management.

**Frontend** (`client/src/pages/DashboardPage.tsx`, `client/src/features/dashboard/`):
- `/dashboard` — list of organiser's tournaments; status filter (Draft / Registration / Active / Completed); "New tournament" button
- Tournament detail page gains management actions:
  - **Open registration** (Draft → Registration)
  - **Start tournament** (Registration → Active); closes registration
  - **Complete tournament** (Active → Completed)
  - **Add player** by email
  - **Override handicap** for a player entry
  - **View / download** round scores (table view per round)
- Player list shows: name, handicap, playing handicap, rounds submitted

**Backend** (`src/GolfTournament.Application/Tournaments/`):
- `UpdateTournamentStatusCommand` — transitions status; validates allowed transitions (Draft→Registration, Registration→Active, Active→Completed); prevents backward transitions

**Done when**: Organiser can run a tournament end-to-end from the dashboard without using the API directly.

---

### Phase 13 — Public Leaderboard Page

**Delivers**: A display-screen-friendly, unauthenticated leaderboard page suitable for projecting at events.

**Frontend** (`client/src/pages/PublicLeaderboardPage.tsx`):
- `/tournaments/:id/leaderboard` — same route, same component, but accessible without auth
- Large-format table: position, name (surname bold), score vs par (Stableford or net Strokeplay), round status
- Auto-refreshes via SignalR (no auth required for leaderboard hub)
- Minimal chrome — no nav bar, no sidebar; tournament name + logo area at top
- Responsive: works on a laptop display at 1080p and on a TV at 1920×1080

**Done when**: Public URL loads without a JWT; updates live when a score is saved; renders cleanly at 1080p.

---

### v1 Complete

All 13 phases done. The following E2E test flow should pass (Playwright):

1. Organiser registers → creates course → creates tournament → copies invite link
2. Player visits invite link → registers → enters handicap → joins tournament
3. Organiser opens registration → starts tournament
4. Player opens scorecard → enters 18 holes → submits with partner verification
5. Public leaderboard reflects the submission in real time

---

## Testing Expectations

- All scoring calculation functions must have **unit tests before merging**
- Handicap calculation logic must be tested with known WHS reference values
- API endpoints should have **integration tests** using `WebApplicationFactory`
- Offline queue logic must have unit tests covering: save, deduplication, flush order, and retry behaviour
- E2E tests (Playwright) should cover: register → join tournament → submit hole scores → partner verify and submit → view leaderboard

---

## Notes for Claude Code

- Always check this file before starting a new feature to understand context and conventions
- When creating new endpoints, follow the existing minimal API pattern in `Endpoints/`
- When adding EF Core migrations, always review the migration before applying
- Scoring logic lives in `GolfTournament.Domain` — keep it free of infrastructure dependencies
- If you're unsure whether a feature is in scope for v1, check the "Out of Scope" section above
- Prefer explicit, readable code over clever one-liners — this codebase will be maintained long-term
- When handicap provider APIs are unavailable or credentials are missing, fall back to manual mode gracefully
- The offline queue (IndexedDB) is the source of truth for unsynced scores — never discard queued items without explicit user action or successful server confirmation
- Partner verification is a UI-level checkbox only in v1 — do not build a separate partner approval workflow

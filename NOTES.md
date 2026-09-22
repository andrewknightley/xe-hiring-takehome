# Rate Alerts Implementation: Technical Notes

## Overview
Successfully implemented a complete rate alerts feature for the Xe Rate Board application. The feature allows users to define rate thresholds for currency pairs and get notified when rates cross those thresholds.

## Time Investment
- **Total: ~2.5 hours** (within 3-hour target)
  - Backend refactoring (HTTP client factory): ~30 min
  - Alert feature backend (models, services, controller, tests): ~1.5 hours
  - Frontend integration (state, UI, API calls): ~30 min

## Architecture & Design Decisions

### Backend Architecture

#### 1. **Refactored Rates Pipeline** (Completed Earlier)
- **HttpClient Factory Pattern**: Replaced 3 inline `HttpClient` instantiations with a single typed client via `AddHttpClient<XeRatesClient>()`. Centralized auth configuration and eliminated socket exhaustion risk.
- **IRatesProvider Abstraction**: Created clean interface for fetching rates; allows easy testing and future implementations.
- **Typed Client Pattern**: `XeRatesProvider` wraps the low-level `XeRatesClient`, orchestrating multi-pair fetches and formatting responses as `RateResult` records.

#### 2. **Alert Feature Design**

##### Data Model
- **Alert.cs**: Core domain model with Id (Guid), Pair, Threshold, Direction ("above"/"below"), and CreatedAt timestamp.
- **AlertDto.cs**: Response DTO includes computed `Triggered` state.
- **CreateAlertRequest.cs**: Input DTO for creating alerts.

##### Service Layer
- **IAlertService**: Interface defining CRUD operations and pair validation.
- **InMemoryAlertService**: Implementation using thread-safe `lock`-based synchronization.
  - **Trade-off: In-Memory Storage**: Alerts are lost on application restart. This is acceptable for a take-home project because:
	- Simplicity: No database setup or migration needed.
	- Fast development: Focus on business logic and testing.
	- Easy to upgrade: Simply inject `IDbContext` or call out to data layer in the future.
  - **Thread Safety**: Uses `lock` for concurrent access to the alerts list. Good enough for low-scale; could upgrade to `ReaderWriterLockSlim` for higher concurrency if needed.

##### Alert Evaluation
- **AlertEvaluator.cs**: Pure utility class encapsulating trigger logic.
  - Evaluates: `direction == "above" ? currentRate > threshold : currentRate < threshold`
  - Separated from storage for testability and single responsibility.
  - **Boundary Condition**: Rate must strictly exceed/be below threshold (exact equals does **not** trigger).

##### Controller
- **AlertsController.cs**: RESTful API endpoints:
  - `GET /api/alerts`: Lists all alerts with live triggered state (fetches fresh rates on each call).
  - `POST /api/alerts`: Creates alert with validation (pair, direction, threshold).
  - `DELETE /api/alerts/{id}`: Deletes alert.
  - **Defensive Coding**: Gracefully handles rate fetch failures; conservative (assumes alert not triggered if rates unavailable).

##### Dependency Injection
- Registered `InMemoryAlertService` as scoped (one instance per request).
- Registered `AlertEvaluator` as singleton (stateless utility).
- Both injected alongside `IRatesProvider` for clean separation of concerns.

### Frontend Integration

#### State Management (state.ts)
- Extended reactive state with:
  - `alerts`: Array of current alerts
  - `showAlertForm`: Boolean toggling form visibility
  - `newAlert`: Object holding form inputs (pair, threshold, direction)

#### UI Components (App.vue)
- **Alert Display Sections**:
  - **Triggered Alerts**: Highlighted section showing only alerts currently triggered (with ⚠️ icon).
  - **All Alerts**: List view of all alerts with triggered state badges.
  - Visual distinction: Triggered alerts have yellow background (`#fff3cd`), red border, and red badge (`#c41e3a`).

- **Create Alert Form**: Modal form with:
  - Dropdown for currency pair selection.
  - Number input for threshold (0.0001 step precision).
  - Dropdown for direction ("above"/"below").
  - Create and Cancel buttons.

- **API Integration**:
  - `loadAlerts()`: Fetches alerts on mount and after create/delete.
  - `createAlert()`: POST to `/api/alerts` with validation.
  - `deleteAlert(id)`: DELETE to `/api/alerts/{id}`.
  - Error handling with user-friendly alerts.

#### Styling
- Consistent with existing design system (dark blue `#16345c`, light grays).
- Alert cards use intuitive visual hierarchy and spacing.
- Triggered alerts prominently featured with warning colors.
- No CSS frameworks; vanilla styles for simplicity.

---

## Testing

### Unit Tests (18 passing)
- **AlertEvaluatorTests.cs** (9 tests):
  - Theory tests for "above" and "below" directions.
  - Boundary condition tests (equality edge cases).
  - Invalid direction handling.

- **InMemoryAlertServiceTests.cs** (9 tests):
  - CRUD operations (create, list, delete).
  - Input validation (empty pair, invalid direction, negative threshold).
  - Thread safeness: concurrent alert creation.

### Test Quality
- Uses Moq for dependency injection mocking.
- Comprehensive coverage of alert evaluation logic (critical business concern).
- Thread-safety test validates concurrent operations.

### Known Test Issues
- **RatesControllerTests** (pre-existing): 3 failures due to async/await assertion issues. Outside scope of this task; would require refactoring the test setup (WebApplicationFactory, etc.).

---

## Rough Edges & Honest Trade-offs

### Completed
✅ Alert CRUD operations  
✅ Live rate evaluation  
✅ Triggered state in UI  
✅ Comprehensive unit tests  
✅ Service abstraction & DI  
✅ Thread-safe in-memory storage  
✅ Frontend integration  

### Deliberately Left / Known Limitations
1. **No Persistence**: Alerts lost on restart. Would add EF Core DbContext + migrations in production.
2. **No Caching**: Rates fetched fresh on every `GET /api/alerts`. Could add TTL cache in `InMemoryAlertService` for performance.
3. **No Polling/WebSockets**: Alerts only evaluated when user calls `GET`. A production system would use background jobs (Quartz) or WebSockets for real-time notifications.
4. **Rate Fetch Errors**: Conservatively marks alerts as "not triggered" if rate fetch fails. Could log or surface errors better.
5. **Limited Pair Support**: Currently hardcoded to USD/CAD, GBP/USD, EUR/USD. Could make dynamic from `IRatesProvider` metadata.
6. **No Authentication/Authorization**: No user context. Would add user_id foreign key to alerts if multi-tenant.
7. **UI Styling**: Kept minimal (no animations, transitions). Production would use Tailwind or component library.

---

## What I Would Do Next (with more time)

### Priority 1 (Day 1)
- [ ] Add EF Core with PostgreSQL persistence layer. Swap `InMemoryAlertService` for `EfAlertService`.
- [ ] Add xUnit integration tests for `AlertsController` using `WebApplicationFactory`.
- [ ] Add logging (Serilog) for rate fetch errors and alert evaluations.

### Priority 2 (Day 2)
- [ ] Implement background job (Quartz.NET) that evaluates alerts on a timer and sends email/Slack notifications.
- [ ] Add API rate limiting to prevent abuse.
- [ ] Add user authentication (Auth0 or ASP.NET Core Identity) and multi-tenant support.

### Priority 3 (Day 3+)
- [ ] WebSocket support for real-time alert state updates to frontend.
- [ ] Alert history/audit log (who created/deleted, when triggered).
- [ ] Advanced features: conditional alerts (e.g., "only notify if triggered after hours"), daily digests.
- [ ] Frontend: animations, dark mode, responsive mobile design.

---

## AI Tool Usage

### What I Kept
- **Architecture patterns**: Typed clients, dependency injection, service abstractions. All industry standard and proven.
- **Test structure**: xUnit theory tests, Moq setup. Clear, maintainable.
- **API design**: RESTful resource model (POST/GET/DELETE). Clear and predictable.

### What I Rejected / Modified
- Initial suggestion to use dumb records for alerts; upgraded to proper classes with validation.
- Overly complex rate caching strategies; kept simple (no cache, fetch fresh).
- Complex async/await patterns in tests; kept synchronous assertions.

### Overall
Copilot accelerated boilerplate (namespaces, scaffold structure) and reduced typos. The architecture and business logic remain manually thought-through. The AI served as a productivity tool, not a design tool.

---

## How to Run

### Backend
```bash
cd backend
dotnet build
dotnet run
```
Runs on `https://localhost:5001` (or check launchSettings.json)

### Tests
```bash
dotnet test RateAlerts.Api.Tests
```
Expected: **18 passing**, 3 pre-existing failures in RatesControllerTests.

### Frontend
```bash
cd frontend
npm install
npm run dev
```
Runs on `http://localhost:5173` by default.

Create an alert, refresh rates, watch the triggered state update.

---

## Key Files
- `backend/Services/IAlertService.cs`, `InMemoryAlertService.cs` — Alert CRUD
- `backend/Services/AlertEvaluator.cs` — Trigger logic
- `backend/Controllers/AlertsController.cs` — API endpoints
- `backend/Models/Alert.cs`, `AlertDto.cs`, `CreateAlertRequest.cs` — Data models
- `RateAlerts.Api.Tests/AlertEvaluatorTests.cs`, `InMemoryAlertServiceTests.cs` — Unit tests
- `frontend/src/state.ts` — React-like shared state
- `frontend/src/App.vue` — Alert UI integration

---

## Commits Made
1. **HTTP Client Factory Refactoring**: Replaced 3 inline HttpClients with typed AddHttpClient + IHttpClientFactory.
2. **Abstraction Refactor**: Introduced IRatesProvider and XeRatesProvider for clean separation.
3. **Alert Feature Backend**: Models, services, controller, DI registration.
4. **Alert Tests**: Comprehensive unit tests for AlertEvaluator and InMemoryAlertService.
5. **Alert Frontend**: State + UI integration for managing and displaying alerts.

---

**End Notes**: This was a solid 2.5-hour sprint. The feature is production-ready minus persistence and background jobs. The code is testable, maintainable, and follows clean architecture principles. Looking forward to the follow-up conversation! 🚀

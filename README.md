# MiniShop API

A small ASP.NET Core 9 Web API built as a **learning project** — one concept at a time,
typed rather than copied, with each decision made deliberately and written down.

It's a deliberately tiny e-commerce domain (products, orders, users) used as a vehicle for
the parts of .NET that actually come up in production: the DI container, the request
pipeline, EF Core's change tracker, the repository/unit-of-work argument, and claims-based
authorization.

The domain is the excuse. **The concepts are the point.**

---

## Stack

| | |
| --- | --- |
| Runtime | .NET 9 / C# 13 |
| Web | ASP.NET Core Web API (controllers) |
| Data | EF Core 9, SQL Server LocalDB, code-first migrations |
| Auth | JWT bearer validation, role policies (`dotnet user-jwts` for dev tokens) |
| Docs | Swashbuckle / Swagger UI with a bearer scheme |

---

## Architecture

```
HTTP request
  └─ ExceptionHandlingMiddleware      → maps domain exceptions to ProblemDetails
      └─ Routing → CORS → Authentication → Authorization
          └─ Controller                → HTTP only: bind, call, status code
              └─ Service               → business rules, DTO mapping
                  └─ IUnitOfWork       → owns the request's DbContext, one commit
                      └─ Repository    → query composition, loading strategy
                          └─ DbContext → change tracking, LINQ → T-SQL
                              └─ SQL Server
```

One HTTP request → one DI scope → one `DbContext` → one `UnitOfWork` → three repositories
sharing it → one change tracker → **one `SaveChangesAsync` = one transaction.**

```
MiniShop.Api/
├─ Controllers/        ProductsController, OrdersController, MeController, LifetimeDemoController
├─ Services/           IProductService/ProductService, IOrderService/OrderService,
│                      IClock/SystemClock, ICurrentUser/CurrentUser, Exceptions.cs
├─ Repositories/       IProductRepository, IOrderRepository, IUserRepository,
│                      IUnitOfWork/UnitOfWork
├─ Data/               MiniShopDbContext, Migrations/
├─ Entities/           Product, User, Order, OrderItem, OrderStatus
├─ Dtos/               ProductDto, CreateProductRequest, OrderDto, CreateOrderRequest, PagedResult
├─ Middleware/         ExceptionHandlingMiddleware
└─ Demo/               SingletonOp, ScopedOp, TransientOp — DI lifetime experiment
```

---

## Concept map

Every concept below is implemented somewhere in this repo. This table is the actual index
of the project.

### Hosting, configuration and DI

| Concept | Where | The takeaway |
| --- | --- | --- |
| The three startup phases | `Program.cs` | `CreateBuilder` → register services → `Build()` freezes them → pipeline → `Run()`. Startup code runs **once**; the pipeline runs per request. |
| Configuration layering | `appsettings*.json`, user secrets | Later sources win: base file → `{Environment}` file → user secrets → env vars → CLI. `__` in an env var maps to `:` in a key. Secrets never live in a committed file. |
| Service lifetimes | `Demo/`, `GET /api/demo/lifetimes` | Singleton = one per app; Scoped = one per request; Transient = one per injection. Proven with GUIDs injected twice per request. |
| Captive dependency | same endpoint | A Singleton holding a Scoped service resolves it once from the **root scope** and keeps it forever. Development throws at `Build()` (`ValidateScopes` + `ValidateOnBuild`); **in Production both checks are off by default.** |
| Interface → implementation swap | `ICurrentUser` / `CurrentUser` | Registered as a hard-coded stub on day one, filled in with real claim reading later. Same class, same registration, no consumer changed. |
| Testability over statics | `IClock` / `SystemClock` | `DateTime.UtcNow` is a hidden static dependency. Injecting a clock is what makes time fakeable. (.NET 8+ ships `TimeProvider` for this.) |

### EF Core

| Concept | Where | The takeaway |
| --- | --- | --- |
| Code-first modelling | `Entities/`, `OnModelCreating` | Precedence: conventions < data annotations < fluent API. Fluent keeps entities clean and wins on conflict. |
| Navigation properties | `Order`, `OrderItem` | FK property + reference navigation + collection navigation = one relationship. Declare the FK explicitly or EF creates a shadow property. |
| Fluent configuration | `MiniShopDbContext` | `HasPrecision(18,2)` on money; `HasMaxLength` **required** before a unique index (`nvarchar(max)` can't be an index key); `HasConversion<string>()` for the status enum. |
| Delete behaviour | all relationships | EF defaults required FKs to **Cascade** — deleting a user would delete their order history. Set `Restrict`. SQL Server also rejects multiple cascade paths to one table. |
| Migrations as diffs | `Data/Migrations/` | A migration is the delta between the current model and `ModelSnapshot`. `__EFMigrationsHistory` lives in the database, so re-running is a no-op. |
| Additive migrations | `AddProductDescription`, `AddUserExternalId` | Add nullable → deploy → backfill → drop later. `Down()` can recreate a column but never its data. |
| Filtered unique index | `Users.ExternalId` | SQL Server treats multiple `NULL`s as duplicates in a plain unique index — `HasFilter("[ExternalId] IS NOT NULL")` fixes it. |
| Seeding | `HasData` | Explicit PKs required (EF needs a stable identity to diff), and no dynamic values or every model build produces a new migration. |
| Deferred execution | proven with SQL logging | `IQueryable` = expression tree + provider. Building a query logs nothing; the SQL appears only at the terminal operator. |
| `IQueryable` vs `IEnumerable` | repositories | Before execution, `Where` becomes SQL; after it, `Where` filters in memory over every row already fetched. `ToList()` mid-chain is the accidental version. |
| Change tracking | `ProductService.UpdateAsync` | Load → mutate → `SaveChangesAsync`, with **no `Update()` call**. EF diffs against the original snapshot and writes only changed columns. |
| The `SaveChanges` sequence | `UnitOfWork` | DetectChanges → order by FK dependency → implicit transaction → batched commands → read back generated values → commit. |
| Avoiding N+1 | `IProductRepository.GetByIdsAsync` | In EF Core, N+1 comes from **your loop**, not lazy loading (which is off). Ordered products load in one `WHERE Id IN (...)`. |
| Tracking vs projection | GET endpoints vs writes | `AsNoTracking` + `Select` into a DTO for reads; tracked entities for writes. A DTO projection is untracked anyway. |

### The repository / unit-of-work layer

| Concept | Where | The takeaway |
| --- | --- | --- |
| `DbContext` **is** a unit of work | `UnitOfWork` | The change tracker plus `SaveChanges` already is the pattern; `DbSet<T>` already is a repository. Anything you add is a layer **on top**. |
| One shared context | `UnitOfWork` constructor | All three repositories receive the **same** `DbContext`. That's what makes one `SaveChangesAsync` atomic across them. |
| Per-entity repositories | `Repositories/` | Chosen over `IRepository<T>`: a generic repo re-implements `DbSet` and pushes composition back to callers via an `Expression` parameter or an `IQueryable` return — which leaks EF anyway. Cost: more code, no reuse. |
| Ownership and disposal | `UnitOfWork` | It does **not** implement `IDisposable`. DI owns the `DbContext` and disposes it at scope end; disposing an injected dependency is a classic bug. |
| The deliberate leak | `Query()` | Returning `IQueryable` keeps composition open for OData-style querying — and is exactly what makes the abstraction partly decorative. Kept knowingly. |
| One transaction boundary | `OrderService.PlaceOrderAsync` | Validate everything → build the order → decrement stock → **one** `SaveChangesAsync`. A failure before that line has mutated only in-memory objects. |

### The request pipeline

| Concept | Where | The takeaway |
| --- | --- | --- |
| Middleware as nested delegates | `Program.cs` | Each middleware acts, calls `next`, then regains control on the way out. That two-phase shape is why the exception handler must be outermost. |
| Ordering | pipeline registration | `UseAuthentication` **before** `UseAuthorization` (the first builds `HttpContext.User`, the second reads it); both after `UseRouting`, because authorization needs the matched endpoint's metadata. |
| Custom middleware | `ExceptionHandlingMiddleware` | Constructed **once** at startup, so per-request services go in `InvokeAsync` parameters, not the constructor — the middleware form of the captive-dependency rule. |
| Error contract | `ProblemDetails` (RFC 9457) | `NotFoundException` → 404, `ValidationException` → 400, everything else → 500 with a generic message (never leak `ex.Message`; log it server-side). Same shape `[ApiController]` already returns for model validation. |
| Middleware vs filters | — | Middleware sees every request and knows only `HttpContext`; filters run inside MVC and know the action, its arguments and model state. |

### API design

| Concept | Where | The takeaway |
| --- | --- | --- |
| DTO boundary | `Dtos/` | Prevents over-posting, serialisation cycles, and coupling the API contract to the schema. `CreateProductRequest` has **no `Id`** — the omission is the defence. |
| Automatic validation | `[ApiController]` | DataAnnotations failures return 400 + `ProblemDetails` **before** the action runs. |
| REST semantics | controllers | 201 + `Location` via `CreatedAtAction`, 204 for PUT/DELETE, 404 for a missing resource. |
| Paging guard | `ProductService.GetPagedAsync` | `pageSize` is clamped server-side — the plain-endpoint equivalent of OData's `SetMaxTop`. `Skip` requires an `OrderBy`, because SQL Server's `OFFSET` needs a deterministic order. |

### Authentication and authorization

| Concept | Where | The takeaway |
| --- | --- | --- |
| Authn vs authz | `Program.cs` | Authentication proves who you are (the identity provider's job); authorization decides what you may do (the API's job, on every request). |
| Token validation | `AddJwtBearer()` | Configuration, not code: signature, `iss`, `aud`, `exp`. Locally it binds the `Authentication:Schemes:Bearer` section `dotnet user-jwts` wrote; against Entra you'd set `Authority` + `Audience` (or use `Microsoft.Identity.Web`) and keys come from the discovery document. **The enforcement code is identical.** |
| Policies over scattered roles | `CanManageCatalog` | One named, composable rule defined once, instead of `[Authorize(Roles="Admin")]` repeated across attributes. |
| Claims → local user | `OrderService.ResolveCallerIdAsync` | The stable id claim (`sub` here, `oid` in Entra) maps to `Users.ExternalId` → local `Users.Id`, created on first authenticated call (just-in-time provisioning). Never key data on the provider's id or on email. |
| Identity never from the body | `CreateOrderRequest` | It carries no `UserId`. The caller comes from the token, or anyone can order as anyone. |
| 401 vs 403 | verified end to end | 401 = I don't know who you are. 403 = I know, and you may not. |

---

## Endpoints

| Method | Route | Auth |
| --- | --- | --- |
| `GET` | `/api/products?search=&page=&pageSize=` | anonymous |
| `GET` | `/api/products/{id}` | anonymous |
| `POST` `PUT` `DELETE` | `/api/products` | policy `CanManageCatalog` (role `Admin`) |
| `POST` | `/api/orders` | any authenticated user |
| `GET` | `/api/orders/mine` | any authenticated user |
| `GET` | `/api/me` | any authenticated user — returns the caller's claims |
| `GET` | `/api/demo/lifetimes` | anonymous — the DI lifetime experiment |

---

## Running it

```bash
# 1. Database (SQL Server LocalDB)
dotnet tool install --global dotnet-ef --version 9.*
dotnet ef database update            # creates MiniShop + applies 4 migrations + seeds

# 2. Dev tokens — real signed JWTs, local signing key
dotnet user-jwts create --name alice --role Admin
dotnet user-jwts create --name bob   --role Customer

# 3. Run, then open /swagger and paste a token into Authorize
dotnet run
```

Seeded users are `alice` (Admin) and `bob` (Customer), matching the `ExternalId` values in
the seed data. Try `POST /api/products` as each to see 201 vs 403.

---

## Known limitations — deliberate, not overlooked

- **`PlaceOrderAsync` oversells under concurrency.** Two simultaneous orders for the last
  unit both read `Stock = 1` and both write `Stock = 0`; the reads happened outside the
  transaction, so `SaveChanges` doesn't prevent it. The fix I'd add first is a `RowVersion`
  concurrency token (fails loudly, one column and one config line), then an atomic guarded
  update (`WHERE Id = @id AND Stock >= @q`) and a `CHECK (Stock >= 0)` backstop.
- **No OData.** The products list is a plain paged endpoint instead. The OData equivalent
  would take `ODataQueryOptions<ProductDto>` and call `options.ApplyTo(...)` in the service
  layer, so the service can add its own `Where` before the client's options run.
- **Entities are anaemic** (public setters, rules in the service layer) rather than
  encapsulated with private setters and behaviour — a trade-off for EF materialisation and
  a smaller surface.
- **No tests.** The decoupling is there for them (`ProductService` takes `IUnitOfWork`, so
  a fake with in-memory lists replaces the database), but they aren't written.
- **No caching, no rate limiting, no API versioning.** Out of scope.

---

## What I'd change if this were production

Integration tests with `WebApplicationFactory` against a real SQL Server (the EF in-memory
provider enforces no FKs and translates differently); migrations applied from a generated
idempotent script in a pipeline stage rather than at startup; `IEntityTypeConfiguration<T>`
per entity instead of one growing `OnModelCreating`; structured logging with correlation
ids; and the concurrency fix above.

---
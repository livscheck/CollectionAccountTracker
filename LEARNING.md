# What I Learned

Notes from building this project, coming from ASP.NET Core MVC.

## What carried over from MVC

- **Controllers work the same.** `[ApiController]`, `[Route]`, `[HttpGet]`/`[HttpPost]`, returning `NotFound()` or `BadRequest()`, and getting services through the constructor are all things I already knew. The API in this project is plain ASP.NET Core.
- **Dependency injection.** Registering services in `Program.cs` (`AddDbContext`, `AddScoped`) and receiving them in a constructor, or with `@inject` in a page, is the same idea as in MVC.
- **Data annotations for validation.** `[Required]` and `[Range]` work in both. Here the same `CreatePaymentDTO` class is checked by the Blazor form and by the API, so both enforce the same rules.
- **EF Core and LINQ.** `DbContext`, `DbSet`, `Where`, `Select` and `ToListAsync` were all familiar.

## What was new

### Blazor vs ASP.NET Core MVC
Blazor isn't separate from ASP.NET Core; it's one part of it. This app runs both in the same process: **MVC-style controllers** for the API (`MapControllers()`) and **Blazor components** for the pages (`MapRazorComponents<App>()`).

**The big difference:** in MVC, every click is a new request. The server builds a whole page, sends it, and forgets you. In Blazor (Interactive Server), the page is a **component that stays alive** on the server. Clicks travel over a live SignalR connection, Blazor re-runs only what changed, and sends back just the parts of the page that are different. No full page reloads.

| | ASP.NET Core MVC | Blazor (Interactive Server) |
|---|---|---|
| **The UI** | A controller action loads data and returns `View(model)` | A component (`.razor`) loads its own data and renders itself |
| **Routing** | `[Route]` on controllers, or conventions like `/Accounts/Details/5` | `@page "/accounts/{AccountId:int}"` at the top of the component |
| **State** | Gone after each request; kept in the database, session or hidden fields | Kept in the component's fields (`search`, `account`) between clicks |
| **User actions** | A link or a form `POST` → a new request → a new page | `@onclick="..."` calls a C# method directly → only the changed HTML updates |
| **Getting input** | **Model binding**: request data (form fields, query string) → action parameters | **Data binding**: `@bind` keeps an input and a C# field in sync as you type |
| **Forms** | `<form asp-action>` with `asp-for` tag helpers; the action checks `ModelState.IsValid` | `<EditForm Model="...">` with `InputNumber`, `InputSelect` and `OnValidSubmit`; the form is checked before it's submitted |
| **Reusable pieces** | Partial views and view components | Components with `[Parameter]`s, e.g. `StatCard`, `StatusBadge`, `MonthPicker` |
| **Child → parent** | Not really possible; the page posts back | `EventCallback`, e.g. `MonthPicker` telling the page the month changed |
| **Services** | Constructor injection in the controller | `@inject AccountAPIClient Api` in the component |
| **Layout** | `_Layout.cshtml` with `@RenderBody()` | `MainLayout.razor` with `@Body` |
| **CSS** | Mostly global stylesheets | Each component can have its own `.razor.css` that only affects that component |
| **Page lifecycle** | Constructor → action → view | `OnInitializedAsync`, `OnParametersSetAsync`, `OnAfterRenderAsync` |

#### Binding
**Model binding (MVC)** happens once per request: ASP.NET reads the form or URL and fills in the action's parameters. My API controllers still use it, e.g. `GetAll(string? search, string? status)` gets its values from `?search=...&status=...`.

**Data binding (Blazor)** is two-way and continuous: the input shows the field's value, and typing updates the field.

- **`@bind`** connects an input to a field. By default the field updates when you leave the input (`onchange`):
  ```razor
  <select @bind="statusCode">
  ```
- **`@bind:event="oninput"`** updates on every keystroke instead, used for the search box so results update as you type:
  ```razor
  <input @bind="search" @bind:event="oninput" />
  ```
- **`@bind:after`** runs a method after the value changes. That's how the Accounts page reloads from the API whenever the search or status changes:
  ```razor
  <input @bind="search" @bind:event="oninput" @bind:after="LoadAccountsAsync" />
  ```
- **`@bind-Value`** is the version for Blazor's form inputs inside an `EditForm`, so validation knows which field changed:
  ```razor
  <InputNumber @bind-Value="paymentForm.Amount" />
  ```
- **Binding to my own component:** `@bind-Month` works on `MonthPicker` because it has a `Month` parameter **and** a `MonthChanged` `EventCallback`. Blazor connects the two by name.
  ```razor
  <MonthPicker @bind-Month="month" @bind-Month:after="LoadReportAsync" />
  ```

#### Other differences I ran into
- **Parameters** pass data into a component, e.g. `<StatCard StatTitle="Total paid" StatBalance="..." />`. They flow one way, from parent to child. A child tells its parent something changed through an `EventCallback`.
- **Lifecycle methods:** `OnInitializedAsync` runs once when a page opens. `OnParametersSetAsync` runs again if the URL changes, e.g. going from `/accounts/1` to `/accounts/2` without leaving the page.
- **Render modes:** a page with no `@rendermode` is plain server-rendered HTML, like MVC: the About page doesn't need clicks, so it has none. `@rendermode InteractiveServer` makes a page live. Each page chooses.
- **Validation runs before submit:** `DataAnnotationsValidator` and `ValidationSummary` check the form in the browser session before anything is sent. MVC usually checks `ModelState` after the post. The API still checks again, because the server is the source of truth.
- **Re-rendering is automatic after events:** after an `@onclick` or a bound change, Blazor re-renders the component by itself. I only call `StateHasChanged()` when something changes outside a UI event.

### Prerendering
By default Blazor renders a page once on the server (so something appears quickly), then again when the live connection starts. That means `OnInitializedAsync` runs **twice** and the API gets called twice. I turned prerendering off with `InteractiveServerRenderMode(prerender: false)`: the first paint is slightly slower, but data loading is simpler.

### Calling my own API from the pages
In Blazor Server, the page already runs on the server, so it could use the `DbContext` directly. I went through an API anyway, because:
- the UI only deals with DTOs, not database details
- other systems (imports, accounting exports) could use the same API
- the UI could move to WebAssembly later, which runs in the browser and can't reach a database

### RESTful API design
"REST API" and "RESTful API" mean the same thing: an API that follows the REST style. ("RESTful" is just the adjective.) The rules, and how my API follows them:

| Rule | In this project |
|---|---|
| URLs name things (nouns), not actions | `/api/accounts`, `/api/accounts/5`, `/api/remittance`, not `/getAccounts` |
| The HTTP method says what to do | `GET` reads; `POST /accounts/5/payments` creates a payment |
| Nested URLs show relationships | `/accounts/5/payments`, because a payment always belongs to one account |
| Status codes say what happened | 200 OK, 201 Created, 400 bad input, 404 not found |
| Stateless | Every request carries its own IDs, filters and body; the server doesn't remember earlier requests |
| Query strings filter, never change data | `?search=smith&status=SUIT`, `?year=2026&month=11` |

**Where it bends the rules:** `/api/accounts/dashboard` is a summary rather than a real "thing", which is closer to calling a function. I kept it on purpose, so the stat cards need one request instead of several. `/api/remittance` is a report rather than a stored record, but reports are commonly treated as read-only resources.

### IQueryable vs IEnumerable
- **`IQueryable`**: EF turns the whole LINQ chain into **one SQL statement**, so the database does the filtering, sorting and counting. Nothing runs until `ToListAsync()`.
- **`IEnumerable`**: plain C# working on data already in memory.

I build queries as `IQueryable` and switch to in-memory work only after `ToListAsync()`, e.g. to build DTOs. If I'd used `IEnumerable` from the start, every account would load first, and related data like the debtor's name wouldn't be loaded at all.

**The catch:** EF can only translate things it understands. I wrote a helper `Balance(a)` and used it in a query. It compiled, but `OrderBy` and `Sum` failed at runtime because EF can't turn a C# method into SQL. The fix was a **computed column** that SQL Server calculates itself, so `a.Balance` works everywhere.

### Avoiding N+1 queries
An N+1 happens when you run one query for a list, then another query for each row. Using `Select` projections (instead of loading whole objects and looping over them) keeps every endpoint to a fixed number of queries. Counts and "last payment date" become subqueries inside the same SQL statement. It can also happen over HTTP: a grid row should never call the API for its own data, so the page loads everything once and passes it down.

### DTOs, and records vs classes
- **DTOs** are the shapes the API sends. They hold only what the page needs, not whole database rows.
- **Use a `record`** for data that's filled in once and only read, like a receipt. All the response DTOs are records.
- **Use a `class`** for data that gets filled in or changed over time, like a form. `CreatePaymentDTO` is a class because the form fills it in as the user types.

### Code-first database
The C# classes are the source of truth for the tables, so there are no hand-written `CREATE TABLE` scripts to keep in sync.

**`EnsureCreated()`** runs once at startup in `Program.cs`. If the database doesn't exist, it creates it and every table from my model (the `DbSet`s, the classes, and `OnModelCreating`). If the database already exists, it does nothing.

**The catch:** it never updates existing tables. If I add a property to `Account`, the new column won't appear, and the app fails with "Invalid column name". The fix in a demo is to delete the database (`docker compose down -v`) so it's rebuilt on the next run.

| | `EnsureCreated` | Migrations |
|---|---|---|
| Creates tables | Yes, once | Yes |
| Updates tables when models change | No; delete and recreate | Yes, step by step, **keeping the data** |
| Setup | One line | The `dotnet ef` tool, plus a migration file for each change |
| Good for | Demos, prototypes, tests | Real apps with data you can't throw away |

> `EnsureCreated` builds the schema from my model on first run. It's ideal for a demo, but it can't evolve an existing schema, so a production app would use EF Core migrations to apply changes without losing data.

### Money and business rules
- Use `decimal` for money, and pick a rounding rule on purpose. `Math.Round` rounds half-cents to the nearest even number by default, so I used `MidpointRounding.AwayFromZero` instead.
- **Store** the fee on each payment instead of recalculating it, so changing a rate never rewrites past reports.
- Filter a month as `start <= date < first of next month`. An "end of the month" date would miss payments made late on the last day.

### Environment setup
- SQL Server in Docker only reads its password the **first time** its data volume is created, and it rejects weak passwords outright: the container just stops. Changing `.env` afterwards does nothing until you remove the volume.
- Secrets go in `.env` (git-ignored) and `dotnet user-secrets`, never in committed files.
- A NuGet package's major version has to match the target framework. The newest EF Core package needed .NET 11, so I pinned it to 10.x.

# Collection Account Tracker

A small Blazor app that models one real stage of a debt-collection law firm's work: **posting payments and paying creditors what they're owed.**

## The real-world process

1. A **creditor** (a hospital, a credit union, a landlord) hands an unpaid debt to the firm. That's an **account**.
2. The **debtor** makes payments to the firm.
3. The firm keeps a **contingency fee**, an agreed percentage of every payment, and owes the rest to the creditor.
4. Once a month the firm sends each creditor a **remittance**: what was collected on their accounts, minus the firm's fee.

**Example:** a debtor pays $150 on an account with a 33% contingency fee. The firm keeps **$49.50** (the firm fee) and owes the creditor **$100.50**.

## What you can do

- **Accounts:** see every account with its balance and status. Search by debtor or creditor, filter by status, and see totals per status.
- **Account detail:** see one account's history and post a payment. The firm fee is shown live as you type. Overpaying is rejected, and paying off the exact balance marks the account **Paid**.
- **Remittance:** pick any month and see, for each creditor, the number of payments, the amount collected, the firm's fee and the net owed.

## How it's built

```
Blazor page  →  AccountAPIClient  →  API controllers  →  EF Core + LINQ  →  SQL Server (Docker)
 (the UI)        (HttpClient)         (/api/...)          (queries)           (the data)
```

- **Blazor (Interactive Server)** for the pages. Clicks are handled on the server over a live connection, with no full page reloads.
- **ASP.NET Core Web API** controllers return JSON. The pages never touch the database directly; they go through the API.
- **EF Core** (code-first): the C# classes in `Models/` define the tables, and the database is created automatically on first run.
- **SQL Server 2022** runs in a Docker container.

### Tables

| Table | Holds |
|---|---|
| `Debtors` | The people or businesses who owe money |
| `Creditors` | The clients the money is owed to |
| `Accounts` | One debt: who owes whom, how much, its status and the fee percentage |
| `Payments` | Each payment on an account, with the firm fee worked out when it was posted |
| `Notes` | Notes on an account |

One account has many payments and many notes. Each account belongs to one debtor and one creditor.

### API endpoints

| Method | URL | What it does |
|---|---|---|
| GET | `/api/accounts?search=&status=` | List accounts, optionally searched and filtered |
| GET | `/api/accounts/{id}` | One account with its payments |
| GET | `/api/accounts/dashboard` | Count and balance per status, plus collected this month |
| POST | `/api/accounts/{id}/payments` | Post a payment (`{ "amount": 100.00, "method": "Check" }`) |
| GET | `/api/remittance?year=&month=` | The remittance report for a month |

Every endpoint has a ready-made request in [`CollectionAccountTracker.http`](CollectionAccountTracker/CollectionAccountTracker.http), including the error cases.

## Demoing it

### 1. Before the demo (2 minutes)

1. **Open Docker Desktop** and wait until it says it's running.
2. **Start SQL Server.** From the repo folder:
   ```bash
   docker compose up -d
   ```
   Wait about 15 seconds for it to start.
3. **Start the app:**
   ```bash
   cd CollectionAccountTracker
   dotnet run
   ```
   On the very first run it creates the tables and loads 25 fake accounts. All names are made up.
4. **Open http://localhost:5156** in one browser tab and keep it open.
5. **Quick check:** the Accounts page shows the five stat cards and a list of accounts.

**Don't press F5 while `dotnet run` is running.** Two copies can't share the port, and you'll get "address already in use".

### 2. The demo (about 3 minutes)

| Step | Do | Say |
|---|---|---|
| **1. The problem** | Open **Remittance** (this month). | "When a debtor pays, the firm keeps a contingency fee and owes the rest to the creditor. Each month it sends every creditor this remittance: collected, firm fee, net owed." |
| **2. Find an account** | Open **Accounts**. Type a name in **Search**, then pick a **Status**. Click a debtor's name. | "Filtering happens in SQL through the API. Only the matching rows come back." |
| **3. Server-side rules** | In **Post a payment**, enter more than the remaining balance, pick a method and click **Post payment**. | "The form checks the basics, but the API enforces business rules. That red message came from the server." |
| **4. Post a payment** | Enter **100**, pick **Check** and post. Point at the fee preview and the formula box while typing. | "The fee is calculated on the server and **stored** on the payment, so a later rate change never rewrites past remittances." |
| **5. See it flow through** | Open **Remittance** again. | "That creditor's collected, fee and net all went up." |
| **6. Paying off** | Back on **Accounts**, click the **Balance** header to sort smallest first and open that account. Enter its exact remaining balance and post. | "The status flips to **Paid** automatically, and the form is replaced because there's nothing left to collect." |

**If there's time:** open **About** to show this README inside the app, or open `CollectionAccountTracker.http` to show the API on its own, including the 400 and 404 cases.

### 3. After the demo
- **Stop the app:** `Ctrl+C` in its terminal.
- **Stop SQL Server:** `docker compose down`. The data is kept for next time.

### First-time setup (once per computer)

You'll need the [.NET 10 SDK](https://dotnet.microsoft.com/download) or newer, [Docker Desktop](https://www.docker.com/products/docker-desktop/), and VS Code with the C# Dev Kit, Rider or Visual Studio. On an Apple Silicon Mac, turn on **Docker Desktop → Settings → General → "Use Rosetta for x86_64/amd64 emulation"**.

1. **Set the database password.** From the repo folder:
   ```bash
   cp .env.example .env        # Windows: copy .env.example .env
   ```
   Open `.env` and replace `change_me` with a password of at least 8 characters using three of: uppercase, lowercase, numbers and symbols. SQL Server won't start with a weaker one. `.env` is git-ignored, so it's never committed.
2. **Tell the app how to connect,** using the same password:
   ```bash
   cd CollectionAccountTracker
   dotnet user-secrets set "ConnectionStrings:Default" "Server=localhost,1433;Database=CollectionAccountTracker;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True"
   ```
   On a Mac, use single quotes around the connection string if your password contains `!`.

**After changing a model:** run `docker compose down -v` and `docker compose up -d`. The tables are rebuilt on the next run.

## Project layout

```
CollectionAccountTracker/
├── Components/
│   ├── Pages/        Accounts, AccountDetail, Remittance
│   ├── Feature/      AccountsGrid, PaymentsGrid, RemittanceGrid
│   ├── Common/       StatCard, StatusBadge, MonthPicker
│   └── Layout/       Main layout and navigation
├── Controllers/      AccountsController, RemittanceController (the API)
├── Data/             AppDbContext, SeedData, FeeCalculator, MonthRange
├── Models/           Entity classes (the tables)
│   └── DTO/          Shapes the API sends and receives
├── Services/         AccountAPIClient (how pages call the API)
├── Scripts/          SQL for checking and resetting data by hand
└── CollectionAccountTracker.http   Test requests for every endpoint
```

## Design decisions

- **The firm fee is stored on each payment, not recalculated.** Remittances are financial records. If a creditor's rate changes later, last month's report must not change.
- **Balance is a computed column** (`InitialBalance - TotalPaid`, calculated by SQL Server). It's defined once, so every query, page and report agrees.
- **Money uses `decimal`,** never `double`, with half-cents rounded up in one place (`FeeCalculator`).
- **The server enforces the rules.** The form checks the basics (an amount and a method). The API rejects overpayments, calculates the real fee and marks accounts Paid.
- **Queries run in SQL.** Filtering, grouping and totals happen in the database, and only the rows a page needs come back. Every endpoint uses a fixed number of queries, so there's no N+1.
- **Prerendering is turned off** on the pages, so each page loads its data once instead of twice.

## What I'd add next

1. Change an account's status from the app (a `PUT` endpoint and a dropdown)
2. Mark a month's remittance as "sent" and lock those payments
3. Payment reversals for bounced checks, which restore the balance and adjust the next remittance
4. Export the remittance report to CSV
5. Sign-in, roles and an audit log
6. EF Core migrations instead of `EnsureCreated`, so the schema can change without resetting the data

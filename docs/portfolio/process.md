<!-- PROCESS: how it works and why you built it this way. 3–6 short paragraphs.
     - Overview: the main pieces and how they connect (link media/architecture.png)
     - Key decisions: what you chose, what you rejected, and WHY
     - Mobile/web: client ↔ API ↔ data flow, auth, offline, deployment
     - AI: model choice, prompting/RAG/agent design, how you measured quality
     - Embedded/IoT: board choice, sensors, power budget, device ↔ cloud ↔ app flow -->

## How it works

The app has three screens. **Accounts** lists every debt with search, a status filter and clickable summary cards. **Account detail** shows one account's history and lets you post a payment, with the firm's fee shown live as you type. **Remittance** shows, for any month, what was collected for each creditor, the firm's fee and the net owed.

Every screen follows the same path: a Blazor page calls a small API client, which calls my REST API, which uses EF Core and LINQ to query SQL Server running in Docker. The pages never touch the database directly. The app also includes a **How It Works** page that walks through each action (search, add a payment, the remittance report) and shows the real Blazor, REST and LINQ code behind it, pulled live from the source files.

## Key decisions

- **An API between the pages and the database.** In Blazor Server the pages already run on the server, so they could use the database directly. I went through an API anyway, so the UI only deals with simple data objects, other systems could reuse the same endpoints, and the UI could move to WebAssembly later.
- **The fee is stored on each payment, not recalculated.** A remittance is a financial record. If a creditor's rate changes next year, last month's report must not change.
- **The server enforces the rules.** The form checks the basics, but the API is what rejects an overpayment, calculates the real fee and marks an account Paid, all saved together in one database transaction.
- **The database does the heavy lifting.** Filtering, grouping by creditor and totals all run as SQL, so only the rows a page needs come back. Every endpoint uses a fixed number of queries, however much data there is.
- **Money is handled carefully:** `decimal` instead of floating point, an explicit rounding rule for half-cents, and the fee math in one shared place used by the API, the seed data and the on-screen preview.
- **Bootstrap instead of custom CSS.** Late in the project I replaced most of my own styles with Bootstrap's utility classes, which cut the custom CSS by more than half and made every screen consistent.

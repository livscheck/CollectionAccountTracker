<!-- CHALLENGES: 1–3 real problems. This is where you show how you think.
     Use the same shape for each one. Failed attempts count. -->

## A helper method that compiled but broke the database queries

**Problem:** "Balance" (amount owed minus amount paid) appeared in five places in my API, so I moved it into a small C# helper method. It compiled, but sorting accounts by balance and adding up balances per status both failed when they ran.

**What I tried:** I had EF Core print the SQL it would generate for each query. It could handle the helper when building results, but not when sorting or summing, because it can't translate a C# method into SQL. Writing the subtraction out by hand worked, but meant repeating the same formula everywhere.

**Fix:** I made Balance a computed column that SQL Server calculates itself. Now it's defined once and works in every query, sort and total, and I checked the generated SQL for each endpoint to confirm it. The lesson I took away: code inside a database query has to be something the database understands.

## The database container kept shutting down

**Problem:** SQL Server ran in Docker, but the container would start and then stop a few seconds later, so the app couldn't connect.

**What I tried:** Its logs showed the password I'd set was rejected as too weak, so SQL Server never finished starting. Fixing the password in my settings file didn't help at first, because SQL Server only reads that password the very first time it creates its storage, and the password in the app's connection settings didn't match it either.

**Fix:** I chose a password that met SQL Server's rules, started the container fresh so it picked up the new one, and made sure the app used the same password. I wrote the password rules into the README so the next person setting it up doesn't hit the same wall.

## The grid ignored my styling

**Problem:** I wanted more breathing room in the table rows, but my padding had no effect. The rows stayed cramped no matter what I set.

**What I tried:** QuickGrid, the table component I used, ships its own stylesheet, and its rule was slightly more specific than mine, so the browser chose its padding. Making my rule more specific worked, but it was a workaround I'd have to repeat for every grid.

**Fix:** I turned off QuickGrid's built-in theme so all the grids use Bootstrap's standard table styles instead, defined once and shared everywhere. This was part of moving the whole app onto Bootstrap's utility classes.

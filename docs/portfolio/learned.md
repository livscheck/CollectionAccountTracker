<!-- LEARNED / NEXT: 3–5 honest bullets. What surprised you, what you'd change in v2. -->

- **Blazor felt different from MVC in one big way:** a page is a component that stays alive and updates only what changed, instead of a new page on every click. Two-way data binding replaced most of what I used to do with form posts.
- **Code that compiles isn't always code the database can run.** LINQ queries are translated into SQL, so I learned to keep anything inside a query to things SQL understands and do the rest after the data comes back.
- **Some Blazor defaults have side effects.** By default a page renders twice when it first loads, which called my API twice; turning that off made data loading simpler.
- **In v2 I'd add** database migrations, so the schema can change without rebuilding the database, plus a way to change an account's status, payment reversals for bounced checks, and sign-in with user roles.
- **I'd also add automated tests** around the fee calculation and the overpayment rule, since those are the parts where a mistake costs real money.

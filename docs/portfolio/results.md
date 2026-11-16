<!-- RESULTS: bullets. Real numbers only (battery life, latency, users, cost, accuracy).
     Before/after if you have it. Link a demo or video. -->

- **Working app in 5 days** (11/12–11/16/2026): three main screens, a code walkthrough page and an in-app demo checklist.
- **5 REST endpoints across 2 controllers**, each tested with a ready-made request file that also covers the error cases (400 and 404).
- **No N+1 queries:** every endpoint runs a fixed number of database queries (one or two), which I confirmed by checking the SQL that EF Core generates.
- **Custom CSS cut from 878 lines to 393** by moving the styling to Bootstrap's utility classes.
- **Seeds 25 fake accounts** with payments in the current and previous month, so the demo always has realistic data.
- Code: [github.com/livscheck/CollectionAccountTracker](https://github.com/livscheck/CollectionAccountTracker)

# Milestone 6 Budgets and multi currency reporting

## Objective

Provide reproducible monthly budgets and core multi-currency financial calculations using stored historical valuations.

## Prerequisites

- Milestone 5 is completed.
- Read the budget, exchange-rate, dashboard, and reporting rules in the functional specification.

## Product decisions required before implementation

- Approve the guided base-currency change policy for reports spanning a change.
- Confirm whether MVP budget lines are leaf-only. The current recommendation is leaf-only to prevent overlap.

## In scope

- Calendar-month budget periods in base currency.
- One active budget line per eligible category and period.
- Rollover modes: none, positive-only, and full balance.
- Explicit handling of negative rollover.
- Configurable warning thresholds, defaulting to 80 and 100 percent.
- Budget actuals from posted expense splits only.
- Historical base-currency income, expense, transfer, savings-flow, and net-worth calculations.
- Dashboard totals and budget drill-down to contributing records.
- Manual and import-derived exchange-rate history management.
- Budgets UI and the calculation layer needed by later reports.

## Out of scope

- Current-market revaluation, online exchange-rate providers, forecasting, custom budget periods, envelope funding, and report export formats.

## Required design and behavior

- Never recompute historical reports from edited rate tables.
- Exclude drafts, voided records, transfers, savings contributions, income, opening balances, and balance adjustments from budget actuals.
- Purchases from savings accounts remain expenses.
- Prevent parent-child budget overlap; do not rely only on UI validation.
- Preserve and disclose rounding differences.
- Every displayed total must expose the contributing transaction set.
- Use bounded, indexed queries and verify totals against manually calculated datasets.

## Database work

- Add forward migrations for budget periods, budget lines, rollover state if stored, exchange-rate records, and base-currency configuration history.
- Add uniqueness and overlap protections supported by SQLite; validate remaining aggregate rules transactionally.

## Required tests

- Budget inclusion and exclusion matrix for every transaction kind and status.
- Thresholds, overspend, and all rollover modes.
- Parent-child overlap rejection.
- Historical valuation remains stable after rate edits.
- Multiple currency precisions and rounding differences.
- Base-currency change effective-date behavior if approved.
- Golden datasets for cash flow, budget actuals, savings flow, and net worth.
- SQLite integration tests for uniqueness and concurrent budget edits.

## Completion checks

- Restore, build, tests, and formatting pass.
- Golden datasets reconcile exactly at currency precision.
- Every summary can drill into its contributing records.
- No current-rate revaluation is presented as historical reporting.
- Update schema and developer documentation.
- Update [milestone status](milestone-status.md), record the commit, and stop.

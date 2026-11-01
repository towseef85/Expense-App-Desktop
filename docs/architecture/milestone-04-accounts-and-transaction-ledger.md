# Milestone 4 Accounts and transaction ledger

## Objective

Deliver the first usable ledger slice: account and category management, opening balances, manual income and expense entry, transfers, transaction browsing, and safe draft, post, void, and correction workflows.

## Prerequisites

- Milestones 1–3 are completed.
- Read the functional specification sections for accounts, transactions, transfers, savings, exchange rates, and transaction lifecycle.
- Review the existing domain aggregate and EF migration before changing either.

## In scope

- Create, edit, archive, and view personal accounts with one currency each.
- Account types: cash, checking, savings, credit card, e-wallet, loan, and other.
- Create and maintain income and expense categories; posted transactions use leaf categories.
- Represent opening balances exclusively as posted opening-balance transactions.
- Create drafts for income, expenses, splits, transfers, balance adjustments, and corrections.
- Post through the existing posting service and persist the whole aggregate atomically.
- Manual historical exchange-rate entry when transaction currency differs from the reporting base currency.
- Same-currency and cross-currency transfers, including explicit transfer fees as separate expenses.
- Savings designation and correct savings-transfer classification.
- Transaction register with filtering, sorting, running account balance, and detail inspection.
- Void posted unreconciled transactions while preserving audit data.
- Correct posted history through a traceable correction relationship.
- WinUI pages for Accounts, Transactions, and the transaction editor.

## Out of scope

- Recurring schedules, budgets, imports, reconciliation sessions, attachments, reports, Windows Hello, backups, and packaging.
- Automatic exchange-rate retrieval.
- Hard deletion of posted records or master data referenced by history.
- Business accounting or general-ledger terminology.

## Required design and behavior

- Keep financial calculations and posting rules outside code-behind.
- Use application services for account creation, posting, voiding, and corrections; simple read operations do not need command buses.
- Query balances from posted legs. Do not add a mutable balance field to accounts.
- Draft and voided transactions must not affect balances or standard ledger totals.
- Account currency must match every associated leg.
- Transfers must remain one logical transaction with exactly two owned-account legs.
- Foreign transactions must preserve original and historical base values.
- Archive referenced accounts and categories instead of deleting them.
- Preserve user input when validation or concurrency conflicts occur.
- Add pagination or incremental loading if the register query would otherwise load unbounded history.
- Add indexes only for demonstrated register and account-balance queries.

## Database work

- Review the released initial migration; never rewrite it.
- Add a forward migration for any new account, category, transaction metadata, exchange-rate, or audit fields.
- Test migration from the Milestone 3 schema with representative records.
- Verify record counts and `PRAGMA foreign_key_check` after migration.

## Required tests

- Account currency and archive rules.
- Opening balance implemented as a transaction.
- Income and expense posting with exact split equality.
- Same-currency and cross-currency transfers.
- Savings transfers excluded from expenses and budgets.
- Transfer fees represented explicitly.
- Draft and voided records excluded from balances.
- Historical exchange-rate preservation.
- Correction linkage and audit data.
- Optimistic concurrency conflicts.
- Real SQLite integration tests for atomic persistence and register queries.
- WinUI smoke tests for account creation, manual expense, and transfer if UI automation is available.

## Completion checks

- Restore, Release build, all tests, and formatting verification pass.
- Account balances match manually verified posted-leg datasets.
- No posted record can be hard-deleted through the application.
- No account or category referenced by history can be hard-deleted.
- User-visible validation is actionable and keyboard accessible.
- Schema documentation and developer documentation are updated.
- Update [milestone status](milestone-status.md), record the commit, and stop.

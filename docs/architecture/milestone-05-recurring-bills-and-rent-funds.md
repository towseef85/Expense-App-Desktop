# Milestone 5 Recurring bills and quarterly rent sinking funds

## Objective

Add planning for recurring personal obligations and quarterly rent without automatically posting money or misclassifying savings contributions as expenses.

## Prerequisites

- Milestone 4 is completed and manual posting workflows are stable.
- Read the functional specification sections for recurring transactions and rent sinking funds.

## Product decisions required before implementation

- Define rent contribution dates and whether a partial current month counts as a contribution period.
- Confirm the short-month policies exposed to users: last valid day and skip.
- If either decision remains unresolved, mark this milestone `Blocked` rather than embedding an undocumented assumption.

## In scope

- Daily, weekly, monthly, yearly, and interval-in-months schedules.
- Fixed, estimated, and variable-with-no-default expected amounts.
- Explicit short-month policy.
- Rolling 12-month planned-instance generation.
- Upcoming, due, overdue, paid, skipped, and cancelled instance states.
- Idempotent generation using schedule and occurrence date uniqueness.
- Pay flow that creates or links a real posted transaction.
- Schedule edits affecting future unlinked instances only.
- Quarterly rent obligations, cycles, contribution targets, funding status, and suggested contributions.
- Link actual transfers into savings to rent contributions.
- Link the posted rent expense once when rent is paid.
- Recurring and Rent funds WinUI pages plus dashboard due-item data.

## Out of scope

- Automatic posting, background Windows notifications, cloud calendars, generic goals, and envelope budgeting.
- Treating a planned allocation as cash movement.

## Required design and behavior

- Generation must be deterministic and safe to run repeatedly.
- A planned instance is paid only when linked to a posted transaction.
- Historical instances must not be rewritten when a schedule changes.
- Suggested contribution equals the unfunded amount divided across defined remaining contribution dates, with the rounding remainder assigned explicitly to the final period.
- A rent contribution is a transfer, not an expense.
- Rent is recognized exactly once, when its posted expense is linked.
- Voiding a linked transaction must update the planning state without deleting history.

## Database work

- Add forward migrations for schedules, planned instances, rent obligations or cycles, and contribution links.
- Add unique schedule-occurrence constraints and indexes for due-item queries.
- Preserve all existing ledger data in migration tests.

## Required tests

- Short months, leap years, and both short-month policies.
- Every-three-month recurrence across year boundaries.
- Idempotent generation and unique occurrence protection.
- Schedule edit behavior for historical and future instances.
- Paid requires a posted linked transaction.
- Rent contribution calculation, final-period remainder, underfunded and excess-funded states.
- Savings contributions excluded from expense and budget totals.
- Rent payment counted once.
- SQLite integration tests for generation concurrency and transaction links.

## Completion checks

- Restore, build, tests, and formatting pass.
- Manually verified calendars cover month-end, leap-year, and quarterly cases.
- No workflow automatically posts money.
- Schema and user-facing behavior documentation are updated.
- Update [milestone status](milestone-status.md), record the commit, and stop.

# Budget Manager milestone status

This file is the handoff index for implementation work. An agent should select the first milestone marked `Not started`, read its linked brief and the approved functional specification, complete only that milestone, update this file, and stop.

## Status definitions

- `Completed`: implemented, verified, committed, and pushed.
- `In progress`: implementation has started but its completion checks have not all passed.
- `Blocked`: work cannot continue without a recorded product decision or external prerequisite.
- `Not started`: no implementation work should be assumed.

## Milestones

| Milestone | Status | Implementation brief or evidence | Commit |
|---|---|---|---|
| 1 Solution foundation and architecture | Completed | [ADR 0001](ADR-0001-foundation-decisions.md) | `103ccce` |
| 2 Financial domain and posting invariants | Completed | [ADR 0002](ADR-0002-money-rounding-policy.md) | `5d2794f` |
| 3 EF Core SQLite persistence and migrations | Completed | [Database schema](../database/schema.md) | `bcebdc5` |
| 4 Accounts and transaction ledger | Not started | [Milestone 4 brief](milestone-04-accounts-and-transaction-ledger.md) | - |
| 5 Recurring bills and quarterly rent sinking funds | Not started | [Milestone 5 brief](milestone-05-recurring-bills-and-rent-funds.md) | - |
| 6 Budgets and multi-currency reporting | Not started | [Milestone 6 brief](milestone-06-budgets-and-multi-currency-reporting.md) | - |
| 7 CSV and XLSX statement imports | Not started | [Milestone 7 brief](milestone-07-statement-imports.md) | - |
| 8 Reconciliation | Not started | [Milestone 8 brief](milestone-08-reconciliation.md) | - |
| 9 Attachments and reports | Not started | [Milestone 9 brief](milestone-09-attachments-and-reports.md) | - |
| 10 Optional Windows Hello lock | Not started | [Milestone 10 brief](milestone-10-windows-hello-lock.md) | - |
| 11 Backup restore data export and MSIX packaging | Not started | [Milestone 11 brief](milestone-11-backup-restore-export-and-msix.md) | - |
| 12 Accessibility performance upgrade and release hardening | Not started | [Milestone 12 brief](milestone-12-release-hardening.md) | - |

## Handoff procedure

1. Confirm the working tree and preserve unrelated changes.
2. Read `AGENTS.md`, the functional specification, the selected milestone brief, relevant ADRs, and nearby implementation and tests.
3. Change the selected milestone to `In progress` in the same commit that begins substantive work.
4. Do not implement later milestones merely because their interfaces are visible.
5. Run every completion check in the selected brief.
6. Change the status to `Completed` only after implementation, documentation, and verification succeed.
7. Record the final commit hash in this table and stop before the next milestone.

# Milestone 8 Reconciliation

## Objective

Allow users to reconcile one account against non-overlapping statement periods while protecting completed history from silent mutation.

## Prerequisites

- Milestone 7 is completed.
- Read the reconciliation lifecycle, transaction-protection, reopening, and correction rules in the functional specification.

## In scope

- Draft, completed, and reopened reconciliation sessions for one account.
- Statement period, opening balance, closing balance, cleared items, calculated balance, and difference.
- Completion only when the difference is zero at account-currency precision.
- A transaction leg in at most one completed reconciliation.
- Protection of included transactions from direct editing and voiding.
- Reopen newest-to-oldest with an audit record.
- Correction workflow when reopening is not appropriate.
- Reconciliation UI with keyboard-operable item selection, difference explanation, completion confirmation, and reopen warning.

## Out of scope

- Bank API matching, invisible automatic matching, and business accounting close periods.

## Required design and behavior

- Statement periods for completed reconciliations cannot overlap for an account.
- Opening balance defaults from the previous completed reconciliation but remains an auditable snapshot.
- Completion validation and item assignment occur in one database transaction.
- Reopened history must remain traceable; do not delete the completed session.
- Editing a protected transaction requires permitted reopening or a correction transaction.
- Import rollback must respect reconciliation protection.
- Every displayed balance and difference must allow inspection of contributing legs.

## Database work

- Add forward migrations for reconciliation sessions, items, reopen audit data, and required indexes.
- Enforce unique completed item membership and supported period constraints in the database where possible.
- Validate cross-row period and ordering rules inside the application transaction.

## Required tests

- Exact zero completion and nonzero rejection.
- Account-currency precision.
- Period overlap and boundary behavior.
- A leg cannot join two completed reconciliations.
- Protected edit, void, delete, and import rollback attempts.
- Newest-to-oldest reopening and audit creation.
- Optimistic concurrency during completion and reopen.
- Real SQLite atomicity tests with manually verified statement datasets.

## Completion checks

- Restore, build, tests, and formatting pass.
- Reconciliation completion cannot leave partial item assignments.
- Reconciled history cannot be silently mutated through any existing workflow.
- Reopen and correction flows remain auditable.
- Schema and user documentation are updated.
- Update [milestone status](milestone-status.md), record the commit, and stop.

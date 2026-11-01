# Milestone 7 CSV and XLSX statement imports

## Objective

Import bank statements through an explicit, auditable staging and review workflow without silently posting invalid or duplicate rows.

## Prerequisites

- Milestone 6 is completed and manual transaction posting is stable.
- Read the import security, culture, duplicate, staging, and rollback rules in the functional specification.
- Select maintained libraries for CSV and XLSX only after checking licenses, security posture, and current .NET 10 compatibility.

## In scope

- CSV and XLSX input; legacy XLS remains unsupported.
- Reusable mapping profiles with account, header, columns, sign convention, culture, and explicit date formats.
- File hashing and repeated-file warning.
- Import-run and source-row audit records.
- Staging parse, validation, preview, selection, duplicate review, and atomic commit.
- Exact duplicate matching and explainable probable-duplicate scoring.
- User review of category suggestions and posting status.
- Rollback only while reconciliation and later-edit restrictions permit it.
- Import UI for mapping, preview, validation, duplicates, and commit summary.

## Out of scope

- Bank APIs, OFX, QFX, CAMT, OCR, invisible categorization, and automatic posting without review.

## Required design and behavior

- Copy or stage source data only under the import staging directory.
- Parse using the saved profile’s culture and date formats, never the current machine culture.
- Do not log complete rows, sensitive descriptions, or account identifiers.
- Do not rely only on filename for repeated-file detection.
- Invalid and suspected duplicate rows cannot be silently posted.
- Commit all selected valid rows and audit links in one database transaction.
- File finalization must follow a staged protocol when filesystem changes accompany database work.
- Preserve validation errors and user selections after recoverable failures.

## Database work

- Add forward migrations for mapping profiles, immutable import runs, staging rows, validation results, duplicate links, and committed transaction links.
- Add file-hash, account/date, stable row, and external-identifier indexes and constraints.

## Required tests

- CSV quoting, encodings, separators, debit-credit and signed-amount formats.
- XLSX sheets, headers, cell types, and malformed workbooks.
- Multiple explicit cultures and ambiguous dates.
- File hashing and repeated-file warnings.
- Exact and probable duplicate scenarios with explainable outcomes.
- No commit when any selected row is invalid.
- Atomic commit and rollback restrictions.
- Cancellation and cleanup of staging files.
- Tests must use the real SQLite provider and representative files without real financial data.

## Completion checks

- Restore, build, tests, formatting, and dependency-vulnerability scan pass.
- No imported row posts without visible review.
- Logs and errors are verified not to expose full source rows.
- Re-importing the same file produces a warning.
- Schema, supported formats, and mapping documentation are updated.
- Update [milestone status](milestone-status.md), record the commit, and stop.

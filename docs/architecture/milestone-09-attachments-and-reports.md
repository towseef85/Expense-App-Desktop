# Milestone 9 Attachments and reports

## Objective

Add managed receipt and document storage plus the approved personal-finance reports, with every total traceable to source records.

## Prerequisites

- Milestone 8 is completed.
- Read the attachment, privacy, reporting, filesystem, and backup-inclusion rules in the functional specification.

## In scope

- PDF, JPG, JPEG, and PNG attachments; HEIC only if a maintained compatible decoder is explicitly approved.
- Application-managed attachment storage with generated names.
- Original filename, media type, size, SHA-256, created timestamp, and transaction link metadata.
- Default 20 MB size limit with configuration support.
- Safe add, view, unlink, hash verification, and recoverable pending deletion.
- Orphan cleanup after a defined retention period.
- Cash flow, spending by category and payee, income sources, budget variance, net worth, savings flow, and upcoming-obligation reports.
- Date, account, category, payee, and currency filters where applicable.
- Drill-down from every report total to contributing records.
- CSV export for approved tabular reports.

## Out of scope

- OCR, receipt parsing, cloud storage, PDF report generation, a custom report builder, and investment valuation.

## Required design and behavior

- Never build a storage path from a user or imported filename.
- Validate file signature where practical, extension, media type, and size before finalization.
- Copy through staging and finalize safely; compensate database state if filesystem finalization fails.
- Verify SHA-256 when opening or during explicit integrity checks.
- Unlinking moves files into recoverable pending deletion rather than immediately destroying them.
- Do not log filenames when they may expose financial information, attachment contents, or transaction descriptions.
- Reports use posted records and historical valuations only.
- Transfers and savings contributions remain excluded from income and expense reports.
- A purchase from a savings account remains an expense.

## Database work

- Add forward migrations for attachment metadata, transaction links, pending-deletion state, and report-supporting indexes justified by query plans.
- Preserve attachment referential integrity when transactions are corrected or voided.

## Required tests

- Path traversal and malicious filenames.
- Unsupported content, oversized files, hash mismatch, duplicate content, cancellation, and interrupted finalization.
- Recoverable unlink and retention cleanup.
- Report golden datasets in multiple currencies.
- Inclusion and exclusion of every transaction kind and lifecycle state.
- Drill-down record sets exactly match displayed totals.
- CSV escaping, culture-neutral storage, and locale-aware display.

## Completion checks

- Restore, build, tests, formatting, and vulnerability scan pass.
- Attachment files exist only below the managed attachment root.
- No orphan is irreversibly deleted before retention expires.
- All report totals reconcile with golden datasets and expose contributors.
- Backup requirements explicitly include attachments for Milestone 11.
- Update schema and developer documentation.
- Update [milestone status](milestone-status.md), record the commit, and stop.

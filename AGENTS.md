# AGENTS.md

## Purpose

This repository contains a Windows personal budget manager built with WinUI 3, .NET 10, Entity Framework Core, and SQLite.

The application is:

- Personal-only, not business accounting.
- Offline-first.
- Multi-currency.
- Distributed as a self-contained MSIX.
- Designed to run without a separately installed database server.

Read the approved functional specification before making product, domain, database, security, or reporting decisions.

## Instruction priority

When instructions conflict, use this order:

1. The current user request.
2. The approved functional specification and recorded product decisions.
3. This `AGENTS.md`.
4. Existing repository conventions.
5. General framework conventions.

Do not silently resolve material conflicts. Explain the conflict and its consequences before proceeding.

## Scope

The application supports:

- Personal income and expenses.
- Accounts and opening balances.
- Transfers between owned accounts.
- Savings accounts and savings transfers.
- Recurring personal bills.
- Quarterly rent and sinking funds.
- Monthly category budgets.
- Multiple currencies and historical exchange rates.
- Receipt and document attachments.
- CSV and XLSX statement imports.
- Statement reconciliation.
- Personal-finance reports.
- Optional Windows Hello application locking.
- Local backups, restore, and data export.

The following are out of scope unless the specification is formally changed:

- Business bookkeeping.
- Customers and suppliers.
- Sales or purchase invoices.
- Accounts receivable or payable.
- Inventory.
- Payroll.
- VAT, sales-tax, or income-tax accounting.
- General-ledger and journal-entry user interfaces.
- Business profit-and-loss statements.
- Bank API connections.
- Cloud synchronization.
- Multi-user collaboration.
- Web or mobile applications.
- Microservices.

Do not introduce out-of-scope features as infrastructure for hypothetical future requirements.

## Architecture

Use a modular monolith.

The expected project responsibilities are:

- `BudgetManager.App`
  - WinUI 3 presentation.
  - Navigation.
  - Dependency-injection composition root.
  - Windows application lifecycle.
  - Windows-specific user interaction.

- `BudgetManager.Domain`
  - Money and currency value objects.
  - Accounts and transactions.
  - Posting invariants.
  - Recurrence and budgeting rules.
  - Reconciliation rules.
  - Domain calculations that do not depend on WinUI, EF Core, or the filesystem.

- `BudgetManager.Application`
  - Use-case orchestration.
  - Input validation.
  - Transaction coordination.
  - Interfaces for genuine external boundaries.
  - Authorization and workflow policies.

- `BudgetManager.Infrastructure`
  - EF Core and SQLite.
  - Attachment storage.
  - Statement file parsing.
  - Backup and restore.
  - Local logging.
  - Windows-integrated service implementations.

- Test projects corresponding to the layers being tested.

A simpler structure may be used when it clearly improves maintainability. Explain material architectural changes before implementing them.

## Dependency rules

- Domain must not reference Application, Infrastructure, App, WinUI, or EF Core.
- Application may reference Domain.
- Infrastructure may reference Application and Domain.
- App may reference Application and register Infrastructure implementations.
- Avoid circular project references.
- Keep Windows-specific APIs outside Domain.
- Keep EF Core entities and configuration from leaking into the UI.
- Do not make domain calculations depend on a database connection.

## Simplicity rules

Do not apply patterns mechanically.

Avoid introducing:

- A command or event bus for simple local operations.
- Repositories that only duplicate `DbSet` operations.
- A generic repository.
- A generic service base class.
- One interface for every class.
- Empty domain events.
- Microservice abstractions.
- Distributed messaging.
- Event sourcing.
- Kubernetes or container infrastructure.
- A web API for an offline desktop application.

Use an abstraction when it protects domain behavior, enables meaningful testing, or separates a real external boundary.

## Financial correctness

Financial correctness takes priority over UI convenience.

The following rules are mandatory:

- Never use `float` or `double` for money.
- Persist monetary values as integer minor units.
- Use decimal arithmetic for exchange-rate calculations.
- Every monetary value must have a currency.
- An account has exactly one currency.
- Preserve original transaction amounts and currencies.
- Store the historical base-currency valuation used when a transaction is posted.
- Do not recalculate historical reports using newly edited exchange rates.
- Round only at defined currency boundaries.
- Use one documented rounding policy consistently.
- Do not silently discard rounding differences.

## Posting invariants

Only posted transactions affect balances and reports.

Draft, voided, or deleted records must not affect:

- Account balances.
- Budget actuals.
- Income or expense reports.
- Savings reports.
- Sinking-fund progress.
- Reconciliation.

Additional invariants:

- A transfer uses two linked account legs.
- Transfer accounts must be different.
- Each leg’s currency must equal its account currency.
- Same-currency transfer legs must have equal absolute amounts.
- Cross-currency transfers preserve both account amounts and the applied rate.
- Transfers do not count as income or expenses.
- Transfers to savings do not consume budgets.
- Transfer fees are explicit expenses.
- Income and expense split totals must equal the account amount.
- Opening balances are represented by transactions, not mutable account fields.
- Rent savings contributions are transfers.
- Rent becomes an expense only when the rent payment is posted.
- Reconciled history must not be silently edited.
- Corrections must remain auditable.
- Posting operations must be atomic.

Validate aggregate-wide invariants inside a single database transaction.

## Transaction lifecycle

Use explicit transaction states, including:

- Draft.
- Posted.
- Voided.

Hard deletion is allowed only for unposted drafts that have no protected dependencies.

Posted financial records should normally be voided or corrected rather than deleted.

Editing a reconciled transaction requires:

- Reopening the affected reconciliation in the permitted order; or
- Posting a traceable correction.

Do not silently mutate reconciled history.

## Currency and dates

- Use ISO 4217 currency codes for the MVP.
- Store currency minor-unit precision.
- Store financial effective dates as calendar dates without time.
- Store audit timestamps in UTC.
- Display dates, numbers, and currencies using the configured locale.
- Parse imported values using an explicit import culture and date format.
- Never rely on the computer’s current culture when parsing a saved import profile.

## Savings and budgets

- Savings are owned money, not spending.
- Transfers into savings appear in savings-flow reporting.
- Transfers out of savings remain transfers.
- A purchase made from a savings account is an expense.
- Budget actuals include posted expense splits only.
- Income, transfers, opening balances, and balance adjustments do not consume budgets.
- Avoid overlapping parent and child budget lines in the MVP.
- Budget calculations must be reproducible from stored transaction data.

## Recurring transactions

Recurring schedules create planned instances, not automatically posted money movements.

- Generation must be idempotent.
- A schedule and occurrence date combination must be unique.
- A recurring instance is paid only when linked to a posted transaction.
- Editing a schedule must not rewrite historical instances.
- Short-month behavior must follow the configured policy.
- Automatically posting money requires a future approved product decision.

## Imports

Statement import must use a staging workflow:

1. Read the source file.
2. Apply an explicit mapping profile.
3. Parse rows into staging records.
4. Validate dates, amounts, currencies, and required fields.
5. Detect exact and probable duplicates.
6. Show the user a reviewable preview.
7. Commit selected valid rows atomically.

Import rules:

- Support CSV and XLSX for the MVP.
- Do not treat legacy XLS as supported unless an approved library is added.
- Never silently post invalid or suspected duplicate rows.
- Preserve import-run and source-row audit information.
- Reimporting the same file must produce a warning.
- Do not depend only on the filename for duplicate detection.
- Do not log complete imported statement rows.
- Import rollback must respect reconciliation and later-edit restrictions.

## Reconciliation

- A reconciliation belongs to one account.
- Statement periods must not overlap completed reconciliation periods.
- The closing difference must be zero at the account currency precision before completion.
- A transaction leg may belong to only one completed reconciliation.
- Completed reconciliations protect their included transactions from direct editing.
- Reopen reconciliations from newest to oldest.
- Reopening must create an audit record.

## Attachments

- Copy attachments into application-managed storage.
- Do not depend on the original external file remaining available.
- Use generated storage names.
- Preserve the original filename as metadata.
- Validate allowed file types and file sizes.
- Store and verify a SHA-256 hash.
- Prevent path traversal.
- Never construct a storage path directly from an imported or user-provided filename.
- Include attachments in backups.
- Clean up orphan files using a recoverable retention process.

## Security

Do not implement an application-specific PIN.

The optional application lock uses Windows Hello:

- Disabled by default.
- Enabled explicitly by the user.
- Uses supported Windows Hello APIs.
- May authenticate through the Windows PIN, fingerprint, or face configured by Windows.
- The application must never receive, inspect, validate, transmit, or store the Windows PIN.
- Disabling an enabled lock requires successful Windows Hello authentication.
- Startup and inactivity locking are configurable.
- If Windows Hello is unavailable, the application remains usable unless the user previously enabled and required the lock.
- If the user required the lock and Windows Hello is unavailable, fail closed and display recovery guidance.
- Never delete or reset financial data because authentication failed.

The MVP does not encrypt the SQLite database or attachments.

- Store data under the current Windows user profile.
- Rely on Windows user-profile permissions.
- Do not describe the optional lock as encryption.
- Do not claim protection from administrators, malware running as the user, or offline disk access.
- Keep authentication behind a boundary that permits future encryption work without changing the financial domain.

Never log:

- Authentication responses.
- PINs or credentials.
- Account numbers.
- Attachment contents.
- Complete imported rows.
- Sensitive transaction descriptions.

## Database

Use EF Core with SQLite.

- Use EF Core migrations for production schema changes.
- Do not use `EnsureCreated` for production databases.
- Enable SQLite foreign-key enforcement.
- Use Write-Ahead Logging when validated for the application lifecycle.
- Configure a reasonable busy timeout.
- Use optimistic concurrency for editable aggregate roots.
- Add indexes based on demonstrated query paths.
- Do not duplicate derived balances as mutable account fields unless a measured performance requirement justifies a safely maintained projection.
- Preserve referential integrity when master data is archived.
- Do not hard-delete master data referenced by financial history.

## Migrations

Before changing the schema:

1. Review existing migrations.
2. Determine whether existing user data can be preserved.
3. Add a forward migration.
4. Test migration from supported earlier schemas.
5. Verify foreign keys and record counts.
6. Update schema documentation when the model changes.

Released migrations must not be rewritten.

Use a pre-migration backup before applying production migrations.

Downgrades should restore a compatible backup rather than reverse released migrations.

## Filesystem

All application data must be stored in an application-specific directory under the current Windows user profile.

Maintain separate locations for:

- SQLite database.
- Attachments.
- Logs.
- Backups.
- Import staging.
- Restore staging.

Rules:

- Do not write user data into the installation directory.
- Do not assume the current working directory is writable.
- Do not build paths through string concatenation.
- Prevent directory traversal.
- Use atomic replacement where practical.
- Clean temporary files after success and recover safely after interruption.
- Do not store backups only beside the live database by default.

## Backup and restore

A backup must contain:

- A transactionally consistent SQLite snapshot.
- Attachments.
- A manifest.
- File hashes.
- Application version.
- Schema version.

Do not copy an active SQLite database file as though it were a safe backup. Use the SQLite backup mechanism or another verified consistent snapshot process.

Before restore:

- Validate archive structure.
- Validate hashes.
- Validate schema compatibility.
- Validate available disk space.
- Validate attachment references.
- Create a safety backup of current data.

Restore into staging before replacing live data.

Never overwrite the last known-good backup after a failed verification.

## Packaging

The release format is self-contained MSIX.

The package must not require the user to separately install:

- .NET.
- SQLite.
- A database server.
- Developer tooling.

Before changing package versions:

- Verify .NET 10 compatibility.
- Verify the Windows App SDK and WinUI 3 compatibility.
- Verify EF Core compatibility.
- Verify the target Windows versions.
- Prefer stable supported packages.
- Do not guess compatibility.
- Do not silently move to preview packages.

Keep user data outside the MSIX installation directory so upgrades do not remove it.

## Coding standards

- Use the repository’s `.editorconfig`.
- Enable nullable reference types.
- Treat application-owned warnings as errors.
- Prefer clear names over abbreviations.
- Keep methods focused.
- Avoid boolean parameters when an enum or named operation expresses intent more clearly.
- Use immutable value objects for money and currency-sensitive calculations where practical.
- Validate arguments at system boundaries.
- Do not catch exceptions only to ignore them.
- Do not expose raw exception messages as user guidance.
- Support cancellation for meaningful long-running operations.
- Use asynchronous APIs for database and file I/O where appropriate.
- Avoid blocking asynchronous work with `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()`.

Do not add source comments that merely restate the code. Document financial rules, non-obvious constraints, and important trade-offs.

## UI rules

- Keep financial calculations outside code-behind.
- Use code-behind only for view-specific interaction that does not contain domain behavior.
- Keep navigation and dialogs accessible through keyboard operation.
- Provide visible focus indicators.
- Support Windows text scaling.
- Do not communicate status using color alone.
- Use confirmation dialogs for destructive or history-changing operations.
- Display actionable validation messages.
- Never silently correct an imported amount or date.
- Every report total should allow the user to inspect its contributing records.

## Error handling

Classify failures where useful:

- Validation failure.
- Concurrency conflict.
- Database failure.
- File access failure.
- Import parsing failure.
- Authentication failure.
- Backup or restore failure.
- Unexpected application failure.

For recoverable failures:

- Preserve user input.
- Explain what failed.
- Explain what the user can do.
- Avoid technical stack traces in the UI.
- Record a redacted diagnostic log.

## Testing

Every implemented financial rule requires automated tests.

Use:

- Unit tests for domain calculations and invariants.
- Property-based tests where they materially improve confidence.
- SQLite integration tests against the real SQLite provider.
- Migration tests from supported previous schemas.
- Report tests against manually verified datasets.
- UI smoke tests for critical WinUI workflows.
- Packaging and upgrade tests for MSIX releases.

Do not use EF Core’s InMemory provider as proof that SQLite behavior works.

Important required test areas include:

- Money arithmetic and rounding.
- Transaction split equality.
- Same-currency and cross-currency transfers.
- Savings exclusion from expenses.
- Historical exchange-rate preservation.
- Recurrence across short months and leap years.
- Sinking-fund contribution calculations.
- Budget inclusion and rollover.
- Import parsing across supported cultures.
- Duplicate detection.
- Atomic import commit.
- Reconciliation completion and reopening.
- Reconciled-history protection.
- Attachment path safety.
- Backup integrity and restore.
- Migration of existing data.
- Windows Hello unavailable and cancellation paths.

## Verification

Before reporting a task as complete, run the relevant available commands.

Typical checks include:

```powershell
dotnet restore
dotnet build
dotnet test
dotnet format --verify-no-changes
```

Also run relevant migration, packaging, or UI checks when the task affects those areas.

Never claim a build, test, migration, package, installer, or application launch succeeded unless it was actually executed successfully.

If an environmental limitation prevents verification:

- State exactly what was not verified.
- State why.
- Report the checks that were completed.
- Do not describe unverified work as production-ready.

## Change discipline

Before editing:

- Inspect the repository.
- Read applicable nested `AGENTS.md` files.
- Review nearby implementation and tests.
- Check the worktree for existing user changes.
- Preserve unrelated changes.
- Identify the actual defect or requirement before rewriting code.

While editing:

- Keep the change within the requested milestone.
- Prefer small, reviewable commits or logical change groups.
- Avoid unrelated cleanup.
- Do not rename public concepts without justification.
- Update tests with behavior changes.
- Update documentation when architecture, schema, packaging, or user-visible behavior changes.

After editing:

- Review the diff.
- Run relevant checks.
- Report changed files.
- Report test and build results.
- Report assumptions, limitations, and deviations.

## Milestone boundaries

Work on one approved milestone at a time.

Do not begin later features merely because their future interfaces are visible.

Foundation work must not prematurely implement:

- Transaction posting.
- Budgets.
- Recurring bills.
- Imports.
- Reconciliation.
- Windows Hello.
- Backups.
- MSIX release behavior.

Minimal interfaces or placeholders are acceptable only when they establish a real boundary required by the current milestone. Clearly label placeholders and do not report them as completed functionality.

At the end of a milestone, stop and provide:

- Completed scope.
- Files changed.
- Architectural decisions.
- Commands executed.
- Test results.
- Known limitations.
- Deferred work.
- Recommended next milestone.

## Decision records

Create or update an architecture decision record when changing:

- Project boundaries.
- Money representation.
- Rounding policy.
- Database technology.
- Historical currency valuation.
- Import architecture.
- Authentication mechanism.
- Backup format.
- Packaging technology.
- Supported Windows versions.

Do not use decision records for trivial implementation details.

## Documentation

Keep developer documentation accurate for:

- Prerequisites.
- Restore, build, test, and run commands.
- Database migrations.
- Application data locations.
- Backup format.
- MSIX packaging.
- Supported Windows versions.
- Security limitations.

Do not document planned functionality as though it already exists.

## Prohibited shortcuts

Do not:

- Store money as floating-point values.
- Store plaintext credentials or PINs.
- Read or store the Windows PIN.
- Count transfers as expenses.
- Count savings contributions as expenses.
- Revalue historical reports silently.
- Auto-post imported rows without review.
- Edit reconciled history silently.
- Copy a live SQLite file as an assumed-consistent backup.
- Use `EnsureCreated` in production.
- Put user data in the MSIX installation directory.
- Introduce business-accounting features.
- Add cloud services without explicit approval.
- Mark placeholder code as complete.
- suppress failing tests to make a build appear successful.
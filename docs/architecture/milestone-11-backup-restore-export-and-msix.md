# Milestone 11 Backup restore data export and MSIX packaging

## Objective

Deliver verified local backup and restore, complete user-data export, and a self-contained signed MSIX packaging path that requires no separately installed .NET runtime, SQLite, or database server.

## Prerequisites

- Milestone 10 is completed.
- Read the backup, restore, filesystem, migration, security, packaging, and data-location rules in the functional specification.
- Verify current .NET 10, Windows App SDK, WinUI 3, Windows SDK, EF Core, and target Windows compatibility from official sources.

## Product decisions required before implementation

- Select the signing and distribution channel: Store, direct MSIX plus App Installer, or managed enterprise distribution.
- Approve backup schedule, destination defaults, and retention. Current recommendation: first close after changes, keeping seven daily and four weekly backups.
- Confirm supported processor architectures and minimum serviced Windows versions.

## In scope

- Transactionally consistent SQLite snapshot through the SQLite backup mechanism or another verified supported method.
- Backup archive containing database snapshot, attachments, manifest, hashes, application version, and schema version.
- Explicit backup verification and health display.
- Restore validation for archive structure, hashes, schema compatibility, disk space, and attachment references.
- Safety backup before restore.
- Restore into staging followed by controlled replacement of live data.
- Failure recovery that preserves the last known-good live data and backup.
- User-data export for transactions, accounts, categories, budgets, schedules, reconciliations, and attachment manifest in documented formats.
- Self-contained MSIX configuration for supported architectures.
- Signing, installation, upgrade, uninstall, and user-data preservation documentation.

## Out of scope

- Cloud backup, synchronization, server-side recovery, and encryption-at-rest unless separately approved through a new decision record.

## Required design and behavior

- Never copy an active SQLite database file as an assumed-consistent backup.
- Write the manifest last and verify every included hash.
- Do not keep backups only beside the live database by default.
- Restore must close live connections and use staging plus atomic replacement where practical.
- Never overwrite the last known-good backup after failed verification.
- Clean interrupted staging safely on later startup.
- The package must keep user data outside the installation directory.
- End users must not install developer tools or runtimes.
- Package identity and signing secrets must not be committed.

## Database and architecture work

- Add forward migrations for backup metadata only when it provides real audit value.
- Create or update ADRs if the backup format, supported Windows versions, or packaging approach changes.
- Integrate pre-migration safety backup before production migrations can run automatically.

## Required tests

- Backup during active use produces a consistent snapshot.
- Byte-identical attachment restore.
- Manifest and hash tampering detection.
- Missing files, corrupt archives, incompatible schema, insufficient disk space, cancellation, and interrupted replacement.
- Safety backup creation and recovery after failed restore.
- Export completeness and escaping.
- Clean-machine MSIX installation for every supported architecture.
- Upgrade from an earlier supported package while preserving user data.
- Uninstall and reinstall behavior is documented and tested.

## Completion checks

- Restore, build, tests, formatting, vulnerability scan, package build, and installer tests pass.
- A verified backup restores successfully on a clean data directory.
- A signed self-contained MSIX installs without separate runtime or database installation.
- Package output, versions, signing process, data locations, and recovery procedures are documented.
- Update [milestone status](milestone-status.md), record the commit, and stop.

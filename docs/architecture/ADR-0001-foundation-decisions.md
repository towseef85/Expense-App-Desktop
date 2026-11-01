# ADR 0001 Foundation architecture decisions

- Status: Accepted for Milestone 1
- Date: 2026-11-01

## Context

Budget Manager is a personal-only, offline-first Windows desktop application. Financial correctness and recoverability matter more than speculative extensibility. The application must run without a database server or separately installed .NET runtime.

## Decisions

### Modular monolith

Use one deployable desktop application with Domain, Application, Infrastructure, and App projects. Dependencies point inward: Domain has no project dependencies; Application references Domain; Infrastructure references Application and Domain; App composes Application and Infrastructure. No command bus, event bus, generic repository, web API, or distributed-system abstraction is introduced.

### Money representation

The approved representation is an immutable currency-aware value object with integer minor units. Exchange-rate calculations will use `decimal`. The exact rounding policy remains an approved-design prerequisite for Milestone 2; no money implementation is added in this milestone.

### SQLite and Entity Framework Core

Use EF Core 10 with SQLite for local persistence. The context shell exists only to establish the boundary. The financial model and first migration are deferred until the domain model is reviewed, avoiding an accidental schema commitment. Production code will use migrations, never `EnsureCreated`.

### Offline-first storage

Store the database, attachments, logs, backups, import staging, and restore staging in separate directories below `%LOCALAPPDATA%\BudgetManager`. The installation directory is never used for user data. Network services and telemetry are absent by default.

### Windows Hello boundary

Application owns an `IApplicationLock` boundary; the Windows-specific implementation is deferred to Milestone 10. It will use supported Windows Hello APIs and will never read, receive, validate, log, transmit, or store a Windows PIN or biometric data. The lock is access control, not database encryption.

### MSIX packaging

The release artifact will be a self-contained MSIX. The application project currently uses unpackaged self-contained startup so the foundation can build without prematurely committing signing, identity, update-channel, and architecture assets. MSIX authoring and clean-machine package verification are reserved for Milestone 11. This is a delivery staging decision, not a change to the approved release format.

## Consequences

The domain remains independently testable and future encryption can be placed behind infrastructure boundaries. The executable is larger because it carries the .NET and Windows App SDK runtimes. Package signing and update distribution remain unresolved release concerns and must be decided before public distribution.

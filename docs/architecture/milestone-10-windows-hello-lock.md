# Milestone 10 Optional Windows Hello lock

## Objective

Implement optional application access control through supported Windows Hello APIs without reading, validating, transmitting, logging, or storing the Windows PIN or biometric information.

## Prerequisites

- Milestone 9 is completed.
- Read the final security decision in `AGENTS.md`; it overrides every PIN-related statement in the original functional specification.
- Verify the selected Windows Hello API against current official Microsoft documentation and supported target Windows versions before implementation.

## In scope

- Lock disabled by default and enabled only through explicit user action.
- Availability detection and clear explanation of what the lock does and does not protect.
- Authentication at startup when configured.
- Authentication after a configurable inactivity period when configured.
- Successful Windows Hello verification required to disable an enabled lock.
- Windows Hello cancellation, failure, unavailable, and recovery paths.
- Fail closed when the user previously required the lock and Windows Hello becomes unavailable.
- Recovery guidance that never deletes, resets, exposes, or modifies financial data.
- Windows-specific implementation behind the existing application lock boundary.
- Accessible lock and recovery UI.

## Out of scope

- Application-specific PINs, credential storage, database encryption, attachment encryption, remote authentication, accounts, and cloud recovery.

## Required design and behavior

- The application never receives or knows which Windows authentication method was used.
- Never call the feature encryption or claim protection from administrators, malware running as the user, or offline disk access.
- Keep financial domain and persistence models independent of Windows Hello.
- Store only nonsecret lock configuration and audit-safe state.
- Do not log authentication responses, credential details, challenge data, or sensitive recovery context.
- Inactivity tracking must be based on supported application/window lifecycle events and must not create background surveillance.
- Authentication cancellation leaves the application locked without data mutation.

## Database or configuration work

- Prefer nonsecret application configuration unless an auditable persisted setting is required.
- If a schema change is needed, add a forward migration containing no credential, hash, salt, PIN, or biometric column.
- Do not recreate the specification’s obsolete `pin_credential` table.

## Required tests

- Disabled-by-default behavior.
- Enable and disable workflows.
- Startup and inactivity locking.
- Success, cancellation, failure, unavailable, and exception paths through a test double.
- Previously required lock becoming unavailable fails closed.
- No authentication failure modifies or deletes financial data.
- Log inspection confirms authentication data is absent.
- Keyboard, screen-reader, and focus behavior for lock and recovery screens.

## Completion checks

- Restore, build, tests, and formatting pass.
- Manual Windows Hello verification succeeds on supported hardware or the unverified hardware path is reported precisely.
- No PIN or credential storage exists in code, schema, logs, or documentation.
- Security limitations are documented accurately.
- Update [milestone status](milestone-status.md), record the commit, and stop.

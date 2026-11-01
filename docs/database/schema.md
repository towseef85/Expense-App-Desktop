# Budget Manager database schema

The initial SQLite schema persists the financial aggregate approved through Milestone 2. It does not yet implement recurring bills, budgets, imports, reconciliation sessions, attachments, authentication, or backups.

## Storage conventions

- Monetary values use signed 64-bit integer minor units.
- Exchange rates use SQLite `TEXT` through the EF Core decimal mapping, avoiding binary floating-point storage.
- Identifiers use 16-byte SQLite `BLOB` values.
- Effective dates use invariant calendar-date text.
- Audit timestamps use UTC values stored as invariant text.
- Editable aggregate roots use integer optimistic-concurrency tokens.
- Foreign keys, a 10-second busy timeout, and write-ahead logging are configured whenever EF opens a connection.

## Tables

### currency

Stores the ISO 4217 code and minor-unit precision. Accounts and transaction valuations reference it with restrictive foreign keys.

### account

Stores the account identity, display name, type, currency, savings designation, archive timestamp, and concurrency token. Opening balances are not stored here; they remain transactions.

### category

Stores income and expense category hierarchy, archive state, and concurrency token. A filtered expression index prevents duplicate active root or sibling categories when `parent_id` is null.

### financial_transaction

Stores transaction identity, kind, lifecycle status, effective date, UTC audit timestamps, correction and future reconciliation references, explicit base-rounding difference, savings-transfer designation, and concurrency token.

### transaction_leg

Stores account legs, original integer amount and currency, historical integer base amount and base currency, and the decimal rate used at posting. Legs cascade only when their owning transaction is deleted; account and currency deletion is restricted.

### transaction_split

Stores positive category allocations with original and historical base valuations. Splits cascade only with their owning transaction; category and currency deletion is restricted.

## Migration policy

The initial migration supports a new empty database. Released migrations must not be rewritten. Later migrations require preservation checks, record-count validation, foreign-key validation, and tests from every supported prior schema.

# ADR 0002 Money rounding policy

- Status: Accepted for Milestone 2
- Date: 2026-11-01

## Context

Financial amounts are persisted as integer minor units, while exchange-rate input and calculation use decimal arithmetic. Converting a value at exactly half a minor unit requires one explicit and reproducible rule. The functional specification permitted midpoint-away-from-zero or midpoint-to-even and described away-from-zero as easier to explain to users.

## Decision

Use `MidpointRounding.AwayFromZero` whenever a decimal value crosses a currency minor-unit boundary. Do not round intermediate exchange-rate values. Round only when producing a currency amount for persistence or display as a definitive monetary result.

Historical valuations store the original amount and currency, the rate used at posting, the base currency, and the resulting integer base amount. Later exchange-rate changes never recompute that stored amount.

Split conversion can create a base-currency remainder when each split is valued separately. Preserve and expose that remainder on the transaction; do not silently discard it.

## Consequences

Half-minor-unit results are predictable to users but can introduce a small aggregate directional bias compared with midpoint-to-even. Tests must cover positive and negative midpoints, currencies with different minor-unit precision, historical valuation preservation, and split rounding differences.

# ToSic.Sys.OData.UriQueryParsers

Minimal, permissive OData system query options parser with no EDM dependency. It accepts any identifiers and produces an AST for:

- $filter (binary/unary ops, functions, identifiers, literals)
- $orderby (list of expressions with asc/desc)
- $select / $expand (as simple string lists)
- $search (terms/phrases with AND/OR/NOT)
- $compute (list of expr as alias)
- $top / $skip / $index / $count / $skiptoken / $deltatoken

This is not a full OData validator; it only parses syntax and builds a simple AST.

Based on official [OASIS OData ABNF](https://github.com/oasis-tcs/odata-abnf/).

Tests: `ToSic.Sys.OData.Tests` includes golden tests with ABNF-style examples.

## What’s supported

- General
  - No EDM model required; parses purely by syntax and builds a minimal AST.
  - Identifiers: permissive; allow `$`, `@`, dots and slashes for path-like names (for example: `Orders/Customer.Name`).
  - Literals: strings (single/double-quoted), numbers, boolean, `null`.

- $filter
  - Binary operators: `eq`, `ne`, `gt`, `ge`, `lt`, `le`, `and`, `or`, `add`, `sub`, `mul`, `div`, `divby`, `mod`, `in`, `has`.
  - Unary operators: `not`, unary `-`.
  - Function calls: any identifier followed by `(args...)` with nested expressions supported.
  - Minimal tolerance for `any()` / `all()` shapes in the form `var: predicate` (parsed as a call with two arguments); semantic validation is intentionally out of scope.

- $orderby
  - Comma-separated list of expressions with optional `asc`/`desc` direction.

- $select / $expand
  - Parsed as simple string lists for selected/expanded paths; no deep options processing.

- $search
  - AND/OR/NOT with parentheses, terms and phrases; permissive lexer mode that treats punctuation inside terms as-is.

- $compute
  - List of `<expr> as <alias>` entries; expressions re-use the $filter grammar.

- Other simple options
  - `$top`, `$skip`, `$index`, `$count`, `$skiptoken`, `$deltatoken` parsed to simple values without further validation.

## What’s not supported (yet)

This library is intentionally minimalist and permissive. The following are out of scope or not implemented:

- No EDM/type system, no semantic checks (for example: operand type compatibility, function existence, property paths vs. navigation).
- Spatial literals (geography/geometry), temporal and duration literal nuances beyond basic numeric/string forms.
- Cast/`isof`, `has` with enum type checks, and other type-related operators beyond syntactic parsing.
- Deep $expand/$select options (for example: `$expand=Nav($filter=..., $select=..., $search=...)`), `$levels`, `$ref`, or nested system options inside $expand.
- URL decoding/encoding edge cases; strict percent-encoding validation.
- JSON-in-parameters, alias/JSON-reference heavy inputs (for example: `@p={...}` bodies) beyond simple alias names.
- Path forms like `.../$count(...)` and other URI path expressions outside the system query options themselves.
- Full negative-case validation (the parser is permissive; many errors that require semantic knowledge are intentionally not enforced).

## ABNF conformance tests (official testcases)

The test project integrates the official [OASIS OData ABNF](https://github.com/oasis-tcs/odata-abnf/) testcases from `odata-abnf-testcases.yaml` and runs a harness that maps applicable cases to this parser’s supported subset.

As of 2025-09-12 (latest local run):

- Total ABNF cases discovered: 840
- Included positive cases parsed successfully: 134
- Included negative cases (expected to be rejected): 0
- Skipped (unsupported/out-of-scope/pathological for this minimal parser): 651

Notes:
- Skips primarily include geometry/geography literals, deep $expand/$select options, alias/JSON-reference payload-style inputs, strict URL-encoding cases, path `/$count(...)`, and negatives that require semantic or quote/encoding strictness beyond this parser’s remit.
- Numbers will evolve as features are added and filters are relaxed.

## Running tests

Tests live in `ToSic.Sys.OData.Tests` and include golden examples plus the ABNF harness.

## Design goals

- Keep the grammar permissive and fast, suitable for basic syntax inspection and AST generation without a model.
- Favor clear AST nodes (identifiers, literals, unary/binary expressions, calls, lists) and a Pratt-style expression parser for maintainability.
- Defer semantic checks to higher layers; don’t attempt to emulate OData’s full runtime binding behavior.

## Caveats

- This is not a drop-in replacement for OData.NET’s query parsing; it is a lightweight, EDM-free parser useful for tooling, diagnostics, or preliminary analysis.
- Error messages are oriented around syntax shape, not semantics. Some malformed inputs may still parse into a best-effort AST by design.

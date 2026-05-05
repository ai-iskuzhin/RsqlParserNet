# Syntax

`RsqlParserNet` targets common RSQL/FIQL syntax compatible with the Java `jirutka/rsql-parser` project, while keeping .NET-specific adapter concerns outside the core package.

## Operators

| Syntax | Meaning | Example |
| --- | --- | --- |
| `==` | Equal | `status==active` |
| `!=` | Not equal | `status!=draft` |
| `>` / `=gt=` | Greater than | `count>10` |
| `>=` / `=ge=` | Greater than or equal | `count>=10` |
| `<` / `=lt=` | Less than | `count<10` |
| `<=` / `=le=` | Less than or equal | `count<=10` |
| `=in=` | In | `status=in=(active,draft)` |
| `=out=` | Not in | `status=out=(archived,deleted)` |
| configured custom operator | Custom | `title=contains=Bike` |
| `;` / `and` | Logical AND | `status==active;title==Bike` |
| `,` / `or` | Logical OR | `status==active,status==draft` |
| `(...)` | Grouping | `status==active;(title==Bike,status==draft)` |

AND has higher precedence than OR. Parentheses can override precedence.

```text
status==active;title==Bike,status==draft
```

is parsed as:

```text
(status==active AND title==Bike) OR status==draft
```

## Grammar

```text
input          = or, EOF;
or             = and, { ( "," | word-or ), and };
and            = primary, { ( ";" | word-and ), primary };
primary        = comparison | group;
group          = "(", or, ")";

comparison     = selector, comparison-op, arguments;
arguments      = value | "(", value, { ",", value }, ")";
value          = unreserved-str | single-quoted | double-quoted;

comparison-op  = built-in-op | configured-custom-op;
built-in-op    = "==" | "!=" | ">" | ">=" | "<" | "<=" |
                 "=gt=" | "=ge=" | "=lt=" | "=le=" |
                 "=in=" | "=out=";

word-and       = " and ";
word-or        = " or ";
```

Word logical operators are recognized case-insensitively when surrounded by whitespace and when `RsqlParseOptions.AllowWordLogicalOperators` is enabled.

## Selectors

The default selector grammar is intentionally stricter than the Java parser's unreserved selector rule:

```text
selector       = segment, { ".", segment };
segment        = selector-start, { selector-part };
selector-start = ASCII letter | "_";
selector-part  = ASCII letter | digit | "_" | "-";
```

The stricter default helps adapter packages avoid ambiguous field names. Dotted selectors can be disabled with `RsqlParseOptions.AllowDottedSelectors`.

## Values

Values can be unquoted or quoted with single or double quotes:

```text
status==active
status=="active"
title=='SUP board'
```

Quoted strings support backslash escaping:

```text
title=="SUP\" board"
title=='SUP\' board'
```

The parser classifies values as:

| Kind | Examples |
| --- | --- |
| `String` | `active`, `"SUP board"` |
| `Number` | `10`, `10.5` |
| `Boolean` | `true`, `false` |
| `Null` | `null` |

`RsqlValue.Text` contains normalized value text. `RsqlValue.RawText` contains the exact value text from the source expression, including quotes and escape characters for quoted strings.

Wildcard characters are preserved as string text by the core parser:

```text
actor==*Bale
director==Que*Tarantino
title=="SUP*"
```

The parser does not decide whether `*` means starts-with, contains, SQL `LIKE`, or something else. That behavior belongs in adapters.

Date and date-time values are also intentionally not coerced by the core parser. Values such as `2026-01-01` and `2026-01-01T10:15:30Z` are preserved as string text. Type coercion belongs in adapters where the target field type is known.

# PMAppDeclare grammar: reference guide

PMAppDeclare is a grammar that defines a text language able to represent the logic described in Declare. It is used to **filter the traces** of a process-mining log and, optionally, to **extract** the pieces of trace that satisfy a pattern. A PMAppDeclare expression is evaluated on each trace and returns true or false (does this trace satisfy the pattern?). The expressions are evaluated by the PMAppDeclarativeEngine.

This guide covers all the supported syntax, with examples, and a final section with frequent errors. Everything stated about the behaviour of the engine was checked by running it.

*Translated into English from the Spanish reference guide of the project (`Declare_Gramatica.md`). The expressions and the activity names in the examples are unchanged; some activity names are in Spanish because they come from the example logs.*

---

## 1. Events and activities

An event is referenced by the **name of its activity**, always in single quotes:

```
'Triage'
'Alta Enfermeria'
```

Single quotes are mandatory if the name contains spaces or any character that is not a letter, a digit or `_`. It is good practice to always use them, even when the name is "simple".

The wildcard `?` stands for **exactly one event of any activity**:

```
('A' > ? > 'B')     # A, followed by exactly one arbitrary event, followed by B
```

A quoted name that **starts with `#`** is not an activity name but a regular expression (.NET) that is tested against the name of each activity. It is not anchored: use `^` at the beginning and `$` at the end to avoid partial names (`'#^Etapa'`, `'#^(Alta|Exitus)$'`).

---

## 2. Sequences: `>` and `>>`

Inside a sequence in parentheses, events are chained with two operators:

| Operator | Meaning |
|---|---|
| `A > B` | A immediately followed by B (no event in between) |
| `A >> B` | A followed by B at some later point (there may be other events in between) |

More than two events can be chained:

```
('Consulta' > 'ECO' >> 'Rehab')
```

A sequence alone, in parentheses, is the condition "this sequence exists in the trace" (at any point of the trace, not only at the beginning):

```
('Triage' >> 'Alta')          # the trace has a Triage followed at some point by an Alta
```

This is the way to say **"the case has A and later B"**. The operators of section 4 do *not* say that (see section 4).

---

## 3. Restrictions on a sequence or an event: `[...]`

After a sequence (or a single event) restrictions can be added in square brackets.

### 3.1 Restrictions on **trace variables** (no dot)

A **trace** variable is a datum of the whole trace (age of the patient, sex, length-of-stay category...). It is **always** written without a dot:

```
[EdadNum > 20]
[Sexo == 'F']
['Tiempo de estancia' == 'B. De 5 a 15 minutos']
```

> If the name of the variable has spaces, it goes in single quotes just like a value: `'Tiempo de estancia' == 'valor'`, never `Tiempo de estancia == 'valor'` without quotes.

### 3.2 Restrictions on **event variables** (with a dot)

An **event** variable is a datum that can vary from one event to another inside the same trace (for example a value measured in each activity). It is **always** written with a dot in front:

```
('Triage' >> 'Alta')[.Prioridad == 'Alta']
```

A restriction with a dot is applied to **all the events of the sequence** it accompanies. If the restriction stands alone, with no sequence in parentheses before it, it is evaluated only on the first event of the trace.

> **The dot is not interchangeable.** Using `.` on a trace variable (or not using it on an event variable) makes the restriction look for a field that does not exist where it looks for it, and **it does not work as expected: it can give an error or select no trace** (see section 8).

### 3.3 Comparison operators

```
== != > >= < <=
```

* `>`, `>=`, `<`, `<=` compare **as numbers**. The value can be written with or without quotes: `EdadNum > 50` and `EdadNum > '50'` are equivalent. A text variable whose value is a number is also compared as a number (`Codigo > 5` with the value `'10'` holds).
* `==` and `!=` are used with **categorical or text** variables, and the value goes in single quotes: `Sexo == 'F'`. If the value is a number stored as text, it also works without quotes (`Codigo == 3` is equivalent to `Codigo == '3'`).
* `==` and `!=` **also work with numeric variables**: the value is compared as a number (`EdadNum == 65` selects the traces with age 65; `Puntuacion == 2.5` too, with a decimal point).

### 3.4 Combining several restrictions inside `[...]`

```
[cond1, cond2]          # AND: "," and "&" are equivalent
[cond1 & cond2]          # AND
[cond1 | cond2]          # OR
[(cond1 | cond2) & cond3]   # parentheses can be nested, as in Boolean algebra
```

`&` and `,` bind more strongly than `|` (AND before OR), as usual, so `[A | B & C]` is equivalent to `[A | (B & C)]`. If in doubt, write explicit parentheses.

Real example:

```
('Ingreso' >> 'Exitus')[EdadNum > 65, Sexo == 'F']
```

### 3.5 Real time interval

```
[-Mh]       # duration LESS than M hours
[Nh-]       # duration MORE than N hours
[Nh-Mh]     # duration between N and M
```

Units: `d` (days), `h` (hours), `m` (minutes), `s` (seconds). Each end can have its own unit and units can be combined at one end, for example `[1h-2h]`, `[-400d1h]`, `[30m-2h]`.

**Which duration is measured:** the time from the **start of the first event** of the matched sequence to the **start of the last event** of that sequence. If the "sequence" is a single event, the duration of that event is measured (its end minus its start).

**The ends of `[-Mh]` and `[Nh-]` are strict**: `[-2h]` does not hold for a duration of exactly 2 hours, and neither does `[2h-]`. Avoid setting an end equal to a duration that can occur.

**The interval goes inside the same bracket** as the other restrictions, separated by a comma:

```
('Ingreso' >> 'Alta')[-2h]                          # less than 2 hours between admission and discharge
('Ingreso' >> 'Alta')[Sexo == 'F', 1h-2h]           # women, between 1 and 2 hours
('Ingreso' >> 'Alta')[Sexo == 'F', EdadNum > 65, 2h-]   # women older than 65, more than 2 hours
```

It is the correct way to filter by elapsed time, better than comparing with a categorical variable of time ranges.

> It is **not possible** to write two groups of brackets in a row (`(...)[Sexo == 'F'][2h-]`) nor to put an interval after parentheses that contain a compound expression with `&` or `|` (`(A & B)[-2h]`). Both are syntax errors. The interval only accompanies a sequence or an event.

### 3.6 Alias

```
[?a]
```

Gives a name to the sequence or event, so that it can be referenced when extracting pieces of trace (advanced use, typically combined with extraction tools outside the filter).

---

## 4. Classic Declare operators between two events

Outside a sequence in parentheses, two separate events can be related with the standard Declare operators. In each one, "A" is the event that triggers the condition and "B" the one that must occur.

| Operator | Name | Holds when... |
|---|---|---|
| `A ->* B` | Response | **every** A is followed by a B **at some later point** |
| `A ->+ B` | Alternate Response | **every** A is followed by a B **before the next A** |
| `A -> B` | Chain Response | **every** A is **immediately** followed by B |
| `B *<- A` | Precedence | **every** B has an A **at some earlier point** |
| `B +<- A` | Alternate Precedence | **every** B has an earlier A **with no other B between them** |
| `B <- A` | Chain Precedence | **every** B is **immediately** preceded by A |
| `A =>* B` | Succession | Response (`A ->* B`) and Precedence (`B *<- A`) hold at the same time |
| `A =>+ B` | Alternate Succession | Alternate Response and Alternate Precedence hold at the same time (A and B strictly alternate) |
| `A => B` | Chain Succession | Chain Response and Chain Precedence hold at the same time (A and B always appear as a consecutive pair) |

Each version is stricter than the previous one: Chain implies Alternate, which implies the unmodified form.

> ### Warning: these operators also hold if the triggering event does not appear
>
> The Response and Precedence types say "**every** A..." or "**every** B...". A trace **without any A** (for `A ->* B`) satisfies that condition *vacuously*. Therefore **`A ->* B` does NOT mean "the case has A and later B"**: it also returns the cases that never had A.
>
> * For "the case has A and later B" use the sequence `('A' >> 'B')`.
> * If you want an operator of this table **and also** that A occurs, combine them: `'A' & 'A' ->* 'B'`.
> * The Succession types (`=>`) do not hold if only one of the two events occurs (a trace with only B, or with only A, does not satisfy `A => B`), but they do hold if **neither occurs**.

### 4.1 Check with small traces

Each letter is an event (A, B or C) in the order given. `1` = the trace is kept, `0` = it is discarded. Results obtained by running the engine.

| Expression | `C` | `A` | `B` | `AB` | `AAB` | `ABB` | `ABA` | `ACB` | `ABAB` |
|---|---|---|---|---|---|---|---|---|---|
| `'A' ->* 'B'` | 1 | 0 | 1 | 1 | 1 | 1 | 0 | 1 | 1 |
| `'A' ->+ 'B'` | 1 | 0 | 1 | 1 | 0 | 1 | 0 | 1 | 1 |
| `'A' -> 'B'` | 1 | 0 | 1 | 1 | 0 | 1 | 0 | 0 | 1 |
| `'B' *<- 'A'` | 1 | 1 | 0 | 1 | 1 | 1 | 1 | 1 | 1 |
| `'B' +<- 'A'` | 1 | 1 | 0 | 1 | 1 | 0 | 1 | 1 | 1 |
| `'B' <- 'A'` | 1 | 1 | 0 | 1 | 1 | 0 | 1 | 0 | 1 |
| `'A' =>* 'B'` | 1 | 0 | 0 | 1 | 1 | 1 | 0 | 1 | 1 |
| `'A' =>+ 'B'` | 1 | 0 | 0 | 1 | 0 | 0 | 0 | 1 | 1 |
| `'A' => 'B'` | 1 | 0 | 0 | 1 | 0 | 0 | 0 | 0 | 1 |
| `('A' >> 'B')` (for comparison) | 0 | 0 | 0 | 1 | 1 | 1 | 1 | 1 | 1 |

Note the column `C` (no A and no B): all the operators of the table hold, and the sequence `('A' >> 'B')` does not.

```
'ECO' ->* 'Rehab'            # every ECO is followed by a Rehab later (a trace without ECO also holds)
'ECO' & 'ECO' ->* 'Rehab'    # there is an ECO and every ECO is followed by a Rehab later
'Rehab' *<- 'ECO'            # every Rehab has an ECO before it (a trace without Rehab also holds)
('ECO' >> 'Rehab')           # there is an ECO and, later, a Rehab
```

---

## 5. Existence operators

| Operator | Meaning |
|---|---|
| `!A` | Absence: A never occurs in the trace |
| `^A` | The **first event** of the trace is A (if the trace has no A, it does **not** hold) |
| `A$` | The **last event** of the trace is A (if the trace has no A, it does **not** hold) |

```
!'Exitus'           # traces where Exitus never occurs
^'Ingreso'          # traces whose first event is Ingreso
'Alta'$             # traces whose last event is Alta
```

> **Beware of artificial events.** Some log readers (for example the JSONL reader of PMApp) add to each trace an `@Start` event at the beginning and an `@End` event at the end. In a log loaded that way, the first event of **all** the traces is `@Start` and the last one is `@End`: `^'Ingreso'` and `'Alta'$` **never hold**, and the wildcard `?` can also match those two events. Check how your log is loaded before using `^` and `$`.

---

## 6. Combining expressions (compound filters)

```
Expr1 & Expr2     # AND: both hold
Expr1 | Expr2     # OR: at least one holds
!( Expr )         # negates the whole expression in parentheses
( Expr )          # grouping
```

**Important about `!`:** the `!` only negates the term that comes *immediately after it*: an event, a sequence, or whatever is inside the parenthesis that follows. It does not extend across a `&` or `|` without explicit parentheses:

```
!A & B        # means (not A) AND B
!(A & B)      # means NOT (A and B): the parenthesis is needed for this
```

### Example: "exclude those who had A or B" (De Morgan)

Asking that "the patients with voluntary discharge and those who died do not appear" is `NOT (A or B)`, which by De Morgan's laws is equivalent to `(NOT A) AND (NOT B)`. Both forms are valid and give the same result:

```
!(('Voluntary discharge') | ('Exitus'))
```
```
!('Voluntary discharge') & !('Exitus')
```

### Example combining negation, AND and a trace-variable restriction

```
(!('Ingreso' >> 'Traslado') & ('Ingreso' >> 'Alta')) & [EdadNum > 20]
```

It selects the traces of patients older than 20 in which there is an Ingreso followed by an Alta and there is **no** Ingreso followed by a Traslado. (Remember: a time interval cannot go after these compound parentheses; see section 3.5.)

---

## 7. Precedence summary (from highest to lowest priority)

1. Parentheses `( ... )`
2. `!` (negation, applied only to the immediate term)
3. `>` `>>` (chaining of events inside a sequence)
4. `,` / `&` (AND, both in `[...]` and between whole expressions)
5. `|` (OR)

If in doubt, use explicit parentheses: they never hurt and make the intention unambiguous for the reader as well as for the parser.

---

## 8. Common errors

| Error | Why it fails | Correction |
|---|---|---|
| `Attention 2 >> Exitus` | The activity name has a space and is not in quotes | `'Attention 2' >> 'Exitus'` |
| `[.EdadNum > 20]` when `EdadNum` is a **trace** variable | The dot looks for the field in each **event**, not in the trace; the restriction does not work (it can give an error or select no trace) | `[EdadNum > 20]` (no dot) |
| `Tiempo de estancia == 'X'` | The field name has spaces and is not in quotes | `'Tiempo de estancia' == 'X'` |
| `!A & B` expecting it to negate the whole `A & B` | `!` only negates the immediate term (see section 6) | `!(A & B)` |
| `'A' ->* 'B'` expecting "the case has A and later B" | It also holds if A does not appear (section 4) | `('A' >> 'B')`, or `'A' & 'A' ->* 'B'` |
| `('A' >> 'B')[Sexo == 'F'][2h-]` | Two groups of brackets in a row: syntax error | `('A' >> 'B')[Sexo == 'F', 2h-]` |
| `(A & B)[-2h]` | An interval cannot go after parentheses with a compound expression: syntax error | Put the interval on a sequence: `('A' >> 'B')[-2h]` |
| `^'A'` or `'A'$` on a log that adds `@Start` / `@End` | The first and the last event are the artificial ones (section 5) | Check how the log is loaded |
| Using a categorical variable of time ranges to express a duration | It is an approximation by ranges, less precise | Use a real time interval: `(sequence)[-2h]` |

---

## 9. Complete examples

```
# Traces where Arrival is followed at some point by Triage
('Arrival' >> 'Triage')

# Traces of women older than 65 with more than 2 hours between admission and discharge
('Ingreso' >> 'Alta')[Sexo == 'F', EdadNum > 65, 2h-]

# Traces with neither voluntary discharge nor exitus
!(('Voluntary discharge') | ('Exitus'))

# Traces where there is never a Consulta followed later by an ECO, and where there is an ECO
# followed later by a Rehab
!('Consulta' >> 'ECO') & ('ECO' >> 'Rehab')

# Traces where there is an ECO and every ECO is followed by a Rehab before the next ECO
'ECO' & 'ECO' ->+ 'Rehab'

# Extraction with alias: sequence Arrival->Triage with a restriction on sex,
# marked with the alias 'a'
('Arrival' > 'Triage')[Sexo == 'F', ?a]

# Combination of AND/OR with parentheses inside a restriction
('Ingreso' >> 'Alta')[(Sexo == 'F' | Sexo == 'M') & EdadNum > 18]

# An Ingreso, exactly one arbitrary event, and then an Alta
('Ingreso' > ? > 'Alta')
```

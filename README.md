# DeclarativeIPM

This repository contains the **source code of the DeclarativeIPM agent** and the **material needed to reproduce the experiment of the article**: the evaluation harness, the order banks (v2), the prompts and the documentation.

DeclarativeIPM is an LLM agent that turns one natural-language order into one operation on a process-mining event log (`filter`, `circuits`, `regions` or `milestones`). The operation is expressed with Declare models, validated once, and applied to the log. The agent never asks back: it has no second turn.

**Terminology.** *Declare* is the original language and its templates (van der Aalst et al.). **PMAppDeclare** is a grammar that defines a text language able to represent the logic described in Declare: a sentence is a declarative expression that identifies subtraces in each trace (operators `>` and `>>`, the wildcard `?`, `^` and `$`, activity names in quotes or as a regular expression, negation, restrictions in square brackets on trace variables and time intervals, and the Declare templates). **PMAppDeclarativeEngine** is the engine that receives PMAppDeclare sentences, evaluates them on the log, applies the functionality (it labels events in circuits, protected regions and milestones, or removes patients in the filter) and runs process discovery again to show the new model.

## Read this first: what you can and cannot do with this repository

The agent is part of the **SVELTA plugin of PMApp**. The code here lets you **inspect and test the agent's logic** (prompt, validation, dispatch), read the banks and the documentation, and adapt the ideas. You **cannot build a plugin from this repository alone**: the agent depends on PMApp/SVELTA (chains, `OllamaClient`, `PoliGptClient`, the `Apply*` chains) and on the **PMAppDeclarativeEngine**, none of which is included here. See `PMAPP_PLUGIN.md`.

For the same reason, **the experiment cannot be repeated end to end without that software**. In particular, without PMApp/SVELTA and the PMAppDeclarativeEngine you cannot:
- run the agent or the evaluation harness (the harness builds on the agent and the engine);
- validate a model answer (the syntax check uses the PMAppDeclarativeEngine's own PMAppDeclare grammar);
- score an answer (the harness executes the expression on the engine and compares the selected cases with the known answer of the bank);
- reproduce the engine times.

What you can do without them: read the 17 order banks with their four wordings and their known answers, read the exact prompts that were sent to the models, read the code, and check the settings of every run (`run_metadata/`).

## Contents

| Folder | Content |
|---|---|
| `agent/` | Agent code, including `DeclarativeIPMPrompt.cs` (prompt template) and the validation. |
| `harness/` | Evaluation harness (`Stage1.cs` runs the stages, `BankVerify.cs` scores, `ModelCaller.cs` calls Ollama/PoliGPT); `models-stage1.json` lists the models. |
| `bank_v2/` | The order banks of the study: one JSON per log (17), with the expected answer of every order (`count` and `case_hash` of the selected cases, computed on the 2 000-case version of the simulated logs and on the real size of the public ones). `ORDERS.md` lists the orders; `phrasings/` holds the four wordings (0 robotic, 1 natural, 2 polite, 3 ambiguous). |
| `prompts_v2/` | The exact system prompt (v2) sent to the model for each log, as used in stage 2. |
| `docs/` | `PMAppDeclare_grammar.md` (reference guide of the PMAppDeclare grammar), `logs-manifest.md` (logs with SHA-256). |
| `run_metadata/` | `run.json` of the stage-2 run of each log: settings (temperature 0, fixed seed, `num_ctx`), file hashes, model digests, server version, commit of the code. Local paths are replaced by placeholders. |

## Licenses

| License | Covers |
|---|---|
| **Apache-2.0** (`LICENSE`) | `agent/`, `harness/`, `prompts_v2/` |
| **CC BY 4.0** (`LICENSE-CC-BY-4.0.txt`, also `bank_v2/LICENSE`) | `bank_v2/` (order banks and wordings), `docs/`, `run_metadata/` |

Copyright 2026 Carlos Fernandez-Llatas. Details in `LICENSES.md`. The event logs are not covered (see below).

## What this repository does not contain

- **No results.** The outputs of the runs (calls, verdicts, reports) are not included; only the settings of each run (`run_metadata/`).
- **No logs.** The logs are identified by name and SHA-256 in `docs/logs-manifest.md`. The 5 simulated scenarios (2 000, 10 000, 50 000 and 200 000 cases) are listed there; their files are not included.
- **The 12 public logs** (BPIC 2012, 2017, 2019, 2020 ×5, Helpdesk, Hospital Billing, Road Traffic Fine, Sepsis Cases) must be downloaded from 4TU.ResearchData (https://data.4tu.nl) and **are not redistributed**. Each dataset must be cited with its DOI and authors. **The manifest does not list the DOI and authors yet**; they are to be added.
- **PMApp/SVELTA and the PMAppDeclarativeEngine** (see above).

## About the PMAppDeclarativeEngine

The experiments were run with a version of the PMAppDeclarativeEngine that carries **three small local fixes** (handling of the `?` wildcard in `>` and `>>` on an empty rest of trace, and numeric comparison of values). A different version of the engine can give different results on the expressions that depend on them; in a probe, 9 of 171 expression rows changed with the fixes. The commit of the engine used is recorded in each `run.json`.

## How a run is launched

(Only possible with PMApp/SVELTA and the engine built from source.)

`DeclarativeIpmEval --stage1 <bank_v2/LOG.json> <log.jsonl> <__ModelInfo> --stage 2 --phrasings 0,1,2,3 --models <model label> --ctx <N> [--resume "<folder>"]`

Quote paths with spaces. Results go to `<results root>/stage<N>/<log>/<yyyyMMdd-HHmmss>/`. Context per log: 16384 (most), 32768 (prepaid, international), 49152 (bpic2019, atencion_primaria), 81920 (travel_permit). Paths in `harness/Stage1.cs` (engine) and `harness/ResultsRoot.cs` are placeholders and must be adapted.

## Caveats

- `case_hash` and `count` are valid only for the log version they were computed on (2 000-case simulated logs, public logs). On the larger simulated logs only the times are meaningful.
- Model answers are not bit-exact reproducible across servers and versions, even with temperature 0 and a fixed seed; model digests and server version are in `run_metadata/`.

## How to cite

The article is to be presented at **PODS4H 2027** (workshop of ICPM 2027). The citation, DOI and BibTeX will be added **when the article is accepted**.

```
To be completed after acceptance.
```

# DeclarativeIPM as a PMApp plugin

The source code of the DeclarativeIPM agent, as it is used inside **PMApp** (as part of the SVELTA plugin), is included in this repository, in [`agent/`](agent/).

It is the same code that was evaluated in the study: the prompt in `agent/DeclarativeIPMPrompt.cs` is the v2 prompt whose per-log instances are in [`prompts_v2/`](prompts_v2/).

The agent code is released under the **Apache License 2.0** (see [`LICENSE`](LICENSE)); the question banks, wordings and documentation under CC BY 4.0 (see [`LICENSES.md`](LICENSES.md)).

## What is included

| File in `agent/` | Role |
|---|---|
| `DeclarativeIPMAgent.cs` | The chain: builds the prompt, makes one model call, validates the answer. |
| `DeclarativeIPMDispatch.cs` | Applies the validated operation (`filter`, `circuits`, `regions`, `milestones`) with PMApp's `Apply*` chains. |
| `DeclarativeIPMPrompt.cs` | System prompt (PMAppDeclare syntax, operations, output contract, examples). |
| `DeclarativeIPMContext.cs` | Description of the log's activities, trace variables and observed patterns given to the model. |
| `DeclarativeIPMValidation.cs` | Syntax, activity and value checks. |
| `DeclarativeIPMState.cs` | Reads the circuits, regions and milestones currently defined in the runner. |
| `IDeclarativeIPMLlm.cs`, `DeclarativeIPMOllama.cs`, `DeclarativeIPMPoliGpt.cs` | Model access (Ollama, PoliGPT) and its configuration. |
| `DeclarativeIPMFactory.cs` | Entry point: `Create()` returns the chain; `AsChainAgent()` registers it as an agent of the PMApp chat. |

## How it plugs into PMApp

The agent is a chain (`order → DeclarativeIPMAgent → DeclarativeIPMDispatch`) and registers in the chat's agent list:

```csharp
SVELTAHelper.ChainsInstalled.Add(DeclarativeIPMFactory.AsChainAgent());
```

Namespace: `PMAppSVELTA.DeclarativeIPM`.

## What is NOT included

This repository contains only the agent's files, not the rest of PMApp. The code depends on PMApp and the SVELTA plugin (the `pm4h.svelta` chains, `OllamaClient`, `PoliGptClient`, the `Apply*` chains and the PMAppDeclarativeEngine), which are not distributed here, so it **does not build on its own**. To use it, copy `agent/` into the SVELTA plugin project of a PMApp installation as the `DeclarativeIPM` folder and register the agent as shown above.

## Configuration

The model endpoint, model name and key are not in the code. They are read from `%APPDATA%\DeclarativeIPM\ollama.json` (or from the path in the environment variable `DECLARATIVEIPM_CONFIG`):

```json
{ "Endpoint": "http://server:port", "Model": "model-name", "ApiKey": "..." }
```

See the main [`README.md`](README.md) for the reproducibility package.

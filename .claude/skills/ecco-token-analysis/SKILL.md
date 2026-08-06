---
name: ecco-token-analysis
description: Analyze Hugging Face language-model token predictions, saliency, hidden states, attention, and activations with the local Ecco repository. Use when asked to inspect why a model produced a token, compare token rankings across layers, visualize attribution, or run Ecco notebooks/scripts from D:/Tinh/Test_tools/Save_token.
---

# Ecco token analysis

Use the local source repository at `D:/Tinh/Test_tools/Save_token`. Keep its Python dependencies isolated in `.venv`; never install them into the Unity project or global Python.

## Workflow

1. Run `powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/doctor.ps1` before analysis.
2. If the environment is absent or unhealthy, run `powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/setup.ps1` once.
3. Prefer a small Python script or notebook that imports `ecco`; the bundled CLI only prints arguments and is not an analysis interface.
4. Start with `gpu=False` unless CUDA availability was explicitly verified.
5. Save generated notebooks/results outside Unity `Assets/` unless the user requests otherwise.
6. Report the model ID, prompt, Ecco options, device, and output paths.

## Python entry point

Use:

```powershell
& "D:/Tinh/Test_tools/Save_token/.venv/Scripts/python.exe" script.py
```

Minimal API:

```python
import ecco
lm = ecco.from_pretrained(
    "distilgpt2",
    activations=False,
    attention=False,
    hidden_states=True,
    gpu=False,
    verbose=False,
)
output = lm.generate("The capital of France is", generate=1)
```

Read `references/usage.md` for supported analyses and compatibility constraints.

## Guardrails

- Do not claim this tool reduces API token usage. It explains local Hugging Face model behavior.
- Do not point Ecco at Claude, Codex, or another hosted model unless model weights and hooks are locally available.
- Model downloads may be large; state the model before downloading it.
- Treat failures involving modern Transformers APIs as version compatibility issues before patching Ecco source.

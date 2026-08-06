---
description: Phân tích token prediction, saliency, attribution, attention và activation bằng Ecco local.
argument-hint: <model Hugging Face> <prompt hoặc yêu cầu phân tích>
---

Use the `ecco-token-analysis` skill for this request.

User arguments:

$ARGUMENTS

Before running analysis:

1. Read `.claude/skills/ecco-token-analysis/SKILL.md` completely.
2. Run its `scripts/doctor.ps1` command using `powershell.exe -NoProfile -ExecutionPolicy Bypass`.
3. If the environment is unhealthy, run `scripts/setup.ps1` using the same PowerShell flags.
4. State the Hugging Face model before downloading weights.
5. Default to CPU (`gpu=False`) unless CUDA has been verified.
6. Report model, prompt, analysis options, device, results, and output paths.

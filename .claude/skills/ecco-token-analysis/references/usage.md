# Local Ecco integration

- Source: `D:/Tinh/Test_tools/Save_token`
- Package: `ecco==0.1.2`
- Environment: `D:/Tinh/Test_tools/Save_token/.venv`
- Intended runtime: Python 3.11

Useful methods on Ecco outputs include token prediction display, saliency/primary attribution, hidden-state layer predictions, token rankings, attention, and NMF activation exploration. Ecco runs local Hugging Face models and is primarily designed for Jupyter notebooks.

The repository's `ecco` console command is only a stub. Use Python imports or the included notebooks.

Compatibility note: this is an older codebase. Keep Transformers at 4.30.2 and Matplotlib below 3.9; newer releases remove APIs used by Ecco 0.1.2. Use the environment versions recorded by `scripts/doctor.ps1`; avoid casually upgrading dependencies.

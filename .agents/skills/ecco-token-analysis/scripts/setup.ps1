$ErrorActionPreference = 'Stop'

$repo = 'D:\Tinh\Test_tools\Save_token'
$venv = Join-Path $repo '.venv'
$python311 = 'C:\Users\gifft\AppData\Roaming\uv\python\cpython-3.11.15-windows-x86_64-none\python.exe'

if (-not (Test-Path -LiteralPath $repo)) {
    throw "Ecco source repository not found: $repo"
}

if (-not (Get-Command uv -ErrorAction SilentlyContinue)) {
    throw 'uv is required but was not found on PATH.'
}

if (-not (Test-Path -LiteralPath $python311)) {
    uv python install 3.11
}

if (-not (Test-Path -LiteralPath (Join-Path $venv 'Scripts\python.exe'))) {
    uv venv --python 3.11 $venv
}

$python = Join-Path $venv 'Scripts\python.exe'

# Ecco 0.1.2 predates several breaking Transformers changes. Pin a compatible
# 4.x stack while retaining a current CPU-capable PyTorch wheel on Python 3.11.
uv pip install --python $python `
    'torch>=2.1,<3' `
    'transformers==4.30.2' `
    'tokenizers<0.14,>=0.11.1' `
    'numpy<2' `
    'scikit-learn<2' `
    'seaborn>=0.11' `
    'PyYAML>=6' `
    'captum>=0.6,<0.8' `
    'ipython>=8' `
    'matplotlib>=3.7,<3.9'

uv pip install --python $python --no-deps --editable $repo

& $python -c "import ecco, torch, transformers; print('ecco', ecco.__version__); print('torch', torch.__version__); print('transformers', transformers.__version__)"
if ($LASTEXITCODE -ne 0) {
    throw "Ecco import smoke test failed with exit code $LASTEXITCODE"
}

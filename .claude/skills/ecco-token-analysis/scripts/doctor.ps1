$ErrorActionPreference = 'Stop'

$repo = 'D:\Tinh\Test_tools\Save_token'
$python = Join-Path $repo '.venv\Scripts\python.exe'

if (-not (Test-Path -LiteralPath $python)) {
    Write-Error "Ecco environment is missing. Run setup.ps1 first: $python"
}

& $python -c @'
import sys
import ecco
import torch
import transformers
print('python', sys.version.split()[0])
print('ecco', ecco.__version__)
print('torch', torch.__version__)
print('transformers', transformers.__version__)
print('cuda_available', torch.cuda.is_available())
print('source', ecco.__file__)
'@

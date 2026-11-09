# build_all.ps1 - Windows: build every project against your INSTALLED Inventor 2026 / Plant 3D SDK.
#   pwsh -File tools\build_all.ps1 [-InventorBin "C:\Program Files\Autodesk\Inventor 2026\Bin"] [-PlantSdkInc "C:\...\inc-x64"]
param(
  [string]$InventorBin = "C:\Program Files\Autodesk\Inventor 2026\Bin",
  [string]$PlantSdkInc = ""
)
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root
$fail = 0
$projects = Get-ChildItem code\inventor\csharp\0*, code\inventor\vbnet\*, code\inventor\design-automation\*, code\autocad\csharp\* -Directory
foreach ($p in $projects) {
  "== $($p.Name)"
  dotnet build $p.FullName -nologo -v q "-p:InventorBin=$InventorBin" -p:SkipDeploy=true
  if ($LASTEXITCODE) { $fail = 1 }
}
$plantArgs = if ($PlantSdkInc) { "-p:PlantSdkInc=$PlantSdkInc" } else { "-p:PlantStubs=true" }
dotnet build code\plant3d\csharp\Plant3DDataTools -nologo -v q $plantArgs; if ($LASTEXITCODE) { $fail = 1 }
python tools\check_ilogic.py --inventor-bin $InventorBin; if ($LASTEXITCODE) { $fail = 1 }
python tools\test_tools.py; if ($LASTEXITCODE) { $fail = 1 }
python tools\lisp_check.py; if ($LASTEXITCODE) { $fail = 1 }
pwsh -NoProfile -File tools\check_powershell.ps1; if ($LASTEXITCODE) { $fail = 1 }
python tools\check_links.py; if ($LASTEXITCODE) { $fail = 1 }
exit $fail

<#
.SYNOPSIS
    Unity 4.6.8 batch build wrapper for multi-platform builds.
.DESCRIPTION
    Wraps Unity.exe -batchmode calls for one or more platforms,
    checks log output for errors, and archives build artifacts.
.PARAMETER UnityPath
    Full path to Unity.exe (e.g., "C:\Program Files (x86)\Unity\Editor\Unity.exe")
.PARAMETER ProjectPath
    Full path to the Unity project directory.
.PARAMETER ExecuteMethod
    Static method to invoke (e.g., "BuildScript.BuildAll").
.PARAMETER Targets
    Array of build targets (e.g., @("StandaloneWindows", "Android")).
    If omitted, ExecuteMethod is called once without target override.
.PARAMETER OutputDir
    Root output directory for log files and build artifacts.
    Defaults to "$ProjectPath\Builds".
.EXAMPLE
    .\unity_batch_build.ps1 -UnityPath "C:\Unity\Editor\Unity.exe" -ProjectPath "C:\MyProject" -ExecuteMethod "Builder.DoBuild"
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$UnityPath,

    [Parameter(Mandatory=$true)]
    [string]$ProjectPath,

    [Parameter(Mandatory=$true)]
    [string]$ExecuteMethod,

    [string[]]$Targets,

    [string]$OutputDir
)

$ErrorActionPreference = "Stop"

if (-not $OutputDir) {
    $OutputDir = Join-Path $ProjectPath "Builds"
}

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$logDir = Join-Path $OutputDir "logs"
New-Item -ItemType Directory -Force -Path $logDir | Out-Null

$overallSuccess = $true

if (-not $Targets) {
    $Targets = @("")  # single build with no target override
}

foreach ($target in $Targets) {
    $logFile = if ($target) {
        Join-Path $logDir "build_${target}_$timestamp.log"
    } else {
        Join-Path $logDir "build_$timestamp.log"
    }

    $args = @(
        "-batchmode",
        "-quit",
        "-projectPath", "`"$ProjectPath`"",
        "-executeMethod", $ExecuteMethod,
        "-logFile", "`"$logFile`""
    )

    if ($target) {
        $args += "-buildTarget", $target
    }

    Write-Host "[$(Get-Date -Format 'HH:mm:ss')] Building: $target" -ForegroundColor Cyan
    Write-Host "  Log: $logFile"

    $proc = Start-Process -FilePath $UnityPath -ArgumentList $args -Wait -NoNewWindow -PassThru

    # Unity 4.x always returns 0; check log manually
    $errors = Select-String -Path $logFile -Pattern "fatal error|Unhandled Exception|error CS\d|Exiting batchmode with fatal error" -CaseSensitive -Quiet

    if ($errors) {
        Write-Host "  [FAIL] Build error detected in log: $target" -ForegroundColor Red
        Get-Content $logFile | Select-String "error|exception|fatal" -CaseSensitive | ForEach-Object {
            Write-Host "    $_" -ForegroundColor Red
        }
        $overallSuccess = $false
    } else {
        Write-Host "  [OK] Build completed: $target" -ForegroundColor Green
    }
}

if (-not $overallSuccess) {
    Write-Host "One or more builds failed. Check logs in: $logDir" -ForegroundColor Red
    exit 1
}

Write-Host "All builds succeeded." -ForegroundColor Green
exit 0

[CmdletBinding()]
param(
    [string]$ResourceGroup = "nakama-dev",
    [string]$AppName = "nakama-appservice",
    [string]$Configuration = "Release",
    [switch]$SkipConfirmation
)

$ErrorActionPreference = "Stop"

function Invoke-Step([string]$Message, [scriptblock]$Action) {
    Write-Host "==> $Message" -ForegroundColor Cyan
    & $Action
    if ($LASTEXITCODE -ne 0) {
        throw "Falló el paso: $Message (código $LASTEXITCODE)"
    }
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repoRoot "apps/backend/api/Api.csproj"
$workDir = Join-Path ([System.IO.Path]::GetTempPath()) "nakama-deploy"
$publishDir = Join-Path $workDir "publish"
$zipPath = Join-Path $workDir "publish.zip"

$account = az account show --query "{name:name, id:id}" -o json 2>$null | ConvertFrom-Json
if (-not $account) {
    throw "No hay sesión en Azure. Ejecuta 'az login' y selecciona la suscripción."
}

Write-Host "Suscripción:    $($account.name) ($($account.id))"
Write-Host "Grupo:          $ResourceGroup"
Write-Host "App Service:    $AppName"
Write-Host "Configuración:  $Configuration"

if (-not $SkipConfirmation) {
    $answer = Read-Host "¿Desplegar? (s/N)"
    if ($answer -notmatch "^[sSyY]") {
        Write-Host "Cancelado."
        return
    }
}

if (Test-Path $workDir) {
    Remove-Item $workDir -Recurse -Force
}
New-Item -ItemType Directory -Path $publishDir | Out-Null

Invoke-Step "Publicando API y frontend" {
    dotnet publish $project -c $Configuration -o $publishDir
}

Write-Host "==> Comprimiendo" -ForegroundColor Cyan
Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $zipPath -Force

Invoke-Step "Subiendo a Azure" {
    az webapp deploy --resource-group $ResourceGroup --name $AppName --src-path $zipPath --type zip
}

$healthUrl = "https://$AppName.azurewebsites.net/health"
Write-Host "==> Comprobando $healthUrl" -ForegroundColor Cyan
$healthy = $false
for ($attempt = 1; $attempt -le 12 -and -not $healthy; $attempt++) {
    try {
        $response = Invoke-WebRequest -Uri $healthUrl -UseBasicParsing -TimeoutSec 20
        $healthy = $response.StatusCode -eq 200
    }
    catch {
        Start-Sleep -Seconds 10
    }
}

Remove-Item $workDir -Recurse -Force

if ($healthy) {
    Write-Host "Despliegue correcto: https://$AppName.azurewebsites.net" -ForegroundColor Green
}
else {
    Write-Warning "La app no respondió 200 en /health. Revisa el flujo de registro en el portal."
    exit 1
}

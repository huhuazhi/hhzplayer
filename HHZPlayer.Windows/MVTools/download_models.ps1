<#
PowerShell helper to download model files listed in models_needed_report.txt
Usage:
  1) Edit models_urls.json to add direct URLs for each model, or run script and paste URLs when prompted.
  2) Run in MVTools folder: .\download_models.ps1

Notes:
- This script does not redistribute any third-party models; you must ensure you have rights to download and use them.
- Use reliable direct download URLs (raw GitHub user content, releases, or CDN links).
#>

$cwd = Get-Location
$modelsDir = Join-Path $cwd 'models'
if (-not (Test-Path $modelsDir)) { New-Item -ItemType Directory -Path $modelsDir | Out-Null }

$report = Join-Path $cwd 'models_needed_report.txt'
$urlsFile = Join-Path $cwd 'models_urls.json'

if (-not (Test-Path $report)) { Write-Error "�Ҳ��� models_needed_report.txt���������� scan_needed_models.ps1"; exit 1 }

# ��ȡ�����б�
$needed = @()
Get-Content $report | ForEach-Object {
    if ($_ -and $_ -notmatch '^#') {
        $parts = $_ -split '->'
        if ($parts.Length -ge 1) {
            $name = $parts[0].Trim()
            if ($name) { $needed += $name }
        }
    }
}

# ��ȡ�򴴽� URL ӳ��
if (Test-Path $urlsFile) {
    try { $mapping = Get-Content $urlsFile -Raw | ConvertFrom-Json } catch { $mapping = @{} }
} else {
    $mapping = @{}
}

foreach ($model in $needed) {
    Write-Host "\n=== ����: $model ==="
    $targetPath = Join-Path $modelsDir $model
    if (Test-Path $targetPath) { Write-Host "�Ѵ���: $targetPath������"; continue }

    $url = $null
    if ($mapping.PSObject.Properties.Name -contains $model) { $url = $mapping.$model }

    if (-not $url) {
        Write-Host "δ�� models_urls.json ���ҵ� $model ���������ӡ����ṩ��ģ�͵�ֱ������ URL����������������"
        $url = Read-Host "������ $model �� URL"
        if ([string]::IsNullOrWhiteSpace($url)) { Write-Warning "���� $model"; continue }
        # ����ӳ��
        $mapping | Add-Member -NotePropertyName $model -NotePropertyValue $url -Force
        ($mapping | ConvertTo-Json -Depth 5) | Out-File -FilePath $urlsFile -Encoding UTF8
    }

    Write-Host "�� $url ���ص� $targetPath ..."
    try {
        Invoke-WebRequest -Uri $url -OutFile $targetPath -UseBasicParsing -TimeoutSec 600
        Write-Host "�������: $targetPath"
    }
    catch {
        Write-Warning "����ʧ��: $($_.Exception.Message)"
        # �������ʧ�ܣ�ɾ�����ܵĲ����ļ�
        if (Test-Path $targetPath) { Remove-Item $targetPath -Force }
    }
}

Write-Host "������������ɡ����� $modelsDir �е��ļ���ȷ�ϡ�"
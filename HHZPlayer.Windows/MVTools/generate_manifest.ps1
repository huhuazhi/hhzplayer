# ���� MVTools �嵥��scripts / plugins / models / ���󱨸�
# �� MVTools Ŀ¼���У� .\generate_manifest.ps1

$cwd = Get-Location
$scriptsDir = Join-Path $cwd 'scripts'
$modelsDir = Join-Path $cwd 'models'
$pluginsDir = Join-Path $cwd 'plugins'
$report = Join-Path $cwd 'models_needed_report.txt'

$manifest = [ordered]@{
    generated = (Get-Date).ToString('u')
    scripts = @()
    plugins = @()
    models = @()
    models_needed = @()
}

if (Test-Path $scriptsDir) {
    Get-ChildItem -Path $scriptsDir -File -Recurse | ForEach-Object { $manifest.scripts += $_.Name }
}
if (Test-Path $pluginsDir) {
    Get-ChildItem -Path $pluginsDir -File -Recurse | ForEach-Object { $manifest.plugins += $_.Name }
}
if (Test-Path $modelsDir) {
    Get-ChildItem -Path $modelsDir -File -Recurse | ForEach-Object { $manifest.models += $_.Name }
}
if (Test-Path $report) {
    $lines = Get-Content $report | Where-Object { $_ -and $_ -notmatch '^#' }
    foreach ($l in $lines) {
        $parts = $l -split '->'
        if ($parts.Length -ge 1) {
            $name = $parts[0].Trim()
            $manifest.models_needed += $name
        }
    }
}

$out = Join-Path $cwd 'manifest.json'
$manifest | ConvertTo-Json -Depth 5 | Out-File -FilePath $out -Encoding UTF8
Write-Host "�������嵥: $out" 
Write-Host "�ű�����: $($manifest.scripts.Count), �������: $($manifest.plugins.Count), ģ�ͼ���: $($manifest.models.Count), ����ģ��: $($manifest.models_needed.Count)"
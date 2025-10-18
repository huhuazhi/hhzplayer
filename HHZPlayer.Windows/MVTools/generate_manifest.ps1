# 生成 MVTools 清单：scripts / plugins / models / 需求报告
# 在 MVTools 目录运行： .\generate_manifest.ps1

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
Write-Host "已生成清单: $out" 
Write-Host "脚本计数: $($manifest.scripts.Count), 插件计数: $($manifest.plugins.Count), 模型计数: $($manifest.models.Count), 需求模型: $($manifest.models_needed.Count)"
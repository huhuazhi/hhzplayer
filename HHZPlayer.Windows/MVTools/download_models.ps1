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

if (-not (Test-Path $report)) { Write-Error "找不到 models_needed_report.txt，请先运行 scan_needed_models.ps1"; exit 1 }

# 读取需求列表
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

# 读取或创建 URL 映射
if (Test-Path $urlsFile) {
    try { $mapping = Get-Content $urlsFile -Raw | ConvertFrom-Json } catch { $mapping = @{} }
} else {
    $mapping = @{}
}

foreach ($model in $needed) {
    Write-Host "\n=== 处理: $model ==="
    $targetPath = Join-Path $modelsDir $model
    if (Test-Path $targetPath) { Write-Host "已存在: $targetPath，跳过"; continue }

    $url = $null
    if ($mapping.PSObject.Properties.Name -contains $model) { $url = $mapping.$model }

    if (-not $url) {
        Write-Host "未在 models_urls.json 中找到 $model 的下载链接。请提供该模型的直接下载 URL（或留空跳过）："
        $url = Read-Host "请输入 $model 的 URL"
        if ([string]::IsNullOrWhiteSpace($url)) { Write-Warning "跳过 $model"; continue }
        # 保存映射
        $mapping | Add-Member -NotePropertyName $model -NotePropertyValue $url -Force
        ($mapping | ConvertTo-Json -Depth 5) | Out-File -FilePath $urlsFile -Encoding UTF8
    }

    Write-Host "从 $url 下载到 $targetPath ..."
    try {
        Invoke-WebRequest -Uri $url -OutFile $targetPath -UseBasicParsing -TimeoutSec 600
        Write-Host "下载完成: $targetPath"
    }
    catch {
        Write-Warning "下载失败: $($_.Exception.Message)"
        # 如果出现失败，删除可能的部分文件
        if (Test-Path $targetPath) { Remove-Item $targetPath -Force }
    }
}

Write-Host "所有任务尝试完成。请检查 $modelsDir 中的文件并确认。"
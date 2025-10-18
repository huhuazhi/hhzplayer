Param(
    [string]$RepoUrl = "https://github.com/hooke007/mpv_PlayKit",
    [string]$Branch = "master",
    [string]$TempDir = "$env:TEMP\mpv_PlayKit_extract"
)

Write-Host "准备从 $RepoUrl 提取文件到 $(Get-Location)\scripts, models, plugins..."

if (Test-Path $TempDir) { Remove-Item -Recurse -Force $TempDir }

# 先尝试按给定分支克隆，如果失败则退回到默认分支克隆
$cloneSuccess = $false
if ($Branch -and $Branch.Trim() -ne "") {
    Write-Host "尝试使用分支 '$Branch' 克隆..."
    git clone --depth 1 --branch $Branch $RepoUrl $TempDir
    if ($LASTEXITCODE -eq 0) { $cloneSuccess = $true }
    else { Write-Warning "按分支克隆失败，尝试使用仓库默认分支克 clone..." }
}

if (-not $cloneSuccess) {
    git clone --depth 1 $RepoUrl $TempDir
    if ($LASTEXITCODE -ne 0) { Write-Error "git clone 失败，请检查网络与 Git 可用性。"; exit 1 }
}

# 创建目标子目录
$cwd = Get-Location
$scriptsDir = Join-Path $cwd "scripts"
$modelsDir = Join-Path $cwd "models"
$pluginsDir = Join-Path $cwd "plugins"
New-Item -ItemType Directory -Force -Path $scriptsDir, $modelsDir, $pluginsDir | Out-Null

# 查找 .vpy 脚本
Get-ChildItem -Path $TempDir -Recurse -Include *.vpy -ErrorAction SilentlyContinue | ForEach-Object {
    Copy-Item $_.FullName -Destination $scriptsDir -Force
}

# 查找包含 mvtools 字样的脚本/文本文件
Get-ChildItem -Path $TempDir -Recurse -File -ErrorAction SilentlyContinue | Where-Object {
    try { (Get-Content $_.FullName -ErrorAction Stop) -match "mvtools" } catch { $false }
} | ForEach-Object {
    Copy-Item $_.FullName -Destination $scriptsDir -Force
}

# 常见模型扩展名
$exts = @('*.pth','*.pt','*.onnx','*.t7','*.h5','*.bin')
foreach ($e in $exts) {
    Get-ChildItem -Path $TempDir -Recurse -Include $e -ErrorAction SilentlyContinue | ForEach-Object { Copy-Item $_.FullName -Destination $modelsDir -Force }
}

# 查找常见插件 / dll / pyd
Get-ChildItem -Path $TempDir -Recurse -Include *.dll,*.pyd,*.so -ErrorAction SilentlyContinue | Where-Object { $_.DirectoryName -notmatch "\\.git" } | ForEach-Object {
    Copy-Item $_.FullName -Destination $pluginsDir -Force
}

Write-Host "提取完成。请手动检查 $scriptsDir, $modelsDir, $pluginsDir 中的文件并确认许可证与合规性。"
Write-Host "临时目录: $TempDir（如果不需要可删除）"
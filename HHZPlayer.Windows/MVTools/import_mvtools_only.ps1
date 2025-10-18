Param(
    [string]$RepoUrl = "https://github.com/hooke007/mpv_PlayKit",
    [string]$Branch = "",
    [string]$TempDir = "$env:TEMP\mpv_PlayKit_extract_mvtools"
)

Write-Host "准备从 $RepoUrl 提取仅与 MVTools/补帧/RIFE 相关的文件到 $(Get-Location)\scripts, models, plugins..."

if (Test-Path $TempDir) { Remove-Item -Recurse -Force $TempDir }

$cloneSuccess = $false
if ($Branch -and $Branch.Trim() -ne "") {
    Write-Host "尝试使用分支 '$Branch' 克隆..."
    git clone --depth 1 --branch $Branch $RepoUrl $TempDir
    if ($LASTEXITCODE -eq 0) { $cloneSuccess = $true }
    else { Write-Warning "按分支克隆失败，尝试使用仓库默认分支克隆..." }
}

if (-not $cloneSuccess) {
    git clone --depth 1 $RepoUrl $TempDir
    if ($LASTEXITCODE -ne 0) { Write-Error "git clone 失败，请检查网络与 Git 可用性。"; exit 1 }
}

# 目标目录
$cwd = Get-Location
$scriptsDir = Join-Path $cwd "scripts"
$modelsDir = Join-Path $cwd "models"
$pluginsDir = Join-Path $cwd "plugins"
New-Item -ItemType Directory -Force -Path $scriptsDir, $modelsDir, $pluginsDir | Out-Null

Write-Host "扫描临时仓库中与 mvtools / MVT / RIFE 相关的文件..."

# 1) 优先拷贝 portable_config/vs 下的脚本（mpv-lazy 的约定位置）
$portableVs = Join-Path $TempDir "portable_config\vs"
if (Test-Path $portableVs) {
    Get-ChildItem -Path $portableVs -File -ErrorAction SilentlyContinue | ForEach-Object {
        Copy-Item $_.FullName -Destination $scriptsDir -Force
    }
}

# 2) 拷贝仓库中显式命名为 MEMC_* / MIX_* / RIFE* / MVT* 的 vpy/py 文件
$patterns = @('MEMC_*','MIX_*','*RIFE*','*mvt*')
foreach ($p in $patterns) {
    Get-ChildItem -Path $TempDir -Recurse -Include "$p.vpy","$p.py" -ErrorAction SilentlyContinue | ForEach-Object {
        Copy-Item $_.FullName -Destination $scriptsDir -Force
    }
}

# 3) 拷贝任意 .vpy/.py 文件中包含关键字 mvtools 或 mvtools2 或 rife 的脚本
Get-ChildItem -Path $TempDir -Recurse -Include *.vpy,*.py -ErrorAction SilentlyContinue | ForEach-Object {
    try { $content = Get-Content $_.FullName -Raw -ErrorAction Stop } catch { return }
    if ($content -match '(?i)mvtools' -or $content -match '(?i)mvt' -or $content -match '(?i)rife') {
        Copy-Item $_.FullName -Destination $scriptsDir -Force
    }
}

# 4) 如果仓库包含 models/ 或 weights/ 或 portable_config/models 等目录，尽量拷贝其中文件到 modelsDir
$maybeModelDirs = @('models','weights','portable_config\models','portable_config\weights')
foreach ($d in $maybeModelDirs) {
    $full = Join-Path $TempDir $d
    if (Test-Path $full) {
        Get-ChildItem -Path $full -File -Recurse -ErrorAction SilentlyContinue | ForEach-Object {
            $ext = $_.Extension.ToLower()
            if ($ext -in '.onnx','.pth','.pt','.t7','.h5','.bin') {
                Copy-Item $_.FullName -Destination $modelsDir -Force
            }
        }
    }
}

# 5) 如果存在编译好的插件或 DLL（通常不在该仓库），也尽量拷贝（仅作为备份），但请注意许可
Get-ChildItem -Path $TempDir -Recurse -Include *.dll,*.pyd,*.so -ErrorAction SilentlyContinue | Where-Object { $_.DirectoryName -notmatch "\\.git" } | ForEach-Object {
    Copy-Item $_.FullName -Destination $pluginsDir -Force
}

# 6) 扫描脚本中引用的模型文件名并生成报告，若模型已拷贝则标注为已存在
$regex = [regex]::new('([\w\-/\\:.]+\.(onnx|pth|pt|t7|h5|bin))', 'IgnoreCase')
$foundModels = @{}
Get-ChildItem -Path $scriptsDir -Recurse -Include *.vpy,*.py -ErrorAction SilentlyContinue | ForEach-Object {
    try { $content = Get-Content $_.FullName -Raw -ErrorAction Stop } catch { return }
    foreach ($m in $regex.Matches($content)) {
        $match = $m.Groups[1].Value
        if (-not $foundModels.ContainsKey($match)) { $foundModels[$match] = @() }
        $foundModels[$match] += $_.FullName
    }
}

$outReport = Join-Path $cwd 'models_needed_report.txt'
"# models_needed_report generated: $(Get-Date)`n" | Out-File -FilePath $outReport -Encoding UTF8
foreach ($k in $foundModels.Keys) {
    $files = ($foundModels[$k] -join ', ')
    # 获取模型名称并检查是否已拷贝
    $modelName = [System.IO.Path]::GetFileName($k)
    $exists = Test-Path (Join-Path $modelsDir $modelName)
    if ($exists) {
        "$modelName -> AVAILABLE (copied) referenced by: $files" | Out-File -FilePath $outReport -Append -Encoding UTF8
    } else {
        "$modelName -> MISSING referenced by: $files" | Out-File -FilePath $outReport -Append -Encoding UTF8
    }
}

if ($foundModels.Count -eq 0) {
    Write-Host "未在脚本中发现明确的模型引用。若脚本运行时动态下载或使用相对路径，请手动核对。"
} else {
    Write-Host "模型引用扫描完成，报告: $outReport"
}

Write-Host "提取完成。请检查 $scriptsDir, $modelsDir, $pluginsDir 中的文件并确认许可证与合规性。"
Write-Host "临时目录: $TempDir（如果不需要可删除）"
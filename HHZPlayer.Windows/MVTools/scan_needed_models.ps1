# 扫描 MVTools/scripts 目录中可能引用的模型文件名
# 使用方法：在 MVTools 目录中运行：
#   .\scan_needed_models.ps1

$scriptDir = Join-Path (Get-Location) 'scripts'
if (-not (Test-Path $scriptDir)) { Write-Error "未找到 scripts 目录: $scriptDir"; exit 1 }

$extPatterns = @('\.pth', '\.pt', '\.onnx', '\.t7', '\.h5', '\.bin', '\.nn', '\.ncnnmodel')
$regex = [regex]::new('([\w\-/\\:.]+\.(pth|pt|onnx|t7|h5|bin|nn|ncnnmodel))', 'IgnoreCase')

$results = @{}

Get-ChildItem -Path $scriptDir -Filter *.vpy -Recurse | ForEach-Object {
    $path = $_.FullName
    try {
        $content = Get-Content $path -Raw -ErrorAction Stop
    } catch {
        return
    }
    foreach ($m in $regex.Matches($content)) {
        $match = $m.Groups[1].Value
        if (-not $results.ContainsKey($match)) {
            $results[$match] = @()
        }
        $results[$match] += $_.Name
    }
}

if ($results.Count -eq 0) {
    Write-Host "未在 scripts/*.vpy 中明显找到模型文件引用。部分脚本可能在运行时动态从配置或网络加载模型。"
    exit 0
}

Write-Host "在 scripts 中发现可能需要的模型文件（文件名 -> 脚本列表）：`n"
foreach ($k in $results.Keys) {
    $files = ($results[$k] -join ', ')
    Write-Host "$k  ->  $files"
}

# 输出为本地文件，便于后续下载或人工核对
$outFile = Join-Path (Get-Location) 'models_needed_report.txt'
"# models_needed_report generated: $(Get-Date)`n" | Out-File -FilePath $outFile -Encoding UTF8
foreach ($k in $results.Keys) {
    $files = ($results[$k] -join ', ')
    "$k -> $files" | Out-File -FilePath $outFile -Append -Encoding UTF8
}

Write-Host "报告已写入: $outFile"
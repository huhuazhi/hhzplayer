# ɨ�� MVTools/scripts Ŀ¼�п������õ�ģ���ļ���
# ʹ�÷������� MVTools Ŀ¼�����У�
#   .\scan_needed_models.ps1

$scriptDir = Join-Path (Get-Location) 'scripts'
if (-not (Test-Path $scriptDir)) { Write-Error "δ�ҵ� scripts Ŀ¼: $scriptDir"; exit 1 }

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
    Write-Host "δ�� scripts/*.vpy �������ҵ�ģ���ļ����á����ֽű�����������ʱ��̬�����û��������ģ�͡�"
    exit 0
}

Write-Host "�� scripts �з��ֿ�����Ҫ��ģ���ļ����ļ��� -> �ű��б���`n"
foreach ($k in $results.Keys) {
    $files = ($results[$k] -join ', ')
    Write-Host "$k  ->  $files"
}

# ���Ϊ�����ļ������ں������ػ��˹��˶�
$outFile = Join-Path (Get-Location) 'models_needed_report.txt'
"# models_needed_report generated: $(Get-Date)`n" | Out-File -FilePath $outFile -Encoding UTF8
foreach ($k in $results.Keys) {
    $files = ($results[$k] -join ', ')
    "$k -> $files" | Out-File -FilePath $outFile -Append -Encoding UTF8
}

Write-Host "������д��: $outFile"
Param(
    [string]$RepoUrl = "https://github.com/hooke007/mpv_PlayKit",
    [string]$Branch = "",
    [string]$TempDir = "$env:TEMP\mpv_PlayKit_extract_mvtools"
)

Write-Host "׼���� $RepoUrl ��ȡ���� MVTools/��֡/RIFE ��ص��ļ��� $(Get-Location)\scripts, models, plugins..."

if (Test-Path $TempDir) { Remove-Item -Recurse -Force $TempDir }

$cloneSuccess = $false
if ($Branch -and $Branch.Trim() -ne "") {
    Write-Host "����ʹ�÷�֧ '$Branch' ��¡..."
    git clone --depth 1 --branch $Branch $RepoUrl $TempDir
    if ($LASTEXITCODE -eq 0) { $cloneSuccess = $true }
    else { Write-Warning "����֧��¡ʧ�ܣ�����ʹ�òֿ�Ĭ�Ϸ�֧��¡..." }
}

if (-not $cloneSuccess) {
    git clone --depth 1 $RepoUrl $TempDir
    if ($LASTEXITCODE -ne 0) { Write-Error "git clone ʧ�ܣ����������� Git �����ԡ�"; exit 1 }
}

# Ŀ��Ŀ¼
$cwd = Get-Location
$scriptsDir = Join-Path $cwd "scripts"
$modelsDir = Join-Path $cwd "models"
$pluginsDir = Join-Path $cwd "plugins"
New-Item -ItemType Directory -Force -Path $scriptsDir, $modelsDir, $pluginsDir | Out-Null

Write-Host "ɨ����ʱ�ֿ����� mvtools / MVT / RIFE ��ص��ļ�..."

# 1) ���ȿ��� portable_config/vs �µĽű���mpv-lazy ��Լ��λ�ã�
$portableVs = Join-Path $TempDir "portable_config\vs"
if (Test-Path $portableVs) {
    Get-ChildItem -Path $portableVs -File -ErrorAction SilentlyContinue | ForEach-Object {
        Copy-Item $_.FullName -Destination $scriptsDir -Force
    }
}

# 2) �����ֿ�����ʽ����Ϊ MEMC_* / MIX_* / RIFE* / MVT* �� vpy/py �ļ�
$patterns = @('MEMC_*','MIX_*','*RIFE*','*mvt*')
foreach ($p in $patterns) {
    Get-ChildItem -Path $TempDir -Recurse -Include "$p.vpy","$p.py" -ErrorAction SilentlyContinue | ForEach-Object {
        Copy-Item $_.FullName -Destination $scriptsDir -Force
    }
}

# 3) �������� .vpy/.py �ļ��а����ؼ��� mvtools �� mvtools2 �� rife �Ľű�
Get-ChildItem -Path $TempDir -Recurse -Include *.vpy,*.py -ErrorAction SilentlyContinue | ForEach-Object {
    try { $content = Get-Content $_.FullName -Raw -ErrorAction Stop } catch { return }
    if ($content -match '(?i)mvtools' -or $content -match '(?i)mvt' -or $content -match '(?i)rife') {
        Copy-Item $_.FullName -Destination $scriptsDir -Force
    }
}

# 4) ����ֿ���� models/ �� weights/ �� portable_config/models ��Ŀ¼���������������ļ��� modelsDir
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

# 5) ������ڱ���õĲ���� DLL��ͨ�����ڸòֿ⣩��Ҳ��������������Ϊ���ݣ�������ע�����
Get-ChildItem -Path $TempDir -Recurse -Include *.dll,*.pyd,*.so -ErrorAction SilentlyContinue | Where-Object { $_.DirectoryName -notmatch "\\.git" } | ForEach-Object {
    Copy-Item $_.FullName -Destination $pluginsDir -Force
}

# 6) ɨ��ű������õ�ģ���ļ��������ɱ��棬��ģ���ѿ������עΪ�Ѵ���
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
    # ��ȡģ�����Ʋ�����Ƿ��ѿ���
    $modelName = [System.IO.Path]::GetFileName($k)
    $exists = Test-Path (Join-Path $modelsDir $modelName)
    if ($exists) {
        "$modelName -> AVAILABLE (copied) referenced by: $files" | Out-File -FilePath $outReport -Append -Encoding UTF8
    } else {
        "$modelName -> MISSING referenced by: $files" | Out-File -FilePath $outReport -Append -Encoding UTF8
    }
}

if ($foundModels.Count -eq 0) {
    Write-Host "δ�ڽű��з�����ȷ��ģ�����á����ű�����ʱ��̬���ػ�ʹ�����·�������ֶ��˶ԡ�"
} else {
    Write-Host "ģ������ɨ����ɣ�����: $outReport"
}

Write-Host "��ȡ��ɡ����� $scriptsDir, $modelsDir, $pluginsDir �е��ļ���ȷ�����֤��Ϲ��ԡ�"
Write-Host "��ʱĿ¼: $TempDir���������Ҫ��ɾ����"
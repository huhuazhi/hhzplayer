Param(
    [string]$RepoUrl = "https://github.com/hooke007/mpv_PlayKit",
    [string]$Branch = "master",
    [string]$TempDir = "$env:TEMP\mpv_PlayKit_extract"
)

Write-Host "׼���� $RepoUrl ��ȡ�ļ��� $(Get-Location)\scripts, models, plugins..."

if (Test-Path $TempDir) { Remove-Item -Recurse -Force $TempDir }

# �ȳ��԰�������֧��¡�����ʧ�����˻ص�Ĭ�Ϸ�֧��¡
$cloneSuccess = $false
if ($Branch -and $Branch.Trim() -ne "") {
    Write-Host "����ʹ�÷�֧ '$Branch' ��¡..."
    git clone --depth 1 --branch $Branch $RepoUrl $TempDir
    if ($LASTEXITCODE -eq 0) { $cloneSuccess = $true }
    else { Write-Warning "����֧��¡ʧ�ܣ�����ʹ�òֿ�Ĭ�Ϸ�֧�� clone..." }
}

if (-not $cloneSuccess) {
    git clone --depth 1 $RepoUrl $TempDir
    if ($LASTEXITCODE -ne 0) { Write-Error "git clone ʧ�ܣ����������� Git �����ԡ�"; exit 1 }
}

# ����Ŀ����Ŀ¼
$cwd = Get-Location
$scriptsDir = Join-Path $cwd "scripts"
$modelsDir = Join-Path $cwd "models"
$pluginsDir = Join-Path $cwd "plugins"
New-Item -ItemType Directory -Force -Path $scriptsDir, $modelsDir, $pluginsDir | Out-Null

# ���� .vpy �ű�
Get-ChildItem -Path $TempDir -Recurse -Include *.vpy -ErrorAction SilentlyContinue | ForEach-Object {
    Copy-Item $_.FullName -Destination $scriptsDir -Force
}

# ���Ұ��� mvtools �����Ľű�/�ı��ļ�
Get-ChildItem -Path $TempDir -Recurse -File -ErrorAction SilentlyContinue | Where-Object {
    try { (Get-Content $_.FullName -ErrorAction Stop) -match "mvtools" } catch { $false }
} | ForEach-Object {
    Copy-Item $_.FullName -Destination $scriptsDir -Force
}

# ����ģ����չ��
$exts = @('*.pth','*.pt','*.onnx','*.t7','*.h5','*.bin')
foreach ($e in $exts) {
    Get-ChildItem -Path $TempDir -Recurse -Include $e -ErrorAction SilentlyContinue | ForEach-Object { Copy-Item $_.FullName -Destination $modelsDir -Force }
}

# ���ҳ������ / dll / pyd
Get-ChildItem -Path $TempDir -Recurse -Include *.dll,*.pyd,*.so -ErrorAction SilentlyContinue | Where-Object { $_.DirectoryName -notmatch "\\.git" } | ForEach-Object {
    Copy-Item $_.FullName -Destination $pluginsDir -Force
}

Write-Host "��ȡ��ɡ����ֶ���� $scriptsDir, $modelsDir, $pluginsDir �е��ļ���ȷ�����֤��Ϲ��ԡ�"
Write-Host "��ʱĿ¼: $TempDir���������Ҫ��ɾ����"
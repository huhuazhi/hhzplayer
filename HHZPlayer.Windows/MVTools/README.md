MVTools 集成（适用于 HHZPlayer）

本目录用于存放与 MVTools 相关的 VapourSynth 脚本、运行时插件和所需模型，便于实现绿色（免安装）部署。

目标
- 提供一个“绿色免安装”目录结构，包含 VapourSynth 脚本、必要的模型文件以及（可选）第三方二进制/插件，用户只需把该目录连同播放器一起放到任意位置即可使用。

重要提示
- 我无法在此环境中直接从 GitHub 下载或复制受版权保护的二进制或模型文件。
- 我可以生成一个自动化脚本，帮你在本地机器上从指定仓库提取可能相关的脚本、插件和模型；但最终下载与分发的合规性需你自己确认并承担。

目录结构建议（放在 HHZPlayer.Windows/MVTools）
- `mvtools_interpolate.vpy`        # 已提供的 mvtools 插值脚本模板
- `scripts/`                       # 从仓库提取的 .vpy 或相关脚本
- `models/`                        # 模型文件（RIFE/FSRCNNX 等）
- `plugins/`                       # 可选：VapourSynth 插件 DLL / pyd（例如 mvtools）
- `import_mvtools_from_playkit.ps1`# 本仓库内的自动化提取脚本（需在本地运行）
- `README.md`                      # 本文件（中文说明）

快速使用指南
1. 在本地运行自动化脚本以提取文件（需安装 Git）：
   - 打开 PowerShell，切换到 `HHZPlayer.Windows\MVTools` 目录
   - 运行：
     `.\import_mvtools_from_playkit.ps1 -RepoUrl 'https://github.com/hooke007/mpv_PlayKit'`
   - 脚本会克隆仓库到临时目录并尝试拷贝 `.vpy`、包含 `mvtools` 关键字的脚本、常见模型扩展名（.pth/.onnx/.t7 等）以及可能位于 `models` 或 `plugins` 子目录的文件到本目录下的 `scripts/`、`models/`、`plugins/`。
   - 请在脚本运行后手动审核 `models/` 和 `plugins/` 的内容并确认许可证合规性。

2. 准备绿色 VapourSynth 运行环境
   - 可在应用目录放置 Python 可嵌入环境并安装 VapourSynth Wheel，或使用便携版 VapourSynth。
   - 将 `plugins/` 下的 DLL / pyd 放到 VapourSynth 插件目录（或设置 `VAPOURSYNTH_PLUGINS` 环境变量指向该目录）。
   - 确保必要依赖（如 mvtools、numpy 等）可用。

3. 在播放器中调用脚本示例
   - 通过现有接口添加 VF：
     `Player.Command($"no-osd vf add vapoursynth=file={Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MVTools", "scripts", "mvtools_interpolate.vpy")} :num=2:den=1");`
   - 或在 UI 中添加开关来动态加载/卸载此 VF。

许可证与合规
- 请在分发前检查每个脚本、模型和插件的许可证（GPL、MIT、CC 等）。对第三方模型的分发，通常需要遵循其许可证和作者要求。

后续我可以做的事
- 把 PowerShell 自动化脚本加入到本目录（已准备），并演示如何在本地运行以提取文件。\
- 为常见模型（例如 RIFE / FSRCNNX）提供下载脚本样例并说明放置位置。

（以后 README 我将默认使用中文）

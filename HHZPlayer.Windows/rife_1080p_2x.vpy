import vapoursynth as vs
core = vs.core
clip = video_in

# RIFE 需要 RGBS 输入
clip = core.resize.Bicubic(clip, format=vs.RGBS, matrix_in_s='709')

# 加载 RIFE 插件（或使用系统自带）
core.std.LoadPlugin(r"rife\librife_windows_x86-64.dll")

# 模型路径或编号
clip = core.rife.RIFE(clip, model_path=r"rife\models\rife-v4.6", factor_num=2)

# ✅ 转回 mpv 能识别的格式（yuv420p）
clip = core.resize.Bicubic(clip, format=vs.YUV420P10, matrix_s='709')

clip.set_output()

import vapoursynth as vs
core = vs.core
clip = video_in

# 加载 RIFE 插件
core.std.LoadPlugin(r"rife\librife_windows_x86-64.dll")
print(f"clip分辨率为:{clip.width}x{clip.height}!")
# 如果分辨率 <= 1080p：直接插帧
if clip.width <= 1920 and clip.height <= 1080:
    print(f"🔹 源分辨率 {clip.width}x{clip.height}，使用 v4.6 模型直接补帧")
    # 转 RGBS
    clip = core.resize.Bicubic(clip, format=vs.RGBS, matrix_in_s='709')
    # RIFE 插帧
    clip = core.rife.RIFE(
        clip,
        model_path=r"rife\models\rife-v4.6",
        factor_num=2        
    )
    # 转回 YUV 输出
    clip = core.resize.Bicubic(clip, format=vs.YUV444P10, matrix_s='709')

# 如果分辨率 > 1080p：先缩小再放大
else:
    print(f"🔹 源分辨率 {clip.width}x{clip.height}，降采样到 1280p 后使用 v4.6 模型补帧")
    # 计算等比缩放
    target_h = int(clip.height * 1280 / clip.width + 0.5)
    # 缩放到 1280 宽
    clip = core.resize.Bicubic(clip, width=1280, height=target_h, format=vs.RGBS, matrix_in_s='709')
    # RIFE 插帧（轻量模型）
    clip = core.rife.RIFE(
        clip,
        model_path=r"rife\models\rife-v4.6",
        factor_num=2
    )
    # 升回原分辨率
    clip = core.resize.Bicubic(clip, width=clip.width, height=clip.height, format=vs.YUV444P10, matrix_s='709')

# 输出结果
clip.set_output()

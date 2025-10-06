import vapoursynth as vs
core = vs.core
clip = video_in
core.std.LoadPlugin(r"rife\librife_windows_x86-64.dll")

target_h = int(clip.height * 1280 / clip.width)
# 先降到 1080 p 做补帧
clip = core.resize.Bicubic(clip, width=1280, height=target_h, format=vs.RGBS, matrix_in_s='709')
clip = core.rife.RIFE(
    clip,
    model_path=r"rife\models\rife-v2.3",  # 轻量版模型
    factor_num=2,
    gpu_id=0
)

# 再升回 4 K 输出
clip = core.resize.Bicubic(clip, width=clip.width, height=clip.height, format=vs.YUV420P10, matrix_s='709')
clip.set_output()

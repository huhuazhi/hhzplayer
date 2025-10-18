MVTools ���ɣ������� HHZPlayer��

��Ŀ¼���ڴ���� MVTools ��ص� VapourSynth �ű�������ʱ���������ģ�ͣ�����ʵ����ɫ���ⰲװ������

Ŀ��
- �ṩһ������ɫ�ⰲװ��Ŀ¼�ṹ������ VapourSynth �ű�����Ҫ��ģ���ļ��Լ�����ѡ��������������/������û�ֻ��Ѹ�Ŀ¼��ͬ������һ��ŵ�����λ�ü���ʹ�á�

��Ҫ��ʾ
- ���޷��ڴ˻�����ֱ�Ӵ� GitHub ���ػ����ܰ�Ȩ�����Ķ����ƻ�ģ���ļ���
- �ҿ�������һ���Զ����ű��������ڱ��ػ����ϴ�ָ���ֿ���ȡ������صĽű��������ģ�ͣ�������������ַ��ĺϹ��������Լ�ȷ�ϲ��е���

Ŀ¼�ṹ���飨���� HHZPlayer.Windows/MVTools��
- `mvtools_interpolate.vpy`        # ���ṩ�� mvtools ��ֵ�ű�ģ��
- `scripts/`                       # �Ӳֿ���ȡ�� .vpy ����ؽű�
- `models/`                        # ģ���ļ���RIFE/FSRCNNX �ȣ�
- `plugins/`                       # ��ѡ��VapourSynth ��� DLL / pyd������ mvtools��
- `import_mvtools_from_playkit.ps1`# ���ֿ��ڵ��Զ�����ȡ�ű������ڱ������У�
- `README.md`                      # ���ļ�������˵����

����ʹ��ָ��
1. �ڱ��������Զ����ű�����ȡ�ļ����谲װ Git����
   - �� PowerShell���л��� `HHZPlayer.Windows\MVTools` Ŀ¼
   - ���У�
     `.\import_mvtools_from_playkit.ps1 -RepoUrl 'https://github.com/hooke007/mpv_PlayKit'`
   - �ű����¡�ֿ⵽��ʱĿ¼�����Կ��� `.vpy`������ `mvtools` �ؼ��ֵĽű�������ģ����չ����.pth/.onnx/.t7 �ȣ��Լ�����λ�� `models` �� `plugins` ��Ŀ¼���ļ�����Ŀ¼�µ� `scripts/`��`models/`��`plugins/`��
   - ���ڽű����к��ֶ���� `models/` �� `plugins/` �����ݲ�ȷ�����֤�Ϲ��ԡ�

2. ׼����ɫ VapourSynth ���л���
   - ����Ӧ��Ŀ¼���� Python ��Ƕ�뻷������װ VapourSynth Wheel����ʹ�ñ�Я�� VapourSynth��
   - �� `plugins/` �µ� DLL / pyd �ŵ� VapourSynth ���Ŀ¼�������� `VAPOURSYNTH_PLUGINS` ��������ָ���Ŀ¼����
   - ȷ����Ҫ�������� mvtools��numpy �ȣ����á�

3. �ڲ������е��ýű�ʾ��
   - ͨ�����нӿ���� VF��
     `Player.Command($"no-osd vf add vapoursynth=file={Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MVTools", "scripts", "mvtools_interpolate.vpy")} :num=2:den=1");`
   - ���� UI ����ӿ�������̬����/ж�ش� VF��

���֤��Ϲ�
- ���ڷַ�ǰ���ÿ���ű���ģ�ͺͲ�������֤��GPL��MIT��CC �ȣ����Ե�����ģ�͵ķַ���ͨ����Ҫ��ѭ�����֤������Ҫ��

�����ҿ���������
- �� PowerShell �Զ����ű����뵽��Ŀ¼����׼����������ʾ����ڱ�����������ȡ�ļ���\
- Ϊ����ģ�ͣ����� RIFE / FSRCNNX���ṩ���ؽű�������˵������λ�á�

���Ժ� README �ҽ�Ĭ��ʹ�����ģ�

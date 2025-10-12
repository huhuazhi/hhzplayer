using HHZPlayer.Windows.WinForms;
using Microsoft.VisualBasic.ApplicationServices; // 需要引用 Microsoft.VisualBasic
using System.Linq;

class SingleInstanceApp : WindowsFormsApplicationBase
{
    public SingleInstanceApp()
    {
        IsSingleInstance = true;     // 关键：单实例
        EnableVisualStyles = true;   // 等同 Application.EnableVisualStyles()
    }

    protected override void OnCreateMainForm()
    {
        // 第一次启动：创建主窗体
        this.MainForm = new HHZPlayer.Windows.WinForms.MainForm();

        // 若首次启动自带参数（比如文件关联启动），也处理一下
        var files = this.CommandLineArgs?.ToArray() ?? [];
        if (files.Length > 0)
            ((MainForm)this.MainForm).OpenFromIpc(files); // 这里调用你封装的打开逻辑
    }

    protected override void OnStartupNextInstance(StartupNextInstanceEventArgs e)
    {
        base.OnStartupNextInstance(e);
        // 后续第二次启动：把新参数转给已开的主窗体
        var files = e.CommandLine?.ToArray() ?? [];
        var f = (MainForm)this.MainForm;
        f.BeginInvoke(new System.Action(() =>
        {
            // 你可以顺便把窗口激活到前台
            if (f.WindowState == System.Windows.Forms.FormWindowState.Minimized)
                f.WindowState = System.Windows.Forms.FormWindowState.Normal;
            f.Activate();

            f.OpenFromIpc(files); // 这里按你的播放逻辑打开文件
        }));
        e.BringToForeground = true;
    }
}
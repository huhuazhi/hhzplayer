
namespace MpvNet.Windows.WinForms;

partial class MainForm
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        CursorTimer = new System.Windows.Forms.Timer(components);
        ProgressTimer = new System.Windows.Forms.Timer(components);
        btn3D = new System.Windows.Forms.Button();
        btnBack = new System.Windows.Forms.PictureBox();
        progressBar = new System.Windows.Forms.ProgressBar();
        ((System.ComponentModel.ISupportInitialize)btnBack).BeginInit();
        SuspendLayout();
        // 
        // CursorTimer
        // 
        CursorTimer.Enabled = true;
        CursorTimer.Interval = 500;
        CursorTimer.Tick += CursorTimer_Tick;
        // 
        // ProgressTimer
        // 
        ProgressTimer.Tick += ProgressTimer_Tick;
        // 
        // btn3D
        // 
        btn3D.BackColor = System.Drawing.Color.Transparent;
        btn3D.ForeColor = System.Drawing.Color.White;
        btn3D.Location = new System.Drawing.Point(49, 139);
        btn3D.Name = "btn3D";
        btn3D.Size = new System.Drawing.Size(118, 109);
        btn3D.TabIndex = 0;
        btn3D.Text = "3D";
        btn3D.UseVisualStyleBackColor = false;
        btn3D.Visible = false;
        btn3D.Click += btn3D_Click;
        // 
        // btnBack
        // 
        btnBack.BackColor = System.Drawing.Color.Transparent;
        btnBack.Image = (System.Drawing.Image)resources.GetObject("btnBack.Image");
        btnBack.Location = new System.Drawing.Point(49, 29);
        btnBack.Name = "btnBack";
        btnBack.Size = new System.Drawing.Size(118, 104);
        btnBack.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
        btnBack.TabIndex = 1;
        btnBack.TabStop = false;
        btnBack.Visible = false;
        btnBack.Click += btnBack_Click;
        // 
        // progressBar
        // 
        progressBar.Location = new System.Drawing.Point(236, 928);
        progressBar.Name = "progressBar";
        progressBar.Size = new System.Drawing.Size(1866, 49);
        progressBar.TabIndex = 2;
        progressBar.Visible = false;
        progressBar.MouseDown += progressBar_MouseDown;
        // 
        // MainForm
        // 
        AllowDrop = true;
        AutoScaleDimensions = new System.Drawing.SizeF(192F, 192F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        BackColor = System.Drawing.Color.Black;
        ClientSize = new System.Drawing.Size(2333, 1284);
        Controls.Add(progressBar);
        Controls.Add(btnBack);
        Controls.Add(btn3D);
        Font = new System.Drawing.Font("Segoe UI", 9F);
        Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
        Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
        Name = "MainForm";
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        ((System.ComponentModel.ISupportInitialize)btnBack).EndInit();
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.Timer CursorTimer;
    private System.Windows.Forms.Timer ProgressTimer;
    private System.Windows.Forms.Button btn3D;
    private System.Windows.Forms.PictureBox btnBack;
    private System.Windows.Forms.ProgressBar progressBar;
}
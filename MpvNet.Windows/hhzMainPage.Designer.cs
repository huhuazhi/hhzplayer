namespace MpvNet.Windows
{
    partial class HHZMainPage
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // HHZMainPage
            // 
            AllowDrop = true;
            AutoScaleDimensions = new System.Drawing.SizeF(14F, 31F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.Black;
            Name = "HHZMainPage";
            Size = new System.Drawing.Size(1392, 946);
            DragDrop += HHZMainPage_DragDrop;
            DragEnter += HHZMainPage_DragEnter;
            Paint += hhzMainPage_Paint;
            ResumeLayout(false);
        }

        #endregion
    }
}

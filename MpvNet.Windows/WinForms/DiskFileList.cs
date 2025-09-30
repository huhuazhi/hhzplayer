using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MpvNet.Windows.WinForms
{
    public class DiskFileList : SKControl
    {
        // 外部属性
        public bool Enable3DMode { get; set; } = false;

        // 内部模式
        private enum ViewMode { Disk, File }

        // 左右两套状态
        private class PanelState
        {
            public ViewMode Mode = ViewMode.Disk;
            public string CurrentPath = "";       // 目录路径
            public int ScrollY = 0;               // 滚动偏移
            public int HoverIndex = -1;
            public int SelectedIndex = -1;
            public List<Item> Items = new();
        }

        private readonly PanelState _left = new();
        private readonly PanelState _right = new();

        // 公共 Item 数据模型
        private class Item
        {
            public string Name = "";
            public string Path = "";
            public bool IsDirectory;
            public long Size = -1; // 文件大小，文件夹 -1
        }

        public event EventHandler<string> FileOpened;

        public DiskFileList()
        {
            DoubleBuffered = true;

            // 初始加载磁盘
            ReloadDisks(_left);
            ReloadDisks(_right);

            MouseMove += OnMouseMove;
            MouseLeave += (_, __) => { _left.HoverIndex = _right.HoverIndex = -1; Invalidate(); };
            MouseDown += OnMouseDown;
            MouseWheel += OnMouseWheel;
        }

        // =============== 绘制 ===============
        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Black);

            if (Enable3DMode)
            {
                int halfWidth = Width / 2;
                DrawPanel(canvas, _left, new SKRect(0, 0, halfWidth, Height));
                DrawPanel(canvas, _right, new SKRect(halfWidth, 0, Width, Height));
            }
            else
            {
                DrawPanel(canvas, _left, new SKRect(0, 0, Width, Height));
            }
        }

        private void DrawPanel(SKCanvas canvas, PanelState state, SKRect bounds)
        {
            float rowHeight = 40;
            float y = bounds.Top + state.ScrollY;

            using var paintText = new SKPaint
            {
                Color = SKColors.White,
                TextSize = 16,
                IsAntialias = true
            };

            using var paintHover = new SKPaint { Color = new SKColor(40, 120, 200, 100) };
            using var paintSel = new SKPaint { Color = new SKColor(80, 160, 255, 120) };

            for (int i = 0; i < state.Items.Count; i++)
            {
                var item = state.Items[i];
                var rowRect = new SKRect(bounds.Left, y, bounds.Right, y + rowHeight);

                if (i == state.HoverIndex) canvas.DrawRect(rowRect, paintHover);
                if (i == state.SelectedIndex) canvas.DrawRect(rowRect, paintSel);

                string text = item.Name;
                if (!item.IsDirectory && item.Size >= 0)
                    text += $"   {FormatSize(item.Size)}";

                canvas.DrawText(text, rowRect.Left + 40, rowRect.MidY + 5, paintText);

                y += rowHeight;
            }
        }

        // =============== 输入 ===============
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            var state = HitPanel(e.X);
            if (state == null) return;

            int index = HitTest(state, e.Location, state == _right);
            if (index != state.HoverIndex)
            {
                state.HoverIndex = index;
                Invalidate();
            }
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            var state = HitPanel(e.X);
            if (state == null) return;

            int index = HitTest(state, e.Location, state == _right);
            if (index < 0 || index >= state.Items.Count) return;

            state.SelectedIndex = index;
            var item = state.Items[index];

            if (state.Mode == ViewMode.Disk)
            {
                if (item.IsDirectory)
                {
                    NavigateTo(state, item.Path);
                }
            }
            else // FileMode
            {
                if (item.IsDirectory)
                {
                    if (item.Name == "..")
                    {
                        NavigateUp(state);
                    }
                    else
                    {
                        NavigateTo(state, item.Path);
                    }
                }
                else
                {
                    FileOpened?.Invoke(this, item.Path);
                }
            }

            Invalidate();
        }

        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            var state = HitPanel(e.X);
            if (state == null) return;

            state.ScrollY += Math.Sign(e.Delta) * 40; // 一行
            Invalidate();
        }

        // =============== HitTest ===============
        private PanelState HitPanel(int x)
        {
            if (Enable3DMode)
            {
                return (x < Width / 2) ? _left : _right;
            }
            else return _left;
        }

        private int HitTest(PanelState state, Point p, bool rightPanel)
        {
            int offsetX = rightPanel && Enable3DMode ? Width / 2 : 0;
            float rowHeight = 40;

            int index = (int)((p.Y - state.ScrollY) / rowHeight);
            return (index >= 0 && index < state.Items.Count) ? index : -1;
        }

        // =============== 数据加载 ===============
        private void ReloadDisks(PanelState state)
        {
            state.Mode = ViewMode.Disk;
            state.Items.Clear();

            foreach (var di in DriveInfo.GetDrives())
            {
                if (!di.IsReady) continue;
                state.Items.Add(new Item
                {
                    Name = $"{di.VolumeLabel} ({di.Name.TrimEnd('\\')})",
                    Path = di.RootDirectory.FullName,
                    IsDirectory = true
                });
            }
        }

        private void NavigateTo(PanelState state, string path)
        {
            try
            {
                var dir = new DirectoryInfo(path);
                if (!dir.Exists) return;

                state.Mode = ViewMode.File;
                state.CurrentPath = path;
                state.Items.Clear();

                // 上级目录
                if (dir.Parent != null)
                {
                    state.Items.Add(new Item
                    {
                        Name = "..",
                        Path = dir.Parent.FullName,
                        IsDirectory = true
                    });
                }
                else
                {
                    // 回到磁盘模式
                    ReloadDisks(state);
                    return;
                }

                foreach (var subDir in dir.GetDirectories())
                {
                    state.Items.Add(new Item
                    {
                        Name = subDir.Name,
                        Path = subDir.FullName,
                        IsDirectory = true
                    });
                }
                foreach (var file in dir.GetFiles())
                {
                    state.Items.Add(new Item
                    {
                        Name = file.Name,
                        Path = file.FullName,
                        IsDirectory = false,
                        Size = file.Length
                    });
                }
            }
            catch { }
        }

        private void NavigateUp(PanelState state)
        {
            if (string.IsNullOrEmpty(state.CurrentPath)) return;

            var dir = new DirectoryInfo(state.CurrentPath);
            if (dir.Parent == null)
            {
                ReloadDisks(state);
            }
            else
            {
                NavigateTo(state, dir.Parent.FullName);
            }
        }

        // =============== 工具 ===============
        private static string FormatSize(long bytes)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB" };
            double v = bytes;
            int i = 0;
            while (v >= 1024 && i < units.Length - 1) { v /= 1024; i++; }
            return $"{v:0.##} {units[i]}";
        }
    }
}

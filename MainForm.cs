﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using OpenQA.Selenium;
using System.Threading;
using AutoItX3Lib;

namespace KSTN_Facebook_Tool
{
    public partial class MainForm : Form
    {
        // Disable WebBrowser Sounds
        const int FEATURE_DISABLE_NAVIGATION_SOUNDS = 21;
        const int SET_FEATURE_ON_PROCESS = 0x00000002;

        [DllImport("urlmon.dll")]
        [PreserveSig]
        [return: MarshalAs(UnmanagedType.Error)]
        static extern int CoInternetSetFeatureEnabled(
            int FeatureEntry,
            [MarshalAs(UnmanagedType.U4)] int dwFlags,
            bool fEnable);

        static void DisableClickSounds()
        {
            CoInternetSetFeatureEnabled(
                FEATURE_DISABLE_NAVIGATION_SOUNDS,
                SET_FEATURE_ON_PROCESS,
                true);
        }

        SeleniumControl SE;
        // public DataTable dt;
        //public System.Threading.Thread t;
        public AutoItX3 autoIt = new AutoItX3();

        public MainForm()
        {
            InitializeComponent();
            DisableClickSounds();

            autoIt.AutoItSetOption("WinTitleMatchMode", 2);

            //dt = new DataTable();
            //dt.Columns.Add("group_name");
            //dt.Columns.Add("group_link");
            //dt.Columns.Add("group_mem");
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            Program.loadingForm = new LoadingForm();
            //new Thread(() => new LoadingForm().Show()).Start();
            // t = new System.Threading.Thread(new System.Threading.ThreadStart(() => Program.loadingForm.ShowDialog()));

            SE = new SeleniumControl();
            cbMethods.SelectedIndex = 0;
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Program.mainForm.autoIt.WinSetState("Mozilla Firefox", "", 1);
            SE.quit();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            SE.FBLogin(txtUser.Text, txtPass.Text);
        }

        private void btnBrowse1_Click(object sender, EventArgs e)
        {
            var fDialog = new System.Windows.Forms.OpenFileDialog();
            fDialog.Title = "Open Arial Bitmap File";
            fDialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
            fDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            DialogResult result = fDialog.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                string file = fDialog.FileName;
                txtBrowse1.Text = file;
            }
        }

        private void btnBrowse2_Click(object sender, EventArgs e)
        {
            var fDialog = new System.Windows.Forms.OpenFileDialog();
            fDialog.Title = "Open Arial Bitmap File";
            fDialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
            fDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            DialogResult result = fDialog.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                string file = fDialog.FileName;
                txtBrowse2.Text = file;
            }
        }

        private void btnBrowse3_Click(object sender, EventArgs e)
        {
            var fDialog = new System.Windows.Forms.OpenFileDialog();
            fDialog.Title = "Open Arial Bitmap File";
            fDialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
            fDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            DialogResult result = fDialog.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                string file = fDialog.FileName;
                txtBrowse3.Text = file;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        private void btnPost_Click(object sender, EventArgs e)
        {
            if (txtBrowse1.Text == "" && txtBrowse2.Text == "" && txtBrowse3.Text == "" && txtContent.Text == "")
            {
                MessageBox.Show("Điền nội dung trước khi post bài!");
                return;
            }

            int delay;

            if (!int.TryParse(txtDelay.Text, out delay) || delay < 10)
            {
                MessageBox.Show("Số giây Delay: số nguyên không nhỏ hơn 10");
                return;
            }

            if (SE.ready == false)
            {
                MessageBox.Show("Chương trình đang thực hiện 1 tác vụ khác");
                return;
            }

            btnPost.Enabled = false;
            txtContent.Enabled = false;
            txtDelay.Enabled = false;
            cbMethods.Enabled = false;
            txtBrowse1.Enabled = false;
            txtBrowse2.Enabled = false;
            txtBrowse3.Enabled = false;
            btnBrowse1.Enabled = false;
            btnBrowse2.Enabled = false;
            btnBrowse3.Enabled = false;

            SE.AutoPost();
        }

        private void lblViewProfile_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(lblViewProfile.Text);
        }

        private void btnGroupExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.FileName = "GROUPS.txt";
            saveFile.ShowDialog();

            using (StreamWriter sw = new StreamWriter(saveFile.FileName, false))
            {
                if (dgGroups.Rows.Count > 0)
                {
                    foreach (DataGridViewRow row in dgGroups.Rows)
                    {
                        sw.WriteLine(row.Cells[1].Value + "");
                    }
                }
                else
                {
                    sw.WriteLine("No group found.");
                }
                sw.Close();
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabControl1.TabPages["tabPageInvite"])
            {
                dgGroups.Parent = GroupBoxInvite;
                dgGroups.Height = 310;
            }
            else
            {
                dgGroups.Parent = groupBox4;
                dgGroups.Height = 160;
            }
        }

        private void btnInvite_Click(object sender, EventArgs e)
        {
            if (txtInviteName.Text == "")
            {
                MessageBox.Show("Điền tên muốn mời trước khi bắt đầu Auto!");
                return;
            }

            int delay;

            if (!int.TryParse(txtInviteDelay.Text, out delay) || delay < 10)
            {
                MessageBox.Show("Số giây Delay: số nguyên không nhỏ hơn 10");
                return;
            }

            if (SE.ready == false)
            {
                MessageBox.Show("Chương trình đang thực hiện 1 tác vụ khác");
                return;
            }

            txtInviteDelay.Enabled = false;
            txtInviteName.Enabled = false;
            btnInvite.Enabled = false;

            SE.AutoInvite();
        }

        private void btnGroupSearch_Click(object sender, EventArgs e)
        {
            if (txtGroupSearch.Text == "")
            {
                MessageBox.Show("Điền từ khóa tìm kiếm!");
                return;
            }

            int min;
            if (!int.TryParse(txtGroupSearchMin.Text, out min))
            {
                MessageBox.Show("Số lượng thành viên tối thiểu???");
            }

            if (SE.ready == false)
            {
                MessageBox.Show("Chương trình đang thực hiện 1 tác vụ khác");
                return;
            }

            txtGroupSearch.Enabled = false;
            txtGroupSearchMin.Enabled = false;
            btnGroupSearch.Enabled = false;

            SE.GroupSearch();
        }
    }
}

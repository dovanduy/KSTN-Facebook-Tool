﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Net;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using OpenQA.Selenium;
using System.Threading;
using System.Reflection;
//using AutoItX3Lib;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Excel_12 = Microsoft.Office.Interop.Excel;

namespace KSTN_Facebook_Tool
{
    public partial class MainForm : Form
    {
        #region SMALL STUFFS
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


        private const int CS_DROPSHADOW = 0x00020000;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams parameters = base.CreateParams;
                if (OSFeature.IsPresent(SystemParameter.DropShadow))
                {
                    parameters.ClassStyle = parameters.ClassStyle | CS_DROPSHADOW;
                }
                return parameters;
            }
        }

        private void pbClose_MouseHover(object sender, EventArgs e)
        {
            pbClose.Image = System.Drawing.Bitmap.FromFile("close2.png");
            pbClose.BackColor = Color.OrangeRed;
        }

        private void pbClose_MouseLeave(object sender, EventArgs e)
        {
            pbClose.Image = System.Drawing.Bitmap.FromFile("close1.png");
            pbClose.BackColor = Color.White;
        }

        private void pbClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void pbMinimize_MouseHover(object sender, EventArgs e)
        {
            pbMinimize.Image = System.Drawing.Bitmap.FromFile("min2.png");
            pbMinimize.BackColor = Color.Silver;
        }

        private void pbMinimize_MouseLeave(object sender, EventArgs e)
        {
            pbMinimize.Image = System.Drawing.Bitmap.FromFile("min1.png");
            pbMinimize.BackColor = Color.White;
        }

        private void pbMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        #endregion

        SeleniumControl SE;
        //public AutoItX3 autoIt = new AutoItX3();

        private String CHAT_URL = "http://kstnk57.com/AUTO/chatlog.php";

        #region GENERAL MAINFORM
        public MainForm()
        {
            InitializeComponent();
            DisableClickSounds();

            //autoIt.AutoItSetOption("WinTitleMatchMode", 2);
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            Program.loadingForm = new LoadingForm();
            SE = new SeleniumControl();
            txtUser.Focus();
            txtUser.Text = Properties.Settings.Default.user;
            txtPass.Text = Properties.Settings.Default.pass;
            cbGroupReload.Checked = Properties.Settings.Default.group_reload;
            ChatRefresh();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Program.loadingForm.RequestStop();
            SE.quit();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (btnLogin.Text == "Đăng nhập")
            {
                SE.FBLogin(txtUser.Text, txtPass.Text);
                if (cbRemember.Checked)
                {
                    Properties.Settings.Default.user = txtUser.Text;
                    Properties.Settings.Default.Save();
                    Properties.Settings.Default.pass = txtPass.Text;
                    Properties.Settings.Default.Save();
                }
                else
                {
                    Properties.Settings.Default.user = "";
                    Properties.Settings.Default.Save();
                    Properties.Settings.Default.pass = "";
                    Properties.Settings.Default.Save();
                }
            }
            else
            {
                if (!SE.ready)
                {
                    MessageBox.Show("Chương trình đang thực hiện 1 tác vụ, không thể đăng xuất!");
                    return;
                }
                btnLogin.Enabled = false;
                SE.Logout();
            }
        }

        private void lblViewProfile_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(lblViewProfile.Text);
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TabControl1.SelectedTab == TabControl1.TabPages["tabPageInvite"])
            {
                dgGroups.Parent = GroupBoxInvite;
                dgGroups.Height = 310;
            }
            else if (TabControl1.SelectedTab == TabControl1.TabPages["tabPagePost"])
            {
                dgGroups.Parent = groupBox4;
                dgGroups.Height = 160;
            }
            else if (TabControl1.SelectedTab == TabControl1.TabPages["tabPageFanpage"])
            {
                fanpage_init();
            }
            else if (TabControl1.SelectedTab == TabControl1.TabPages["tabPageEvents"])
            {
                events_init();
            }
        }

        private void btnToggle_Click(object sender, EventArgs e)
        {
            if (SE.driver == null)
            {
                MessageBox.Show("Trình duyệt chưa được khởi tạo!");
                btnToggle.Checked = false;
            }
            else
            {
                SE.Toggle();
            }
        }

        private void btnPauseAll_Click(object sender, EventArgs e)
        {
            SE.pause = true;
            btnPauseAll.Enabled = false;
        }
        #endregion

        #region TAB AUTOPOST
        private void btnBrowse1_Click(object sender, EventArgs e)
        {
            var fDialog = new System.Windows.Forms.OpenFileDialog();
            fDialog.Title = "Open Arial Bitmap File";
            fDialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
            //fDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            DialogResult result = fDialog.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                string file = fDialog.FileName;
                txtBrowse1.Text = file;
            }
            else
            {
                txtBrowse1.Text = "";
            }
        }

        private void btnBrowse2_Click(object sender, EventArgs e)
        {
            var fDialog = new System.Windows.Forms.OpenFileDialog();
            fDialog.Title = "Open Arial Bitmap File";
            fDialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
            //fDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            DialogResult result = fDialog.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                string file = fDialog.FileName;
                txtBrowse2.Text = file;
            }
            else
            {
                txtBrowse2.Text = "";
            }
        }

        private void btnBrowse3_Click(object sender, EventArgs e)
        {
            var fDialog = new System.Windows.Forms.OpenFileDialog();
            fDialog.Title = "Open Arial Bitmap File";
            fDialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
            //fDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            DialogResult result = fDialog.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                string file = fDialog.FileName;
                txtBrowse3.Text = file;
            }
            else
            {
                txtBrowse3.Text = "";
            }
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            btnPause.Enabled = false;
            SE.pause = true;
        }

        private void btnPost_Click(object sender, EventArgs e)
        {
            if (txtBrowse1.Text == "" && txtBrowse2.Text == "" && txtBrowse3.Text == "" && txtContent.Text == "")
            {
                MessageBox.Show("Điền nội dung trước khi post bài!");
                return;
            }

            int delay;

            if (!int.TryParse(txtDelay.Text, out delay) || delay < 0)
            {
                MessageBox.Show("Số giây Delay: số nguyên không nhỏ hơn 0");
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
            txtBrowse1.Enabled = false;
            txtBrowse2.Enabled = false;
            txtBrowse3.Enabled = false;
            btnBrowse1.Enabled = false;
            btnBrowse2.Enabled = false;
            btnBrowse3.Enabled = false;
            dgGroups.Enabled = false;
            btnPause.Enabled = true;

            SE.AutoPost();
        }

        private void btnGroupExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Filter = "TXT files (*.txt)|*.txt";
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

        private void btnPostResultExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Filter = "TXT files (*.txt)|*.txt";
            saveFile.FileName = "POSTS.txt";
            saveFile.ShowDialog();

            using (StreamWriter sw = new StreamWriter(saveFile.FileName, false))
            {
                if (dgPostResult.Rows.Count > 0)
                {
                    foreach (DataGridViewRow row in dgPostResult.Rows)
                    {
                        sw.WriteLine(row.Cells[1].Value + "");
                    }
                }
                else
                {
                    sw.WriteLine("Không tìm thấy bài đăng nào cả.");
                }
                sw.Close();
            }
        }

        private void btnPostClearGroups_Click(object sender, EventArgs e)
        {
            dgGroups.Rows.Clear();
        }

        private void cbGroupReload_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.group_reload = cbGroupReload.Checked;
            Properties.Settings.Default.Save();
        }

        private void btnGroupJoinFromFile_Click(object sender, EventArgs e)
        {
            var fDialog = new System.Windows.Forms.OpenFileDialog();
            fDialog.Title = "Open Groups File";
            fDialog.Filter = "TXT Files (*.txt) | *.txt";

            DialogResult result = fDialog.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                string file = fDialog.FileName;
                txtGroupJoinFromFile.Text = file;

                MessageBox.Show("Import File có thể gây treo chương trình trong vài giây! Nhấn OK để tiếp tục.");

                int counter = 0;
                string line;

                // Read the file and display it line by line.
                System.IO.StreamReader fileStr = new System.IO.StreamReader(file);
                while ((line = fileStr.ReadLine()) != null)
                {
                    string[] _line = new string[] { "", line, "" };
                    dgGroupSearch.Rows.Insert(0, _line);
                    counter++;
                }

                fileStr.Close();

                MessageBox.Show("Đọc thành công: " + counter + " nhóm");
            }
            else
            {
                txtGroupJoinFromFile.Text = "";
            }
        }

        private void btnGroupImport_Click(object sender, EventArgs e)
        {
            var fDialog = new System.Windows.Forms.OpenFileDialog();
            fDialog.Title = "Open Groups File";
            fDialog.Filter = "TXT Files (*.txt) | *.txt";

            DialogResult result = fDialog.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                string file = fDialog.FileName;

                MessageBox.Show("Import File có thể gây treo chương trình trong vài giây! Nhấn OK để tiếp tục.");

                int counter = 0;
                string line;

                // Read the file and display it line by line.
                System.IO.StreamReader fileStr = new System.IO.StreamReader(file);
                while ((line = fileStr.ReadLine()) != null)
                {
                    string[] _line = new string[] { "", line, "" };
                    dgGroups.Rows.Insert(0, _line);
                    counter++;
                }

                fileStr.Close();

                MessageBox.Show("Đọc thành công: " + counter + " nhóm");
            }
        }

        private void btnGroupImportFriends_Click(object sender, EventArgs e)
        {
            if (!SE.ready)
            {
                MessageBox.Show("Chương trình đang thực hiện 1 tác vụ khác!");
                return;
            }

            btnGroupImportFriends.Enabled = false;

            SE.GroupImportFriends();
        }

        private void btnPostImportGroups_Click(object sender, EventArgs e)
        {
            if (SE.ready == false)
            {
                MessageBox.Show("Chương trình đang thực hiện 1 tác vụ khác");
                return;
            }
            dgGroups.Rows.Clear();
            SE.getGroups();
        }

        private void dgGroups_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            groups_to_xml();
        }

        public void groups_to_xml()
        {
            try
            {
                //Create a datatable to store XML data
                DataTable dt = new DataTable();

                foreach (DataGridViewColumn col in dgGroups.Columns)
                {
                    dt.Columns.Add(col.HeaderText);
                }

                foreach (DataGridViewRow row in dgGroups.Rows)
                {
                    DataRow dRow = dt.NewRow();
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        dRow[cell.ColumnIndex] = cell.Value;
                    }
                    dt.Rows.Add(dRow);
                }
                DataSet DS = new DataSet();
                DS.Tables.Add(dt);
                DS.WriteXml(SE.RemoveSpecialCharacters(SE.user_id) + "_groups.xml");
            }
            catch (Exception ex) { MessageBox.Show(ex + ""); }
        }

        private void dgGroups_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;

            if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn && e.RowIndex >= 0)
                dgGroups.Rows.Remove(dgGroups.Rows[e.RowIndex]);
        }

        #endregion

        #region TAB AUTOJOIN
        private void btnGroupJoin_Click(object sender, EventArgs e)
        {
            if (dgGroupSearch.Rows.Count == 0)
            {
                MessageBox.Show("Chưa có nhóm nào trong List!");
                return;
            }

            if (SE.ready == false)
            {
                MessageBox.Show("Chương trình đang thực hiện 1 tác vụ khác");
                return;
            }
            int delay;

            if (!int.TryParse(txtJoinDelay.Text, out delay) || delay < 0)
            {
                MessageBox.Show("Số giây Delay: số nguyên không nhỏ hơn 0");
                return;
            }

            btnGroupJoin.Enabled = false;
            txtJoinDelay.Enabled = false;

            SE.AutoJoin();
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

            dgGroupSearch.Rows.Clear();

            SE.GroupSearch();
        }
        #endregion

        #region TAB AUTOINVITE
        private void btnInvite_Click(object sender, EventArgs e)
        {
            if (txtInviteName.Text == "")
            {
                MessageBox.Show("Điền tên muốn mời trước khi bắt đầu Auto!");
                return;
            }

            int delay;

            if (!int.TryParse(txtInviteDelay.Text, out delay) || delay < 0)
            {
                MessageBox.Show("Số giây Delay: số nguyên không nhỏ hơn 0");
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
            btnInvitePause.Enabled = true;

            SE.AutoInvite();
        }

        private void btnInvitePause_Click(object sender, EventArgs e)
        {
            btnInvitePause.Enabled = false;
            SE.pause = true;
        }
        #endregion

        #region TAB AUTOCOMMENT
        private void btnCommentPause_Click(object sender, EventArgs e)
        {
            btnCommentPause.Enabled = false;
            SE.pause = true;
        }

        private void btnCommentBrowse_Click(object sender, EventArgs e)
        {
            var fDialog = new System.Windows.Forms.OpenFileDialog();
            fDialog.Title = "Open Post IDS File";
            fDialog.Filter = "TXT Files (*.txt) | *.txt";

            DialogResult result = fDialog.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                string file = fDialog.FileName;
                txtCommentBrowse.Text = file;

                MessageBox.Show("Import File có thể gây treo chương trình trong vài giây! Nhấn OK để tiếp tục.");

                int counter = 0;
                string line;

                // Read the file and display it line by line.
                System.IO.StreamReader fileStr = new System.IO.StreamReader(file);
                while ((line = fileStr.ReadLine()) != null)
                {
                    dgCommentBrowse.Rows.Insert(0, line);
                    counter++;
                }

                fileStr.Close();

                MessageBox.Show("Đọc thành công: " + counter + " bài đăng");
            }
            else
            {
                txtCommentBrowse.Text = "";
            }
        }

        private void btnCommentImportComment_Click(object sender, EventArgs e)
        {
            if (SE.ready == false)
            {
                MessageBox.Show("Chương trình đang thực hiện 1 tác vụ khác");
                return;
            }
            int delay;

            if (!int.TryParse(txtCommentDelay.Text, out delay) || delay < 0)
            {
                MessageBox.Show("Số giây Delay: số nguyên không nhỏ hơn 0");
                return;
            }

            if (txtComment.Text == "")
            {
                MessageBox.Show("Không được bỏ trống nội dung bình luận");
                return;
            }

            txtComment.Enabled = false;
            txtCommentDelay.Enabled = false;
            btnCommentPause.Enabled = true;
            btnCommentBrowse.Enabled = false;
            btnCommentImportComment.Enabled = false;
            dgCommentBrowse.Enabled = false;

            SE.AutoComment2();
        }

        private void btnCommentScan_Click(object sender, EventArgs e)
        {
            if (SE.ready == false)
            {
                MessageBox.Show("Chương trình đang thực hiện 1 tác vụ khác");
                return;
            }

            btnCommentScan.Enabled = false;
            btnCommentScanPause.Enabled = true;

            SE.AutoCommentScan();
        }

        private void btnCommentScanPause_Click(object sender, EventArgs e)
        {
            SE.pause = true;
        }
        #endregion

        #region TAB EVENTS
        private async void events_init()
        {
            if (SE.events.Count > 0) return;

            await SE.getEvents();

            if (SE.events.Count > 0)
            {
                cbEvents.Items.Clear();
                foreach (string event_title in SE.events.Keys)
                {
                    cbEvents.Items.Add(event_title);
                }

                cbEvents.SelectedIndex = 0;
                cbEvents.Enabled = true;
            }
            else
            {
                cbEvents.Items.Add("Bạn không có Sự kiện nào cả!");
                cbEvents.SelectedIndex = 0;
                cbEvents.Enabled = false;
            }
        }

        private void btnEventReload_Click(object sender, EventArgs e)
        {
            if (SE.ready == false)
            {
                MessageBox.Show("Chương trình đang thực hiện 1 tác vụ khác");
                return;
            }

            SE.events = new Dictionary<string, string>();
            events_init();
        }

        private void btnEventInviteFriends_Click(object sender, EventArgs e)
        {
            if (SE.ready == false)
            {
                MessageBox.Show("Chương trình đang thực hiện 1 tác vụ khác");
                return;
            }

            btnEventInviteFriends.Enabled = false;
            btnEventInviteFriendsPause.Enabled = true;

            SE.EventsInviteFriends();
        }

        private void btnEventInviteFriendsPause_Click(object sender, EventArgs e)
        {
            SE.pause = true;
        }
        #endregion

        #region TAB PM
        private void btnPMImportFriends_Click(object sender, EventArgs e)
        {
            if (!SE.ready)
            {
                MessageBox.Show("Chương trình đang thực hiện 1 tác vụ khác!");
                return;
            }

            btnPMImportFriends.Enabled = false;

            SE.ImportFriendList();
        }

        private void btnPMImportGroup_Click(object sender, EventArgs e)
        {
            if (!SE.ready)
            {
                MessageBox.Show("Chương trình đang thực hiện 1 tác vụ khác!");
                return;
            }

            if (txtPMImportGroup.Text == "")
            {
                MessageBox.Show("Điền URL nhóm!");
                return;
            }

            if (txtPMImportGroup.Text.Contains("/"))
            {
                MessageBox.Show("Xem lại Group ID\nVí dụ: https://facebook.com/groups/saletour/ hoăc https://facebook.com/groups/123/\nThì điền saletour hoặc 123 là Group ID");
                return;
            }

            txtPMImportGroup.Enabled = false;
            btnPMImportGroup.Enabled = false;

            SE.ImportGroupMembers("https://m.facebook.com/groups/" + txtPMImportGroup.Text + "/?view=members&refid=18");
        }

        private void btnPMImportProfile_Click(object sender, EventArgs e)
        {
            if (!SE.ready)
            {
                MessageBox.Show("Chương trình đang thực hiện 1 tác vụ khác!");
                return;
            }

            if (txtPMImportProfile.Text == "")
            {
                MessageBox.Show("Điền URL Profile!");
                return;
            }

            txtPMImportProfile.Enabled = false;
            btnPMImportProfile.Enabled = false;

            SE.ImportProfileFriends(txtPMImportProfile.Text);
        }

        private void btnPMPause_Click(object sender, EventArgs e)
        {
            btnPMPause.Enabled = false;
            SE.pause = true;
        }

        private void btnPMImportFile_Click(object sender, EventArgs e)
        {
            var fDialog = new System.Windows.Forms.OpenFileDialog();
            fDialog.Title = "Open UID File";
            fDialog.Filter = "TXT Files (*.txt) | *.txt";
            //fDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            DialogResult result = fDialog.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                string file = fDialog.FileName;
                txtPMImportFile.Text = file;

                MessageBox.Show("Import File có thể gây treo chương trình trong vài giây! Nhấn OK để tiếp tục.");

                int counter = 0;
                string line;

                // Read the file and display it line by line.
                System.IO.StreamReader fileStr = new System.IO.StreamReader(file);
                while ((line = fileStr.ReadLine()) != null)
                {
                    dgUID.Rows.Insert(0, "", line);
                    counter++;
                }

                fileStr.Close();

                MessageBox.Show("Đọc thành công: " + counter + " Profile");
            }
            else
            {
                txtPMImportFile.Text = "";
            }
        }

        private void btnPMClear_Click(object sender, EventArgs e)
        {
            dgUID.Rows.Clear();
        }

        private void btnPMExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Filter = "TXT files (*.txt)|*.txt";
            saveFile.FileName = "MEMBERS.txt";
            saveFile.ShowDialog();

            using (StreamWriter sw = new StreamWriter(saveFile.FileName, false))
            {
                if (dgUID.Rows.Count > 0)
                {
                    foreach (DataGridViewRow row in dgUID.Rows)
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

        private void btnPMExportXLS_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Xuất file Exel có thể gây treo chương trình trong vài giây! Nhấn OK để tiếp tục.");
            ExportDataGridViewTo_Excel12(dgUID);
        }

        public static void ExportDataGridViewTo_Excel12(DataGridView myDataGridViewQuantity)
        {

            Excel_12.Application oExcel_12 = null; //Excel_12 Application 

            Excel_12.Workbook oBook = null; // Excel_12 Workbook 

            Excel_12.Sheets oSheetsColl = null; // Excel_12 Worksheets collection 

            Excel_12.Worksheet oSheet = null; // Excel_12 Worksheet 

            Excel_12.Range oRange = null; // Cell or Range in worksheet 

            Object oMissing = System.Reflection.Missing.Value;


            // Create an instance of Excel_12. 

            oExcel_12 = new Excel_12.Application();


            // Make Excel_12 visible to the user. 

            oExcel_12.Visible = true;


            // Set the UserControl property so Excel_12 won't shut down. 

            oExcel_12.UserControl = true;

            // System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("en-US"); 

            //object file = File_Name;

            //object missing = System.Reflection.Missing.Value;



            // Add a workbook. 

            oBook = oExcel_12.Workbooks.Add(oMissing);

            // Get worksheets collection 

            oSheetsColl = oExcel_12.Worksheets;

            // Get Worksheet "Sheet1" 

            oSheet = (Excel_12.Worksheet)oSheetsColl.get_Item("Sheet1");
            oSheet.Name = "Danh sách UID";

            // Export titles 

            for (int j = 0; j < myDataGridViewQuantity.Columns.Count; j++)
            {

                oRange = (Excel_12.Range)oSheet.Cells[1, j + 1];

                oRange.Value2 = myDataGridViewQuantity.Columns[j].HeaderText;

            }

            // Export data 

            for (int i = 0; i < myDataGridViewQuantity.Rows.Count; i++)
            {

                for (int j = 0; j < myDataGridViewQuantity.Columns.Count; j++)
                {
                    oRange = (Excel_12.Range)oSheet.Cells[i + 2, j + 1];
                    try
                    {
                        oRange.Value2 = myDataGridViewQuantity[j, i].Value;
                    }
                    catch { }

                }

            }
            oBook = null;
            //oExcel_12.Quit();
            //oExcel_12 = null;
            GC.Collect();
        }

        private void btnPM_Click(object sender, EventArgs e)
        {
            if (!SE.ready)
            {
                MessageBox.Show("Chương trình đang thực hiện 1 tác vụ khác!");
                return;
            }

            if (txtPM.Text == "")
            {
                MessageBox.Show("Điền nội dung tin nhắn!");
                return;
            }

            int delay;

            if (!int.TryParse(txtPMDelay.Text, out delay) || delay < 0)
            {
                MessageBox.Show("Số giây Delay: số nguyên không nhỏ hơn 0");
                return;
            }

            dgUID.Enabled = false;
            txtPM.Enabled = false;
            txtPMDelay.Enabled = false;
            btnPM.Enabled = false;
            btnPMPause.Enabled = true;

            SE.AutoPM();
        }

        private void btnPMSendFrRequests_Click(object sender, EventArgs e)
        {
            if (SE.ready == false)
            {
                MessageBox.Show("Chương trình đang thực hiện 1 tác vụ khác");
                return;
            }

            if (dgUID.Rows.Count == 0)
            {
                MessageBox.Show("Danh sách kết bạn trống! Hãy nạp DS từ nhóm hoặc từ bạn bè hoặc từ file trước khi thực hiện tác vụ này!");
                return;
            }

            int delay;

            if (!int.TryParse(txtPMDelay.Text, out delay) || delay < 0)
            {
                MessageBox.Show("Số giây Delay: số nguyên không nhỏ hơn 0");
                return;
            }

            btnPMSendFrRequests.Enabled = false;
            btnPMSendFrRequestsPause.Enabled = true;

            SE.AutoAddFriends();
        }

        private void btnPMSendFrRequestsPause_Click(object sender, EventArgs e)
        {
            SE.pause = true;
        }

        private void btnPMInsertName_Click(object sender, EventArgs e)
        {
            txtPM.AppendText("{username}");
            txtPM.Focus();
        }
        #endregion

        #region OTHER HELPERS
        public void addGroup2Grid(IWebElement k)
        {
            //dgGroups.Rows.Add(k.GetAttribute("innerHTML"), k.GetAttribute("href"), "");
            Thread t = new Thread(() => Program.mainForm.Invoke(new MethodInvoker(delegate() { dgGroups.Rows.Insert(0, k.GetAttribute("innerHTML"), k.GetAttribute("href"), ""); })));
            t.Start();
        }

        private void lblVer_Click(object sender, EventArgs e)
        {
            if (!btnToggle.Enabled)
            {
                Random rnd = new Random();
                if (rnd.Next(10) == 0)
                {
                    btnToggle.Enabled = true;
                }
            }
        }

        private void btnLicense_Click(object sender, EventArgs e)
        {
            License licForm = new License();
            licForm.ShowDialog();
        }

        private void btnTermsPolicies_Click(object sender, EventArgs e)
        {
            TermsPolicies TPForm = new TermsPolicies();
            TPForm.ShowDialog();
        }
        #endregion

        #region CHAT
        private async void ChatRefresh()
        {
            await Chat_Refresh();
        }

        private async Task Chat_Refresh()
        {
            try
            {
                String machine_id = "";
                if (Properties.Settings.Default.license_id == "")
                {
                    machine_id = await Task.Factory.StartNew(() => FingerPrint.Value());
                    Properties.Settings.Default.license_id = machine_id;
                    Properties.Settings.Default.Save();
                }
                else
                {
                    machine_id = Properties.Settings.Default.license_id;
                }

                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(CHAT_URL + "?id=" + machine_id);
                myRequest.Method = "GET";
                WebResponse myResponse = await myRequest.GetResponseAsync();
                StreamReader sr = new StreamReader(myResponse.GetResponseStream(), System.Text.Encoding.UTF8);
                string result = sr.ReadToEnd();
                txtChatLog.Text = result;
                sr.Close();
                myResponse.Close();

                if (result == "trial?")
                {
                    if (MessageBox.Show("Bạn có muốn kích hoạt bản dùng thử trong 7 ngày ngay bây giờ?", "Kích hoạt bản dùng thử", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.Yes)
                    {
                        myRequest = (HttpWebRequest)WebRequest.Create(CHAT_URL + "?id=" + machine_id + "&trial=1");
                        myResponse = await myRequest.GetResponseAsync();
                        myResponse.Close();
                        MessageBox.Show("Bạn đã kích hoạt thành công! Nhấn OK để đóng chương trình! Khởi động lại để sử dụng phiên bản dùng thử 7 ngày!");
                        Process.GetCurrentProcess().Kill();
                    }
                }

                if (result == "DENIED")
                {
                    MessageBox.Show("Vui lòng gia hạn bản quyền!\nGia hạn bản quyền bằng cách Copy lại MACHINE ID, email tới kitsudo1412@gmail.com\nNhấn OK để xem MACHINE ID!");
                    License licForm = new License();
                    licForm.ShowDialog();
                    Process.GetCurrentProcess().Kill();
                }
            }
            catch
            {
                MessageBox.Show("Không thể kết nối đến Server! Vui lòng thử lại trong giây lát hoặc liên hệ với chúng tôi để được hỗ trợ!");
                Process.GetCurrentProcess().Kill();
            }
        }
        #endregion

        #region TAB FANPAGE
        private async void fanpage_init()
        {
            if (SE.pages.Count > 0) return;

            PictureBox loading = new PictureBox();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoadingForm));
            loading.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            loading.Size = new System.Drawing.Size(172, 107);
            loading.Location = new Point(89, 0);
            panelFanpageComment.Controls.Add(loading);

            await SE.getPages();

            panelFanpageComment.Controls.Remove(loading);
            loading.Dispose();
            // logical :]]
            if (SE.pages.Count > 0)
            {
                int top_pos = 0;
                cbFanpage.Items.Clear();
                panelFanpageComment.Controls.Clear();
                foreach (string page_title in SE.pages.Keys)
                {
                    CheckBox cb = new CheckBox();
                    panelFanpageComment.Controls.Add(cb);
                    cb.Location = new Point(10, top_pos);
                    cb.Size = new System.Drawing.Size(300, 20);
                    cb.Checked = true;
                    cb.Text = page_title;
                    top_pos += 20;

                    cbFanpage.Items.Add(page_title);
                }

                cbFanpage.SelectedIndex = 0;
                cbFanpage.Enabled = true;
            }
            else
            {
                CheckBox cb = new CheckBox();
                panelFanpageComment.Controls.Add(cb);
                cb.Location = new Point(10, 0);
                cb.Size = new System.Drawing.Size(300, 20);
                cb.Text = "Bạn chưa thích Fanpage nào cả!";
                cb.Enabled = false;
                cbFanpage.Items.Add("Bạn chưa thích Fanpage nào cả!");
                cbFanpage.SelectedIndex = 0;
                cbFanpage.Enabled = false;
            }
        }

        private void btnFanpageReload_Click(object sender, EventArgs e)
        {
            if (SE.ready == false)
            {
                MessageBox.Show("Chương trình đang thực hiện 1 tác vụ khác");
                return;
            }

            SE.pages = new Dictionary<string, string>();
            fanpage_init();
        }

        private void btnFanpageComment_Click(object sender, EventArgs e)
        {
            if (SE.pages.Count == 0)
            {
                MessageBox.Show("Bạn chưa like Page nào cả!");
                return;
            }

            if (SE.ready == false)
            {
                MessageBox.Show("Chương trình đang thực hiện 1 tác vụ khác");
                return;
            }
            int delay;

            if (!int.TryParse(txtFanpageCommentDelay.Text, out delay) || delay < 0)
            {
                MessageBox.Show("Số giây Delay: số nguyên không nhỏ hơn 0");
                return;
            }

            if (txtFanpageComment.Text == "")
            {
                MessageBox.Show("Điền nội dung bình luận");
                return;
            }

            btnFanpageComment.Enabled = false;
            btnFanpageCommentPause.Enabled = true;

            SE.FanpageComment();
        }

        private void btnFanpageCommentPause_Click(object sender, EventArgs e)
        {
            SE.pause = true;
        }

        private void btnFanpageGroupPost_Click(object sender, EventArgs e)
        {
            if (SE.pages.Count == 0)
            {
                MessageBox.Show("Bạn chưa like Page nào cả!");
                return;
            }

            if (SE.ready == false)
            {
                MessageBox.Show("Chương trình đang thực hiện 1 tác vụ khác");
                return;
            }
            int delay;

            if (!int.TryParse(txtFanpageGroupPostDelay.Text, out delay) || delay < 0)
            {
                MessageBox.Show("Số giây Delay: số nguyên không nhỏ hơn 0");
                return;
            }

            btnFanpageGroupPost.Enabled = false;
            btnFanpageGroupPostPause.Enabled = true;

            SE.FanpagePost();
        }

        private void btnFanpageGroupPostPause_Click(object sender, EventArgs e)
        {
            SE.pause = true;
        }

        private void btnFanpageInviteFriends_Click(object sender, EventArgs e)
        {
            if (btnFanpageInviteFriends.Text != "Mời tất cả bạn bè")
            {
                SE.pause = true;
                return;
            }

            if (MessageBox.Show("Mời toàn bộ bạn bè thích trang này?", "Fanpage", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.Yes)
            {
                if (SE.ready == false)
                {
                    MessageBox.Show("Chương trình đang thực hiện 1 tác vụ khác");
                    return;
                }

                btnFanpageInviteFriends.Enabled = false;

                SE.FanpageInviteFriends();
            }
        }
        #endregion

        #region TAB EDIT
        private void btnEditBrowse_Click(object sender, EventArgs e)
        {
            var fDialog = new System.Windows.Forms.OpenFileDialog();
            fDialog.Title = "Open Post IDS File";
            fDialog.Filter = "TXT Files (*.txt) | *.txt";

            DialogResult result = fDialog.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                string file = fDialog.FileName;
                txtEditBrowse.Text = file;

                MessageBox.Show("Import File có thể gây treo chương trình trong vài giây! Nhấn OK để tiếp tục.");

                int counter = 0;
                string line;

                // Read the file and display it line by line.
                System.IO.StreamReader fileStr = new System.IO.StreamReader(file);
                while ((line = fileStr.ReadLine()) != null)
                {
                    if (line != "")
                    {
                        dgEditBrowse.Rows.Insert(0, line);
                        counter++;
                    }
                }

                fileStr.Close();

                MessageBox.Show("Đọc thành công: " + counter + " bài đăng");
            }
            else
            {
                txtEditBrowse.Text = "";
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (SE.ready == false)
            {
                MessageBox.Show("Chương trình đang thực hiện 1 tác vụ khác");
                return;
            }
            int delay;

            if (!int.TryParse(txtEditDelay.Text, out delay) || delay < 0)
            {
                MessageBox.Show("Số giây Delay: số nguyên không nhỏ hơn 0");
                return;
            }

            if (txtEditContent.Text == "")
            {
                MessageBox.Show("Không được bỏ trống nội dung Sửa");
                return;
            }

            txtEditContent.Enabled = false;
            txtEditDelay.Enabled = false;
            btnEditPause.Enabled = true;
            btnEditBrowse.Enabled = false;
            btnEdit.Enabled = false;
            dgEditBrowse.Enabled = false;

            SE.AutoEdit();
        }

        private void btnEditPause_Click(object sender, EventArgs e)
        {
            btnEditPause.Enabled = false;
            SE.pause = true;
        }
        #endregion    
    }
}

using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using System.Collections.Generic;

public class GuestReseter : Form
{
    private Button btnReset;
    private Button btnScan;
    private Label lblStatus;
    private Label lblTitle;
    private ListBox lstFiles;
    private string adbPath = @"C:\platform-tools\adb.exe";
    private string adbDevice = "127.0.0.1:5555";
    private string foundPackage = "";

    private static readonly string[] FF_PACKAGES = {
        "com.dts.freefireth",
        "com.garena.game.fftw",
        "com.dts.freefiremax"
    };

    private static readonly string[] GUEST_KEYWORDS = {
        "guest", "token", "uid", "account", "login", "auth"
    };

    public GuestReseter()
    {
        this.Text = "FF Guest ID Reseter -- ADB";
        this.Size = new System.Drawing.Size(500, 420);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = System.Drawing.Color.FromArgb(20, 20, 30);

        lblTitle = new Label();
        lblTitle.Text = "Free Fire Guest ID Reseter";
        lblTitle.Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold);
        lblTitle.ForeColor = System.Drawing.Color.Orange;
        lblTitle.Location = new System.Drawing.Point(100, 15);
        lblTitle.Size = new System.Drawing.Size(300, 28);
        lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

        lblStatus = new Label();
        lblStatus.Text = "MSI App Player চালু রাখো, তারপর Scan করো";
        lblStatus.Font = new System.Drawing.Font("Segoe UI", 9);
        lblStatus.ForeColor = System.Drawing.Color.LightGray;
        lblStatus.Location = new System.Drawing.Point(15, 50);
        lblStatus.Size = new System.Drawing.Size(460, 20);
        lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

        lstFiles = new ListBox();
        lstFiles.Location = new System.Drawing.Point(15, 80);
        lstFiles.Size = new System.Drawing.Size(460, 220);
        lstFiles.BackColor = System.Drawing.Color.FromArgb(30, 30, 45);
        lstFiles.ForeColor = System.Drawing.Color.LightGreen;
        lstFiles.Font = new System.Drawing.Font("Consolas", 9);

        btnScan = new Button();
        btnScan.Text = "Scan Guest Files";
        btnScan.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
        btnScan.BackColor = System.Drawing.Color.SteelBlue;
        btnScan.ForeColor = System.Drawing.Color.White;
        btnScan.FlatStyle = FlatStyle.Flat;
        btnScan.Location = new System.Drawing.Point(60, 320);
        btnScan.Size = new System.Drawing.Size(160, 38);
        btnScan.Click += BtnScan_Click;

        btnReset = new Button();
        btnReset.Text = "Reset Guest ID";
        btnReset.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
        btnReset.BackColor = System.Drawing.Color.OrangeRed;
        btnReset.ForeColor = System.Drawing.Color.White;
        btnReset.FlatStyle = FlatStyle.Flat;
        btnReset.Location = new System.Drawing.Point(270, 320);
        btnReset.Size = new System.Drawing.Size(160, 38);
        btnReset.Enabled = false;
        btnReset.Click += BtnReset_Click;

        this.Controls.Add(lblTitle);
        this.Controls.Add(lblStatus);
        this.Controls.Add(lstFiles);
        this.Controls.Add(btnScan);
        this.Controls.Add(btnReset);
    }

    private string RunAdb(string args)
    {
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = adbPath;
            psi.Arguments = args;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;

            Process p = Process.Start(psi);
            string output = p.StandardOutput.ReadToEnd();
            string err = p.StandardError.ReadToEnd();
            p.WaitForExit();
            return output + err;
        }
        catch (Exception ex)
        {
            return "ERROR: " + ex.Message;
        }
    }

    private bool IsGuestFile(string fname)
    {
        string lower = fname.ToLower();
        for (int i = 0; i < GUEST_KEYWORDS.Length; i++)
        {
            if (lower.Contains(GUEST_KEYWORDS[i]))
                return true;
        }
        return false;
    }

    private void BtnScan_Click(object sender, EventArgs e)
    {
        lstFiles.Items.Clear();
        btnReset.Enabled = false;
        btnScan.Enabled = false;
        foundPackage = "";
        lblStatus.ForeColor = System.Drawing.Color.Yellow;
        lblStatus.Text = "Connecting to MSI App Player...";

        Thread t = new Thread(delegate()
        {
            RunAdb("connect " + adbDevice);
            Thread.Sleep(1000);

            string pkg = "";
            string fileList = "";

            for (int i = 0; i < FF_PACKAGES.Length; i++)
            {
                string result = RunAdb("-s " + adbDevice + " shell ls /data/data/" + FF_PACKAGES[i] + "/shared_prefs/");
                if (!string.IsNullOrEmpty(result.Trim()) && !result.Contains("No such file") && !result.Contains("Permission denied"))
                {
                    pkg = FF_PACKAGES[i];
                    fileList = result;
                    break;
                }
            }

            string pkgCopy = pkg;
            string fileListCopy = fileList;

            this.Invoke(new Action(delegate()
            {
                if (string.IsNullOrEmpty(pkgCopy))
                {
                    lblStatus.ForeColor = System.Drawing.Color.Red;
                    lblStatus.Text = "Free Fire খুঁজে পায়নি। MSI App Player চালু আছে?";
                    btnScan.Enabled = true;
                    return;
                }

                foundPackage = pkgCopy;
                lstFiles.Items.Add("Package: " + pkgCopy);
                lstFiles.Items.Add("------------------------");

                string[] lines = fileListCopy.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                int guestCount = 0;

                for (int i = 0; i < lines.Length; i++)
                {
                    string fname = lines[i].Trim();
                    if (string.IsNullOrEmpty(fname)) continue;

                    if (IsGuestFile(fname))
                    {
                        lstFiles.Items.Add("[GUEST] " + fname);
                        guestCount++;
                    }
                    else
                    {
                        lstFiles.Items.Add("[KEEP]  " + fname);
                    }
                }

                if (guestCount > 0)
                {
                    lblStatus.ForeColor = System.Drawing.Color.LimeGreen;
                    lblStatus.Text = string.Format("{0} guest file পাওয়া গেছে। Reset করতে পারো।", guestCount);
                    btnReset.Enabled = true;
                }
                else
                {
                    lblStatus.ForeColor = System.Drawing.Color.Yellow;
                    lblStatus.Text = "Scan শেষ। Reset চাপো।";
                    btnReset.Enabled = true;
                }

                btnScan.Enabled = true;
            }));
        });
        t.IsBackground = true;
        t.Start();
    }

    private void BtnReset_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(foundPackage)) return;

        var result = MessageBox.Show(
            "শুধু Guest ID এর file গুলো delete হবে।\nবাকি game data থাকবে।\n\nনিশ্চিত?",
            "Confirm Reset",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning
        );
        if (result != DialogResult.Yes) return;

        btnReset.Enabled = false;
        btnScan.Enabled = false;
        lblStatus.ForeColor = System.Drawing.Color.Yellow;
        lblStatus.Text = "Resetting...";

        List<string> toDelete = new List<string>();
        for (int i = 0; i < lstFiles.Items.Count; i++)
        {
            string item = lstFiles.Items[i].ToString();
            if (item.StartsWith("[GUEST]"))
            {
                toDelete.Add(item.Substring(8).Trim());
            }
        }

        string pkg = foundPackage;
        List<string> toDeleteCopy = toDelete;

        Thread t = new Thread(delegate()
        {
            int deleted = 0;

            for (int i = 0; i < toDeleteCopy.Count; i++)
            {
                string fname = toDeleteCopy[i];
                RunAdb("-s " + adbDevice + " shell rm /data/data/" + pkg + "/shared_prefs/" + fname);
                deleted++;
            }

            string[] extraFiles = {
                "guest_info.xml",
                "GuestAccount.xml",
                "guest_account.xml",
                "GuestLogin.xml",
                "guest_token.xml"
            };

            for (int i = 0; i < extraFiles.Length; i++)
            {
                RunAdb("-s " + adbDevice + " shell rm /data/data/" + pkg + "/shared_prefs/" + extraFiles[i]);
            }

            int deletedCopy = deleted;
            this.Invoke(new Action(delegate()
            {
                lblStatus.ForeColor = System.Drawing.Color.LimeGreen;
                lblStatus.Text = string.Format("Done! {0} file deleted। FF চালাও -- নতুন Guest ID পাবে।", deletedCopy);
                btnScan.Enabled = true;
            }));
        });
        t.IsBackground = true;
        t.Start();
    }

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new GuestReseter());
    }
}

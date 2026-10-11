using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;

public class GuestReseter : Form
{
    private Button btnReset;
    private Button btnScan;
    private Label lblStatus;
    private Label lblTitle;
    private ListBox lstFiles;
    private string adbPath = @"C:\platform-tools\adb.exe";
    private string adbDevice = "127.0.0.1:5555";

    private static readonly string[] FF_PACKAGES = {
        "com.dts.freefireth",
        "com.garena.game.fftw",
        "com.dts.freefiremax"
    };

    private static readonly string[] GUEST_PREF_KEYWORDS = {
        "guest", "Guest", "GUEST",
        "token", "Token", "TOKEN",
        "uid", "Uid", "UID",
        "account", "Account",
        "login", "Login",
        "auth", "Auth"
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

    private void BtnScan_Click(object sender, EventArgs e)
    {
        lstFiles.Items.Clear();
        btnReset.Enabled = false;
        btnScan.Enabled = false;
        lblStatus.ForeColor = System.Drawing.Color.Yellow;
        lblStatus.Text = "Connecting to MSI App Player...";

        Thread t = new Thread(delegate()
        {
            // Connect ADB
            RunAdb("connect " + adbDevice);
            Thread.Sleep(1000);

            string foundPackage = "";
            string foundFiles = "";

            foreach (string pkg in FF_PACKAGES)
            {
                string result = RunAdb("-s " + adbDevice + " shell ls /data/data/" + pkg + "/shared_prefs/ 2>/dev/null");
                if (!string.IsNullOrEmpty(result.Trim()) && !result.Contains("No such file"))
                {
                    foundPackage = pkg;
                    foundFiles = result;
                    break;
                }
            }

            this.Invoke(new Action(delegate()
            {
                if (string.IsNullOrEmpty(foundPackage))
                {
                    lblStatus.ForeColor = System.Drawing.Color.Red;
                    lblStatus.Text = "Free Fire খুঁজে পায়নি। MSI App Player চালু আছে?";
                    btnScan.Enabled = true;
                    return;
                }

                lstFiles.Items.Add("Package: " + foundPackage);
                lstFiles.Items.Add("---shared_prefs files---");

                string[] lines = foundFiles.Split(new char[]{'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries);
                int guestCount = 0;

                foreach (string line in lines)
                {
                    string fname = line.Trim();
                    if (string.IsNullOrEmpty(fname)) continue;

                    bool isGuest = false;
                    foreach (string kw in GUEST_PREF_KEYWORDS)
                    {
                        if (fname.ToLower().Contains(kw.ToLower()))
                        {
                            isGuest = true;
                            break;
                        }
                    }

                    if (isGuest)
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
                    lblStatus.Text = "Guest file আলাদা করা যাচ্ছে না -- সব file দেখাচ্ছে।";
                    btnReset.Enabled = true;
                }

                btnScan.Enabled = true;
                this.Tag = foundPackage;
            }));
        });
        t.IsBackground = true;
        t.Start();
    }

    private void BtnReset_Click(object sender, EventArgs e)
    {
        string pkg = this.Tag as string;
        if (string.IsNullOrEmpty(pkg)) return;

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

        Thread t = new Thread(delegate()
        {
            int deleted = 0;

            foreach (ListBox.ObjectCollection items = lstFiles.Items;;)
            {
                string[] itemsCopy = null;
                this.Invoke(new Action(delegate()
                {
                    itemsCopy = new string[lstFiles.Items.Count];
                    for (int i = 0; i < lstFiles.Items.Count; i++)
                        itemsCopy[i] = lstFiles.Items[i].ToString();
                }));

                foreach (string item in itemsCopy)
                {
                    if (item.StartsWith("[GUEST]"))
                    {
                        string fname = item.Replace("[GUEST] ", "").Trim();
                        string delCmd = "-s " + adbDevice + " shell rm /data/data/" + pkg + "/shared_prefs/" + fname;
                        RunAdb(delCmd);
                        deleted++;
                    }
                }
                break;
            }

            // Also delete guest_info if exists
            string[] extraFiles = {
                "guest_info.xml",
                "GuestAccount.xml",
                "guest_account.xml",
                "GuestLogin.xml",
                "guest_token.xml",
                "PlayerPrefs.xml"
            };

            foreach (string f in extraFiles)
            {
                RunAdb("-s " + adbDevice + " shell rm /data/data/" + pkg + "/shared_prefs/" + f + " 2>/dev/null");
            }

            this.Invoke(new Action(delegate()
            {
                lblStatus.ForeColor = System.Drawing.Color.LimeGreen;
                lblStatus.Text = string.Format("Done! {0} file deleted। FF চালাও -- নতুন Guest ID পাবে।", deleted);
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

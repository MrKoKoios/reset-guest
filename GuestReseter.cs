using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

public class GuestReseter : Form
{
    private Button btnReset;
    private Label lblStatus;
    private Label lblTitle;

    private static readonly string APPDATA_LOCAL =
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private static readonly string PROGRAMDATA =
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
    private static readonly string USERPROFILE =
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    public GuestReseter()
    {
        this.Text = "FF Guest ID Reseter - BlueStacks";
        this.Size = new System.Drawing.Size(420, 220);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = System.Drawing.Color.FromArgb(20, 20, 30);

        lblTitle = new Label();
        lblTitle.Text = "Free Fire Guest ID Reseter";
        lblTitle.Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold);
        lblTitle.ForeColor = System.Drawing.Color.Orange;
        lblTitle.Location = new System.Drawing.Point(70, 20);
        lblTitle.Size = new System.Drawing.Size(280, 30);
        lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

        lblStatus = new Label();
        lblStatus.Text = "Ready -- BlueStacks Free Fire data clear করবে";
        lblStatus.Font = new System.Drawing.Font("Segoe UI", 9);
        lblStatus.ForeColor = System.Drawing.Color.LightGray;
        lblStatus.Location = new System.Drawing.Point(20, 65);
        lblStatus.Size = new System.Drawing.Size(380, 50);
        lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

        btnReset = new Button();
        btnReset.Text = "Reset Guest ID";
        btnReset.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
        btnReset.BackColor = System.Drawing.Color.OrangeRed;
        btnReset.ForeColor = System.Drawing.Color.White;
        btnReset.FlatStyle = FlatStyle.Flat;
        btnReset.Location = new System.Drawing.Point(130, 140);
        btnReset.Size = new System.Drawing.Size(160, 38);
        btnReset.Click += BtnReset_Click;

        this.Controls.Add(lblTitle);
        this.Controls.Add(lblStatus);
        this.Controls.Add(btnReset);
    }

    private void BtnReset_Click(object sender, EventArgs e)
    {
        var result = MessageBox.Show(
            "BlueStacks এ Free Fire এর Guest ID reset হবে।\nBlueStacks বন্ধ আছে তো?\n\nনিশ্চিত?",
            "Confirm Reset",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning
        );

        if (result != DialogResult.Yes) return;

        btnReset.Enabled = false;
        lblStatus.Text = "Resetting...";
        lblStatus.ForeColor = System.Drawing.Color.Yellow;

        try
        {
            int cleared = 0;

            // BlueStacks NXT (latest) paths
            string[] bsNxtPaths = {
                Path.Combine(PROGRAMDATA, @"BlueStacks_nxt\Engine\UserData\SharedFolder"),
                Path.Combine(PROGRAMDATA, @"BlueStacks_nxt\Engine\Android\data\com.dts.freefireth"),
                Path.Combine(PROGRAMDATA, @"BlueStacks_nxt\Engine\Android\data\com.garena.game.fftw"),
                Path.Combine(APPDATA_LOCAL, @"BlueStacks_nxt\GameData\com.dts.freefireth"),
                Path.Combine(APPDATA_LOCAL, @"BlueStacks_nxt\GameData\com.garena.game.fftw"),
            };

            // BlueStacks 5 paths
            string[] bs5Paths = {
                Path.Combine(PROGRAMDATA, @"BlueStacks\Engine\UserData\SharedFolder"),
                Path.Combine(PROGRAMDATA, @"BlueStacks\Engine\Android\data\com.dts.freefireth"),
                Path.Combine(PROGRAMDATA, @"BlueStacks\Engine\Android\data\com.garena.game.fftw"),
                Path.Combine(APPDATA_LOCAL, @"BlueStacks\GameData\com.dts.freefireth"),
                Path.Combine(APPDATA_LOCAL, @"BlueStacks\GameData\com.garena.game.fftw"),
            };

            // BlueStacks HD (older) paths
            string[] bsHDPaths = {
                Path.Combine(USERPROFILE, @"BlueStacks\Android\data\com.dts.freefireth"),
                Path.Combine(USERPROFILE, @"BlueStacks\Android\data\com.garena.game.fftw"),
            };

            // Registry keys for BlueStacks FF
            string[] bsRegKeys = {
                @"SOFTWARE\BlueStacks\Guests\com.dts.freefireth",
                @"SOFTWARE\BlueStacks\Guests\com.garena.game.fftw",
                @"SOFTWARE\BlueStacks_nxt\Guests\com.dts.freefireth",
                @"SOFTWARE\BlueStacks_nxt\Guests\com.garena.game.fftw",
                @"SOFTWARE\WOW6432Node\BlueStacks\Guests\com.dts.freefireth",
                @"SOFTWARE\WOW6432Node\BlueStacks_nxt\Guests\com.dts.freefireth",
            };

            // SharedPreferences -- guest token lives here
            string[] sharedPrefPaths = {
                Path.Combine(PROGRAMDATA, @"BlueStacks_nxt\Engine\Android\data\com.dts.freefireth\shared_prefs"),
                Path.Combine(PROGRAMDATA, @"BlueStacks_nxt\Engine\Android\data\com.garena.game.fftw\shared_prefs"),
                Path.Combine(PROGRAMDATA, @"BlueStacks\Engine\Android\data\com.dts.freefireth\shared_prefs"),
                Path.Combine(PROGRAMDATA, @"BlueStacks\Engine\Android\data\com.garena.game.fftw\shared_prefs"),
            };

            // Clear all data paths
            string[][] allPaths = { bsNxtPaths, bs5Paths, bsHDPaths, sharedPrefPaths };
            foreach (string[] group in allPaths)
            {
                foreach (string path in group)
                {
                    if (Directory.Exists(path))
                    {
                        ClearDirectory(path);
                        cleared++;
                    }
                }
            }

            // Clear registry
            foreach (string regKey in bsRegKeys)
            {
                try
                {
                    Registry.CurrentUser.DeleteSubKeyTree(regKey, throwOnMissingSubKey: false);
                    Registry.LocalMachine.DeleteSubKeyTree(regKey, throwOnMissingSubKey: false);
                    cleared++;
                }
                catch { }
            }

            if (cleared == 0)
            {
                lblStatus.ForeColor = System.Drawing.Color.Yellow;
                lblStatus.Text = "BlueStacks data খুঁজে পায়নি।\nBlueStacks installed আছে?";
            }
            else
            {
                lblStatus.ForeColor = System.Drawing.Color.LimeGreen;
                lblStatus.Text = string.Format(
                    "Done! {0} location(s) cleared.\nBlueStacks খুলে FF চালাও -- নতুন Guest ID পাবে।",
                    cleared
                );
            }
        }
        catch (Exception ex)
        {
            lblStatus.ForeColor = System.Drawing.Color.Red;
            lblStatus.Text = "Error: " + ex.Message;
        }
        finally
        {
            btnReset.Enabled = true;
        }
    }

    private void ClearDirectory(string path)
    {
        foreach (string file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
        {
            try { File.Delete(file); } catch { }
        }
        foreach (string dir in Directory.GetDirectories(path))
        {
            try { Directory.Delete(dir, recursive: true); } catch { }
        }
    }

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new GuestReseter());
    }
}

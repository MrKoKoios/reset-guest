using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

public class GuestReseter : Form
{
    private Button btnReset;
    private Label lblStatus;
    private Label lblTitle;

    private static readonly string[] FF_DATA_PATHS = {
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            @"Garena\Free Fire"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            @"Garena\Free Fire"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low",
            @"Garena\Free Fire"),
    };

    private static readonly string[] FF_REG_KEYS = {
        @"SOFTWARE\Garena\Free Fire",
        @"SOFTWARE\WOW6432Node\Garena\Free Fire",
    };

    public GuestReseter()
    {
        this.Text = "FF Guest ID Reseter";
        this.Size = new System.Drawing.Size(380, 200);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = System.Drawing.Color.FromArgb(20, 20, 30);

        lblTitle = new Label();
        lblTitle.Text = "Free Fire Guest ID Reseter";
        lblTitle.Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold);
        lblTitle.ForeColor = System.Drawing.Color.Orange;
        lblTitle.Location = new System.Drawing.Point(70, 20);
        lblTitle.Size = new System.Drawing.Size(260, 30);
        lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

        lblStatus = new Label();
        lblStatus.Text = "Ready";
        lblStatus.Font = new System.Drawing.Font("Segoe UI", 9);
        lblStatus.ForeColor = System.Drawing.Color.LightGray;
        lblStatus.Location = new System.Drawing.Point(20, 65);
        lblStatus.Size = new System.Drawing.Size(330, 40);
        lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

        btnReset = new Button();
        btnReset.Text = "Reset Guest ID";
        btnReset.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
        btnReset.BackColor = System.Drawing.Color.OrangeRed;
        btnReset.ForeColor = System.Drawing.Color.White;
        btnReset.FlatStyle = FlatStyle.Flat;
        btnReset.Location = new System.Drawing.Point(110, 120);
        btnReset.Size = new System.Drawing.Size(150, 35);
        btnReset.Click += BtnReset_Click;

        this.Controls.Add(lblTitle);
        this.Controls.Add(lblStatus);
        this.Controls.Add(btnReset);
    }

    private void BtnReset_Click(object sender, EventArgs e)
    {
        var result = MessageBox.Show(
            "Guest ID reset করলে তোমার current guest account চলে যাবে।\nনিশ্চিত?",
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

            foreach (string path in FF_DATA_PATHS)
            {
                if (Directory.Exists(path))
                {
                    ClearDirectory(path);
                    cleared++;
                }
            }

            foreach (string regKey in FF_REG_KEYS)
            {
                try
                {
                    using (var key = Registry.CurrentUser.OpenSubKey(regKey, writable: true))
                    {
                        if (key != null)
                        {
                            foreach (string valueName in key.GetValueNames())
                            {
                                if (valueName.ToLower().Contains("guest") ||
                                    valueName.ToLower().Contains("token") ||
                                    valueName.ToLower().Contains("uid") ||
                                    valueName.ToLower().Contains("userid"))
                                {
                                    key.DeleteValue(valueName, throwOnMissingValue: false);
                                    cleared++;
                                }
                            }
                        }
                    }
                    Registry.CurrentUser.DeleteSubKeyTree(regKey, throwOnMissingSubKey: false);
                }
                catch { }
            }

            string unityPrefsKey = @"SOFTWARE\Garena Online Pte. Ltd\Free Fire";
            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(unityPrefsKey, throwOnMissingSubKey: false);
                Registry.LocalMachine.DeleteSubKeyTree(unityPrefsKey, throwOnMissingSubKey: false);
            }
            catch { }

            lblStatus.ForeColor = System.Drawing.Color.LimeGreen;
            lblStatus.Text = string.Format("Done! {0} location(s) cleared.\nFF খুললে নতুন Guest ID পাবে।", cleared);
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

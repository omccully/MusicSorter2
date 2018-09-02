using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using Shell32;
using System.IO;

namespace MusicSorter2
{
    public partial class MainForm : Form
    {
        enum SorterMode
        {
            Full, Unpack, Move, Rename
        };

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FreeConsole();

        SorterMode SelectedSorterMode
        {
            get
            {
                return (SorterMode)Enum.Parse(typeof(SorterMode), ModeComboBox.SelectedValue.ToString());
            }
        }

        UserSettings Settings;

        public MainForm(UserSettings settings)
        {
            FreeConsole();
            InitializeComponent();
            ModeComboBox.DataSource = Enum.GetValues(typeof(SorterMode));
            
            ModeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;

            try
            {
                InitializeFromSettings(settings);
            }
            catch (Exception e)
            {
                MessageBox.Show("Failed to restore from settings" + Environment.NewLine +
                    e.ToString());
            }
        }

        const string FolderBoxSettingKey = "DefaultSortRootFolder";
        const string FormatComboBoxSettingKey = "DefaultSongNameFormatter";
        const string RenameNotificationKey = "RenameNotificationKey";
        const string CreatedNotificationKey = "CreatedNotificationKey";
        const string MovedNotificationKey = "MovedNotificationKey";
        void InitializeFromSettings(UserSettings settings)
        {
            this.Settings = settings;

            FolderBox.Text = settings.GetValue(FolderBoxSettingKey, "").ToString();

            string formatter = settings.GetValue(FormatComboBoxSettingKey, "").ToString();
            if(String.IsNullOrWhiteSpace(formatter))
            {
                FormatComboBox.SelectedIndex = 0;
            }
            else
            {
                FormatComboBox.Text = formatter;
            }



            RenameCheck.Checked = settings.GetBoolean(RenameNotificationKey, false);
            CreatedCheck.Checked = settings.GetBoolean(CreatedNotificationKey, false);
            MovedCheck.Checked = settings.GetBoolean(MovedNotificationKey, false);
        }


        void SaveStateToSettings(UserSettings settings)
        {
            settings.SetValue(FolderBoxSettingKey, FolderBox.Text);
            settings.SetValue(FormatComboBoxSettingKey, FormatComboBox.Text);
            settings.SetValue(RenameNotificationKey, RenameCheck.Checked);
            settings.SetValue(CreatedNotificationKey, CreatedCheck.Checked);
            settings.SetValue(MovedNotificationKey, MovedCheck.Checked);
        }

        private void BrowseBut_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                FolderBox.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void HelpLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            FormatHelpForm fnf = new FormatHelpForm();
            fnf.ShowDialog();
        }

        void ExecuteStep(int step_num, string step_description, Action step_action)
        {
            Stopwatch step_time = new Stopwatch();
            LogInColor($"\nStarting step {step_num}: {step_description}\n", ConsoleColor.Green);
            step_time.Start();
            step_action.Invoke();
            step_time.Stop();
            LogInColor($"Completed step {step_num}. {step_time.ElapsedMilliseconds} ms\n", ConsoleColor.Green);
        }

        private void StartBut_Click(object sender, EventArgs e)
        {
            FixFolderBox();
            if (!Directory.Exists(FolderBox.Text))
            {
                MessageBox.Show($"The directory {FolderBox.Text} does not exist");
                return;
            }

            SaveStateToSettings(Settings);

            Stopwatch full_time = new Stopwatch();

            AllocConsole();

            try
            {
                Sorter sorter = new Sorter(FolderBox.Text, new SongFileNameFormatter(FormatComboBox.Text));
                if (MovedCheck.Checked) sorter.FileUnpacked += Sorter_FileUnpacked;
                if (CreatedCheck.Checked)
                {
                    sorter.FolderCreated += Sorter_FolderCreated;
                    sorter.FileMoved += Sorter_FileMoved;
                }
                if (RenameCheck.Checked) sorter.FileRenamed += Sorter_FileRenamed;

                LogInColor("Run at your own risk!\n" + 
                    "\t- Files may be renamed or lost.\n" + 
                    "\t- " + FolderBox.Text + " folder should not contain any files that you don't want moved.\n" +
                    "\t- You should make a backup of " + FolderBox.Text + " before proceeding.\n" + 
                    "\t- " + FolderBox.Text + " should contain mostly audio files and folders.\n", ConsoleColor.Red);
                LogInColor("You selected the " + SelectedSorterMode + " mode.", ConsoleColor.DarkYellow);
                
                Console.WriteLine();
                Console.WriteLine("Press ENTER to continue.");
                Console.ReadLine();
                Console.WriteLine("Starting...");
                full_time.Start();

                if (SelectedSorterMode == SorterMode.Full || SelectedSorterMode == SorterMode.Unpack)
                {
                    ExecuteStep(1, "Unpacking files", delegate
                    {
                        sorter.UnpackAll();
                    });
                }
                Console.Out.Flush();
                if (SelectedSorterMode == SorterMode.Full || SelectedSorterMode == SorterMode.Move)
                {
                    ExecuteStep(2, "Making folders and moving files", delegate
                    {
                        // rename files during PackAll if we're doing a full sort
                        sorter.PackAll(SelectedSorterMode == SorterMode.Full);
                    });
                }
                else if (SelectedSorterMode == SorterMode.Rename)
                {
                    // This step isn't necessary when doing a Full sort because
                    // step 2 renames the files (for performance reasons)
                    ExecuteStep(3, "Renaming files", delegate
                    {
                        sorter.NameChange();
                    });
                }

                full_time.Stop();

                Console.WriteLine("\nDone in " + full_time.ElapsedMilliseconds + "ms. ");
            }
            catch (Exception ex)
            {
                LogInColor("An error has been caught. The sorter has been stopped. The error message is:\n" + ex,
                    ConsoleColor.Red);
                Console.WriteLine();
            }
            Console.WriteLine("Press ENTER to continue.");
            Console.ReadLine();
            FreeConsole();
        }

        private void Sorter_FileMoved(object sender, FileChangedEventArgs e)
        {
            Console.WriteLine("Moved " + e.PathA + " -> " + e.PathB);
        }

        void Sorter_FileUnpacked(object o, FileChangedEventArgs e)
        {
            Console.WriteLine("Unpacked " + e.PathA + " -> " + e.PathB);
        }

        private void Sorter_FileRenamed(object sender, FileChangedEventArgs e)
        {
            Console.WriteLine("Renamed " + e.PathA + " -> " + e.PathB);
        }

        private void Sorter_FolderCreated(object sender, FolderCreatedEventArgs e)
        {
            Console.WriteLine("Created " + e.Path);
        }

        private string FixFolderBox()
        {
            FolderBox.Text = FolderBox.Text.Replace("/", @"\").MakeLegalPath(true);

            if (!FolderBox.Text.EndsWith(@"\"))
            {
                FolderBox.Text += @"\";
            }
            return FolderBox.Text;
        }

        /// <summary>
        /// Prints s to console in given color
        /// </summary>
        /// <param name="s">String to be written to console</param>
        /// <param name="cc">Color of the string written to console</param>
        public static void LogInColor(string s, ConsoleColor cc)
        {
            Console.ForegroundColor = cc;
            Console.Write(s);
            Console.ResetColor();
        }
    }
}

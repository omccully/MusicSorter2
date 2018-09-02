using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using Shell32;

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

        public MainForm()
        {
            FreeConsole();
            InitializeComponent();
            ModeComboBox.DataSource = Enum.GetValues(typeof(SorterMode));
            FormatComboBox.SelectedIndex = 0;
            ModeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
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

        private void StartBut_Click(object sender, EventArgs e)
        {
            Stopwatch full_time = new Stopwatch();
            Stopwatch step_time = new Stopwatch();

            AllocConsole();

            try
            {
                Sorter sorter = new Sorter(FixFolderBox(), new SongFileNameFormatter(FormatComboBox.Text));
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
                    LogInColor("\nStarting step 1: Unpacking files\n", ConsoleColor.Green);
                    step_time.Start();
                    sorter.UnpackAll();
                    step_time.Stop();
                    LogInColor("Completed step 1. " + step_time.ElapsedMilliseconds + " ms\n", ConsoleColor.Green);
                }
                Console.Out.Flush();
                if (SelectedSorterMode == SorterMode.Full || SelectedSorterMode == SorterMode.Move)
                {
                    step_time.Reset();
                    LogInColor("\nStarting step 2: Making folders and moving files.\n", ConsoleColor.Green);
                    step_time.Start();
                    sorter.PackAll(ModeComboBox.SelectedIndex == 0);
                    step_time.Stop();
                    LogInColor("Completed step 2. " + step_time.ElapsedMilliseconds + " ms\n", ConsoleColor.Green);
                }
                else if (SelectedSorterMode == SorterMode.Rename)
                {
                    step_time.Reset();
                    LogInColor("\nStarting step 3: Renaming files.\n", ConsoleColor.Green);
                    step_time.Start();
                    sorter.NameChange();
                    step_time.Stop();
                    LogInColor("Completed step 3. " + step_time.ElapsedMilliseconds + " ms\n", ConsoleColor.Green);
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

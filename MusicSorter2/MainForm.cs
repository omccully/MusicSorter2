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
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FreeConsole();

        public MainForm()
        {
            InitializeComponent();
            ModeComboBox.SelectedIndex = 0;
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
            Stopwatch full = new Stopwatch();
            Stopwatch s = new Stopwatch();
            
            AllocConsole();

            try
            {
                //Console.WriteLine("Finding extended file property IDs...");
                Sorter sorter = new Sorter(FixFolderBox(), FormatComboBox.Text);
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
                    "\t- " + FolderBox.Text + " should contain mostly audio files and folders.", ConsoleColor.Red);
                Console.WriteLine();
                Console.WriteLine("Press ENTER to continue.");
                Console.ReadLine();
                Console.WriteLine("Starting...");
                full.Start();

                if (ModeComboBox.SelectedIndex == 0 || ModeComboBox.SelectedIndex == 1)
                {
                    LogInColor("\nStarting step 1: Unpacking files\n", ConsoleColor.Green);
                    s.Start();
                    sorter.UnpackAll();
                    s.Stop();
                    LogInColor("Completed step 1. " + s.ElapsedMilliseconds + " ms\n", ConsoleColor.Green);
                }
                Console.Out.Flush();
                if (ModeComboBox.SelectedIndex == 0 || ModeComboBox.SelectedIndex == 2)
                {
                    s.Reset();
                    LogInColor("\nStarting step 2: Making folders and moving files.\n", ConsoleColor.Green);
                    s.Start();
                    sorter.MakeDirs(ModeComboBox.SelectedIndex == 0);
                    s.Stop();
                    LogInColor("Completed step 2. " + s.ElapsedMilliseconds + " ms\n", ConsoleColor.Green);
                }
                else if (ModeComboBox.SelectedIndex == 3)
                {
                    s.Reset();
                    LogInColor("\nStarting step 3: Renaming files.\n", ConsoleColor.Green);
                    s.Start();
                    sorter.NameChange();
                    s.Stop();
                    LogInColor("Completed step 3. " + s.ElapsedMilliseconds + " ms\n", ConsoleColor.Green);
                }

                full.Stop();

                Console.WriteLine("\nDone in " + full.ElapsedMilliseconds + "ms. ");
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
            FolderBox.Text = Sorter.MakeLegal(FolderBox.Text.Replace("/", @"\"), true);

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

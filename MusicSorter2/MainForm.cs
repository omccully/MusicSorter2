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

        Sorter sorter = new Sorter();

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
            sorter.nameBuilder = new NameBuilder(FormatComboBox.Text);
            AllocConsole();
            sorter.RootPath = FixFolderBox();
            sorter.ShowMovedUpdates = MovedCheck.Checked;
            sorter.ShowCreatedUpdates = CreatedCheck.Checked;
            sorter.ShowRenameUpdates = RenameCheck.Checked;
            Console.WriteLine("Finding extended file property IDs...");
            try
            {
                Shell shell = new Shell();
                Folder fold = shell.NameSpace(FolderBox.Text);

                foreach (Shell32.FolderItem2 item in fold.Items())
                {
                    for (int i = 0; i < short.MaxValue; i++)
                    {
                        string header = fold.GetDetailsOf(null, i);
                        if (String.IsNullOrEmpty(header))
                            break;

                        switch (header)
                        {
                            case "Title":
                                sorter.TitleNum = i;
                                Console.WriteLine("Title = " + i);
                                break;
                            case "Album artist":
                            case "Artist":
                                sorter.ArtistNum = i;
                                Console.WriteLine("Artist = " + i);
                                break;
                            case "Album Title":
                            case "Album":
                                sorter.AlbumNum = i;
                                Console.WriteLine("Album = " + i);
                                break;
                            case "Contributing artists":
                                sorter.ContribArtistsNum = i;
                                Console.WriteLine("Constributing artists = " + i);
                                break;
                            case "#":
                            case "Track Number":
                                sorter.TrackNum = i;
                                Console.WriteLine("Track# = " + i);
                                break;
                        }
                    }
                    break;
                }
                Sorter.logc("Run at your own risk! Files may be renamed or lost.\n" + FolderBox.Text + " folder should not contain any files that you don't want moved! This folder should contain mostly audio files and folders.", ConsoleColor.Red);
                Console.WriteLine("Press ENTER to continue.");
                Console.ReadLine();

                full.Start();

                if (ModeComboBox.SelectedIndex == 0 || ModeComboBox.SelectedIndex == 1)
                {
                    Sorter.logc("\nStarting step 1: Unpacking files\n", ConsoleColor.Green);
                    s.Start();
                    sorter.UnpackAll(FolderBox.Text);
                    s.Stop();
                    Sorter.logc("Completed step 1. " + s.ElapsedMilliseconds.ToString() + " ms\n", ConsoleColor.Green);
                }
                if (ModeComboBox.SelectedIndex == 0 || ModeComboBox.SelectedIndex == 2)
                {
                    s.Reset();
                    Sorter.logc("\nStarting step 2: Making folders and moving files.\n", ConsoleColor.Green);
                    s.Start();
                    sorter.MakeDirs(FolderBox.Text, ModeComboBox.SelectedIndex == 0);
                    s.Stop();
                    Sorter.logc("Completed step 2. " + s.ElapsedMilliseconds.ToString() + " ms\n", ConsoleColor.Green);
                }
                else if (ModeComboBox.SelectedIndex == 3)
                {
                    s.Reset();
                    Sorter.logc("\nStarting step 3: Renaming files.\n", ConsoleColor.Green);
                    s.Start();
                    sorter.NameChange(FolderBox.Text);
                    s.Stop();
                    Sorter.logc("Completed step 3. " + s.ElapsedMilliseconds.ToString() + " ms\n", ConsoleColor.Green);
                }

                full.Stop();

                Console.WriteLine("\nDone in " + full.ElapsedMilliseconds.ToString() + "ms. ");
            }
            catch (Exception ex)
            {
                Sorter.logc("An error has been caught. The sorter has been stopped. The error message is:\n" + ex.ToString(),
                    ConsoleColor.Red);
            }
            Console.WriteLine("Press ENTER to continue.");
            Console.ReadLine();
            FreeConsole();
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
    }
}

using System;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace MusicSorter2
{
    class Sorter
    {
        public int TitleNum = -1;
        public int ArtistNum = -1;
        public int AlbumNum = -1;
        public int ContribArtistsNum = -1;
        public int TrackNum = -1;
        public NameBuilder nameBuilder { get; set; }
        public string RootPath { get; set; }

        public bool ShowMovedUpdates = false;
        public bool ShowCreatedUpdates = false;
        public bool ShowRenameUpdates = false;


        /// <summary>
        /// Prints s to console in given color
        /// </summary>
        /// <param name="s">String to be written to console</param>
        /// <param name="cc">Color of the string written to console</param>
        public static void logc(string s, ConsoleColor cc)
        {
            Console.ForegroundColor = cc;
            Console.Write(s);
            Console.ResetColor();
        }

        /// <summary>
        /// Removes wildcard characters from a file name or path string
        /// </summary>
        /// <param name="s">File name or path string</param>
        /// <param name="IsPath">Set true if is a path</param>
        /// <returns>String without wildcard characters</returns>
        public static string MakeLegal(string s, bool IsPath)
        {
            string Result = s.Replace("*", "").
                Replace("?", "").Replace("\"", "").
                Replace("<", "").Replace(">", "").
                Replace("|", "");

            return IsPath ? Result : Result.Replace(@"\", "").
                Replace("/", "").Replace(":", "");
        }

        /// <summary>
        /// Step 1: Moves files from folders inside dir and moves files to FolderBox.Text.
        /// </summary>
        /// <param name="dir">The folder path to start in. Uses recursion to access subfolders.</param>
        public void UnpackAll(string dir)
        {
            foreach (string i in Directory.GetDirectories(dir))
            {
                UnpackAll(i);
                foreach (string o in Directory.GetFiles(i))
                {
                    string filename = Path.GetFileName(o);

                    if (File.Exists(Path.Combine(RootPath, filename)))
                    {
                        filename = Path.GetFileNameWithoutExtension(o);
                        string ext = Path.GetExtension(o);
                        string errorsuffix = "";
                        int errors = 1;

                        do
                        {
                            errors++;
                            errorsuffix = errors.ToString();
                        } while (File.Exists(Path.Combine(RootPath, filename + errorsuffix + ext)));

                        filename += errorsuffix + ext;
                    }

                    File.Move(o, Path.Combine(RootPath, filename));

                    if (ShowMovedUpdates)
                    {
                        Console.WriteLine("Moved " + o);
                    }
                }
                Directory.Delete(i);
            }
        }

        /// <summary>
        /// Step 2: Makes folders and moves files into folders based on root\artist\album\song.mp3
        /// </summary>
        /// <param name="dir">The root folder path</param>
        public void MakeDirs(string dir, bool RenameFiles)
        {
            Shell32.Shell shell = new Shell32.Shell();
            Shell32.Folder objFolder = shell.NameSpace(dir);

            foreach (Shell32.FolderItem2 item in objFolder.Items())
            {
                if (item.IsFolder) continue;

                string FileName = Path.GetFileName(item.Path);
                string Artist = GetArtist(objFolder, item);
                string Album = "unknown";

                //14 Album
                string temp = objFolder.GetDetailsOf(item, this.AlbumNum);
                if (temp != "")
                {
                    Album = MakeLegal(temp, false);
                }

                temp = Path.Combine(Path.Combine(dir, Artist), Album);
                if (!Directory.Exists(temp))
                {
                    Directory.CreateDirectory(temp);
                    if (ShowCreatedUpdates)
                    {
                        Console.WriteLine(temp + "\\ created");
                    }
                }

                if (RenameFiles)
                {
                    FileName = GetNewFileName(temp, item, objFolder);
                }

                File.Move(item.Path, Path.Combine(temp, FileName));
            }
        }



        /// <summary>
        /// Step 3: Changes file names
        /// </summary>
        /// <param name="dir">The folder path to start in. Uses recursion to access subfolders.</param>
        public void NameChange(string dir)
        {
            Shell32.Shell shell = new Shell32.Shell();
            Shell32.Folder objFolder = shell.NameSpace(dir);

            foreach (Shell32.FolderItem2 item in objFolder.Items())
            {
                if (item.IsFolder)
                {
                    NameChange(item.Path);
                    continue;
                }
                string NewName = GetNewFileName(dir, item, objFolder);
                string NewPath;
                if (item.Path != (NewPath = Path.Combine(dir, NewName)))
                {
                    File.Move(item.Path, NewPath);
                }
            }
        }

        public string GetNewFileName(string dir, Shell32.FolderItem2 item, Shell32.Folder objFolder)
        {
            string FileName = Path.GetFileName(item.Path);

            string ext = Path.GetExtension(FileName);
            string Title = MakeLegal(objFolder.GetDetailsOf(item, this.TitleNum), false);
            string Album = MakeLegal(objFolder.GetDetailsOf(item, this.AlbumNum), false);
            string Artist = GetArtist(objFolder, item);
            string TrackNumber = MakeLegal(objFolder.GetDetailsOf(item, this.TrackNum), false);
            int Errors = 1;
            string NewName;

            if (TrackNumber == "" || Title == "" || Album == "" || Artist == "")
            {
                NewName = Path.GetFileNameWithoutExtension(FileName);
            }
            else
            {
                NewName = nameBuilder.Build(TrackNumber, Title, Album, Artist);
            }

            string NewFileName = NewName + ext;

            if (File.Exists(Path.Combine(dir, NewFileName)))
            {
                while (File.Exists(Path.Combine(dir, (NewFileName = NewName + Errors.ToString() + ext))))
                {
                    Errors++;
                }
            }
            if (ShowRenameUpdates && FileName != NewFileName)
            {
                logc(FileName + " ==> " + NewFileName + "\n",
                    Errors != 1 ? ConsoleColor.Yellow : ConsoleColor.Gray);
            }
            return MakeLegal(NewFileName, false);
        }

        public string GetArtist(Shell32.Folder objFolder, Shell32.FolderItem2 item)
        {
            //216 Album artist
            string temp = objFolder.GetDetailsOf(item, this.ArtistNum);
            if (temp != "")
            {
                return MakeLegal(temp, false);
            }
            else if (this.ContribArtistsNum != -1)
            {
                //13 Contributing artists, uses this if (Album artist == "")
                temp = objFolder.GetDetailsOf(item, this.ContribArtistsNum);
                if (temp != "")
                {
                    return MakeLegal(temp, false);
                }
            }
            return "unknown";
        }
    }
}

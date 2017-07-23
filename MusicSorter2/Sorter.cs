using System;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using Shell32;

namespace MusicSorter2
{
    class FileChangedEventArgs : EventArgs
    {
        public string PathA { get; private set; }
        public string PathB { get; private set; }

        public FileChangedEventArgs(string PathA, string PathB)
        {
            this.PathA = PathA;
            this.PathB = PathB;
        }
    }

    class FolderCreatedEventArgs : EventArgs
    {
        public string Path { get; private set; }

        public FolderCreatedEventArgs(string Path)
        {
            this.Path = Path;
        }
    }

    class Sorter
    {
        // The indexes for each of the file properties
        // These indexes may change depending on the operating system
        int TitleNum { get; set; }
        int ArtistNum { get; set; }
        int AlbumNum { get; set; }
        int ContribArtistsNum { get; set; }
        int TrackNum { get; set; }

        NameBuilder Bob { get; set; }
        string RootPath { get; set; }

        #region Events
        public delegate void FileChangedEventHandler(object sender, FileChangedEventArgs e);
        public event FileChangedEventHandler FileUnpacked;
        protected virtual void OnFileUnpacked(FileChangedEventArgs e)
        {
            FileUnpacked(this, e);
        }

        public delegate void FolderCreatedEventHandler(object sender, FolderCreatedEventArgs e);
        public event FolderCreatedEventHandler FolderCreated;
        protected virtual void OnFolderCreated(FolderCreatedEventArgs e)
        {
            FolderCreated(this, e);
        }

        public event FileChangedEventHandler FileMoved;
        protected virtual void OnFileMoved(FileChangedEventArgs e)
        {
            FileMoved(this, e);
        }

        public event FileChangedEventHandler FileRenamed;
        protected virtual void OnFileRenamed(FileChangedEventArgs e)
        {
            FileRenamed(this, e);
        }
        #endregion

        public Sorter(string RootPath, string FileNameFormat)
        {
            this.RootPath = RootPath;
            TitleNum = ArtistNum = AlbumNum = ContribArtistsNum = TrackNum = -1;
            Bob = new NameBuilder(FileNameFormat);

            Shell shell = new Shell();
            Folder fold = shell.NameSpace(RootPath);

            // Loop through property names to find indexes for needed properties
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
                            TitleNum = i;
                            break;
                        case "Album artist":
                        case "Artist":
                        case "Authors":
                            ArtistNum = i;
                            break;
                        case "Album Title":
                        case "Album":
                            AlbumNum = i;
                            break;
                        case "Contributing artists":
                            ContribArtistsNum = i;
                            break;
                        case "#":
                        case "Track Number":
                            TrackNum = i;
                            break;
                    }
                }
                break;
            }

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
        /// Step 1: Moves files inside dir and moves files to RootPath.
        /// </summary>
        /// <param name="dir">The folder path to start in. Uses recursion to access subfolders.</param>
        public void UnpackAll(string dir=null)
        {
            if (dir == null) dir = RootPath;
            foreach (string d in Directory.GetDirectories(dir))
            {
                UnpackAll(d); // recursively unpack this subdirectory
                foreach (string f in Directory.GetFiles(d))
                {
                    string filename = Path.GetFileName(f);

                    if (File.Exists(Path.Combine(RootPath, filename)))
                    {
                        // a file with this name already exists in RootPath
                        filename = Path.GetFileNameWithoutExtension(f);
                        string ext = Path.GetExtension(f);
                        string errorsuffix = "";
                        int errors = 1;

                        // add a number at the end of the file name until no other file with this name exists
                        do
                        {
                            errors++;
                            errorsuffix = errors.ToString();
                        } while (File.Exists(Path.Combine(RootPath, filename + errorsuffix + ext)));

                        filename += errorsuffix + ext;
                    }

                    // move file f to the RootPath directory
                    File.Move(f, Path.Combine(RootPath, filename));

                    // Notify client of file change
                    FileUnpacked(this, new FileChangedEventArgs(f, Path.Combine(RootPath, filename)));
                }

                Directory.Delete(d); // directory should be empty now, so delete it
            }
        }

        /// <summary>
        /// Step 2: Makes folders and moves files into folders based on root\artist\album\song.mp3
        /// </summary>
        /// <param name="RenameFiles">If true, this method also does Step 3 (to improve efficiency)</param>
        public void MakeDirs(bool RenameFiles)
        {
            Shell shell = new Shell();
            Folder objFolder = shell.NameSpace(RootPath);

            // Loop through all files in RootPath
            // move files into RootPath/<artist>/<album>/ based on each file's properties
            // create directories when necessary
            foreach (FolderItem2 item in objFolder.Items())
            {
                if (item.IsFolder) continue;

                string FileName = Path.GetFileName(item.Path);
                string Artist = GetArtist(objFolder, item);
                string Album = "unknown";

                // Album
                string temp = objFolder.GetDetailsOf(item, this.AlbumNum);
                if (temp != "")
                {
                    Album = MakeLegal(temp, false);
                }

                temp = Path.Combine(Path.Combine(RootPath, Artist), Album);
                if (!Directory.Exists(temp))
                {
                    Directory.CreateDirectory(temp);
                    FolderCreated(this, new FolderCreatedEventArgs(temp));
                }

                if (RenameFiles)
                {
                    // Do step 3's job more effiently while we're at it
                    FileName = GetNewFileName(temp, item, objFolder);
                    // Perhaps add an event here?
                }

                // File moved
                File.Move(item.Path, Path.Combine(temp, FileName));
                FileMoved(this, new FileChangedEventArgs(item.Path, Path.Combine(temp, FileName)));
            }
        }



        /// <summary>
        /// Step 3: Changes file names of all 
        /// </summary>
        /// <param name="dir">The folder path to start in. Subfolders are recursively accessed.</param>
        public void NameChange(string dir=null)
        {
            if (dir == null) dir = RootPath;
            Shell shell = new Shell();
            Folder objFolder = shell.NameSpace(dir);

            foreach (FolderItem2 item in objFolder.Items())
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
                    FileRenamed(this, new FileChangedEventArgs(item.Path, NewPath));
                }
            }
        }

        string GetNewFileName(string dir, Shell32.FolderItem2 item, Shell32.Folder objFolder)
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
                NewName = Bob.Build(TrackNumber, Title, Album, Artist);
            }

            string NewFileName = NewName + ext;

            if (File.Exists(Path.Combine(dir, NewFileName)))
            {
                // a file with this name already exists in dir
                // append an integer to the end of the file name until it's unique
                while (File.Exists(Path.Combine(dir, (NewFileName = NewName + Errors.ToString() + ext))))
                {
                    Errors++;
                }
            }

            return MakeLegal(NewFileName, false);
        }

        public string GetArtist(Shell32.Folder objFolder, Shell32.FolderItem2 item)
        {
            // Album artist
            string temp = objFolder.GetDetailsOf(item, this.ArtistNum);
            if (temp != "")
            {
                return MakeLegal(temp, false);
            }
            else if (this.ContribArtistsNum != -1)
            {
                // Contributing artists, uses this if (Album artist == "")
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

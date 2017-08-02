using System;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Globalization;
using System.Diagnostics;
using System.Linq;
using Shell32;


namespace MusicSorter2
{
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
        public void PackAll(bool RenameFiles)
        {
            FilePropertiesReader directory_reader = new FilePropertiesReader(RootPath);

            // Loop through all files in RootPath
            // move files into RootPath/<artist>/<album>/ based on each file's properties
            // create directories when necessary
            foreach (FileProperties fp in directory_reader.GetFiles())
            {
                string FileName = Path.GetFileName(fp.Path);
                string Artist = MakeLegal(Unknownify(fp.AnyArtist), false);
                string Album = MakeLegal(Unknownify(fp.Album), false);

               
                string temp = Path.Combine(Path.Combine(RootPath, Artist), Album);
                if (!Directory.Exists(temp))
                {
                    Directory.CreateDirectory(temp);
                    FolderCreated(this, new FolderCreatedEventArgs(temp));
                }

                if (RenameFiles)
                {
                    // Do step 3's job more effiently while we're at it
                    FileName = GetNewFileName(temp, fp);
                    // Perhaps add an event here?
                }

                // File moved
                File.Move(fp.Path, Path.Combine(temp, FileName));
                FileMoved(this, new FileChangedEventArgs(fp.Path, Path.Combine(temp, FileName)));
            }
        }

        /// <summary>
        /// Step 3: Changes file names of all 
        /// </summary>
        /// <param name="dir">The folder path to start in. Subfolders are recursively accessed.</param>
        public void NameChange(string dir=null)
        {
            if (dir == null) dir = RootPath;

            FilePropertiesReader directory_reader = new FilePropertiesReader(dir);

            // Loop through 
            foreach (FileProperties fp in directory_reader.GetFilesAndDirectories())
            {
                if (fp.IsDirectory)
                {
                    NameChange(fp.Path);
                    continue;
                }
                string NewFileName = BuildNewFileNameFromProperties(fp);
                
                if (Path.GetFileName(fp.Path) != NewFileName)
                {
                    string NewPath = GetAvailableFilePath(dir, NewFileName);
                    // if the name is actually different, change it
                    File.Move(fp.Path, NewPath);
                    FileRenamed?.Invoke(this, new FileChangedEventArgs(fp.Path, NewPath));
                }
            }
        }


        /// <summary>
        /// Determine the new name for the file based on the format
        /// and FileProperties <paramref name="fp"/>
        /// </summary>
        /// <param name="fp">Properties for the file</param>
        /// <returns></returns>
        string BuildNewFileNameFromProperties(FileProperties fp)
        {
            string FileName = Path.GetFileName(fp.Path);
            string ext = Path.GetExtension(FileName);

            if ((Bob.RequiresTag("#") && fp.TrackNumber == "") ||
                (Bob.RequiresTag("T") && fp.Title == "") ||
                (Bob.RequiresTag("AL") && fp.Album == "") ||
                (Bob.RequiresTag("AR") && fp.AnyArtist == ""))
            {
                // if the NameBuilder requires any properties that do
                // not have values for this file, don't change the name.
                return FileName;
            }

            string NewName = Bob.Build(fp.TrackNumber, fp.Title, fp.Album, fp.AnyArtist);
            return (NewName + ext).MakeLegalPath();
        }

        /// <summary>
        /// Gets a file name generated from <paramref name="path"/> is not already taken. 
        /// If <paramref name="path"/> is already taken, then an integer is appended to 
        /// the file before the extension. 
        /// </summary>
        /// <param name="path">Path for desired file name</param>
        /// <returns>Path to an avaialble file name in the same directory as 
        /// <paramref name="filepath"/></returns>
        string GetAvailableFilePath(string filepath)
        {
            return GetAvailableFilePath(Path.GetDirectoryName(filepath),
                Path.GetFileName(filepath));
        }

        /// <summary>
        /// Gets a file name generated from <paramref name="path"/> is not already taken. 
        /// If <paramref name="path"/> is already taken, then an integer is appended to 
        /// the file before the extension. 
        /// </summary>
        /// <param name="dir">Path to look in</param>
        /// <param name="filename">Desired file name</param>
        /// <returns></returns>
        string GetAvailableFilePath(string dir, string filename)
        {
        }

        {
        }
    }
}

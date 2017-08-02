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
        public void MakeDirs(bool RenameFiles)
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
                string NewName = GetNewFileName(dir, fp);
                string NewPath;
                if (fp.Path != (NewPath = Path.Combine(dir, NewName)))
                {
                    // if the name is actually different, change it
                    File.Move(fp.Path, NewPath);
                    FileRenamed(this, new FileChangedEventArgs(fp.Path, NewPath));
                }
            }
        }

        string GetNewFileName(string dir, FileProperties fp)
        {
            string FileName = Path.GetFileName(fp.Path);
            string ext = Path.GetExtension(FileName);
            string NewName;

            if (fp.TrackNumber == "" || fp.Title == "" || 
                fp.Album == "" || fp.AnyArtist == "")
            {
                NewName = Path.GetFileNameWithoutExtension(FileName);
            } else
            {
                NewName = Bob.Build(fp.TrackNumber, fp.Title, fp.Album, fp.AnyArtist);
            }

            string NewFileName = MakeLegal(NewName + ext);

            if (File.Exists(Path.Combine(dir, NewFileName)))
            {
                int errors = 1;
                // a file with this name already exists in dir
                // append an integer to the end of the file name until it's unique
                while (File.Exists(Path.Combine(dir, (NewFileName = NewName + errors.ToString() + ext))))
                {
                    errors++;
                }
            }
            return NewFileName;
        }

        #region Static Helper Methods
        static string Unknownify(string prop_val)
        {
            return String.IsNullOrEmpty(prop_val) ? "unknown" : prop_val;
        }

        /// <summary>
        /// Removes diacritics from a string.
        /// </summary>
        /// <param name="text">A string that may or may not contain diacritics</param>
        /// <returns></returns>
        static string RemoveDiacritics(string text)
        {
            // text.Normalize(NormalizationForm.FormD) splits accented character into 2 chars
            return new string(text.Normalize(NormalizationForm.FormD).ToCharArray().Where(c =>
                // removes the 2nd char, leaving the base character in the string
                CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark
            ).ToArray<char>()).Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// Removes wildcard characters from a file name or path string
        /// </summary>
        /// <param name="s">File name or path string</param>
        /// <param name="IsPath">Set true if s is a path</param>
        /// <returns>String without wildcard characters</returns>
        public static string MakeLegal(string s, bool IsPath = false)
        {
            string Result = RemoveDiacritics(s).Replace("*", "").
                Replace("?", "").Replace("\"", "").
                Replace("<", "").Replace(">", "").
                Replace("|", "");

            return IsPath ? Result : Result.Replace(@"\", "").
                Replace("/", "").Replace(":", "");
        }
        #endregion
    }
}

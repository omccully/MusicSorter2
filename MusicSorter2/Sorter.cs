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
    public class Sorter
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
        public void UnpackAll(string dir = null)
        {
            if (dir == null) dir = RootPath;
            foreach (string d in Directory.GetDirectories(dir))
            {
                UnpackAll(d); // recursively unpack this subdirectory
                foreach (string f in Directory.GetFiles(d))
                {
                    string filename = Path.GetFileName(f);
                    string filepath = GetAvailableFilePath(RootPath, filename);

                    // move file f to the RootPath directory
                    File.Move(f, filepath);

                    // Notify client of file change

                    FileUnpacked?.Invoke(this, new FileChangedEventArgs(f, filepath));
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
                string Artist = Unknownify(fp.AnyArtist).MakeLegalPath(false);
                string Album = Unknownify(fp.Album).MakeLegalPath(false);

                string ArtistDir = Path.Combine(RootPath, Artist);
                string ArtistAlbumDir = Path.Combine(ArtistDir, Album);
                if (!Directory.Exists(ArtistAlbumDir))
                {
                    if (!Directory.Exists(ArtistDir))
                    {
                        // creating ArtistAlbumDir automatically creates ArtistDir
                        FolderCreated?.Invoke(this, new FolderCreatedEventArgs(ArtistDir));
                    }
                    Directory.CreateDirectory(ArtistAlbumDir);

                    FolderCreated?.Invoke(this, new FolderCreatedEventArgs(ArtistAlbumDir));
                }

                if (RenameFiles)
                {
                    // Do step 3's job more effiently while we're at it
                    FileName = BuildNewFileNameFromProperties(fp);
                    // Perhaps add an event here?
                }

                // File moved
                string new_path = GetAvailableFilePath(ArtistAlbumDir, FileName);
                File.Move(fp.Path, new_path);

                FileMoved?.Invoke(this, new FileChangedEventArgs(fp.Path, new_path));
            }
        }

        /// <summary>
        /// Step 3: Changes file names of all 
        /// </summary>
        /// <param name="dir">The folder path to start in. Subfolders are recursively accessed.</param>
        public void NameChange(string dir = null)
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
            string filepath = Path.Combine(dir, filename);
            if (!File.Exists(filepath)) return filepath;
            string filename_noext = Path.GetFileNameWithoutExtension(filename);
            string ext = Path.GetExtension(filename);
            string errorsuffix, new_filepath;
            int errors = 0;

            // add a number at the end of the file name until no other file with this name exists
            do
            {
                errors++;
                errorsuffix = errors.ToString();
                new_filepath = Path.Combine(dir, filename_noext + errorsuffix + ext);
            } while (File.Exists(new_filepath));

            return new_filepath;
        }

        static string Unknownify(string prop_val)
        {
            return String.IsNullOrEmpty(prop_val) ? "unknown" : prop_val;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shell32;
using System.IO;

namespace MusicSorter2
{
    public class FilePropertiesReader
    {
        static Shell shell = null;
        Folder folder { get; set; }
        bool UseRelativePaths { get; set; }

        public FilePropertiesReader(string directory)
        {
            if (shell == null) shell = new Shell();

            string directory_absolute_path;
            this.UseRelativePaths = !Path.IsPathRooted(directory);
            if (this.UseRelativePaths)
            {
                directory_absolute_path = Path.GetFullPath(directory);
            }
            else
            {
                directory_absolute_path = directory;
            }
            
            // shell.NameSpace requires absolute path
            this.folder = shell.NameSpace(directory_absolute_path);

            if (this.folder == null)
            {
                throw new DirectoryNotFoundException($"Directory not found. {directory_absolute_path}");
            }
        }

        public List<FileProperties> GetFiles()
        {
            List<FileProperties> result = new List<FileProperties>();

            foreach (FolderItem2 item in folder.Items())
            {
                if (!item.IsFolder)
                {
                    result.Add(new FileProperties(folder, item, UseRelativePaths));
                }
            }

            return result;
        }

        public List<FileProperties> GetFilesAndDirectories()
        {
            List<FileProperties> result = new List<FileProperties>();

            foreach (FolderItem2 item in folder.Items())
            {
                result.Add(new FileProperties(folder, item, UseRelativePaths));
            }

            return result;
        }

        public string PropertyNameAtIndex(int i)
        {
            return folder.GetDetailsOf(null, i);
        }
    }
}

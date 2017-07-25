using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shell32;
using System.IO;

namespace MusicSorter2
{
    class FilePropertiesReader
    {
        static readonly Shell shell = new Shell();
        Folder folder { get; set; }

        public FilePropertiesReader(string directory)
        {
            this.folder = shell.NameSpace(directory);
            if (this.folder == null)
            {
                throw new DirectoryNotFoundException();
            }
        }

        public List<FileProperties> GetFiles()
        {
            List<FileProperties> result = new List<FileProperties>();

            foreach (FolderItem2 item in folder.Items())
            {
                if (!item.IsFolder)
                {
                    result.Add(new FileProperties(folder, item));
                }
            }

            return result;
        }

        public List<FileProperties> GetFilesAndDirectories()
        {
            List<FileProperties> result = new List<FileProperties>();

            foreach (FolderItem2 item in folder.Items())
            {
                result.Add(new FileProperties(folder, item));
            }

            return result;
        }

        public string PropertyNameAtIndex(int i)
        {
            return folder.GetDetailsOf(null, i);
        }
    }
}

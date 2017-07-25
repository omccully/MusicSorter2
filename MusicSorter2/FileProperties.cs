using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shell32;
using System.IO;

namespace MusicSorter2
{
    class NotInitializedException : Exception
    {

    }

    class FileProperties
    {
        Folder folder { get; set; }
        FolderItem2 item { get; set; }

        public FileProperties(Folder folder, FolderItem2 item)
        {
            this.folder = folder;
            this.item = item;
            if (!FilePropertiesMetadata.Initialized)
                FilePropertiesMetadata.Initialize(folder);
        }

        public string PropertyAtIndex(int i)
        {
            return folder.GetDetailsOf(item, i);
        }

        public string Path
        {
            get
            {
                return item.Path;
            }
        }

        public bool IsDirectory
        {
            get
            {
                return item.IsFolder;
            }
        }

        public string Title
        {
            get
            {
                return PropertyAtIndex(FilePropertiesMetadata.TitleIndex);
            }
        }

        public string Artist
        {
            get
            {
                return PropertyAtIndex(FilePropertiesMetadata.ArtistIndex);
            }
        }

        public string Album
        {
            get
            {
                return PropertyAtIndex(FilePropertiesMetadata.AlbumIndex);
            }
        }

        public string ContributingArtists
        {
            get
            {
                return PropertyAtIndex(FilePropertiesMetadata.ContributingArtistsIndex);
            }
        }

        public string TrackNumber
        {
            get
            {
                return PropertyAtIndex(FilePropertiesMetadata.TrackNumberIndex);
            }
        }

        public string AnyArtist
        {
            get
            {
                if (!String.IsNullOrEmpty(Artist))
                {
                    return Artist;
                }
                return ContributingArtists;
            }
        }

        static class FilePropertiesMetadata
        {
            static int _TitleIndex = -1, _ArtistIndex = -1,
                _AlbumIndex = -1, _ContributingArtistsIndex = -1,
                _TrackNumberIndex = -1;
            public static bool Initialized = false;

            /// <summary>
            /// Initialize indexes for various properties using a sample Folder
            /// </summary>
            /// <param name="folder">Any valid sample Folder object</param>
            public static void Initialize(Folder folder)
            {
                for (int i = 0; i < short.MaxValue; i++)
                {
                    string header = folder.GetDetailsOf(null, i);

                    if (String.IsNullOrEmpty(header))
                        break;

                    switch (header)
                    {
                        case "Title":
                            _TitleIndex = i;
                            break;
                        case "Album artist":
                        case "Artist":
                        case "Authors":
                            _ArtistIndex = i;
                            break;
                        case "Album Title":
                        case "Album":
                            _AlbumIndex = i;
                            break;
                        case "Contributing artists":
                            _ContributingArtistsIndex = i;
                            break;
                        case "#":
                        case "Track Number":
                            _TrackNumberIndex = i;
                            break;
                    }
                }

                Initialized = true;
            }

            public static int TitleIndex
            {
                get
                {
                    if (!Initialized) throw new NotInitializedException();
                    return _TitleIndex;
                }
            }

            public static int ArtistIndex
            {
                get
                {
                    if (!Initialized) throw new NotInitializedException();
                    return _ArtistIndex;
                }
            }

            public static int AlbumIndex
            {
                get
                {
                    if (!Initialized) throw new NotInitializedException();
                    return _AlbumIndex;
                }
            }

            public static int ContributingArtistsIndex
            {
                get
                {
                    if (!Initialized) throw new NotInitializedException();
                    return _ContributingArtistsIndex;
                }
            }

            public static int TrackNumberIndex
            {
                get
                {
                    if (!Initialized) throw new NotInitializedException();
                    return _TrackNumberIndex;
                }
            }
        } // end class FilePropertiesMetadata
    } // end class FileProperties
    
}

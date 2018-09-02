using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MusicSorter2
{
    public class SongFileNameFormatter : IFilePropertiesFormatter
    {
        public const string TrackNumberTag = "#";
        public const string TitleTag = "T";
        public const string AlbumTag = "AL";
        public const string ArtistTag = "AR";

        DictionaryFormatter Formatter;

        public SongFileNameFormatter(DictionaryFormatter formatter)
        {
            this.Formatter = formatter;
        }

        public SongFileNameFormatter(string format)
        {
            this.Formatter = new DictionaryFormatter(format);
        }

        public string Format(FileProperties fp)
        {
            string FileName = Path.GetFileName(fp.Path);
            string ext = Path.GetExtension(FileName);

            if (!HasAllRequiredTags(fp))
            {
                // if the NameBuilder requires any properties that do
                // not have values for this file, don't change the name.
                return FileName;
            }

            Dictionary<string, object> dict = new Dictionary<string, object>()
            {
                { TrackNumberTag, fp.TrackNumber },
                { TitleTag, fp.Title },
                { AlbumTag, fp.Album },
                { ArtistTag, fp.AnyArtist }
            };

            string NewName = Formatter.Format(dict);
            return (NewName + ext).MakeLegalPath();
        }

        bool HasAllRequiredTags(FileProperties fp)
        {
            return !(Formatter.HasTag(TrackNumberTag) && fp.TrackNumber == "") ||
                (Formatter.HasTag(TitleTag) && fp.Title == "") ||
                (Formatter.HasTag(AlbumTag) && fp.Album == "") ||
                (Formatter.HasTag(ArtistTag) && fp.AnyArtist == "");
        }
    }
}

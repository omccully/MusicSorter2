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
        NameBuilder Bob;

        public SongFileNameFormatter(NameBuilder bob)
        {
            this.Bob = bob;
        }

        public SongFileNameFormatter(string format)
        {
            this.Bob = new NameBuilder(format);
        }

        public string Format(FileProperties fp)
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
    }
}

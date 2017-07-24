using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MusicSorter2
{
    /// <summary>
    /// This class is used to improve the efficiency of determining the new name for the music files.
    /// A NameBuilder object with a certain format needs to be constructed once, and subsequent calls to
    /// the Build method are much faster than it would be to use the Format string each time. 
    /// </summary>
    class NameBuilder
    {
        class SongProperty
        {
            public string PropertyName { get; private set; }
            public string PropertyTag { get; private set; }

            public SongProperty(string PropertyName, string PropertyTag)
            {
                this.PropertyName = PropertyName;
                this.PropertyTag = PropertyTag;
            }
        }

        static class SongPropertyCollection
        {
            public static readonly SongProperty[] SongProperties = {
                new SongProperty("TRACK_NUMBER", "#"),
                new SongProperty("SONG_TITLE", "T"),
                new SongProperty("ALBUM", "AL"),
                new SongProperty("ARTIST", "AR")
            };

            public static int IndexOf(SongProperty sa_needle)
            {
                for(int i = 0; i < SongProperties.Length; i++)
                {
                    if (SongProperties[i] == sa_needle) return i;
                }
                throw new Exception("sp_needle not in SongPropertyCollection");
            }

            public static SongProperty SongPropertyFromTag(string tag)
            {
                tag = tag.Replace("{", "").Replace("}", "");
                foreach(SongProperty sa in SongProperties)
                {
                    if(sa.PropertyTag == tag) return sa;
                }
                throw new Exception("Invalid tag");
            }

            /// <summary>
            /// Array of song property tags
            /// </summary>
            public static string[] Tags
            {
                get
                {
                    return SongProperties.Select(attr => attr.PropertyTag).ToArray();
                }
            }

            /// <summary>
            /// Regex to match any of the tags (wrapped in curly braces)
            /// </summary>
            public static Regex TagsRegex
            {
                get
                {
                    // map song property tags so elements have escaped curly braces
                    IEnumerable<string> tagged = Tags.Select(s => "\\{" + s + "\\}");

                    // regex OR between tags
                    string regex = String.Join("|", tagged);

                    return new Regex(regex);
                }
            }
        }

        /* Tags/Text - Variables that express the file name format
         * Invariant: Tags.Count + 1 == Text.Count
         * The resultant file names are built like this using the values passed to Build:
         * Text[0] + valueof(Tags[0]) + Text[1] + valueof(Tags[1]) ... valueof(Tags[n-1]) + Text[n]
         */
        SongProperty[] Tags { get; set; }
        string[] Text { get; set; }

        public NameBuilder(string Format)
        {
            Regex r = SongPropertyCollection.TagsRegex;

            // Get array of non-tag text in Format
            this.Text = r.Split(Format); 

            // Get array of SongProperty objects in order in which they appear in Format
            this.Tags = r.Matches(Format).Cast<Match>().Select(
                m => SongPropertyCollection.SongPropertyFromTag(m.Value)).ToArray();
        }

        /// <summary>
        /// Builds a file name with the format specified by the object and the arguments passed. 
        /// </summary>
        /// <param name="Number"></param>
        /// <param name="Title"></param>
        /// <param name="Album"></param>
        /// <param name="Artist"></param>
        /// <returns></returns>
        public string Build(string Number, string Title, string Album, string Artist)
        {
            string[] prop_vals = { Number, Title, Album, Artist };

            StringBuilder bob = new StringBuilder(Text[0]);
            for (int i = 0; i < Tags.Length; i++)
            {
                for (int j = 0; j < SongPropertyCollection.SongProperties.Length; j++)
                {
                    if(Tags[i] == SongPropertyCollection.SongProperties[j])
                    {
                        bob.Append(prop_vals[j]);
                        break;
                    }
                }

                bob.Append(Text[i + 1]);
            }
            return bob.ToString();
        }
    }
}

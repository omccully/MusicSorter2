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
        class SongAttribute
        {
            public string AttributeName { get; private set; }
            public string AttributeTag { get; private set; }

            public SongAttribute(string AttributeName, string AttributeTag)
            {
                this.AttributeName = AttributeName;
                this.AttributeTag = AttributeTag;
            }
        }

        static class SongAttributeCollection
        {
            public static readonly SongAttribute[] SongAttributes = {
                new SongAttribute("TRACK_NUMBER", "#"),
                new SongAttribute("SONG_TITLE", "T"),
                new SongAttribute("ALBUM", "AL"),
                new SongAttribute("ARTIST", "AR")
            };

            public static int IndexOf(SongAttribute sa_needle)
            {
                for(int i = 0; i < SongAttributes.Length; i++)
                {
                    if (SongAttributes[i] == sa_needle) return i;
                }
                throw new Exception("sa_needle not in SongAttributeCollection");
            }

            public static SongAttribute SongAttributeFromTag(string tag)
            {
                tag = tag.Replace("{", "").Replace("}", "");
                foreach(SongAttribute sa in SongAttributes)
                {
                    if(sa.AttributeTag == tag) return sa;
                }
                throw new Exception("Invalid tag");
            }

            /// <summary>
            /// Array of song attribute tags
            /// </summary>
            public static string[] Tags
            {
                get
                {
                    return SongAttributes.Select(attr => attr.AttributeTag).ToArray();
                }
            }

            /// <summary>
            /// Regex to match any of the tags (wrapped in curly braces)
            /// </summary>
            public static Regex TagsRegex
            {
                get
                {
                    // map SongAttributeTags so elements have escaped curly braces
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
        SongAttribute[] Tags { get; set; }
        string[] Text { get; set; }

        public NameBuilder(string Format)
        {
            Regex r = SongAttributeCollection.TagsRegex;

            // Get array of non-tag text in Format
            this.Text = r.Split(Format); 

            // Get array of SongAttribute objects in order in which they appear in Format
            this.Tags = r.Matches(Format).Cast<Match>().Select(
                m => SongAttributeCollection.SongAttributeFromTag(m.Value)).ToArray();
        }

        /// <summary>
        /// Builds a song name with the format specified by the object and the arguments passed. 
        /// </summary>
        /// <param name="Number"></param>
        /// <param name="Title"></param>
        /// <param name="Album"></param>
        /// <param name="Artist"></param>
        /// <returns></returns>
        public string Build(string Number, string Title, string Album, string Artist)
        {
            string[] attrib_vals = { Number, Title, Album, Artist };

            StringBuilder bob = new StringBuilder(Text[0]);
            for (int i = 0; i < Tags.Length; i++)
            {
                for (int j = 0; j < SongAttributeCollection.SongAttributes.Length; j++)
                {
                    if(Tags[i] == SongAttributeCollection.SongAttributes[j])
                    {
                        bob.Append(attrib_vals[j]);
                        break;
                    }
                }

                bob.Append(Text[i + 1]);
            }
            return bob.ToString();
        }
    }
}

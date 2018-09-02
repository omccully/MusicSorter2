using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MusicSorter2
{
    public class DictionaryFormatter
    {
        string[] IntermediateText; // IntermediateText.Length == Tags.Length + 1
        string[] Tags;

        public DictionaryFormatter(string format)
        {
            // TODO: fix this so there's consistent behavior when 
            // additional curly braces are added to format

            // [1] = text before tag
            // [2] = tag text
            // [3] = text after tag
            
            Regex tags_regex = new Regex("([^}]*){([^}]+)}([^{]*)");
            MatchCollection matches = tags_regex.Matches(format);

            Tags = new string[matches.Count];
            IntermediateText = new string[matches.Count + 1];

            int i = 0;
            foreach (Match m in matches)
            {
                Tags[i] = m.Groups[2].Value;

                if(IntermediateText[i] == null) IntermediateText[i] = m.Groups[1].Value;
                else IntermediateText[i] += m.Groups[1].Value;
               
                i++;

                if (IntermediateText[i] == null) IntermediateText[i] = m.Groups[3].Value;
                else IntermediateText[i] += m.Groups[3].Value;
            }

            System.Diagnostics.Debug.Assert(IntermediateText.Length == Tags.Length + 1);
        }

        public string Format(Dictionary<string, object> dict)
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < Tags.Length; i++)
            {
                builder.Append(IntermediateText[i]);
                builder.Append(dict[Tags[i]]);
            }

            builder.Append(IntermediateText[Tags.Length]);

            return builder.ToString();
        }

        public bool HasTag(string tag)
        {
            return Tags.Contains(tag);
        }
    }
}

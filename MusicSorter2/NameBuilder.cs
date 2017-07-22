using System;
using System.Collections.Generic;
using System.Text;

namespace MusicSorter2
{
    class NameBuilder
    {
        int[] Tags { get; set; }
        string[] Text { get; set; }

        public NameBuilder(string Format)
        {
            List<int> Tags = new List<int>();
            List<string> Text = new List<string>();
            string CurrentTag = null;
            string CurrentText = "";
            int CurlyIndex = 0;
            foreach (char c in Format)
            {
                if (c == '{')
                {
                    Text.Add(CurrentText);
                    CurrentText = null;
                    CurrentTag = "";
                    CurlyIndex++;
                    if (CurlyIndex == 2) throw new Exception("Invalid Tag: Embedded braces");
                }
                else if (c == '}')
                {
                    if (CurlyIndex == 0) throw new Exception("Invalid Tag: Embedded braces");
                    switch (CurrentTag.ToUpper())
                    {
                        case "#":
                            Tags.Add(0);
                            break;
                        case "T":
                            Tags.Add(1);
                            break;
                        case "AL":
                            Tags.Add(2);
                            break;
                        case "AR":
                            Tags.Add(3);
                            break;
                        default:
                            throw new Exception("Invalid Tag: '" + CurrentTag + "'");
                    }
                    CurrentText = "";
                    CurrentTag = null;
                    CurlyIndex--;
                }
                else
                {
                    switch (CurlyIndex)
                    {
                        case 0:
                            CurrentText += c;
                            break;
                        case 1:
                            CurrentTag += c;
                            break;
                    }
                }
            }
            Text.Add(CurrentText);
            this.Text = Text.ToArray();
            this.Tags = Tags.ToArray();
        }

        public string Build(string Number, string Title, string Album, string Artist)
        {
            StringBuilder bob = new StringBuilder(Text[0]);
            for (int i = 0; i < Tags.Length; i++)
            {
                switch (Tags[i])
                {
                    case 0:
                        bob.Append(Number);
                        break;
                    case 1:
                        bob.Append(Title);
                        break;
                    case 2:
                        bob.Append(Album);
                        break;
                    case 3:
                        bob.Append(Artist);
                        break;
                }
                bob.Append(Text[i + 1]);
            }
            return bob.ToString();
        }
    }
}

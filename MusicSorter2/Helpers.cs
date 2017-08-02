using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace MusicSorter2
{
    public static class Helpers
    {
        /// <summary>
        /// Removes diacritics from a string.
        /// </summary>
        /// <param name="text">A string that may or may not contain diacritics</param>
        /// <returns></returns>
        public static string RemoveDiacritics(this string text)
        {
            // text.Normalize(NormalizationForm.FormD) splits accented character into 2 chars
            return new string(text.Normalize(NormalizationForm.FormD).ToCharArray().Where(c =>
                // removes the 2nd char, leaving the base character in the string
                CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark
            ).ToArray<char>()).Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// Removes wildcard characters from a file name or path string
        /// </summary>
        /// <param name="s">File name or path string</param>
        /// <param name="IsPath">Set true if s is a path</param>
        /// <returns>String without wildcard characters</returns>
        public static string RemoveWildcards(this string s, bool IsPath = false)
        {
            string Result = RemoveDiacritics(s).Replace("*", "").
                Replace("?", "").Replace("\"", "").
                Replace("<", "").Replace(">", "").
                Replace("|", "");

            return IsPath ? Result : Result.Replace(@"\", "").
                Replace("/", "").Replace(":", "");
        }

        /// <summary>
        /// Converts s into a legal file path
        /// </summary>
        /// <param name="s"></param>
        /// <param name="IsPath"></param>
        /// <returns></returns>
        public static string MakeLegalPath(this string s, bool IsPath = false)
        {
            return s.RemoveDiacritics().RemoveWildcards(IsPath);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from">base directory</param>
        /// <param name="to">directory deeper than base</param>
        /// <returns></returns>
        public static string GetRelativePath(string from, string to)
        {
            // simple implementation. may not be robust

            from = from.Trim();
            // add \ at end of from (working directory)
            if (!from.EndsWith("\\")) from += '\\';

            to = to.Trim();
            // case insensitive
            if (to.ToLower().StartsWith(from.ToLower()))
            {
                return to.Substring(from.Length);
            }

            throw new ArgumentException();
        }
    }
}

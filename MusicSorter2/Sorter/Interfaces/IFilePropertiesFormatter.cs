using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSorter2
{
    public interface IFilePropertiesFormatter
    {
        /// <summary>
        /// Determine the new name for the file based on the format
        /// and FileProperties <paramref name="fp"/>
        /// </summary>
        /// <param name="fp">Properties for the file</param>
        /// <returns></returns>
        string Format(FileProperties fp);
    }
}

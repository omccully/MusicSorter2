using System;

namespace MusicSorter2
{
    public class FolderCreatedEventArgs : EventArgs
    {
        public string Path { get; private set; }

        public FolderCreatedEventArgs(string Path)
        {
            this.Path = Path;
        }

        public override string ToString()
        {
            return $"{Path}";
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            FolderCreatedEventArgs e = obj as FolderCreatedEventArgs;
            if((object)e == null) return false; //cannot be casted

            return this.Path == e.Path;
        }

        public bool Equals(FolderCreatedEventArgs e)
        {
            if ((object)e == null) return false;
            return Path == e.Path;
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode() ^ base.GetHashCode();
        }

        public static bool operator ==(FolderCreatedEventArgs a, FolderCreatedEventArgs b)
        {
            // If both are null, or both are same instance, return true.
            if (Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false
            if (((object)a == null) ^ ((object)b == null))
            {
                return false;
            }

            return a.Path == b.Path;
        }

        public static bool operator !=(FolderCreatedEventArgs a, FolderCreatedEventArgs b)
        {
            return !(a == b);
        }
    }
}

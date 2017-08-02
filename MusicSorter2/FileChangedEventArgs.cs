using System;

namespace MusicSorter2
{
    public class FileChangedEventArgs : EventArgs
    {
        public string PathA { get; private set; }
        public string PathB { get; private set; }

        public FileChangedEventArgs(string PathA, string PathB)
        {
            this.PathA = PathA;
            this.PathB = PathB;
        }

        public override string ToString()
        {
            return $"{PathA} -> {PathB}";
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            FileChangedEventArgs e = obj as FileChangedEventArgs;
            if ((object)e == null) return false; //cannot be casted

            return this.PathA == e.PathA && this.PathB == e.PathB;
        }

        public bool Equals(FileChangedEventArgs e)
        {
            if ((object)e == null) return false;
            return base.Equals(e) &&
                PathA == e.PathA &&
                PathB == e.PathB;
        }

        public override int GetHashCode()
        {
            return PathA.GetHashCode() ^
                PathB.GetHashCode() ^
                base.GetHashCode();
        }

        public static bool operator ==(FileChangedEventArgs a, FileChangedEventArgs b)
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

            return a.PathA == b.PathA && a.PathB == b.PathB;
        }

        public static bool operator !=(FileChangedEventArgs a, FileChangedEventArgs b)
        {
            return !(a == b);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace TWC.SE.StarBundle
{
    public abstract class FileAction
    {
        private string src;
        private string dest;
        private string starFlags;
        private string heId;
        private string type;
        private FileAction.ActionVersion version;
        private bool sbModified;
        private List<string> starFlagsList;

        private static bool SameString(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) && string.IsNullOrEmpty(s2))
                return true;
            return s1 == s2;
        }

        protected FileAction()
        {
        }

        protected FileAction(FileAction action)
        {
            this.src = action.src;
            this.dest = action.dest;
            this.starFlags = action.starFlags;
            this.heId = action.heId;
            this.type = action.type;
            this.version = action.version.Clone();
        }

        public abstract FileAction Clone();

        [XmlAttribute(AttributeName = "src")]
        public string Src
        {
            get
            {
                return this.src;
            }
            set
            {
                this.src = value;
            }
        }

        [XmlAttribute(AttributeName = "dest")]
        public string Dest
        {
            get
            {
                return this.dest.Replace('/', '\\');
            }
            set
            {
                this.dest = value;
            }
        }

        [XmlAttribute(AttributeName = "starFlags")]
        public string StarFlags
        {
            get
            {
                return this.starFlags;
            }
            set
            {
                this.starFlags = value;
                this.starFlagsList = (List<string>)null;
            }
        }

        [XmlAttribute(AttributeName = "heId")]
        public string HeId
        {
            get
            {
                return this.heId;
            }
            set
            {
                this.heId = value;
            }
        }

        [XmlAttribute(AttributeName = "version")]
        public string VersionString
        {
            get
            {
                if (!FileAction.ActionVersion.isNull(this.version))
                    return this.version.ToString();
                return (string)null;
            }
            set
            {
                this.version = new FileAction.ActionVersion(value);
            }
        }

        [XmlIgnore]
        public FileAction.ActionVersion Version
        {
            get
            {
                return this.version;
            }
            set
            {
                this.version = value;
            }
        }

        [XmlIgnore]
        public string FileActionType
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }

        [XmlIgnore]
        public bool SBModified
        {
            get
            {
                return this.sbModified;
            }
            set
            {
                this.sbModified = value;
            }
        }

        [XmlIgnore]
        public IList<string> StarFlagsList
        {
            get
            {
                if (this.starFlagsList != null)
                    return (IList<string>)this.starFlagsList;
                if (!string.IsNullOrEmpty(this.starFlags))
                    this.starFlagsList = new List<string>((IEnumerable<string>)this.starFlags.Split(','));
                else
                    this.starFlagsList = new List<string>();
                return (IList<string>)this.starFlagsList;
            }
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("FileActionType: " + this.FileActionType + "\n");
            stringBuilder.Append("Source: " + this.Src + "\n");
            stringBuilder.Append("Destination: " + this.Dest + "\n");
            stringBuilder.Append("StarFlags: " + this.StarFlags + "\n");
            stringBuilder.Append("Version: " + (object)this.Version + "\n");
            return stringBuilder.ToString();
        }

        public bool IsSupersededBy(FileAction action)
        {
            if (this.Version > action.Version || !FileAction.SameString(this.Dest, action.Dest) || !FileAction.SameString(this.HeId, action.HeId))
                return false;
            if (FileAction.SameString(this.StarFlags, action.StarFlags))
                return true;
            IList<string> starFlagsList = this.StarFlagsList;
            foreach (string starFlags in (IEnumerable<string>)action.StarFlagsList)
            {
                if (!starFlagsList.Contains(starFlags))
                    return false;
            }
            return true;
        }

        public class ActionVersion : IComparable, IComparable<FileAction.ActionVersion>, IEquatable<FileAction.ActionVersion>
        {
            private ulong u_version;

            public static bool isNull(FileAction.ActionVersion obj)
            {
                return (object)obj == null;
            }

            public static bool operator !=(FileAction.ActionVersion v1, FileAction.ActionVersion v2)
            {
                return !(v1 == v2);
            }

            public static bool operator ==(FileAction.ActionVersion v1, FileAction.ActionVersion v2)
            {
                if (FileAction.ActionVersion.isNull(v1) && FileAction.ActionVersion.isNull(v2))
                    return true;
                if (FileAction.ActionVersion.isNull(v1))
                    return false;
                return v1.Equals(v2);
            }

            public static bool operator >(FileAction.ActionVersion v1, FileAction.ActionVersion v2)
            {
                if (FileAction.ActionVersion.isNull(v1) && FileAction.ActionVersion.isNull(v2) || FileAction.ActionVersion.isNull(v1))
                    return false;
                return v1.CompareTo(v2) > 0;
            }

            public static bool operator <(FileAction.ActionVersion v1, FileAction.ActionVersion v2)
            {
                if (FileAction.ActionVersion.isNull(v1) && FileAction.ActionVersion.isNull(v2))
                    return false;
                if (FileAction.ActionVersion.isNull(v1))
                    return true;
                return v1.CompareTo(v2) < 0;
            }

            public static bool operator <=(FileAction.ActionVersion v1, FileAction.ActionVersion v2)
            {
                if (!(v1 == v2))
                    return v1 < v2;
                return true;
            }

            public static bool operator >=(FileAction.ActionVersion v1, FileAction.ActionVersion v2)
            {
                if (!(v1 == v2))
                    return v1 > v2;
                return true;
            }

            public ActionVersion(string version)
            {
                this.u_version = ulong.Parse(version);
            }

            public ActionVersion(FileAction.ActionVersion version)
              : this(version.u_version)
            {
            }

            public ActionVersion(ulong version)
            {
                this.u_version = version;
            }

            public bool Equals(FileAction.ActionVersion obj)
            {
                if (FileAction.ActionVersion.isNull(obj))
                    return false;
                return (long)this.Version == (long)obj.Version;
            }

            public int CompareTo(FileAction.ActionVersion other)
            {
                if (FileAction.ActionVersion.isNull(other) || this.Version > other.Version)
                    return 1;
                return this.Version < other.Version ? -1 : 0;
            }

            public int CompareTo(object obj)
            {
                return this.CompareTo(obj as FileAction.ActionVersion);
            }

            public override bool Equals(object obj)
            {
                return this.Equals(obj as FileAction.ActionVersion);
            }

            public override int GetHashCode()
            {
                return this.ToString().GetHashCode();
            }

            public override string ToString()
            {
                return string.Format("{0}", (object)this.u_version);
            }

            public FileAction.ActionVersion Clone()
            {
                return new FileAction.ActionVersion(this);
            }

            public ulong Version
            {
                get
                {
                    return this.u_version;
                }
                set
                {
                    this.u_version = value;
                }
            }
        }
    }
}

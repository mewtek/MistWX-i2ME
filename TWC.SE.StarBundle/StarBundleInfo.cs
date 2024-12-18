using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace TWC.SE.StarBundle
{
    [XmlType("StarBundle")]
    public class StarBundleInfo
    {
        public static readonly string MANIFEST_UNIX = "MetaData/manifest.xml";
        public static readonly string MANIFEST_DOS = "MetaData\\manifest.xml";
        private DateTime applyDate = DateTime.MinValue;
        private bool okToDelete = true;
        private bool localTime;
        private List<FileAction> fileActions;

        [XmlIgnore]
        public string StagedFileName { get; set; }

        [XmlIgnore]
        public bool OkToDelete
        {
            get
            {
                return this.okToDelete;
            }
            set
            {
                this.okToDelete = value;
            }
        }

        [XmlIgnore]
        public bool IsTimeToProcess
        {
            get
            {
                if (this.localTime)
                    return this.applyDate < DateTime.Now;
                return this.applyDate < DateTime.UtcNow;
            }
        }

        public ulong Version { get; set; }

        public string ApplyDate
        {
            get
            {
                return this.applyDate.ToString();
            }
            set
            {
                this.applyDate = DateTime.Parse(value);
            }
        }

        public string Type { get; set; }

        public bool ApplyLocalTime
        {
            get
            {
                return this.localTime;
            }
            set
            {
                this.localTime = value;
            }
        }

        [XmlArrayItem("Update", typeof(UpdateAction))]
        [XmlArrayItem("Add", typeof(AddAction))]
        [XmlArrayItem("Delete", typeof(DeleteAction))]
        public List<FileAction> FileActions
        {
            get
            {
                return this.fileActions;
            }
            set
            {
                this.fileActions = value;
            }
        }

        public void AddAction(FileAction action)
        {
            if (this.fileActions.Count == 0)
            {
                this.fileActions.Add(action);
            }
            else
            {
                int index = this.fileActions.Count - 1;
                while (index >= 0 && !(this.fileActions[index].Version < action.Version))
                    --index;
                this.fileActions.Insert(index + 1, action);
            }
        }

        public void UpdateFileActionVersions()
        {
            for (int index = 0; index < this.fileActions.Count; ++index)
            {
                FileAction fileAction = this.fileActions[index];
                if (fileAction.Version == (FileAction.ActionVersion)null)
                    fileAction.Version = new FileAction.ActionVersion(this.Version);
                else if ((long)fileAction.Version.Version == 0L)
                    fileAction.Version.Version = this.Version;
                fileAction.Dest = fileAction.Dest.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }
        }

        public bool RemoveSupersededActions()
        {
            return this.RemoveSupersededActions(0, this.fileActions.Count - 1);
        }

        public bool RemoveSupersededActions(int start, int end)
        {
            bool flag1 = false;
            List<FileAction> fileActionList = new List<FileAction>();
            for (int index1 = 0; index1 <= end; ++index1)
            {
                FileAction fileAction1 = this.fileActions[index1];
                bool flag2 = false;
                for (int index2 = index1 + 1; index2 < this.fileActions.Count; ++index2)
                {
                    FileAction fileAction2 = this.fileActions[index2];
                    if (fileAction1.IsSupersededBy(fileAction2))
                    {
                        flag2 = true;
                        flag1 = true;
                        break;
                    }
                }
                if (!flag2)
                    fileActionList.Add(fileAction1);
            }
            this.fileActions = fileActionList;
            return flag1;
        }

        public bool HasSupersededActions(FileAction action)
        {
            foreach (FileAction fileAction in this.FileActions)
            {
                if (action.IsSupersededBy(fileAction))
                    return true;
            }
            return false;
        }

        public XmlDocument GetManifestDoc()
        {
            XmlDocument xmlDocument = new XmlDocument();
            using (XmlWriter xmlWriter = xmlDocument.CreateNavigator().AppendChild())
            {
                XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                namespaces.Add("", "");
                new XmlSerializer(typeof(StarBundleInfo)).Serialize(xmlWriter, (object)this, namespaces);
            }
            return xmlDocument;
        }

        public static StarBundleInfo GetStarBundleInfo(string stagedBundle)
        {
            try
            {
                using (ZipFile zipFile = new ZipFile(stagedBundle))
                {
                    ZipEntry entry = zipFile.GetEntry(StarBundleInfo.MANIFEST_DOS) ?? zipFile.GetEntry(StarBundleInfo.MANIFEST_UNIX);
                    if (entry == null)
                        throw new FileNotFoundException(string.Format("Manifest for Bundle {0} could not be found", (object)stagedBundle));
                    using (XmlReader xmlReader = XmlReader.Create(zipFile.GetInputStream(entry)))
                    {
                        StarBundleInfo starBundleInfo = (StarBundleInfo)new XmlSerializer(typeof(StarBundleInfo)).Deserialize(xmlReader);
                        starBundleInfo.StagedFileName = stagedBundle;
                        starBundleInfo.UpdateFileActionVersions();
                        return starBundleInfo;
                    }
                }
            }
            catch (FileNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FileNotFoundException(string.Format("Manifest for Bundle {0} could not be found", (object)stagedBundle), ex);
            }
        }
    }
}

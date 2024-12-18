using System.IO;
using System.Threading.Tasks;
using System.Xml;
using TWC.Util;

namespace TWC.I2.MsgEncode.ProcessingSteps
{
    public class SplitMsgStep : IMsgEncodeStep, IMsgDecodeStep
    {
        private long splitId;
        private long splitPart;
        private long splitCount;
        private MsgDecoder decoder;

        public SplitMsgStep(MsgDecoder decoder)
        {
            this.decoder = decoder;
        }

        public SplitMsgStep(long splitId, long splitPart, long splitCount)
        {
            this.splitId = splitId;
            this.splitPart = splitPart;
            this.splitCount = splitCount;
        }

        public string Tag
        {
            get
            {
                return "SplitMsg";
            }
        }

        public string Encode(string payloadFile, XmlElement descriptor)
        {
            descriptor.SetAttribute("id", this.splitId.ToString());
            descriptor.SetAttribute("part", this.splitPart.ToString());
            descriptor.SetAttribute("count", this.splitCount.ToString());
            return payloadFile;
        }

        public async Task<string> Decode(string payloadFile, XmlElement descriptor)
        {
            string directoryName = Path.GetDirectoryName(payloadFile);
            string attribute1 = descriptor.GetAttribute("id");
            string attribute2 = descriptor.GetAttribute("part");
            string attribute3 = descriptor.GetAttribute("count");
            string path2 = attribute1 + "_" + attribute2 + "_" + attribute3;
            string destFileName = Path.Combine(directoryName, path2);
            File.Move(payloadFile, destFileName);
            if (this.AllPresent(directoryName, attribute1, attribute3))
            {
                long result = 0;
                long.TryParse(attribute3, out result);
                string str = Path.Combine(directoryName, attribute1);
                FileStream fileStream1 = new FileStream(str, FileMode.Create, FileAccess.Write);
                for (int index = 0; (long)index < result; ++index)
                {
                    string path = Path.Combine(directoryName, attribute1 + "_" + (object)(index + 1) + "_" + attribute3);
                    FileStream fileStream2 = new FileStream(path, FileMode.Open, FileAccess.Read);
                    Toolbox.CopyStream((Stream)fileStream2, (Stream)fileStream1);
                    fileStream2.Close();
                    File.Delete(path);
                }
                fileStream1.Close();
                await this.decoder.DecodeAsync(str);
            }
            return (string)null;
        }

        private bool AllPresent(string workDir, string id, string count)
        {
            bool flag = true;
            long result = 0;
            long.TryParse(count, out result);
            for (int index = 0; (long)index < result; ++index)
            {
                if (!File.Exists(Path.Combine(workDir, id + "_" + (object)(index + 1) + "_" + count)))
                {
                    flag = false;
                    break;
                }
            }
            return flag;
        }
    }
}

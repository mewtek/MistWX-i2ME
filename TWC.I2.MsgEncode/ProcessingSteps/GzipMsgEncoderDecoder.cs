using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Xml;
using TWC.Util;

namespace TWC.I2.MsgEncode.ProcessingSteps
{
    public class GzipMsgEncoderDecoder : IMsgEncodeStep, IMsgDecodeStep
    {
        private string workDir;

        public GzipMsgEncoderDecoder()
          : this((string)null)
        {
        }

        public GzipMsgEncoderDecoder(string workDir)
        {
            this.workDir = workDir;
        }

        public string Tag
        {
            get
            {
                return "GzipCompressedMsg";
            }
        }

        public string Encode(string payloadFile, XmlElement descriptor)
        {
            descriptor.SetAttribute("fname", Path.GetFileName(payloadFile));
            string path = string.Format("{0}.{1}", (object)payloadFile, (object)"gz");
            using (Stream readStream = (Stream)File.OpenRead(payloadFile))
            {
                using (Stream writeStream = (Stream)new GZipStream((Stream)File.Open(path, FileMode.Create, FileAccess.Write), CompressionMode.Compress))
                    Toolbox.CopyStream(readStream, writeStream);
            }
            return path;
        }

        public async Task<string> Decode(string payloadFile, XmlElement descriptor)
        {
            string attribute = descriptor.GetAttribute("fname");
            string path = this.workDir == null ? Path.GetTempFileName() : Path.Combine(this.workDir, attribute);
            using (Stream readStream = (Stream)new GZipStream((Stream)File.OpenRead(payloadFile), CompressionMode.Decompress))
            {
                using (Stream writeStream = (Stream)File.Open(path, FileMode.Create, FileAccess.Write))
                    Toolbox.CopyStream(readStream, writeStream);
            }
            return path;
        }
    }
}

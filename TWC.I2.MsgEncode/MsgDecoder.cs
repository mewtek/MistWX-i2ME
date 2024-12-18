using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace TWC.I2.MsgEncode
{
    public sealed class MsgDecoder
    {
        private static readonly int LEN_MAGIC = Common.Magic.Length;
        private Dictionary<string, IMsgDecodeStep> processorMap = new Dictionary<string, IMsgDecodeStep>();

        public MsgDecoder()
        {
        }

        public MsgDecoder(IMsgDecodeStep decodeStep)
        {
            this.processorMap[decodeStep.Tag] = decodeStep;
        }

        public MsgDecoder(IEnumerable<IMsgDecodeStep> decodeChain)
        {
            this.UpdateDecodeChain(decodeChain);
        }

        public void UpdateDecodeChain(IEnumerable<IMsgDecodeStep> decodeChain)
        {
            if (decodeChain == null)
                return;
            foreach (IMsgDecodeStep msgDecodeStep in decodeChain)
                this.processorMap[msgDecodeStep.Tag] = msgDecodeStep;
        }

        public static void LoadDescriptorDoc(string fname, out long payloadSize, out XmlDocument doc)
        {
            using (FileStream stream = File.Open(fname, FileMode.Open, FileAccess.ReadWrite))
                MsgDecoder.LoadDescriptorDoc(stream, out payloadSize, out doc);
        }

        public async Task<string> DecodeAsync(string msgFile)
        {
            XmlDocument xmlDocument = MsgDecoder.ProcessDescriptorDoc(msgFile);
            List<MsgDecoder.ProcessingStep> processingStepList = new List<MsgDecoder.ProcessingStep>();
            foreach (XmlNode childNode in xmlDocument.DocumentElement.ChildNodes)
            {
                XmlElement descriptor = childNode as XmlElement;
                if (childNode != null)
                {
                    string name = childNode.Name;
                    IMsgDecodeStep processor;
                    if (!this.processorMap.TryGetValue(name, out processor))
                        throw new Exception(string.Format("Don't know how to decode msg type: {0}", (object)name));
                    processingStepList.Add(new MsgDecoder.ProcessingStep(descriptor, processor));
                }
            }
            processingStepList.Reverse();
            string str = msgFile;
            foreach (MsgDecoder.ProcessingStep step in processingStepList)
            {
                string sourceFileName = await this.DecodeOnceAsync(step, str);
                if (sourceFileName != null && str != sourceFileName)
                {
                    File.Delete(str);
                    File.Move(sourceFileName, str);
                }
                if (sourceFileName == null)
                    break;
            }
            return str;
        }

        private static void LoadDescriptorDoc(
          FileStream stream,
          out long payloadSize,
          out XmlDocument doc)
        {
            payloadSize = 0L;
            doc = (XmlDocument)null;
            if (stream.Length == 0L)
                return;
            BinaryReader binaryReader = new BinaryReader((Stream)stream);
            stream.Seek((long)-(MsgDecoder.LEN_MAGIC + 4), SeekOrigin.End);
            if (new string(binaryReader.ReadChars(MsgDecoder.LEN_MAGIC)) != Common.Magic)
                throw new Exception(string.Format("{0} is not a {1} file.", (object)stream.Name, (object)Common.Magic));
            uint num = binaryReader.ReadUInt32();
            payloadSize = stream.Length - ((long)num + (long)MsgDecoder.LEN_MAGIC + 4L);
            stream.Seek(payloadSize, SeekOrigin.Begin);
            string xml = new string(binaryReader.ReadChars((int)num));
            doc = new XmlDocument();
            doc.LoadXml(xml);
        }

        private static XmlDocument ProcessDescriptorDoc(string msgFile)
        {
            long payloadSize = 0;
            XmlDocument doc = (XmlDocument)null;
            using (FileStream stream = File.Open(msgFile, FileMode.Open, FileAccess.ReadWrite))
            {
                MsgDecoder.LoadDescriptorDoc(stream, out payloadSize, out doc);
                stream.SetLength(payloadSize);
            }
            return doc;
        }

        private async Task<string> DecodeOnceAsync(MsgDecoder.ProcessingStep step, string msgFile)
        {
            string outputFile = await step.processor.Decode(msgFile, step.descriptor);
            return outputFile;
        }

        private class ProcessingStep
        {
            public IMsgDecodeStep processor;
            public XmlElement descriptor;

            public ProcessingStep()
            {
            }

            public ProcessingStep(XmlElement descriptor, IMsgDecodeStep processor)
            {
                this.descriptor = descriptor;
                this.processor = processor;
            }
        }
    }
}

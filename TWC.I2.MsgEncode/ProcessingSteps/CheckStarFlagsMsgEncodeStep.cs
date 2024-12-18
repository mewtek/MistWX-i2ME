using System.Collections.Generic;
using System.Xml;

namespace TWC.I2.MsgEncode.ProcessingSteps
{
    public class CheckStarFlagsMsgEncodeStep : IMsgEncodeStep
    {
        private const string TAG = "CheckStarFlags";
        private IEnumerable<string> flags;

        public CheckStarFlagsMsgEncodeStep(IEnumerable<string> flags)
        {
            this.flags = flags;
        }

        public CheckStarFlagsMsgEncodeStep(string flag)
        {
            this.flags = (IEnumerable<string>)new string[1]
            {
        flag
            };
        }

        public string Tag
        {
            get
            {
                return "CheckStarFlags";
            }
        }

        public string Encode(string payloadFile, XmlElement descriptor)
        {
            foreach (string flag in this.flags)
            {
                XmlElement element = descriptor.OwnerDocument.CreateElement("Flag");
                element.InnerText = flag;
                descriptor.AppendChild((XmlNode)element);
            }
            return payloadFile;
        }
    }
}

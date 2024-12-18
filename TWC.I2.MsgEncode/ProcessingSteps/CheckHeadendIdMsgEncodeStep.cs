using System.Collections.Generic;
using System.Xml;

namespace TWC.I2.MsgEncode.ProcessingSteps
{
    public class CheckHeadendIdMsgEncodeStep : IMsgEncodeStep
    {
        private IEnumerable<string> ids;

        public CheckHeadendIdMsgEncodeStep(IEnumerable<string> ids)
        {
            this.ids = ids;
        }

        public CheckHeadendIdMsgEncodeStep(string id)
        {
            this.ids = (IEnumerable<string>)new string[1]
            {
        id
            };
        }

        public string Tag
        {
            get
            {
                return "CheckHeadendId";
            }
        }

        public string Encode(string payloadFile, XmlElement descriptor)
        {
            foreach (string id in this.ids)
            {
                XmlElement element = descriptor.OwnerDocument.CreateElement("HeadendId");
                element.InnerText = id;
                descriptor.AppendChild((XmlNode)element);
            }
            return payloadFile;
        }
    }
}

using System.Xml;

namespace TWC.I2.MsgEncode.ProcessingSteps
{
    public class TransferIdMsgEncodeStep : IMsgEncodeStep
    {
        private string id;

        public TransferIdMsgEncodeStep(string id)
        {
            this.id = id;
        }

        public string Tag
        {
            get
            {
                return "TransferId";
            }
        }

        public string Encode(string payloadFile, XmlElement descriptor)
        {
            XmlElement element = descriptor.OwnerDocument.CreateElement("Id");
            element.InnerText = this.id;
            descriptor.AppendChild((XmlNode)element);
            return payloadFile;
        }
    }
}

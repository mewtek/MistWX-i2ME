using System.Xml;

namespace TWC.I2.MsgEncode
{
    public interface IMsgEncodeStep
    {
        string Tag { get; }

        string Encode(string payloadFile, XmlElement descriptor);
    }
}

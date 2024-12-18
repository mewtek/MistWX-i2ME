using System.Threading.Tasks;
using System.Xml;

namespace TWC.I2.MsgEncode
{
    public interface IMsgDecodeStep
    {
        string Tag { get; }

        Task<string> Decode(string payloadFile, XmlElement descriptor);
    }
}

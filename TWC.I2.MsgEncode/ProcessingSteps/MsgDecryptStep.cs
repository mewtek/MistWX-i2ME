using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Xml;
using TWC.Util;

namespace TWC.I2.MsgEncode.ProcessingSteps
{
    public class MsgDecryptStep : MsgDecryptEncryptBase, IMsgDecodeStep
    {
        public string Tag
        {
            get
            {
                return "EncryptedMsg";
            }
        }

        public async Task<string> Decode(string payloadFile, XmlElement descriptor)
        {
            SymmetricAlgorithm algorithm = this.algorithm;
            algorithm.Key = this.password;
            algorithm.IV = this.iv;
            string tempFileName = Toolbox.GetTempFileName(MsgDecryptEncryptBase.TempFolder, "decrypt");
            CryptoStream cryptoStream = new CryptoStream((Stream)new FileStream(payloadFile, FileMode.Open), algorithm.CreateDecryptor(), CryptoStreamMode.Read);
            FileStream fileStream = new FileStream(tempFileName, FileMode.OpenOrCreate);
            try
            {
                Toolbox.CopyStream((Stream)cryptoStream, (Stream)fileStream);
            }
            finally
            {
                cryptoStream.Close();
                fileStream.Close();
            }
            return tempFileName;
        }
    }
}

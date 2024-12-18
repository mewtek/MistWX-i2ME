using System.IO;
using System.Security.Cryptography;
using System.Xml;
using TWC.Util;

namespace TWC.I2.MsgEncode.ProcessingSteps
{
    public class MsgEncryptStep : MsgDecryptEncryptBase, IMsgEncodeStep
    {
        public string Tag
        {
            get
            {
                return "EncryptedMsg";
            }
        }

        public string Encode(string payloadFile, XmlElement descriptor)
        {
            SymmetricAlgorithm algorithm = this.algorithm;
            algorithm.Key = this.password;
            algorithm.IV = this.iv;
            string tempFileName = Toolbox.GetTempFileName("encrypt");
            FileStream fileStream = new FileStream(payloadFile, FileMode.Open);
            CryptoStream cryptoStream = new CryptoStream((Stream)new FileStream(tempFileName, FileMode.OpenOrCreate), algorithm.CreateEncryptor(), CryptoStreamMode.Write);
            try
            {
                Toolbox.CopyStream((Stream)fileStream, (Stream)cryptoStream);
            }
            finally
            {
                fileStream.Close();
                cryptoStream.Close();
            }
            return tempFileName;
        }
    }
}

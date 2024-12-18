using System.Security.Cryptography;
using System.Text;

namespace TWC.I2.MsgEncode.ProcessingSteps
{
    public abstract class MsgDecryptEncryptBase
    {
        protected byte[] password = Encoding.UTF8.GetBytes("sixteencharactersixteencharacter");
        protected byte[] iv = Encoding.UTF8.GetBytes("sixteencharacter");
        protected string keyfile = "KeyFile.tdes";
        protected SymmetricAlgorithm algorithm = Rijndael.Create();
        public static string TempFolder;
    }
}

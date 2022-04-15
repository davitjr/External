using System;
using System.Security.Cryptography;
using System.Text;

namespace ExternalBanking
{
    public class EncryptionOperations
    {
        private RSACryptoServiceProvider crypto { get; }
        private CspParameters cspParams { get; }

        public EncryptionOperations()
        {
            cspParams = new CspParameters();
            cspParams.KeyContainerName = "KeysForCVVv1";
            cspParams.Flags = CspProviderFlags.UseMachineKeyStore;
            crypto = new RSACryptoServiceProvider(cspParams);
        }

        public string DecryptData(string datastr)
        {
            byte[] data = Convert.FromBase64String(datastr);
            byte[] cipherText = crypto.Decrypt(data, false);
            return Encoding.UTF8.GetString(cipherText);
        }
    }
}

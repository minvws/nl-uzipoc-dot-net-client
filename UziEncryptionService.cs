using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace UziClientPoc
{
    public class UziEncryptionService
    {
        private readonly EncryptionOptions encryptionOptions;
        private X509Certificate2? certificate;

        public UziEncryptionService(IOptions<EncryptionOptions> encryptionOptions)
        {
            this.encryptionOptions = encryptionOptions.Value;
        }

        public string DecryptHex(string hexString)
        {
            certificate ??= X509Certificate2.CreateFromPemFile(encryptionOptions.CertPath, encryptionOptions.KeyPath);
            var bitjes = certificate.GetRSAPrivateKey()!.Decrypt(StringToByteArray(hexString), RSAEncryptionPadding.OaepSHA1);
            return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(System.Text.Encoding.UTF8.GetString(bitjes)));
        }

        private static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }
}

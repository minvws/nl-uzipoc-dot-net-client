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
            var bitjes = certificate.GetRSAPrivateKey()!.Decrypt(Convert.FromHexString(hexString), RSAEncryptionPadding.OaepSHA1);
            return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(System.Text.Encoding.UTF8.GetString(bitjes)));
        }
    }
}

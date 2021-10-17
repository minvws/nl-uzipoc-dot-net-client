using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace UziClientPoc
{
    public class UziCertificateService
    {
        private readonly EncryptionOptions encryptionOptions;

        public UziCertificateService(IOptions<EncryptionOptions> encryptionOptions)
        {
            this.encryptionOptions = encryptionOptions.Value;
        }

        internal IEnumerable<SecurityKey> GetIssuerSigningKeys()
        {
            return encryptionOptions.IssuerSigningKeys
                .Select(fileName => new X509SecurityKey(new X509Certificate2(fileName)));
        }

        internal SecurityKey GetTokenDecryptionKey()
        {
            return new X509SecurityKey(
                X509Certificate2
                    .CreateFromPemFile(
                        this.encryptionOptions.CertPath,
                        this.encryptionOptions.KeyPath));
        }
    }
}

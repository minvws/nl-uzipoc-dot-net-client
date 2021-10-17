using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UziClientPoc
{
    public record EncryptionOptions
    {
        public const string Encryption = "Encryption";
        public string KeyPath { get; set; }  = "Resources/client-certificate.key";
        public string CertPath { get; set; } = "Resources/client-certificate.crt";
        public IEnumerable<string> IssuerSigningKeys { get; set; } = "Resources/issuer-signing-key.crt".ToEnumerable();
    }
}

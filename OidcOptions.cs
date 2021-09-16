using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UziClientPoc
{
    public record OidcOptions
    {
        public const string Oidc = "Oidc";

        public string Authority { get; set; }
        public string ClientId { get; set; }
    }
}
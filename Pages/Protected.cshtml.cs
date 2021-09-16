using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace UziClientPoc.Pages
{
    [Authorize]
    public class ProtectedModel : PageModel
    {
        private readonly ILogger<ProtectedModel> _logger;
        private readonly IHttpClientFactory clientFactory;
        private readonly UraOptions uraOptions;
        public string UziInformationEncrypted { get; set; }
        public string UziInformationDecrypted { get; set; }

        public ProtectedModel(ILogger<ProtectedModel> logger, IHttpClientFactory clientFactory, IOptions<UraOptions> options)
        {
            _logger = logger;
            this.clientFactory = clientFactory;
            uraOptions = options.Value;
        }

        public async Task OnGetAsync()
        {
            var baseUri = "https://inge6:8006";
            var accessToken = await HttpContext.GetTokenAsync("id_token"); // shouldnt this be the access_token?

            var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUri}/userinfo?ura_number={uraOptions.UraNumber}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpClient httpClient = clientFactory.CreateClient();
            ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
            var bsnAttr = await httpClient.SendAsync(request);
            UziInformationEncrypted = await bsnAttr.Content.ReadAsStringAsync();
            var deserializedUziInformation = JsonConvert.DeserializeObject<Dictionary<string, string>>(UziInformationEncrypted);
            
            //// Dit moet nog anders
            var keyPem = System.IO.File.ReadAllText("Resources/client-certificate.key");
            var certPem = System.IO.File.ReadAllText("Resources/client-certificate.crt");

            var cert = X509Certificate2.CreateFromPemFile("Resources/client-certificate.crt", "Resources/client-certificate.key");

            var bitjes = cert.GetRSAPrivateKey().Decrypt(StringToByteArray(deserializedUziInformation["vUZI"]), RSAEncryptionPadding.OaepSHA1);
            UziInformationDecrypted = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(System.Text.Encoding.UTF8.GetString(bitjes)));
        }
        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }
} 

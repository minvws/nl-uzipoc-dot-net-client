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
        private readonly UziEncryptionService encryptionService;
        private readonly OidcOptions oidcOptions;
        private readonly UraOptions uraOptions;
        public string UziInformationEncrypted { get; set; }
        public string UziInformationDecrypted { get; set; }

        public ProtectedModel(ILogger<ProtectedModel> logger, IHttpClientFactory clientFactory, IOptions<UraOptions> options, UziEncryptionService encryptionService, IOptions<OidcOptions> oidcOptions)
        {
            _logger = logger;
            this.clientFactory = clientFactory;
            this.encryptionService = encryptionService;
            this.oidcOptions = oidcOptions.Value;
            uraOptions = options.Value;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var accessToken = await HttpContext.GetTokenAsync("id_token"); // shouldnt this be the access_token?
            var requestUrl = $"{oidcOptions.Authority}/userinfo?ura_number={uraOptions.UraNumber}";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpClient httpClient = clientFactory.CreateClient();
            ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
            var bridgeResponse = await httpClient.SendAsync(request);
            if (!bridgeResponse.IsSuccessStatusCode)
            {
                UziInformationDecrypted = await bridgeResponse.Content.ReadAsStringAsync();
                return Page();
            }

            UziInformationEncrypted = await bridgeResponse.Content.ReadAsStringAsync();
            var deserializedUziInformation = JsonConvert.DeserializeObject<Dictionary<string, string>>(UziInformationEncrypted)!;
            UziInformationDecrypted = encryptionService.DecryptHex(deserializedUziInformation["vUZI"]);

            //requestUrl = $"{oidcOptions.Authority}/bsn_attribute";
            //request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            //request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            //bridgeResponse = await httpClient.SendAsync(request);
            //if (!bridgeResponse.IsSuccessStatusCode)
            //{
            //    UziInformationDecrypted = await bridgeResponse.Content.ReadAsStringAsync();
            //    return Page();
            //}
            //UziInformationEncrypted = await bridgeResponse.Content.ReadAsStringAsync();

            //var keyPair = new KeyPair(
            //    Convert.FromBase64String("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA="),
            //    Convert.FromBase64String("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB=")
            //    );


            //var bitjes = SealedPublicKeyBox.Open(Convert.FromBase64String(UziInformationEncrypted), keyPair);
            //var stringjetje = System.Text.Encoding.UTF8.GetString(bitjes);
            //SealedPublicKeyBox.Create()




            //var key = "RYlG57yfs8qLOPUwn8OUHJV0t+/i9Lnt9gCIeLAEgoc=";
            //var message2 = SecretBox.Open(UziInformationEncrypted, "", System.Text.Encoding.UTF8.GetBytes(key));
            return Page();
        }
    }
} 

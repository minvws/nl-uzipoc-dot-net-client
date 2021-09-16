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
        private readonly UraOptions uraOptions;
        public string UziInformationEncrypted { get; set; }
        public string UziInformationDecrypted { get; set; }

        public ProtectedModel(ILogger<ProtectedModel> logger, IHttpClientFactory clientFactory, IOptions<UraOptions> options, UziEncryptionService encryptionService)
        {
            _logger = logger;
            this.clientFactory = clientFactory;
            this.encryptionService = encryptionService;
            uraOptions = options.Value;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var baseUri = "https://inge6:8006";
            var accessToken = await HttpContext.GetTokenAsync("id_token"); // shouldnt this be the access_token?
            var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUri}/userinfo?ura_number={uraOptions.UraNumber}");
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
            return Page();
        }
    }
} 

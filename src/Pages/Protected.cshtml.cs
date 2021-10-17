using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
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
    [Authorize(AuthenticationSchemes = "oidc1,oidc2")]
    public class ProtectedModel : PageModel
    {
        private readonly ILogger<ProtectedModel> _logger;
        private readonly IHttpClientFactory clientFactory;
        private readonly UziCertificateService uziCertificateService;
        private readonly OidcOptions oidcOptions;

        public string UziInformationEncrypted { get; set; } = "";
        public string UziInformationDecrypted { get; set; } = "";

        public ProtectedModel(ILogger<ProtectedModel> logger,
                              IHttpClientFactory clientFactory,
                              IOptions<OidcOptions> oidcOptions,
                              UziCertificateService uziCertificateService)
        {
            _logger = logger;
            this.clientFactory = clientFactory;
            this.uziCertificateService = uziCertificateService;
            this.oidcOptions = oidcOptions.Value;
        }
        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                await SetUziInformation();
            } catch(Exception e)
            {
                _logger.LogWarning("Error while getting uzi information: ", e);
                UziInformationDecrypted = e.Message;
                await HttpContext.SignOutAsync();
            }
            return Page();
        }

        private async Task SetUziInformation()
        {
            UziInformationEncrypted = await GetUserInfoFromUziBridge();
            UziInformationDecrypted = GetPayloadFromJweToken(UziInformationEncrypted);
        }

        private async Task<string> GetUserInfoFromUziBridge()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var requestUrl = $"{oidcOptions.Authority}/userinfo";

            HttpClient httpClient = clientFactory.CreateClient("userInfo");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
            using var bridgeResponse = await httpClient.GetAsync(requestUrl).ConfigureAwait(false);
            string uziResponseBody = await bridgeResponse.Content.ReadAsStringAsync();
            if (!bridgeResponse.IsSuccessStatusCode)
            {
                throw new Exception("Unexpected responce from UziBridge: " + uziResponseBody);
            }
            return uziResponseBody;
        }

        private string GetPayloadFromJweToken(string uziInformationEncrypted)
        {
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            jwtSecurityTokenHandler.ValidateToken(
                JsonConvert.DeserializeObject<Dictionary<string, string>>(uziInformationEncrypted)!["vUZI"],
                new TokenValidationParameters
                {
                    IssuerSigningKeys = uziCertificateService.GetIssuerSigningKeys(),
                    ValidateAudience = false,
                    ValidIssuer = "cibg",
                    TokenDecryptionKey = uziCertificateService.GetTokenDecryptionKey()
                },
                out SecurityToken securityToken
                );
            return JsonConvert.SerializeObject(((JwtSecurityToken)securityToken).Payload);
        }
    }
} 

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
    [Authorize]
    public class ProtectedModel : PageModel
    {
        private readonly ILogger<ProtectedModel> _logger;
        private readonly IHttpClientFactory clientFactory;
        private readonly UziEncryptionService encryptionService;
        private readonly OidcOptions oidcOptions;
        private readonly UraOptions uraOptions;
        private readonly EncryptionOptions encryptionOptions;

        public string UziInformationEncrypted { get; set; }
        public string UziInformationDecrypted { get; set; }

        public ProtectedModel(ILogger<ProtectedModel> logger, IHttpClientFactory clientFactory, IOptions<UraOptions> options, UziEncryptionService encryptionService, IOptions<OidcOptions> oidcOptions, IOptions<EncryptionOptions> encryptionOptions)
        {
            _logger = logger;
            this.clientFactory = clientFactory;
            this.encryptionService = encryptionService;
            this.oidcOptions = oidcOptions.Value;
            uraOptions = options.Value;
            this.encryptionOptions = encryptionOptions.Value;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var accessToken = await HttpContext.GetTokenAsync("access_token");

                HttpClient httpClient = clientFactory.CreateClient("userInfo");
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
                var requestUrl = $"{oidcOptions.Authority}/userinfo";

                using var bridgeResponse = await httpClient.GetAsync(requestUrl).ConfigureAwait(false);

                if (!bridgeResponse.IsSuccessStatusCode)
                {
                    UziInformationDecrypted = await bridgeResponse.Content.ReadAsStringAsync();
                    await HttpContext.SignOutAsync();
                    return Page();
                    //return Redirect("~/");
                }
                UziInformationEncrypted = await bridgeResponse.Content.ReadAsStringAsync();

                var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
                jwtSecurityTokenHandler.ValidateToken(
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(UziInformationEncrypted)!["vUZI"],
                    new TokenValidationParameters
                    {
                        IssuerSigningKey = new X509SecurityKey(new X509Certificate2(this.encryptionOptions.IssuerSigningKey)),
                        ValidateAudience = false,
                        ValidIssuer = "cibg",
                        TokenDecryptionKey = new X509SecurityKey(X509Certificate2.CreateFromPemFile(this.encryptionOptions.CertPath, this.encryptionOptions.KeyPath))
                    },
                    out SecurityToken securityToken
                    );
                this.UziInformationDecrypted = JsonConvert.SerializeObject(((JwtSecurityToken)securityToken).Payload);
            } catch(Exception e)
            {
                _logger.LogError("Whoops:", e);
                //await HttpContext.SignOutAsync();
                //return Redirect("/index");
            
            }
            return Page();
        }
    }
} 

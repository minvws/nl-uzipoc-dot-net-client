using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace nl_uzipoc_dot_net_client
{
	public class UserInfoTool
	{
		public static async Task FetchUserInfoOnTokenResponseReceived(TokenResponseReceivedContext tokenResponseReceivedContext)
        {
			var configuration = await tokenResponseReceivedContext.Options.ConfigurationManager.GetConfigurationAsync(CancellationToken.None);
			var jwksRequestMessage = new HttpRequestMessage(HttpMethod.Get, configuration.JwksUri);
			var jwksResponse = await tokenResponseReceivedContext.Options.Backchannel.SendAsync(jwksRequestMessage);
			var jwks = await jwksResponse.Content.ReadAsStringAsync();
			var jsonWebKeySet = JsonWebKeySet.Create(jwks);


			var userInfoRequestMessage = new HttpRequestMessage(HttpMethod.Get, configuration.UserInfoEndpoint);
			var accessToken = tokenResponseReceivedContext.TokenEndpointResponse.AccessToken;

			userInfoRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
			var response = await tokenResponseReceivedContext.Options.Backchannel.SendAsync(userInfoRequestMessage);
			var jwe = await response.Content.ReadAsStringAsync();
			var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
			var clientPrivateKey = new X509SecurityKey(X509Certificate2.CreateFromPemFile(
				"Resources/client.cert",
				"Resources/client.key"
				));

			jwtSecurityTokenHandler.ValidateToken(jwe, new TokenValidationParameters
			{
				TokenDecryptionKey = clientPrivateKey,
				IssuerSigningKeys = jsonWebKeySet.Keys,
				ValidateIssuer = false,
				ValidateAudience = false
            }, out SecurityToken securityToken);
			tokenResponseReceivedContext.HttpContext.Session.Set("userinfo", Encoding.UTF8.GetBytes(securityToken.ToString()));
		}
	}
}

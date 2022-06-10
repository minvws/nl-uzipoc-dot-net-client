using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace nl_uzipoc_dot_net_client.Pages
{
    public class ProtectedModel : PageModel
    {
        private readonly ILogger<ProtectedModel> _logger;
        public string userinfo = "Empty userinfo";

        public ProtectedModel(ILogger<ProtectedModel> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> OnGet()
        {
            HttpContext.Session.TryGetValue("userinfo", out byte[] serializedUserInfo);
            if(serializedUserInfo != null)
            {
                userinfo = Encoding.UTF8.GetString(serializedUserInfo);
            } else
            {
                await HttpContext.SignOutAsync();
                return Redirect("/Protected");
            }
            return Page();
        }

        public async Task<IActionResult> OnGetLogout()
        {
            Console.WriteLine("action!!!");

            await HttpContext.SignOutAsync();
            return Redirect("/");
        }
    }
}


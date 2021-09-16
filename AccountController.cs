using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UziClientPoc
{
    public class AccountController   : Controller
    {
        public async Task<IActionResult> LogOutAsync()
        {
            await AuthenticationHttpContextExtensions.SignOutAsync(HttpContext);
            return Redirect("/");
        }
    }
}

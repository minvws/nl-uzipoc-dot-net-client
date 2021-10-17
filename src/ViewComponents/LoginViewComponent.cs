using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace UziClientPoc.ViewComponents
{
    public class LoginViewComponent : ViewComponent
    {
        public LoginViewComponent()
        {
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View((await HttpContext.GetTokenAsync("access_token") != null));
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UziClientPoc.Pages
{
    public class AuthorizeModel : PageModel
    {
        private readonly ILogger<AuthorizeModel> _logger;

        public AuthorizeModel(ILogger<AuthorizeModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
}

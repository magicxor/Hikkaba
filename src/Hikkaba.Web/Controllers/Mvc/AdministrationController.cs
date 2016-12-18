using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hikkaba.Common.Constants;
using Hikkaba.Service;
using Hikkaba.Web.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Controllers.Mvc
{
    // todo: add ban functions: 1) ban by ip 2) ban by ip range 3) ban and delete all posts in category 4) ban and delete all posts

    [TypeFilter(typeof(ExceptionLoggingFilter))]
    [Authorize(Roles = Defaults.DefaultAdminRoleName)]
    public class AdministrationController : Controller
    {
        private readonly IAdministrationService _administrationService;

        public AdministrationController(IAdministrationService administrationService)
        {
            _administrationService = administrationService;
        }

        public IActionResult Index()
        {
            throw new NotImplementedException();
        }

        public IActionResult DeleteAllContent()
        {
            return View();
        }

        [HttpPost, ActionName("DeleteAllContent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAllContentConfirmed(Guid categoryId)
        {
            await _administrationService.DeleteAllContentAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
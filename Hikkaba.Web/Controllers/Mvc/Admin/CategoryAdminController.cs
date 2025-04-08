using System.Threading;
using System.Threading.Tasks;
using Hikkaba.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Controllers.Mvc.Admin;

[Authorize(Roles = Defaults.AdministratorRoleName)]
[Route("admin/categories")]
public class CategoryAdminController : Controller
{
    [HttpGet("{categoryAlias}/moderators/add", Name = "CategoryAddModerator")]
    public async Task<IActionResult> AddModerator(
        [FromRoute] string categoryAlias,
        CancellationToken cancellationToken)
    {
        return View();
    }

    [HttpPost("{categoryAlias}/moderators/add", Name = "CategoryAddModeratorConfirm")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddModeratorConfirm(
        [FromRoute] string categoryAlias,
        CancellationToken cancellationToken)
    {
        return View();
    }

    [HttpGet("{categoryAlias}/moderators/remove", Name = "CategoryRemoveModerator")]
    public async Task<IActionResult> RemoveModerator(
        [FromRoute] string categoryAlias,
        CancellationToken cancellationToken)
    {
        return View();
    }

    [HttpPost("{categoryAlias}/moderators/remove", Name = "CategoryRemoveModeratorConfirm")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveModeratorConfirm(
        [FromRoute] string categoryAlias,
        CancellationToken cancellationToken)
    {
        return View();
    }

    [HttpGet("{categoryAlias}/create", Name = "CategoryCreate")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        return View();
    }

    [HttpPost("{categoryAlias}/create", Name = "CategoryCreateConfirm")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateConfirm(CancellationToken cancellationToken)
    {
        return View();
    }

    [HttpGet("{categoryAlias}/edit", Name = "CategoryEdit")]
    public async Task<IActionResult> Edit(
        [FromRoute] string categoryAlias,
        CancellationToken cancellationToken)
    {
        return View();
    }

    [HttpPost("{categoryAlias}/edit", Name = "CategoryEditConfirm")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditConfirm(
        [FromRoute] string categoryAlias,
        CancellationToken cancellationToken)
    {
        return View();
    }

    [HttpGet("{categoryAlias}/delete", Name = "CategoryDelete")]
    public async Task<IActionResult> Delete(
        [FromRoute] string categoryAlias,
        CancellationToken cancellationToken)
    {
        return View();
    }

    [HttpPost("{categoryAlias}/delete", Name = "CategoryDeleteConfirm")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirm(
        [FromRoute] string categoryAlias,
        CancellationToken cancellationToken)
    {
        return View();
    }
}

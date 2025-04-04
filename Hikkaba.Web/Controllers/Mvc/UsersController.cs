using Hikkaba.Shared.Constants;
using Hikkaba.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hikkaba.Web.Controllers.Mvc;

[Authorize(Roles = Defaults.AdministratorRoleName)]
public sealed class UsersController : Controller
{
    private readonly IApplicationUserService _applicationUserService;

    public UsersController(
        IApplicationUserService applicationUserService)
    {
        _applicationUserService = applicationUserService;
    }

    /*
    [Route("Users/{id}")]
    public async Task<IActionResult> Details(int id)
    {
        var dto = await _applicationUserService.GetAsync(id);
        var viewModel = _mapper.Map<ApplicationUserViewModel>(dto);
        return View(viewModel);
    }

    [Route("Users")]
    public async Task<IActionResult> Index()
    {
        var dtoList = await _applicationUserService.ListAsync(new ApplicationUserFilter
        {
            OrderBy = [nameof(ApplicationUser.UserName)],
        });
        var viewModelList = _mapper.Map<List<ApplicationUserViewModel>>(dtoList);
        return View(viewModelList);
    }

    [Route("Users/Create")]
    public IActionResult Create()
    {
        return View();
    }

    [Route("Users/Create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ApplicationUserViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var dto = _mapper.Map<ApplicationUserDto>(viewModel);
            var id = await _applicationUserService.CreateAsync(dto);
            return RedirectToAction("Index");
        }
        else
        {
            ViewBag.ErrorMessage = ModelState.ModelErrorsToString();
            return View(viewModel);
        }
    }

    [Route("Users/{id}/Edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var dto = await _applicationUserService.GetAsync(id);
        var viewModel = _mapper.Map<ApplicationUserViewModel>(dto);
        return View(viewModel);
    }

    [Route("Users/{id}/Edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ApplicationUserViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var dto = await _applicationUserService.GetAsync(viewModel.Id);
            _mapper.Map(viewModel, dto);
            await _applicationUserService.EditAsync(dto);
            return RedirectToAction("Index");
        }
        else
        {
            ViewBag.ErrorMessage = ModelState.ModelErrorsToString();
            return View(viewModel);
        }
    }

    [Route("Users/{id}/Delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var dto = await _applicationUserService.GetAsync(id);
        var viewModel = _mapper.Map<ApplicationUserViewModel>(dto);
        return View(viewModel);
    }

    [Route("Users/{id}/Delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _applicationUserService.SetIsDeletedAsync(id, true);
        return RedirectToAction("Index");
    }
    */
}

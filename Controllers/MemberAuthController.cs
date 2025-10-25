using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Cms.Core.Security;
using Microsoft.AspNetCore.Identity;
using Umbraco.Cms.Core.Models;

namespace SeniorEventBooking.Controllers;

/// <summary>
/// Surface controller for member authentication (login/logout)
/// </summary>
public class MemberAuthController : SurfaceController
{
    private readonly IMemberService _memberService;
    private readonly IMemberManager _memberManager;
    private readonly SignInManager<MemberIdentityUser> _signInManager;

    public MemberAuthController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider,
        IMemberService memberService,
        IMemberManager memberManager,
        SignInManager<MemberIdentityUser> signInManager)
        : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
    {
        _memberService = memberService;
        _memberManager = memberManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View("~/Views/Partials/MemberLogin.cshtml");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HandleLogin(string username, string password, string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            TempData["LoginError"] = "Please enter both username and password.";
            return RedirectToCurrentUmbracoPage();
        }

        var result = await _memberManager.ValidateCredentialsAsync(username, password);

        if (result)
        {
            var member = await _memberManager.FindByNameAsync(username);
            if (member != null)
            {
                await _signInManager.SignInAsync(member, isPersistent: true);
                TempData["LoginSuccess"] = "You have successfully logged in!";
                
                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                
                return RedirectToCurrentUmbracoPage();
            }
        }

        TempData["LoginError"] = "Invalid username or password.";
        return RedirectToCurrentUmbracoPage();
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        TempData["LogoutSuccess"] = "You have been logged out.";
        return RedirectToCurrentUmbracoPage();
    }
}

using FantasyFootball.Models;
using FantasyFootball.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FantasyFootball.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // ===== Lokalna prijava =====

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid)
                return View(model);

            var email = model.Email.Trim();
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Neispravan email ili lozinka.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Neispravan email ili lozinka.");
                return View(model);
            }

            if (!user.FantasyTeamId.HasValue)
                return RedirectToAction("Build", "FantasyTeam");

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        // ===== Lokalna registracija =====

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var email = model.Email.Trim();

            if (await _userManager.FindByEmailAsync(email) != null)
            {
                ModelState.AddModelError(nameof(model.Email), "Korisnik s tim emailom već postoji.");
                return View(model);
            }

            var user = new AppUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                OIB = model.OIB,
                JMBG = model.JMBG
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, DAL.DbSeeder.UserRole);
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Build", "FantasyTeam");
        }

        // ===== Odjava =====

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied() => View();

        // ===== Vanjska prijava (Google) =====

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            if (remoteError != null)
            {
                TempData["LoginError"] = $"Greška vanjskog providera: {remoteError}";
                return RedirectToAction(nameof(Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                TempData["LoginError"] = "Nije moguće dohvatiti podatke vanjske prijave.";
                return RedirectToAction(nameof(Login));
            }

            // Postojeći korisnik s povezanim vanjskim loginom -> prijava
            var result = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (result.Succeeded)
            {
                var existing = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                if (existing != null && !existing.FantasyTeamId.HasValue)
                    return RedirectToAction("Build", "FantasyTeam");

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction("Index", "Home");
            }

            // Prva prijava ovim providerom -> dovrši registraciju (OIB/JMBG)
            var email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? string.Empty;
            var vm = new ExternalLoginConfirmationViewModel
            {
                Email = email,
                Provider = info.LoginProvider,
                ReturnUrl = returnUrl
            };
            return View("ExternalLoginConfirmation", vm);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model)
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                TempData["LoginError"] = "Sesija vanjske prijave je istekla. Pokušaj ponovno.";
                return RedirectToAction(nameof(Login));
            }

            if (!ModelState.IsValid)
                return View(model);

            var email = model.Email.Trim();
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new AppUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    OIB = model.OIB,
                    JMBG = model.JMBG
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    foreach (var error in createResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                    return View(model);
                }

                await _userManager.AddToRoleAsync(user, DAL.DbSeeder.UserRole);
            }

            var addLoginResult = await _userManager.AddLoginAsync(user, info);
            if (!addLoginResult.Succeeded)
            {
                foreach (var error in addLoginResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            await _signInManager.SignInAsync(user, isPersistent: false);

            if (!user.FantasyTeamId.HasValue)
                return RedirectToAction("Build", "FantasyTeam");

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);
            return RedirectToAction("Index", "Home");
        }
    }
}

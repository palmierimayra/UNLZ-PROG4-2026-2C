// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using TPLudoteca.Data;

namespace TPLudoteca.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class ExternalLoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserStore<ApplicationUser> _userStore;
    private readonly IUserEmailStore<ApplicationUser> _emailStore;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<ExternalLoginModel> _logger;

    public ExternalLoginModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IUserStore<ApplicationUser> userStore,
        ILogger<ExternalLoginModel> logger,
        IEmailSender emailSender)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _userStore = userStore;
        _emailStore = GetEmailStore();
        _logger = logger;
        _emailSender = emailSender;
    }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = default!;

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public string? ProviderDisplayName { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public string? ReturnUrl { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [TempData]
    public string? ErrorMessage { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InputModel
    {
        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [EmailAddress(ErrorMessage = "El email no es válido.")]
        public string Email { get; set; } = default!;
    }
        
    public IActionResult OnGet() => RedirectToPage("./Login");

    public IActionResult OnPost(string provider, string? returnUrl = null)
    {
        // Request a redirect to the external login provider.
        var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return new ChallengeResult(provider, properties);
    }

    public async Task<IActionResult> OnGetCallbackAsync(string? returnUrl = null, string? remoteError = null)
    {
        returnUrl = returnUrl ?? Url.Content("~/");
        if (remoteError != null)
        {
            ErrorMessage = $"Error del proveedor externo: {remoteError}";
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            ErrorMessage = "Error al obtener los datos de la cuenta externa.";
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }

        // Sign in the user with this external login provider if the user already has a login.
        var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
        if (result.Succeeded)
        {
            _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity?.Name, info.LoginProvider);

            var usuarioActual = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (usuarioActual != null && await GuardarFotoPerfilAsync(usuarioActual, info))
            {
                await _signInManager.RefreshSignInAsync(usuarioActual);
            }
            return LocalRedirect(returnUrl);
        }
        if (result.IsLockedOut)
        {
            return RedirectToPage("./Lockout");
        }

        // A partir de acá se inicia sesión automáticamente, sin pedir que confirme el email:
        // el proveedor (Google) ya verificó que el email le pertenece.
        if (result.IsNotAllowed)
        {
            // La cuenta ya está vinculada a Google pero quedó sin confirmar
            // (por ejemplo, creada con el registro anterior): se confirma y se ingresa.
            var usuarioVinculado = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (usuarioVinculado != null && !usuarioVinculado.EmailConfirmed)
            {
                usuarioVinculado.EmailConfirmed = true;
                await _userManager.UpdateAsync(usuarioVinculado);
                return await IniciarSesionExternaAsync(usuarioVinculado, info, returnUrl);
            }
        }

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email))
        {
            // El proveedor no devolvió el email: se le pide al usuario con el formulario.
            ReturnUrl = returnUrl;
            ProviderDisplayName = info.ProviderDisplayName;
            return Page();
        }

        var usuario = await _userManager.FindByEmailAsync(email);
        if (usuario != null)
        {
            // Ya existe una cuenta local con ese email.
            if (!usuario.EmailConfirmed)
            {
                // No se vincula una cuenta sin confirmar: podría haberla creado otra persona
                // con este email, y al vincularla quedaría con acceso a la cuenta.
                ErrorMessage = "Error: ya existe una cuenta con ese email que todavía no fue confirmada. Confirmala desde tu email o ingresá con tu contraseña.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
            if (await _userManager.IsLockedOutAsync(usuario))
            {
                return RedirectToPage("./Lockout");
            }

            var vincular = await _userManager.AddLoginAsync(usuario, info);
            if (!vincular.Succeeded)
            {
                ErrorMessage = "Error al vincular la cuenta de Google.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
            _logger.LogInformation("Se vinculó {LoginProvider} a una cuenta existente.", info.LoginProvider);
            return await IniciarSesionExternaAsync(usuario, info, returnUrl);
        }

        // No existe: se crea la cuenta con el email de Google, ya confirmada.
        var nuevoUsuario = CreateUser();
        await _userStore.SetUserNameAsync(nuevoUsuario, email, CancellationToken.None);
        await _emailStore.SetEmailAsync(nuevoUsuario, email, CancellationToken.None);
        nuevoUsuario.EmailConfirmed = true;

        var crear = await _userManager.CreateAsync(nuevoUsuario);
        if (crear.Succeeded)
        {
            crear = await _userManager.AddLoginAsync(nuevoUsuario, info);
        }
        if (crear.Succeeded)
        {
            crear = await _userManager.AddToRoleAsync(nuevoUsuario, "Socio");
        }
        if (!crear.Succeeded)
        {
            ErrorMessage = "Error al crear la cuenta: " + string.Join(" ", crear.Errors.Select(e => e.Description));
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }

        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
        return await IniciarSesionExternaAsync(nuevoUsuario, info, returnUrl);
    }

    private async Task<IActionResult> IniciarSesionExternaAsync(ApplicationUser user, ExternalLoginInfo info, string returnUrl)
    {
        await GuardarFotoPerfilAsync(user, info);
        await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
        return LocalRedirect(returnUrl);
    }

    // Guarda (o actualiza) la URL de la foto de Google como claim del usuario.
    // Devuelve true si hubo cambios.
    private async Task<bool> GuardarFotoPerfilAsync(ApplicationUser user, ExternalLoginInfo info)
    {
        var foto = info.Principal.FindFirstValue("urn:google:picture");
        if (string.IsNullOrEmpty(foto))
        {
            return false;
        }

        var actual = (await _userManager.GetClaimsAsync(user)).FirstOrDefault(c => c.Type == "urn:google:picture");
        if (actual?.Value == foto)
        {
            return false;
        }

        var nueva = new Claim("urn:google:picture", foto);
        var resultado = actual == null
            ? await _userManager.AddClaimAsync(user, nueva)
            : await _userManager.ReplaceClaimAsync(user, actual, nueva);
        return resultado.Succeeded;
    }

    public async Task<IActionResult> OnPostConfirmationAsync(string? returnUrl = null)
    {
        returnUrl = returnUrl ?? Url.Content("~/");
        // Get the information about the user from the external login provider
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            ErrorMessage = "Error al obtener los datos de la cuenta externa durante el registro.";
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }

        if (ModelState.IsValid)
        {
            var user = CreateUser();

            await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

            var result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                result = await _userManager.AddLoginAsync(user, info);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Socio");

                    _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code },
                        protocol: Request.Scheme)!;

                    await _emailSender.SendEmailAsync(Input.Email, "Confirmá tu email",
                        $"Confirmá tu cuenta haciendo <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>click acá</a>.");

                    // If account confirmation is required, we need to show the link if we don't have a real email sender
                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("./RegisterConfirmation", new { Email = Input.Email });
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                    return LocalRedirect(returnUrl);
                }
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        ProviderDisplayName = info.ProviderDisplayName;
        ReturnUrl = returnUrl;
        return Page();
    }

    private ApplicationUser CreateUser()
    {
        try
        {
            return Activator.CreateInstance<ApplicationUser>();
        }
        catch
        {
            throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                $"override the external login page in /Areas/Identity/Pages/Account/ExternalLogin.cshtml");
        }
    }

    private IUserEmailStore<ApplicationUser> GetEmailStore()
    {
        if (!_userManager.SupportsUserEmail)
        {
            throw new NotSupportedException("The default UI requires a user store with email support.");
        }
        return (IUserEmailStore<ApplicationUser>)_userStore;
    }
}

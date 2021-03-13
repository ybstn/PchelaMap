using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using PchelaMap.Areas.Identity.Data;
using AspNet.Security.OAuth.Vkontakte;
using TimeZoneConverter;

namespace PchelaMap.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<PchelaMapUser> _signInManager;
        private readonly UserManager<PchelaMapUser> _userManager;
        //private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;
        static readonly HttpClient HttpClient = new HttpClient();
        public ExternalLoginModel(
            SignInManager<PchelaMapUser> signInManager,
            UserManager<PchelaMapUser> userManager,
            ILogger<ExternalLoginModel> logger
            //,
            //IEmailSender emailSender
            )
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            //_emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ProviderDisplayName { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }
        TimeZoneInfo _TimeZoneinfo = TZConvert.GetTimeZoneInfo("Russian Standard Time");
        public IActionResult OnGetAsync()
        {
            return RedirectToPage("./Login");
        }
        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            //1
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Ошибка: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();


            if (info == null)
            {
                ErrorMessage = "Ошибка при получении данных от внешнего сайта.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
            //проверка на уже существуещего юзера с таким аккаунтом
            string UserEmail = "";
            PchelaMapUser ExistingUser = null;
            if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
            {
                UserEmail = info.Principal.FindFirstValue(ClaimTypes.Email);
                ExistingUser = await _userManager.FindByEmailAsync(UserEmail);
            }
            if (ExistingUser != null)
            {
                await _signInManager.SignInAsync(ExistingUser, isPersistent: true, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }
            else
            {
                // Sign in the user with this external login provider if the user already has a login.
                var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: true, bypassTwoFactor: true);
                //result = null;//удалить
                if (result.Succeeded)
                {
                    //3 срабатывает, когда пользователь уже имеет запись в базе данных
                    _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                   
                    return LocalRedirect(returnUrl);
                }
                if (result.IsLockedOut)
                {
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    //2 возвращает страницу с вводом почты
                    // If the user does not have an account, then ask the user to create an account.

                    if (!info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                    {
                        Input = new InputModel
                        {
                            Email = ""
                        };
                        ProviderDisplayName = info.ProviderDisplayName;
                        ReturnUrl = returnUrl;
                        return Page();
                    }
                    if (ModelState.IsValid)
                    {
                        var user = new PchelaMapUser();


                        user = new PchelaMapUser
                        {
                            UserName = info.Principal.FindFirstValue(ClaimTypes.Email),
                            Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                        };


                        var resultCreateUser = await _userManager.CreateAsync(user);
                        if (resultCreateUser.Succeeded)
                        {
                            var resultAddLogin = await _userManager.AddLoginAsync(user, info);
                            if (resultAddLogin.Succeeded)
                            {
                                _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                                //if (info.Principal.HasClaim(c => c.Type == ClaimTypes.GivenName))
                                //{
                                //    await _userManager.AddClaimAsync(user,
                                //        info.Principal.FindFirst(ClaimTypes.GivenName));
                                //}
                                //mypart
                                string Image = null;
                                string username = null;
                                string SocId = null;
                                string token = null;
                                if (info.LoginProvider == "Facebook")
                                {
                                    username = info.Principal.FindFirstValue(ClaimTypes.Name);
                                    SocId = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
                                    token = info.AuthenticationTokens.FirstOrDefault(x=> x.Name== "access_token").Value;
                                    string ImgUrl = $"https://graph.facebook.com/"+info.ProviderKey+"/picture?type=square&width=150&access_token=" + token;
                                    HttpResponseMessage response = await HttpClient.GetAsync(ImgUrl);
                                    if(response.IsSuccessStatusCode)
                                    {
                                        Image = $"https://graph.facebook.com/"+info.ProviderKey+"/picture?type=square&width=150&access_token=" + token;
                                    }
                                    else
                                    {
                                        Image = "/Images/EmptyUserRound.png";
                                    }
                                    
                                    //здесь нужна проверка запроса к фэйсбук и если возвращает ошибку, то ставим пресетную картинку
                                }
                                if (info.LoginProvider == "Vkontakte")
                                {
                                    username = info.Principal.FindFirstValue(ClaimTypes.GivenName) + " " + info.Principal.FindFirstValue(ClaimTypes.Surname);
                                    SocId = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
                                    Image = info.Principal.Claims.FirstOrDefault(x => x.Type == "urn:vkontakte:photo_thumb:link").Value;

                                }
                                if (Image == null)
                                {
                                    Image = "/Images/EmptyUserRound.png";
                                }
                                user.Name = username;
                                user.SocialAccountID = SocId;
                                user.UserPhoto = Image;
                                user.CreatedDateUtc = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _TimeZoneinfo).ToString("dd.MM.yyyy HH:mm:ss");
                                user.EmailConfirmed = true;
                                await _userManager.AddToRoleAsync(user, "user");

                                await _signInManager.SignInAsync(user, isPersistent: true, info.LoginProvider);
                                await _userManager.UpdateAsync(user);

                                EmailService emailSender = new EmailService();
                                var userMail = user.Email;
                                string mailHeader = EmailService._onRegistration["header"];
                                string mailMessage = EmailService._onRegistration["body"];
                                await emailSender.SendAsync(userMail, mailHeader, mailMessage);
                                return LocalRedirect("/Home/Instructions");
                                
                            }
                        }
                        foreach (var error in resultCreateUser.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }

                }
            }
            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();

        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Ошибка при получении данных от внешнего сайта.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {
                var user = new PchelaMapUser
                {
                    UserName = Input.Email,
                    Email = Input.Email
                };

                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                        //if (info.Principal.HasClaim(c => c.Type == ClaimTypes.GivenName))
                        //{
                        //    await _userManager.AddClaimAsync(user,
                        //        info.Principal.FindFirst(ClaimTypes.GivenName));
                        //}
                        //mypart

                        string Image = null;
                        string username = null;
                        string SocId = null;
                        string token = null;
                        if (info.LoginProvider == "Facebook")
                        {
                            username = info.Principal.FindFirstValue(ClaimTypes.Name);
                            SocId = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
                            token = info.AuthenticationTokens.FirstOrDefault(x => x.Name == "access_token").Value;
                            Image = $"https://graph.facebook.com/{info.ProviderKey}/picture?type=square&width=150&access_token=" + token;
                        }
                        if (info.LoginProvider == "Vkontakte")
                        {
                            username = info.Principal.FindFirstValue(ClaimTypes.GivenName) + " " + info.Principal.FindFirstValue(ClaimTypes.Surname);
                            SocId = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
                            Image = info.Principal.Claims.FirstOrDefault(x => x.Type == "urn:vkontakte:photo_thumb:link").Value;
                        }
                        if (Image == null)
                        {
                            Image = "/Images/EmptyUserRound.png";
                        }
                        user.Name = username;
                        user.SocialAccountID = SocId;
                        user.UserPhoto = Image;
                        user.CreatedDateUtc = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _TimeZoneinfo).ToString("dd.MM.yyyy HH:mm:ss");
                        await _userManager.AddToRoleAsync(user, "user");
                        //end of mypart
                    

                        var userId = await _userManager.GetUserIdAsync(user);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = userId, code = code },
                            protocol: Request.Scheme);
                        EmailService emailSender = new EmailService();
                        var userMail = user.Email;
                        
                        string mailHeader = "Подтверждение электронной почты";
                        string mailMessage = $"Пожалуйста, подтвердите электронную почту: <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>перейти по ссылке</a>.";
                        await emailSender.SendAsync(userMail, mailHeader, mailMessage , 1);

                       
                        await _signInManager.SignInAsync(user, isPersistent: true, info.LoginProvider);
                        await _userManager.UpdateAsync(user);
                        return LocalRedirect("/Home/Instructions");
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
    }
}

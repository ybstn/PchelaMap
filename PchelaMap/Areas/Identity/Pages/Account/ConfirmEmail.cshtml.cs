using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using PchelaMap.Areas.Identity.Data;
namespace PchelaMap.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<PchelaMapUser> _userManager;

        public ConfirmEmailModel(UserManager<PchelaMapUser> userManager)
        {
            _userManager = userManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Нет пользователя с id: '{userId}'.");
            }
            else
            {
                user.EmailConfirmed = true;
            }
            await _userManager.UpdateAsync(user);
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            StatusMessage = result.Succeeded ? "Спасибо, почта подтверждена." : "Почта не подтверждена.";
            user.SystemMessageForUser = StatusMessage;
            await _userManager.UpdateAsync(user);
            return RedirectToPage("/Index");
            //return LocalRedirect("/Home/Instructions");
        }
    }
}

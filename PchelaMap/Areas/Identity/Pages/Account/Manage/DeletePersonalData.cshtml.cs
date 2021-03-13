using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using PchelaMap.Areas.Identity.Data;
using PchelaMap.Data;

namespace PchelaMap.Areas.Identity.Pages.Account.Manage
{
    public class DeletePersonalDataModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<PchelaMapUser> _userManager;
        private readonly SignInManager<PchelaMapUser> _signInManager;
        private readonly ILogger<DeletePersonalDataModel> _logger;
        private readonly IWebHostEnvironment _env;
        public DeletePersonalDataModel(
            ApplicationDbContext context,
            UserManager<PchelaMapUser> userManager,
            SignInManager<PchelaMapUser> signInManager,
            IWebHostEnvironment env,
            ILogger<DeletePersonalDataModel> logger)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _env = env;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public bool RequirePassword { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            if (RequirePassword)
            {
                if (!await _userManager.CheckPasswordAsync(user, Input.Password))
                {
                    ModelState.AddModelError(string.Empty, "Incorrect password.");
                    return Page();
                }
            }
            var webRoot = _env.WebRootPath;
            string FullDirpath = "";

            List<PchelaMapTask> _tasks = _context.Tasks.Where(x => x.UserId == user.Id).ToList();
            List<PchelaMapUserTasks> _userstasks = _context.UsersTasks.ToList();
            List<PchelaMapUserTasks> _takenTasks = _context.UsersTasks.Where(x => x.UserID == user.Id).ToList();
            List<UsersRefusedTasks> _refusedTasks = _context.UsersRefusedFromTasks.Where(x => x.UserID == user.Id).ToList();
            foreach (PchelaMapTask t in _tasks)
            {
                if (_userstasks.Any(x => x.TaskID == t.id))
                {
                    PchelaMapUserTasks _myTaskTaken = _userstasks.FirstOrDefault(x => x.TaskID == t.id);
                    FullDirpath = System.IO.Path.Combine(webRoot, _myTaskTaken.MediaFolder);
                    if (System.IO.Directory.Exists(FullDirpath))
                    {
                        System.IO.Directory.Delete(FullDirpath, true);
                    }
                    _userstasks.Remove(_myTaskTaken);
                }
                FullDirpath = System.IO.Path.Combine(webRoot, t.ResultMediaFolder);
                if (System.IO.Directory.Exists(FullDirpath))
                {
                    System.IO.Directory.Delete(FullDirpath, true);
                }
            }
            foreach (PchelaMapUserTasks tsk in _takenTasks)
            {
                FullDirpath = System.IO.Path.Combine(webRoot, tsk.MediaFolder);
                if (System.IO.Directory.Exists(FullDirpath))
                {
                    System.IO.Directory.Delete(FullDirpath, true);
                }
            }
            string jsonFolderPath = System.IO.Path.Combine(webRoot, "js/jsons/" + user.Id);
            if (System.IO.Directory.Exists(jsonFolderPath))
            {
                System.IO.Directory.Delete(jsonFolderPath, true);
            }
            string AvatarPath = System.IO.Path.Combine(webRoot, "Images/UsersPhoto/" + user.Id);
            if (System.IO.Directory.Exists(AvatarPath))
            {
                System.IO.Directory.Delete(AvatarPath, true);
            }
            _context.UsersRefusedFromTasks.RemoveRange(_refusedTasks);
            _context.Tasks.RemoveRange(_tasks);
            _context.SaveChanges();
            List<PchelaMapTask> _allTasks = _context.Tasks.ToList();
            foreach (PchelaMapUserTasks tsk in _takenTasks)
            {
                if (_allTasks.Any(x => x.id == tsk.TaskID && x.Status != "closed"))
                {
                    _allTasks.FirstOrDefault(x => x.id == tsk.TaskID && x.Status != "closed").Status = "moderating";
                    _allTasks.FirstOrDefault(x => x.id == tsk.TaskID && x.Status != "closed").AdminComment = "Пользователь, взяший задание удалил аккаунт. " +
                        "Не известно выполнил ли он его фактически";
                }

            }
            await _context.SaveChangesAsync();
            var result = await _userManager.DeleteAsync(user);
            var userId = await _userManager.GetUserIdAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred deleting user with ID '{userId}'.");
            }

            await _signInManager.SignOutAsync();

            _logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);

            return Redirect("~/");
        }
    }
}

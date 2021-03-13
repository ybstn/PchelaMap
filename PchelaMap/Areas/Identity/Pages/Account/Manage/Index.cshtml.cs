using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PchelaMap.Areas.Identity.Data;
using PchelaMap.Models;
using PchelaMap.Data;

namespace PchelaMap.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<PchelaMapUser> _userManager;
        private readonly SignInManager<PchelaMapUser> _signInManager;
        private readonly IWebHostEnvironment _env;
        public IndexModel(
            ApplicationDbContext context,
            UserManager<PchelaMapUser> userManager,
            SignInManager<PchelaMapUser> signInManager,
            IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _env = env;
        }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {


            [Display(Name = "UserName")]
            [Required(ErrorMessage = "введите Ваше имя и фамилию")]
            [MaxLength(50)]
            public string UserName { get; set; }
            [Required(ErrorMessage = "отметьте на карте Ваш адрес")]
            public string UserAdress { get; set; }
            public string UserCoordinates { get; set; }
            public string UserPhoto { get; set; }
            public bool HasCar { get; set; }
            public int UserPoints { get; set; }
            [Phone]
            [Required(ErrorMessage = "введите номер телефона")]
            [DataType(DataType.PhoneNumber)]
            [RegularExpression(@"^\(?([8]{1})\)?([0-9]{10})$", ErrorMessage = "введите номер в формате: 89991112233")]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }
        }

        private async Task LoadAsync(PchelaMapUser user)
        {

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Input = new InputModel
            {
                UserName = user.Name,
                UserAdress = user.UserAdress,
                UserCoordinates = user.UserCoordinateX + ',' + user.UserCoordinateY,
                UserPhoto = user.UserPhoto,
                PhoneNumber = phoneNumber,
                HasCar = (user.HasCar == 1),
                UserPoints = user.UserPoints
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            User UserinfoForParse = new User
            {
                id = user.Id,
                CoordinateX = user.UserCoordinateX,
                CoordinateY = user.UserCoordinateY,
                PhotoUrl = user.UserPhoto
            };
            jsonGenerator(UserinfoForParse);
            ViewData["UserId"] = user.Id;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(IFormFile uploadedFile)
        {
            var webRoot = _env.WebRootPath;
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Ошибка при сохранени номера телефона.";
                    return RedirectToPage();
                }
            }

            user.UserAdress = Input.UserAdress;
            user.Name = Input.UserName;
            user.HasCar = Input.HasCar ? 1 : 0;
            string[] CoordinatesString = Input.UserCoordinates.Split(',');
            user.UserCoordinateX = CoordinatesString[0];
            user.UserCoordinateY = CoordinatesString[1];
            if (uploadedFile != null)
            {
                string fileName = user.Id + ".jpg";
                string Filepath = "Images/UsersPhoto/" + user.Id;
                string FilePathWithName = "Images/UsersPhoto/" + user.Id + "/" + fileName;
                string path = System.IO.Path.Combine(webRoot, Filepath);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string PathWithFileName = System.IO.Path.Combine(path, fileName);
                Image CurrImg = null;
                using (var fileStream = new FileStream(PathWithFileName, FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fileStream);
                    CurrImg = Image.FromStream(fileStream);
                }
                ThumbGenerator(PathWithFileName, CurrImg, 150);
                user.UserPhoto = '/' + FilePathWithName;
            }
            //логика смены информации о заданиях пользователя при изменениях в настройках профиля
            //List<PchelaMapTask> _tasks = _context.Tasks.Where(x => x.UserId == user.Id).ToList();
            //foreach (PchelaMapTask tk in _tasks)
            //{
                //tk.Name = user.Name;
                //tk.Phone = user.PhoneNumber;
                //tk.Photo = user.UserPhoto;
            //}
            await _userManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Ваш профиль обновлён";
            return RedirectToPage();
        }

        //файл лочится при обращении, поэтому надо его копировать в другой объект и править его
        private void ThumbGenerator(string ThumbPath, Image CurrImg, int ThumbWidth)
        {
            Image Thumb = CurrImg;
            int X = CurrImg.Width;
            int Y = CurrImg.Height;
            int width = (int)((X * ThumbWidth) / Y);
            Thumb = Thumb.GetThumbnailImage(width, ThumbWidth, () => false, IntPtr.Zero);
            Thumb.Save(ThumbPath);
        }
        private async void jsonGenerator(User userInfoForParse)
        {
            var webRoot = _env.WebRootPath;
            string _fileName = "UserPersonal.json";
            string readyPath = "";
            var _path = Path.Combine(webRoot, "js/jsons/" + userInfoForParse.id);
            if(!System.IO.Directory.Exists(_path))
            {
                Directory.CreateDirectory(_path);
                
            }
            readyPath = _path + "/" + _fileName;
            using (FileStream fs = System.IO.File.Create(readyPath))
            {
                await JsonSerializer.SerializeAsync(fs, userInfoForParse);
            }

        }
    }
}

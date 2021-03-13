using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using PchelaMap.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using Microsoft.AspNetCore.Identity;
using PchelaMap.Areas.Identity.Data;
using PchelaMap.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using TimeZoneConverter;
using Microsoft.AspNetCore.Http;
using System.IO.Compression;
using CsvHelper;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PchelaMap.Controllers
{

    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<PchelaMapUser> _signInManager;
        private readonly UserManager<PchelaMapUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _env;
        static List<UserWithTasks> _UsersTaskList = new List<UserWithTasks>();
        public AdminController(ApplicationDbContext context, ILogger<HomeController> logger, IWebHostEnvironment env, UserManager<PchelaMapUser> userManager, SignInManager<PchelaMapUser> signInManager, RoleManager<IdentityRole> roleManager, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _env = env;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _httpContextAccessor = httpContextAccessor;
        }
        TimeZoneInfo _TimeZoneinfo = TZConvert.GetTimeZoneInfo("Russian Standard Time");
        private static AdminModal _modalData = new AdminModal();

        public async Task<IActionResult> Index(SortSearchPagin.UserSortState sortOrder, [FromRoute] List<PchelaMapUser> searchResult, bool ShowSingle = false, bool ShowGroupByTask = false, bool ShowDone = false, string UserId = "", string TaskId = "", string active = "all", bool searchCall = false )
        {
            
            var __user = await _userManager.GetUserAsync(User);
            List<PchelaMapUser> DBUsersList = new List<PchelaMapUser>();
            if (ShowGroupByTask && TaskId != "")
            {
                if (ShowDone)
                {
                    List<PchelaMapUserTasks> TasksInLinkTable = _context.UsersTasks.Where(x => x.TaskID == TaskId && x.Status == "done").ToList();
                    List<string> IdsList = TasksInLinkTable.Select(i => i.UserID).ToList();
                    DBUsersList = _context.Users.Where(x => IdsList.Contains(x.Id)).Include(c => c.Tasks).ToList();
                }
                else
                {
                    List<PchelaMapUserTasks> TasksInLinkTable = _context.UsersTasks.Where(x => x.TaskID == TaskId).ToList();
                    List<string> IdsList = TasksInLinkTable.Select(i => i.UserID).ToList();
                    DBUsersList = _context.Users.Where(x => IdsList.Contains(x.Id)).Include(c => c.Tasks).ToList();
                }

            }
            else
            {
                if (ShowSingle && UserId != "")
                {
                    DBUsersList = _context.Users.Where(x => x.Id == UserId).Include(c => c.Tasks).ToList();
                }
                else
                {
                            List<string> UsersIds = new List<string>();
                            switch(active)
                            {
                                case "user":
                                    UsersIds = _context.UserRoles.Where(r => r.RoleId == "5888d3c0-eec7-4e2b-8481-e813089a3c16").Select(b => b.UserId).ToList();
                                    DBUsersList = _context.Users.Where(x => UsersIds.Contains(x.Id)).Include(c => c.Tasks).ToList();
                                    if (DBUsersList.Count()==0)
                                    {
                                        ViewBag.UsersCountMessage = "В базе данных нет пользователей(((";
                                    }
                                    break;
                                case "admin":
                                    UsersIds = _context.UserRoles.Where(r => r.RoleId == "d258c4e4-a974-466b-9189-83fc350a96c8").Select(b => b.UserId).ToList();
                                    DBUsersList = _context.Users.Where(x => UsersIds.Contains(x.Id)).Include(c => c.Tasks).ToList();
                                    if (DBUsersList.Count() == 0)
                                    {
                                        ViewBag.UsersCountMessage = "В базе данных нет администраторов. Свяжитесь с разработчиком";
                                    }
                                    break;
                                case "moder":
                                    UsersIds = _context.UserRoles.Where(r => r.RoleId == "7d8a85a4-6a8a-4d26-9094-23148c2c2abe").Select(b => b.UserId).ToList();
                                    DBUsersList = _context.Users.Where(x => UsersIds.Contains(x.Id)).Include(c => c.Tasks).ToList();
                                    if (DBUsersList.Count() == 0)
                                    {
                                        ViewBag.UsersCountMessage = "В базе данных нет модераторов";
                                    }
                                    break;
                                case "ban":
                                    UsersIds = _context.UserRoles.Where(r => r.RoleId == "24911f26-3b21-47ae-be22-70666990aa05").Select(b => b.UserId).ToList();
                                    DBUsersList = _context.Users.Where(x => UsersIds.Contains(x.Id)).Include(c => c.Tasks).ToList();
                                    if (DBUsersList.Count() == 0)
                                    {
                                        ViewBag.UsersCountMessage = "В базе данных нет забаненных пользователей";
                                    }
                                    break;
                                default:
                                    DBUsersList = _context.Users.Include(c => c.Tasks).ToList();
                                    if (DBUsersList.Count() == 0)
                                    {
                                        ViewBag.UsersCountMessage = "В базе данных нет пользователей(((";
                                    }
                                    break;
                            }
                }

            }
            string nowDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _TimeZoneinfo).ToString("dd.MM.yyyy HH:mm:ss");
            DBUsersList = DBUsersList.OrderByDescending(s => DateTime.ParseExact((s.CreatedDateUtc == null || s.CreatedDateUtc == "" ? nowDate : s.CreatedDateUtc), "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture)).ToList();
            List<string> roles = new List<string>();
            int j = 0;
            foreach (PchelaMapUser user in DBUsersList)
            {
                string str = "";
                var RoleManage = await _userManager.GetRolesAsync(user);
                foreach (var role in RoleManage)
                { str = (str == "") ? role.ToString() : str + " , " + role.ToString(); }
                roles.Add(str);
                j++;
            }
            List<User> UsersList = new List<User>();
            int _index = 0;
            UsersList = (from PchelaMapUser bdvar in DBUsersList
                         select new User
                         {
                             InListNumber = _index + 1,
                             id = bdvar.Id,
                             SocialMediaId = bdvar.SocialAccountID,
                             LoginProviderName = GetLoginProviderName(bdvar),
                             CoordinateX = bdvar.UserCoordinateX,
                             CoordinateY = bdvar.UserCoordinateY,
                             Name = bdvar.Name,
                             Phone = bdvar.PhoneNumber,
                             Email = bdvar.Email,
                             PhotoUrl = bdvar.UserPhoto,
                             TasksCount = bdvar.Tasks.Count().ToString(),
                             TakenTasksCount = _context.UsersTasks.Where(x => x.UserID == bdvar.Id).Count().ToString(),
                             DoneTasksCount = (_context.UsersTasks.Where(x => x.UserID == bdvar.Id && x.Status == "done").Count() + _context.Tasks.Where(x => x.UserId == bdvar.Id && x.Status == "closed").Count()).ToString(),
                             Adress = bdvar.UserAdress,
                             HasCar = bdvar.HasCar,
                             RegistrationDate = bdvar.CreatedDateUtc,
                             UserPoints = bdvar.UserPoints,
                             uncompletedTasks = bdvar.uncompletedTasks,
                             Role = roles.ElementAt(_index),
                             _index = _index++
                         }).ToList();
            if (!ShowSingle && UserId == "")
            {
                UsersList = UsersSorting(sortOrder, UsersList);
            }
            ViewBag.PageActive = "users";
            MenuShowElementsCount();
            return View(UsersList);
        }
        private string GetLoginProviderName(PchelaMapUser bdvar)
        {
            var LoginInfo = _userManager.GetLoginsAsync(bdvar).Result;
            if (LoginInfo.Count() > 0)
            {
                return LoginInfo.Single().ProviderDisplayName;
            }
            else
            {
                return "";
            }
        }
            
        //сорировка пользователей
        private List<User> UsersSorting(SortSearchPagin.UserSortState sortOrder, List<User> UsersList)
        {
            ViewData["IndexSort"] = (sortOrder == SortSearchPagin.UserSortState.IndexDesc ?
               SortSearchPagin.UserSortState.IndexAsc : SortSearchPagin.UserSortState.IndexDesc);
            ViewData["DateSort"] = (sortOrder == SortSearchPagin.UserSortState.DateAsc ?
                  SortSearchPagin.UserSortState.DateDesc : SortSearchPagin.UserSortState.DateAsc);
            ViewData["NameSort"] = (sortOrder == SortSearchPagin.UserSortState.NameAsc ?
                SortSearchPagin.UserSortState.NameDesc : SortSearchPagin.UserSortState.NameAsc);
            ViewData["HasAutoSort"] = (sortOrder == SortSearchPagin.UserSortState.HasAutoAsc ?
               SortSearchPagin.UserSortState.HasAutoDesc : SortSearchPagin.UserSortState.HasAutoAsc);
            ViewData["CreatedTasksSort"] = (sortOrder == SortSearchPagin.UserSortState.CreatedTasksAsc ?
               SortSearchPagin.UserSortState.CreatedTasksAsc : SortSearchPagin.UserSortState.CreatedTasksDesc);
            ViewData["TakenTasksSort"] = (sortOrder == SortSearchPagin.UserSortState.TakenTasksDesc ?
                SortSearchPagin.UserSortState.TakenTasksAsc : SortSearchPagin.UserSortState.TakenTasksDesc);
            ViewData["PointsSort"] = (sortOrder == SortSearchPagin.UserSortState.PointsDesc ?
                SortSearchPagin.UserSortState.PointsAsc : SortSearchPagin.UserSortState.PointsDesc);
            ViewData["UnfinishedSort"] = (sortOrder == SortSearchPagin.UserSortState.UnfinishedDesc ?
                SortSearchPagin.UserSortState.UnfinishedAsc : SortSearchPagin.UserSortState.UnfinishedDesc);
            UsersList = sortOrder switch
            {
                SortSearchPagin.UserSortState.IndexAsc => UsersList.OrderBy(s => s._index).ToList(),
                SortSearchPagin.UserSortState.IndexDesc => UsersList.OrderByDescending(s => s._index).ToList(),
                SortSearchPagin.UserSortState.DateAsc => UsersList.OrderBy(s => DateTime.ParseExact(s.RegistrationDate, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture)).ToList(),
                SortSearchPagin.UserSortState.DateDesc => UsersList.OrderByDescending(s => DateTime.ParseExact(s.RegistrationDate, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture)).ToList(),
                SortSearchPagin.UserSortState.NameAsc => UsersList.OrderBy(s => s.Name).ToList(),
                SortSearchPagin.UserSortState.NameDesc => UsersList.OrderByDescending(s => s.Name).ToList(),
                SortSearchPagin.UserSortState.HasAutoAsc => UsersList.OrderBy(s => s.HasCar).ToList(),
                SortSearchPagin.UserSortState.HasAutoDesc => UsersList.OrderByDescending(s => s.HasCar).ToList(),
                SortSearchPagin.UserSortState.CreatedTasksAsc => UsersList.OrderBy(s => int.Parse(s.DoneTasksCount)).ToList(),
                SortSearchPagin.UserSortState.CreatedTasksDesc => UsersList.OrderByDescending(s => int.Parse(s.DoneTasksCount)).ToList(),
                SortSearchPagin.UserSortState.TakenTasksAsc => UsersList.OrderBy(s => int.Parse(s.TakenTasksCount)).ToList(),
                SortSearchPagin.UserSortState.TakenTasksDesc => UsersList.OrderByDescending(s => int.Parse(s.TakenTasksCount)).ToList(),
                SortSearchPagin.UserSortState.PointsAsc => UsersList.OrderBy(s => s.UserPoints).ToList(),
                SortSearchPagin.UserSortState.PointsDesc => UsersList.OrderByDescending(s => s.UserPoints).ToList(),
                SortSearchPagin.UserSortState.UnfinishedAsc => UsersList.OrderBy(s => s.uncompletedTasks).ToList(),
                SortSearchPagin.UserSortState.UnfinishedDesc => UsersList.OrderByDescending(s => s.uncompletedTasks).ToList(),
                _ => UsersList,
            };
            return UsersList;
        }
        //поиск пользователей 
        public PartialViewResult _UsersSearch()
        {
            UsersSearchModel inputmodel = new UsersSearchModel(); 
            return PartialView(inputmodel);
        }
        [HttpPost]
        public async Task<IActionResult> _UsersSearch(UsersSearchModel _searchmodel)
        {
            List<PchelaMapUser> DBUsersList = new List<PchelaMapUser>();
            DBUsersList = _context.Users.Include(c => c.Tasks).ToList();
            if (_searchmodel.name != "" && _searchmodel.name != null)
            {
                DBUsersList = DBUsersList.Where(x => x.Name?.IndexOf(_searchmodel.name, StringComparison.OrdinalIgnoreCase)>=0).ToList();
            }
            if (_searchmodel.mail != "" && _searchmodel.mail != null)
            {
                DBUsersList = DBUsersList.Where(x=> x.Email?.IndexOf(_searchmodel.mail, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }
            if (_searchmodel.phone != "" && _searchmodel.phone != null)
            {
                DBUsersList = DBUsersList.Where(x =>x.PhoneNumber?.IndexOf(_searchmodel.phone, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }
            if (_searchmodel.adress != "" && _searchmodel.adress != null)
            {
                DBUsersList = DBUsersList.Where(x => x.UserAdress?.IndexOf(_searchmodel.adress, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }
            List<string> roles = new List<string>();
            int j = 0;
            foreach (PchelaMapUser user in DBUsersList)
            {
                string str = "";
                var RoleManage = await _userManager.GetRolesAsync(user);
                foreach (var role in RoleManage)
                { str = (str == "") ? role.ToString() : str + " , " + role.ToString(); }
                roles.Add(str);
                j++;
            }
            List<User> UsersList = new List<User>();
            int _index = 0;

            DBUsersList = DBUsersList.OrderByDescending(s => DateTime.ParseExact(s.CreatedDateUtc, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture)).ToList();
            UsersList = (from PchelaMapUser bdvar in DBUsersList
                         select new User
                         {
                             InListNumber = _index + 1,
                             id = bdvar.Id,
                             SocialMediaId = bdvar.SocialAccountID,
                             LoginProviderName = _userManager.GetLoginsAsync(bdvar).Result.Single().ProviderDisplayName,
                             CoordinateX = bdvar.UserCoordinateX,
                             CoordinateY = bdvar.UserCoordinateY,
                             Name = bdvar.Name,
                             Phone = bdvar.PhoneNumber,
                             Email = bdvar.Email,
                             PhotoUrl = bdvar.UserPhoto,
                             TasksCount = bdvar.Tasks.Count().ToString(),
                             TakenTasksCount = _context.UsersTasks.Where(x => x.UserID == bdvar.Id).Count().ToString(),
                             Adress = bdvar.UserAdress,
                             HasCar = bdvar.HasCar,
                             RegistrationDate = bdvar.CreatedDateUtc,
                             UserPoints = bdvar.UserPoints,
                             uncompletedTasks = bdvar.uncompletedTasks,
                             Role = roles.ElementAt(_index),
                             _index = _index++
                         }).ToList();
            ViewBag.PageActive = "users";
            if (UsersList.Count()==0)
            {
                ViewBag.UsersCountMessage = "Пользователи, попадающие под критерии поиска не найдены.";
            }
            MenuShowElementsCount();
            return View("Index", UsersList);
            //return RedirectToAction("Index", new {searchResult = DBUsersList, searchCall =true });
        }
        //Удаление пользователя
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            PchelaMapUser user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var webRoot = _env.WebRootPath;
                string FullDirpath = "";
                List<PchelaMapTask> _tasks = _context.Tasks.Where(x => x.UserId == id).ToList();
                List<PchelaMapUserTasks> _userstasks = _context.UsersTasks.ToList();
                List<PchelaMapUserTasks> _takenTasks = _context.UsersTasks.Where(x => x.UserID == id).ToList();
                List<UsersRefusedTasks> _refusedTasks = _context.UsersRefusedFromTasks.Where(x => x.UserID == id).ToList();
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
                IdentityResult result = await _userManager.DeleteAsync(user);

            }
            return RedirectToAction("Index");
        }
        //групповое удаление пользователей
        public async Task<IActionResult> DeleteUsers (string SelectedIds)
        {
            List<string> idsConverted = JsonSerializer.Deserialize<List<string>>(SelectedIds);
            foreach (string id in idsConverted)
            {
                PchelaMapUser user = await _userManager.FindByIdAsync(id);
                if (user != null)
                {
                    var webRoot = _env.WebRootPath;
                    string FullDirpath = "";
                    List<PchelaMapTask> _tasks = _context.Tasks.Where(x => x.UserId == id).ToList();
                    List<PchelaMapUserTasks> _userstasks = _context.UsersTasks.ToList();
                    List<PchelaMapUserTasks> _takenTasks = _context.UsersTasks.Where(x => x.UserID == id).ToList();
                    List<UsersRefusedTasks> _refusedTasks = _context.UsersRefusedFromTasks.Where(x => x.UserID == id).ToList();
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
                    IdentityResult result = await _userManager.DeleteAsync(user);

                }
            }
            return RedirectToAction("Index");
        }
        //Смена роли пользователя
        public async Task<IActionResult> EditRole(string id, string role)
        {
            PchelaMapUser user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var UserRole = await _userManager.GetRolesAsync(user);
                if (role != null)
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
                await _userManager.RemoveFromRolesAsync(user, UserRole);
            }
            return RedirectToAction("Index");
        }
        //групповая смена роли
        public async Task<IActionResult> EditRoles(string SelectedIds, string role)
        {
            List<string> idsConverted = JsonSerializer.Deserialize<List<string>>(SelectedIds);
            foreach (string id in idsConverted)
            {
                PchelaMapUser user = await _userManager.FindByIdAsync(id);
                if (user != null)
                {
                    var UserRole = await _userManager.GetRolesAsync(user);
                    if (role != null)
                    {
                        await _userManager.AddToRoleAsync(user, role);
                    }
                    if (!UserRole.Contains(role))
                    {
                        await _userManager.RemoveFromRolesAsync(user, UserRole);
                    }
                   
                }
            }
            return RedirectToAction("Index");
        }
        //Групповая смена аватарки
        public async Task<IActionResult> ChangeUsersAva(string SelectedIds)
        {
            var webRoot = _env.WebRootPath;
            List<string> idsConverted = JsonSerializer.Deserialize<List<string>>(SelectedIds);
            foreach (string id in idsConverted)
            {
                PchelaMapUser user = await _userManager.FindByIdAsync(id);

                string Filepath = "Images/UsersPhoto/" + id;
                string path = System.IO.Path.Combine(webRoot, Filepath);
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
                user.UserPhoto = "/Images/EmptyUserRound.png";
                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction("Index");
        }
        //Страница заданий КОНКРЕТНОГО ПОЛЬЗОВАТЕЛЯ
        public async Task<IActionResult> SingleUserTasksView(string id)
        {
            PchelaMapUser user = await _context.Users.Include(c => c.Tasks).SingleAsync(x => x.Id == id);
          
            int index = 0;
            List<UserWithTasks> TasksList = new List<UserWithTasks>();
            TasksList = (from PchelaMapTask bdvar in user.Tasks.OrderByDescending(s => DateTime.ParseExact(s.CreatedDateUtc, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture))
                         select new UserWithTasks
                         {
                             id = bdvar.id,
                             _index = index++,
                             CreatorID = bdvar.UserId,
                             Email = _userManager.Users.FirstOrDefault(x => x.Id == bdvar.UserId).Email,
                             CoordinateX = bdvar.CoordinateX,
                             CoordinateY = bdvar.CoordinateY,
                             description = bdvar.Description,
                             Name = bdvar.Name,
                             Phone = bdvar.Phone,
                             PhotoUrl = bdvar.Photo,
                             Adress = bdvar.Adress,
                             NeedsCar = bdvar.NeedsCar,
                             GlobalStatus = bdvar.Status,
                             UserTakenCount = _context.UsersTasks.Where(x => x.TaskID == bdvar.id).Count(),
                             UserDoneCount = _context.UsersTasks.Where(x => x.TaskID == bdvar.id && x.Status == "done").Count(),
                             _CreatedDateUtc = bdvar.CreatedDateUtc
                         }).ToList();
            ViewBag.SelectedTasksOrAll = true;
            ViewBag.OwnTasksOrTaken = true;
            ViewBag.AllGlobalStatuses = GlobalStatusEditModel.GlobalTaskStatusDictionary.Keys.ToList();
            MenuShowElementsCount();
            ViewBag.PageActive = "tasks";
            return View("Tasks", TasksList);
        }
        //Страница ВЗЯТЫХ заданий КОНКРЕТНОГО ПОЛЬЗОВАТЕЛЯ
        public IActionResult SingleUserTakenTasksView(string id)
        {
            List<PchelaMapUserTasks> Tasks = _context.UsersTasks.ToList();
            List<PchelaMapUserTasks> TasksInLinkTable = _context.UsersTasks.Where(x => x.UserID == id).ToList();
            List<string> IdsList = TasksInLinkTable.Select(i => i.TaskID).ToList();
            List<PchelaMapTask> _tasksList = _context.Tasks.Where(x => IdsList.Contains(x.id)).ToList();
            List<UserWithTasks> TasksList = new List<UserWithTasks>();
            int index = 0;
            TasksList = (from PchelaMapTask bdvar in _tasksList.OrderByDescending(s => DateTime.ParseExact(s.CreatedDateUtc, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture))
                         join x in Tasks on bdvar.id equals x.TaskID
                         select new UserWithTasks
                         {
                             id = bdvar.id,
                             _index = index++,
                             CreatorID = bdvar.UserId,
                             UserTakenID = x.UserID,
                             Email = _userManager.Users.FirstOrDefault(x => x.Id == bdvar.UserId).Email,
                             CoordinateX = bdvar.CoordinateX,
                             CoordinateY = bdvar.CoordinateY,
                             description = bdvar.Description,
                             Name = bdvar.Name,
                             Phone = bdvar.Phone,
                             PhotoUrl = bdvar.Photo,
                             Adress = bdvar.Adress,
                             NeedsCar = bdvar.NeedsCar,
                             GlobalStatus = bdvar.Status,
                             UserTakenCount = Tasks.Where(x => x.TaskID == bdvar.id).Count(),
                             UserDoneCount = Tasks.Where(x => x.TaskID == bdvar.id && x.Status == "done").Count(),
                             _CreatedDateUtc = bdvar.CreatedDateUtc
                         }).OrderByDescending(s => DateTime.ParseExact(s._CreatedDateUtc, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture)).ToList();
            ViewBag.AllGlobalStatuses = GlobalStatusEditModel.GlobalTaskStatusDictionary.Keys.ToList();
            ViewBag.SelectedTasksOrAll = true;
            ViewBag.OwnTasksOrTaken = false;
            MenuShowElementsCount();
            ViewBag.PageActive = "tasks";
            return View("Tasks", TasksList);
        }

        //Страница ВСЕХ заданий
        public IActionResult Tasks(SortSearchPagin.TaskSortState sortOrder, string active = "all")
        {
            List<PchelaMapUserTasks> Tasks = _context.UsersTasks.ToList();
            List<PchelaMapTask> DBUtasksList = new List<PchelaMapTask>();
            if (active == "all")
            {
                DBUtasksList = _context.Tasks.ToList();
                if (DBUtasksList.Count() == 0)
                {
                    ViewBag.TasksCountMessage = "В базе данных нет заданий(((";
                }
            }
            if (active == "active")
            {
                DBUtasksList = _context.Tasks.Where(x => x.Status == "active").ToList();
                if (DBUtasksList.Count() == 0)
                {
                    ViewBag.TasksCountMessage = "В базе данных нет активных заданий";
                }
            }
            if (active == "moderation")
            {
                DBUtasksList = _context.Tasks.Where(x => x.Status == "moderating" || x.Status== "stoped").ToList();
                if (DBUtasksList.Count() == 0)
                {
                    ViewBag.TasksCountMessage = "В базе данных нет заданий на модерации";
                }
            }
            if (active == "closed")
            {
                List<string> doneTasksIds = new List<string>();
                doneTasksIds = Tasks.Where(x=>x.Status=="done").Select(x => x.TaskID).ToList();
                DBUtasksList = _context.Tasks.Where(x => x.Status == "closed" && !doneTasksIds.Contains(x.id)).ToList();
                if (DBUtasksList.Count() == 0)
                {
                    ViewBag.TasksCountMessage = "В базе данных нет закрытых заданий";
                }
            }
            if(active == "NY")
            {
                List<string> doneTasksIds = new List<string>();
                DBUtasksList = _context.Tasks.Where(x => x.NY_task == 1).ToList();
                if (DBUtasksList.Count() == 0)
                {
                    ViewBag.TasksCountMessage = "В базе данных нет новогодних заданий";
                }
            }
            int index = DBUtasksList.Count();
            List<UserWithTasks> TasksList = new List<UserWithTasks>();
            TasksList = (from PchelaMapTask bdvar in DBUtasksList
                         select new UserWithTasks
                         {
                             id = bdvar.id,
                             CreatorID = bdvar.UserId == null ? "" : bdvar.UserId,
                             UserTakenID = Tasks.FirstOrDefault(x => x.TaskID == bdvar.id) == default ? "" : Tasks.FirstOrDefault(x => x.TaskID == bdvar.id).UserID,
                             Email = bdvar.UserMail,
                             CoordinateX = bdvar.CoordinateX,
                             CoordinateY = bdvar.CoordinateY,
                             description = bdvar.Description,
                             Name = bdvar.Name,
                             Phone = bdvar.Phone,
                             PhotoUrl = bdvar.Photo,
                             Adress = bdvar.Adress,
                             NeedsCar = bdvar.NeedsCar,
                             GlobalStatus = bdvar.Status,
                             UrgentStatus = bdvar.Urgentable,
                             UserTakenCount = Tasks.Where(x => x.TaskID == bdvar.id).Count(),
                             UserDoneCount = Tasks.Where(x => x.TaskID == bdvar.id && x.Status == "done").Count(),
                             _CreatedDateUtc = bdvar.CreatedDateUtc,
                             _index = index--
                         }).ToList();
            TasksList.Reverse();
            TasksList = TasksSorting(sortOrder, TasksList);
            ViewBag.AllTasksOrSelected = false;
            ViewBag.AllGlobalStatuses = GlobalStatusEditModel.GlobalTaskStatusDictionary.Keys.ToList();
            MenuShowElementsCount();
            ViewBag.PageActive = "tasks";
            return View(TasksList);
        }
        //сорировка заданий
        private List<UserWithTasks> TasksSorting(SortSearchPagin.TaskSortState sortOrder, List<UserWithTasks> UsersList)
        {
            //if (_httpContextAccessor.HttpContext.Request.Cookies.ContainsKey("UsersSortOrder"))
            //{
            //    sortOrder = Enum.Parse<SortSearchPagin.SortState>(_httpContextAccessor.HttpContext.Request.Cookies["UsersSortOrder"]);
            //}
            ViewData["IndexSort"] = (sortOrder == SortSearchPagin.TaskSortState.IndexDesc ?
               SortSearchPagin.TaskSortState.IndexAsc : SortSearchPagin.TaskSortState.IndexDesc);
            ViewData["DateSort"] = (sortOrder == SortSearchPagin.TaskSortState.DateAsc ?
                  SortSearchPagin.TaskSortState.DateDesc : SortSearchPagin.TaskSortState.DateAsc);
            ViewData["NameSort"] = (sortOrder == SortSearchPagin.TaskSortState.NameAsc ?
                SortSearchPagin.TaskSortState.NameDesc : SortSearchPagin.TaskSortState.NameAsc);
            ViewData["HasAutoSort"] = (sortOrder == SortSearchPagin.TaskSortState.HasAutoAsc ?
               SortSearchPagin.TaskSortState.HasAutoDesc : SortSearchPagin.TaskSortState.HasAutoAsc);
            ViewData["UrgentableSort"] = (sortOrder == SortSearchPagin.TaskSortState.UrgentableDesc ?
            SortSearchPagin.TaskSortState.UrgentableAsc : SortSearchPagin.TaskSortState.UrgentableDesc);
            ViewData["UsersTakenSort"] = (sortOrder == SortSearchPagin.TaskSortState.UsersTakenDesc ?
             SortSearchPagin.TaskSortState.UsersTakenAsc : SortSearchPagin.TaskSortState.UsersTakenDesc);
            ViewData["UsersDoneSort"] = (sortOrder == SortSearchPagin.TaskSortState.UsersDoneDesc ?
             SortSearchPagin.TaskSortState.UsersDoneAsc : SortSearchPagin.TaskSortState.UsersDoneDesc);
            UsersList = sortOrder switch
            {
                SortSearchPagin.TaskSortState.IndexAsc => UsersList.OrderBy(s => s._index).ToList(),
                SortSearchPagin.TaskSortState.IndexDesc => UsersList.OrderByDescending(s => s._index).ToList(),
                SortSearchPagin.TaskSortState.DateAsc => UsersList.OrderBy(s => DateTime.ParseExact(s._CreatedDateUtc, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture)).ToList(),
                SortSearchPagin.TaskSortState.DateDesc => UsersList.OrderByDescending(s => DateTime.ParseExact(s._CreatedDateUtc, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture)).ToList(),
                SortSearchPagin.TaskSortState.NameAsc => UsersList.OrderBy(s => s.Name).ToList(),
                SortSearchPagin.TaskSortState.NameDesc => UsersList.OrderByDescending(s => s.Name).ToList(),
                SortSearchPagin.TaskSortState.HasAutoAsc => UsersList.OrderBy(s => s.NeedsCar).ToList(),
                SortSearchPagin.TaskSortState.HasAutoDesc => UsersList.OrderByDescending(s => s.NeedsCar).ToList(),
                SortSearchPagin.TaskSortState.UrgentableAsc => UsersList.OrderBy(s => s.UrgentStatus).ToList(),
                SortSearchPagin.TaskSortState.UrgentableDesc => UsersList.OrderByDescending(s => s.UrgentStatus).ToList(),
                SortSearchPagin.TaskSortState.UsersTakenAsc => UsersList.OrderBy(s => s.UserTakenCount).ToList(),
                SortSearchPagin.TaskSortState.UsersTakenDesc => UsersList.OrderByDescending(s => s.UserTakenCount).ToList(),
                SortSearchPagin.TaskSortState.UsersDoneAsc => UsersList.OrderBy(s => s.UserDoneCount).ToList(),
                SortSearchPagin.TaskSortState.UsersDoneDesc => UsersList.OrderByDescending(s => s.UserDoneCount).ToList(),
                _ => UsersList,
            };
            // CookieOptions option = new CookieOptions();
            // option.Expires = DateTime.Now.AddMinutes(30);
            // string coockieVal = sortOrder.ToString();
            //_httpContextAccessor.HttpContext.Response.Cookies.Append("UsersSortOrder", coockieVal, option);

            return UsersList;
        }
        public PartialViewResult _TasksSearch()
        {
            TasksSearchModel inputmodel = new TasksSearchModel();
            return PartialView(inputmodel);
        }
        [HttpPost]
        public IActionResult _TasksSearch(TasksSearchModel _searchmodel)
        {
            List<PchelaMapTask> DBUtasksList = new List<PchelaMapTask>();
            DBUtasksList = _context.Tasks.ToList();
            if (_searchmodel.description != "" && _searchmodel.description != null)
            {
                DBUtasksList = DBUtasksList.Where(x => x.Description?.IndexOf(_searchmodel.description, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }
            if (_searchmodel.name != "" && _searchmodel.name != null)
            {
                DBUtasksList = DBUtasksList.Where(x => x.Name?.IndexOf(_searchmodel.name, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }
            if (_searchmodel.mail != "" && _searchmodel.mail != null)
            {
                DBUtasksList = DBUtasksList.Where(x => x.UserMail?.IndexOf(_searchmodel.mail, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }
            if (_searchmodel.phone != "" && _searchmodel.phone != null)
            {
                DBUtasksList = DBUtasksList.Where(x => x.Phone?.IndexOf(_searchmodel.phone, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }
            if (_searchmodel.adress != "" && _searchmodel.adress != null)
            {
                DBUtasksList = DBUtasksList.Where(x => x.Adress?.IndexOf(_searchmodel.adress, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }
            List<PchelaMapUserTasks> Tasks = _context.UsersTasks.ToList();
            int index = DBUtasksList.Count();
            List<UserWithTasks> TasksList = new List<UserWithTasks>();
            TasksList = (from PchelaMapTask bdvar in DBUtasksList
                         select new UserWithTasks
                         {
                             id = bdvar.id,
                             CreatorID = bdvar.UserId == null ? "" : bdvar.UserId,
                             UserTakenID = Tasks.FirstOrDefault(x => x.TaskID == bdvar.id) == default ? "" : Tasks.FirstOrDefault(x => x.TaskID == bdvar.id).UserID,
                             Email = bdvar.UserMail,
                             CoordinateX = bdvar.CoordinateX,
                             CoordinateY = bdvar.CoordinateY,
                             description = bdvar.Description,
                             Name = bdvar.Name,
                             Phone = bdvar.Phone,
                             PhotoUrl = bdvar.Photo,
                             Adress = bdvar.Adress,
                             NeedsCar = bdvar.NeedsCar,
                             GlobalStatus = bdvar.Status,
                             UrgentStatus = bdvar.Urgentable,
                             UserTakenCount = Tasks.Where(x => x.TaskID == bdvar.id).Count(),
                             UserDoneCount = Tasks.Where(x => x.TaskID == bdvar.id && x.Status == "done").Count(),
                             _CreatedDateUtc = bdvar.CreatedDateUtc,
                             _index = index--
                         }).ToList();
            TasksList.Reverse();
            ViewBag.AllTasksOrSelected = false;
            ViewBag.AllGlobalStatuses = GlobalStatusEditModel.GlobalTaskStatusDictionary.Keys.ToList();
            ViewBag.PageActive = "tasks";
            if (TasksList.Count() == 0)
            {
                ViewBag.UsersCountMessage = "Задания, попадающие под критерии поиска не найдены.";
            }
            MenuShowElementsCount();
            return View("Tasks", TasksList);
        }
        //Редактирование задания
        public IActionResult EditTask(string id, string userId, bool SelectedTasksOrAll, bool OwnTasksOrTaken, string UserTakenId)
        {
            var task = _context.Tasks.FirstOrDefault(x => x.id == id);

            bool Car = task.NeedsCar == 1 ? true : false;
            CreateTaskModel CreatingTaskInfo = new CreateTaskModel
            {
                UserAdress = task.Adress,
                UserCoordX = task.CoordinateX,
                UserCoordY = task.CoordinateY,
                Urgentability = task.Urgentable == 1 ? true : false,
                NYTaskBool = task.NY_task == 1 ? true:false,
                NeedsCar = Car,
                TaskDescription = task.Description,
                Status = task.Status,
                userId = userId,
                UserName = task.Name,
                UserPhone = task.Phone,
                UserMail = task.UserMail,
                UserTakenId = UserTakenId,
                SelectedTasksOrAll = SelectedTasksOrAll,
                OwnTasksOrTaken = OwnTasksOrTaken
            };

            return View("EditHelpPoint", CreatingTaskInfo);
        }
        //Редактирование задания 
        [HttpPost]
        public async Task<IActionResult> EditTask(CreateTaskModel TaskAddingResult)
        {
            var webRoot = _env.WebRootPath;
            var user = await _userManager.GetUserAsync(User);
            string TaskCoordinateX = "";
            string TaskCoordinateY = "";
            string TaskAdress = "";
            string TaskDescription = "";
            int TaskCarNeed = 0;
            //если UserCoordinates == null - то ничего не менялось по адресу
            if (TaskAddingResult.UserCoordX != user.UserCoordinateX || TaskAddingResult.UserCoordY != user.UserCoordinateY)
            {
                TaskCoordinateX = TaskAddingResult.UserCoordX;
                TaskCoordinateY = TaskAddingResult.UserCoordY;
                TaskAdress = TaskAddingResult.UserAdress;
            }
            else
            {
                TaskCoordinateX = user.UserCoordinateX;
                TaskCoordinateY = user.UserCoordinateY;
                TaskAdress = user.UserAdress;
            }
            TaskDescription = TaskAddingResult.TaskDescription;
            TaskCarNeed = TaskAddingResult.NeedsCar ? 1 : 0;
            PchelaMapTask _task = _context.Tasks.FirstOrDefault(x => x.id == TaskAddingResult.id);
            _task.Description = TaskDescription;
            _task.CoordinateX = TaskCoordinateX;
            _task.CoordinateY = TaskCoordinateY;
            _task.Adress = TaskAdress;
            _task.Name = TaskAddingResult.UserName;
            _task.Phone = TaskAddingResult.UserPhone;
            _task.UserMail = TaskAddingResult.UserMail;
            _task.NeedsCar = TaskCarNeed;
            _task.Urgentable = TaskAddingResult.Urgentability ? 1 : 0;
            _task.NY_task = TaskAddingResult.NYTaskBool ? 1 : 0;
            _context.SaveChanges();
            if (TaskAddingResult.SelectedTasksOrAll)
            {
                if (TaskAddingResult.OwnTasksOrTaken)
                {
                    return RedirectToAction("SingleUserTasksView", new { id = TaskAddingResult.userId });
                }
                else
                {
                    return RedirectToAction("SingleUserTakenTasksView", new { id = TaskAddingResult.UserTakenId });
                }
            }
            else
            {
                return RedirectToAction("Tasks");
            }

        }
        //Страница редактирования глобального статуса задания
        public async Task<IActionResult> EditGlobalStatus(string id, string status, string MessageForUser,  string userId, string UserTakenId, bool SelectedTasksOrAll, bool OwnTasksOrTaken)
        {
            PchelaMapTask TaskForModeration = _context.Tasks.FirstOrDefault(x => x.id == id);
            if (TaskForModeration != null)
            {
                if (MessageForUser == "undefined")
                {
                    MessageForUser = "";
                }
                TaskForModeration.AdminComment = MessageForUser;
                TaskForModeration.Status = status;
                _context.SaveChanges();
                string mailHeader = "";
                string mailMessage = "";
                if (status == "active")
                {
                    mailHeader = EmailService._onTaskModerationActive["header"];
                    mailMessage = EmailService._onTaskModerationActive["bodyPrt1"] +
               TaskForModeration.Description.Substring(0, Math.Min(TaskForModeration.Description.Length, 50)) + "..." +
               EmailService._onTaskModerationActive["bodyPrt2"];
                }
                if (status == "stoped")
                {
                    mailHeader = EmailService._onTaskModerationStop["header"];
                    if (MessageForUser == "" || MessageForUser == null)
                    {
                        MessageForUser = "не указана";
                    }
                    mailMessage = EmailService._onTaskModerationStop["bodyPrt1"] +
               TaskForModeration.Description.Substring(0, Math.Min(TaskForModeration.Description.Length, 50)) + "..." +
               EmailService._onTaskModerationStop["bodyPrt2"] + MessageForUser;
                }

                EmailService emailSender = new EmailService();
                var userMail = TaskForModeration.UserMail;
                await emailSender.SendAsync(userMail, mailHeader, mailMessage);
            }
            if (SelectedTasksOrAll)
            {
                if (OwnTasksOrTaken)
                {
                    return RedirectToAction("SingleUserTasksView", new { id = userId });
                }
                else
                {
                    return RedirectToAction("SingleUserTakenTasksView", new { id = UserTakenId });
                }
            }
            else
            {
                return RedirectToAction("Tasks");
            }
            
        }

        //удаление задания
        [HttpPost]
        public async Task<IActionResult> DeleteTask(string id, string userId,  bool SelectedTasksOrAll, bool OwnTasksOrTaken, string UserTakenId)
        {
            PchelaMapTask _task = await _context.Tasks.FindAsync(id);
            if (_task != null)
            {
                var webRoot = _env.WebRootPath;
                string TaskReportFolder = System.IO.Path.Combine(webRoot, _task.ResultMediaFolder);
                if (System.IO.Directory.Exists(TaskReportFolder))
                {
                    System.IO.Directory.Delete(TaskReportFolder, true);
                }
                _context.Tasks.Remove(_task);
                _context.SaveChanges();
            }
            if (SelectedTasksOrAll)
            {
                if (OwnTasksOrTaken)
                {
                    return RedirectToAction("SingleUserTasksView", new { id = userId });
                }
                else
                {
                    return RedirectToAction("SingleUserTakenTasksView", new { id = userId });
                }
            }
            else
            {
                return RedirectToAction("Tasks");
            }
        }

        //Страница заданий на МОДЕРАЦИЮ
        public IActionResult TasksForModeration()
        {
            List<PchelaMapTask> DBUtasksList = _context.Tasks.Where(x => x.Status == "moderating").ToList();
            List<UserWithTasks> TasksList = new List<UserWithTasks>();
            TasksList = (from PchelaMapTask bdvar in DBUtasksList
                         select new UserWithTasks
                         {
                             id = bdvar.id,
                             CoordinateX = bdvar.CoordinateX,
                             CoordinateY = bdvar.CoordinateY,
                             description = bdvar.Description,
                             Name = bdvar.Name,
                             Email = _userManager.Users.FirstOrDefault(x => x.Id == bdvar.UserId).Email,
                             Phone = bdvar.Phone,
                             PhotoUrl = bdvar.Photo,
                             Adress = bdvar.Adress,
                             GlobalStatus = bdvar.Status,
                             NeedsCar = bdvar.NeedsCar,
                             UrgentStatus = bdvar.Urgentable,
                             CreatorID = bdvar.UserId,
                             FromAdminMessage = bdvar.AdminComment,
                             _CreatedDateUtc = bdvar.CreatedDateUtc

                         }).ToList();
            ViewBag.AllGlobalStatuses = GlobalStatusEditModel.GlobalTaskStatusDictionary.Keys.ToList();
            MenuShowElementsCount();
            return View(TasksList);
        }

        //Страница редактирования статуса модерации задания
        public async Task<IActionResult> ChangeModerateGlobalStatus(string id, string status, string MessageForUser)
        {
            PchelaMapTask TaskForModeration = _context.Tasks.FirstOrDefault(x => x.id == id);
            if (TaskForModeration != null)
            {
                if (status == "active")
                {
                    TaskForModeration.AdminComment = "";
                }
                else
                {
                    if (MessageForUser == "undefined")
                    {
                        MessageForUser = "";
                    }
                    TaskForModeration.AdminComment = MessageForUser;
                }
                TaskForModeration.Status = status;
                _context.SaveChanges();
                string mailHeader = "";
                string mailMessage = "";
                if (status == "active")
                {
                    mailHeader = EmailService._onTaskModerationActive["header"];
                    mailMessage = EmailService._onTaskModerationActive["bodyPrt1"] +
              TaskForModeration.Description.Substring(0, Math.Min(TaskForModeration.Description.Length, 50)) + "..." +
              EmailService._onTaskModerationActive["bodyPrt2"];
                }
                if (status == "stoped")
                {
                    mailHeader = EmailService._onTaskModerationStop["header"];
                    if (MessageForUser == "" || MessageForUser == null)
                    {
                        MessageForUser = "не указана";
                    }
                    mailMessage = EmailService._onTaskModerationStop["bodyPrt1"] +
                    TaskForModeration.Description.Substring(0, Math.Min(TaskForModeration.Description.Length, 50)) + "..." +
                    EmailService._onTaskModerationStop["bodyPrt2"] + MessageForUser;
                }

                EmailService emailSender = new EmailService();
                await emailSender.SendAsync(TaskForModeration.UserMail, mailHeader, mailMessage);
            }
            return RedirectToAction("TasksForModeration");

        }

        //Страница ОТЧЁТОВ на МОДЕРАЦИЮ
        public IActionResult ReportsForModeration(string active = "all")
        {
            List<PchelaMapUserTasks> Tasks = new List<PchelaMapUserTasks>();
            List<PchelaMapTask> DBUtasksList = _context.Tasks.Where(x => x.Status != "moderating").ToList();
            if (active == "all")
            {
                Tasks = _context.UsersTasks.Where(x => x.Status == "complite_moderation" || x.Status == "StopedOnModeration").ToList();
                if (Tasks.Count() == 0)
                {
                    ViewBag.ReportsCountMessage = "В базе данных нет отчётов на модерации";
                }
            }
            if (active=="moderation")
            {
                Tasks = _context.UsersTasks.Where(x => x.Status == "complite_moderation").ToList();
                if (Tasks.Count() == 0)
                {
                    ViewBag.ReportsCountMessage = "В базе данных нет отчётов на модерации";
                }
            }
            if (active == "stoped")
            {
                Tasks = _context.UsersTasks.Where(x => x.Status == "StopedOnModeration").ToList();
                if (Tasks.Count() == 0)
                {
                    ViewBag.ReportsCountMessage = "В базе данных нет отчётов, не прошедших модерацию";
                }
            }
          
            
            List<UserWithTasks> TasksList = (from PchelaMapTask bdvar in DBUtasksList
                                             join x in Tasks on bdvar.id equals x.TaskID
                                             select new UserWithTasks
                                             {
                                                 id = bdvar.id,
                                                 CoordinateX = bdvar.CoordinateX,
                                                 CoordinateY = bdvar.CoordinateY,
                                                 description = bdvar.Description,
                                                 Name = bdvar.Name,
                                                 PhotoUrl = bdvar.Photo,
                                                 UserTakenName = _userManager.FindByIdAsync(x.UserID).Result.Name,
                                                 UserTakenPhoto = _userManager.FindByIdAsync(x.UserID).Result.UserPhoto,
                                                 ResultMediaFolder = x.MediaFolder,
                                                 ResultFiles = ResultFiles(x.MediaFolder),
                                                 Adress = bdvar.Adress,
                                                 CreatorID = bdvar.UserId,
                                                 UserTakenID = x.UserID,
                                                 Status = x.Status,
                                                 UrgentStatus = bdvar.Urgentable,
                                                 FromAdminMessage = x.AdminComment,
                                                 FromUserMessage = x.UserComment,
                                                 _CreatedDateUtc = bdvar.CreatedDateUtc,
                                                 _DateTaken = x.DateTaken,
                                                 _DateDone = x.DateDone,
                                                 _TimeInProcess = timeInProcess(x.DateTaken, x.DateDone),
                                                 _isOverdue = IsTaskOverdue(x.DateTaken, x.DateDone, bdvar.Urgentable)
                                             }).ToList();
            ViewBag.AllStatuses = StatusEditModel.TaskStatusDictionary.Keys.ToList();
            MenuShowElementsCount();
            ViewBag.PageActive = "reports";
            return View(TasksList);
        }

        //Страница редактирования статуса выполнения задания (одобрение/неодобрение отчёта)
        public async Task<IActionResult> EditStatus(string id, string userId, string status, string MessageForUser, string MakeActiveAgain)
        {
            PchelaMapUser UserWithDoneReport = await _userManager.FindByIdAsync(userId);
            PchelaMapTask CurrTask = _context.Tasks.FirstOrDefault(x => x.id == id);
            PchelaMapUserTasks TaskUserConnection = _context.UsersTasks.FirstOrDefault(x => x.TaskID == id && x.UserID == UserWithDoneReport.Id);
            if (TaskUserConnection != null)
            {
                if (MessageForUser == "undefined")
                {
                    MessageForUser = "";
                }
                TaskUserConnection.AdminComment = MessageForUser;
                TaskUserConnection.Status = status;
                string mailHeader = "";
                string mailMessage = "";
                if (status == "done")
                {

                    if (CurrTask.Urgentable == 1)
                    {
                        UserWithDoneReport.UserPoints = UserWithDoneReport.UserPoints + 5;
                        mailHeader = EmailService._onTaskUrgentDone["header"];
                        mailMessage = EmailService._onTaskUrgentDone["bodyPrt1"] +
                          CurrTask.Description.Substring(0, Math.Min(CurrTask.Description.Length, 50)) + "..." +
                          EmailService._onTaskUrgentDone["bodyPrt2"];
                        UserWithDoneReport.SystemMessageForUser = mailMessage;
                    }
                    else
                    {
                        UserWithDoneReport.UserPoints = UserWithDoneReport.UserPoints + 3;
                        mailHeader = EmailService._onTaskDone["header"];
                        mailMessage = EmailService._onTaskDone["bodyPrt1"] +
                          CurrTask.Description.Substring(0, Math.Min(CurrTask.Description.Length, 50)) + "..." +
                          EmailService._onTaskDone["bodyPrt2"];
                        UserWithDoneReport.SystemMessageForUser = mailMessage;
                    }
                    CurrTask.ClosedDateUtc = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _TimeZoneinfo).ToString("dd.MM.yyyy HH:mm:ss");
                    CurrTask.Status = "closed";
                    ReportsForDownload report = new ReportsForDownload()
                    {
                        id = CurrTask.id,
                        CreatorID = CurrTask.UserId,
                        Name = CurrTask.Name,
                        description = CurrTask.Description,
                        Phone = CurrTask.Phone,
                        Email = CurrTask.UserMail,
                        Adress = CurrTask.Adress,
                        UrgentStatus = CurrTask.Urgentable,
                        FromUserMessage = TaskUserConnection.UserComment,
                        FromAdminMessage = TaskUserConnection.AdminComment,
                        ResultMediaFolder = TaskUserConnection.MediaFolder,
                        ResultFiles = ZipResultFiles(TaskUserConnection.MediaFolder),
                        UserTakenID = TaskUserConnection.UserID,
                        UserTakenName = _userManager.FindByIdAsync(TaskUserConnection.UserID).Result.Name,
                        _CreatedDateUtc = CurrTask.CreatedDateUtc,
                        _DateTaken = TaskUserConnection.DateTaken,
                        _DateDone = TaskUserConnection.DateDone,
                        _TimeInProcess = timeInProcess(TaskUserConnection.DateTaken, TaskUserConnection.DateDone)
                    };
                    ReportDescriptionGenerator(report);
                    report.ResultFiles = ZipResultFiles(TaskUserConnection.MediaFolder);
                    ReportAddToZip(report);
                    if(TaskUserConnection.Promo!=null)
                    {
                       PromoBd promoRow = _context.PromoCode.FirstOrDefault(x => x.code == TaskUserConnection.Promo);
                        if(promoRow!=null)
                        {
                            promoRow.status = 2;
                        }
                    }
                }
                if (status == "StopedOnModeration")
                {
                    
                    mailHeader = EmailService._onReportModerationStop["header"];
                    if (MessageForUser == "" || MessageForUser == null)
                    {
                        MessageForUser = "не указана";
                    }
                    mailMessage = EmailService._onReportModerationStop["bodyPrt1"] +
                         CurrTask.Description.Substring(0, Math.Min(CurrTask.Description.Length, 50)) + "..." +
                         EmailService._onReportModerationStop["bodyPrt2"] + MessageForUser;
                }
                if (MakeActiveAgain == "1")
                {
                    CurrTask.Status = "in_progress";
                }
                _context.SaveChanges();
                EmailService emailSender = new EmailService();
                var userMail = UserWithDoneReport.Email;
                await emailSender.SendAsync(userMail, mailHeader, mailMessage);
            }

            return RedirectToAction("ReportsForModeration");
        }

        //Показать ЗАДАНИЕ из отчёта со страницы отчетов о выполнении задания
        //Листом, потому что во View модель - лист
        public IActionResult ShowTaskFromTaskReport(string id)
        {
            List<PchelaMapTask> _task = _context.Tasks.Where(x => x.id == id).ToList();
            List<UserWithTasks> _TaskInfo = (
                        from PchelaMapTask bdvar in _task
                        select new UserWithTasks
                        {
                            id = bdvar.id,
                            CreatorID = bdvar.UserId,
                            Email = _userManager.Users.FirstOrDefault(x => x.Id == bdvar.UserId).Email,
                            CoordinateX = bdvar.CoordinateX,
                            CoordinateY = bdvar.CoordinateY,
                            description = bdvar.Description,
                            Name = bdvar.Name,
                            Phone = bdvar.Phone,
                            PhotoUrl = bdvar.Photo,
                            Adress = bdvar.Adress,
                            NeedsCar = bdvar.NeedsCar,
                            GlobalStatus = bdvar.Status,
                            UserTakenCount = _context.UsersTasks.Where(x => x.TaskID == bdvar.id).Count(),
                            UserDoneCount = _context.UsersTasks.Where(x => x.TaskID == bdvar.id && x.Status == "done").Count(),
                            _CreatedDateUtc = bdvar.CreatedDateUtc
                        }).ToList();
            ViewBag.AllGlobalStatuses = GlobalStatusEditModel.GlobalTaskStatusDictionary.Keys.ToList();
            MenuShowElementsCount();
            return View("Tasks", _TaskInfo);
        }

        //обработка списка ссылок на аватарки из папки
        private string[] ResultFiles(string path)
        {
            var webRoot = _env.WebRootPath;
            string[] Outfiles = new string[] { }; ;
            Outfiles = Directory.GetFiles(System.IO.Path.Combine(webRoot, path)).Where(x => !x.Contains("Info.txt")).ToArray();
            Outfiles = FolderShitCleaner(Outfiles);


            for (int t = 0; t < Outfiles.Length; t++)
            {
                    Outfiles[t] = path + Outfiles[t];
            }
            return Outfiles;
        }

        //Просмотр заданий на выполнении (таблица пользователи/задания)
        public IActionResult UsersTasks(string active)
        {
            List<PchelaMapUserTasks> Tasks = new List<PchelaMapUserTasks>();
            List<PchelaMapTask> DBUtasksList = new List<PchelaMapTask>();
            if (active == "in_progress")
            {
                Tasks = _context.UsersTasks.Where(x => x.Status == "active" || x.Status == "StopedOnModeration").ToList();
               DBUtasksList = _context.Tasks.Where(x => x.Status == "in_progress").ToList();
                if (Tasks.Count() == 0)
                {
                    ViewBag.TasksCountMessage = "В базе данных нет заданий в процессе выполнения";
                }
            }
            else
            {
                Tasks = _context.UsersTasks.Where(x => x.Status == "done").ToList();
                DBUtasksList = _context.Tasks.Where(x => x.Status == "closed").ToList();
                if (Tasks.Count() == 0)
                {
                    ViewBag.TasksCountMessage = "В базе данных нет выполненных заданий";
                }
            }
            int index = 0;
            List<UserWithTasks> TasksList = (from PchelaMapTask bdvar in DBUtasksList
                                             join x in Tasks on bdvar.id equals x.TaskID
                                             select new UserWithTasks
                                             {
                                                 id = bdvar.id,
                                                 _index= index++,
                                                 CoordinateX = bdvar.CoordinateX,
                                                 CoordinateY = bdvar.CoordinateY,
                                                 description = bdvar.Description,
                                                 Name = bdvar.Name,
                                                 PhotoUrl = bdvar.Photo,
                                                 UserTakenName = _userManager.FindByIdAsync(x.UserID).Result.Name,
                                                 UserTakenPhoto = _userManager.FindByIdAsync(x.UserID).Result.UserPhoto,
                                                 ResultMediaFolder = x.MediaFolder,
                                                 ResultFiles = ResultFiles(x.MediaFolder),
                                                 Adress = bdvar.Adress,
                                                 CreatorID = bdvar.UserId,
                                                 UserTakenID = x.UserID,
                                                 Status = x.Status,
                                                 UrgentStatus = bdvar.Urgentable,
                                                 GlobalStatus = bdvar.Status,
                                                 FromAdminMessage = x.AdminComment,
                                                 FromUserMessage = x.UserComment,
                                                 _CreatedDateUtc = bdvar.CreatedDateUtc,
                                                 _DateTaken = x.DateTaken,
                                                 _DateDone = x.DateDone,
                                                 _TimeInProcess = timeInProcess(x.DateTaken, x.DateDone),
                                                 _isOverdue = IsTaskOverdue(x.DateTaken, x.DateDone, bdvar.Urgentable)
                                             }).ToList();
            TasksList.Reverse();
            MenuShowElementsCount();
            ViewBag.PageActive = "tasks";
            return View(TasksList);
        }

        //Окно просмотра файлов отчёта
        public PartialViewResult _ReportFilesPartialView(string _reportFolder)
        {
            var webRoot = _env.WebRootPath;
            List<string> reportFiles = ResultFiles(_reportFolder).ToList();
            return PartialView(reportFiles);
        }
        //подсчёт времени выполнения задания
        private string timeInProcess(string DateTaken, string DateDone)
        {
            DateTime _ReadyDateDone;
            if (DateDone == "" || DateDone == null)
            {
                _ReadyDateDone = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _TimeZoneinfo);
            }
            else
            {
                    _ReadyDateDone = DateTime.ParseExact(DateDone, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            }
            DateTime DateTakenDate;
            DateTakenDate = DateTime.ParseExact(DateTaken, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);

            int dateRangeDays = (_ReadyDateDone - DateTakenDate).Days;
            int dateRangeHours = (_ReadyDateDone - DateTakenDate).Hours;
            int dateRangeMinutes = (_ReadyDateDone - DateTakenDate).Minutes;
            string dateRange = "дней: " + dateRangeDays.ToString() + Environment.NewLine +
                ", часов: " + dateRangeHours.ToString();
            return dateRange;
        }

        //проверка задания на просроченность
        private bool IsTaskOverdue(string DateTaken, string DateDone, int isUrgentable)
        {
            DateTime _ReadyDateDone;
            if (DateDone == "" || DateDone == null)
            {
                _ReadyDateDone = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _TimeZoneinfo);
            }
            else
            {
                _ReadyDateDone = DateTime.ParseExact(DateDone, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            }
            DateTime DateTakenDate;
            DateTakenDate = DateTime.ParseExact(DateTaken, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);

            int dateRangeDays = (_ReadyDateDone - DateTakenDate).Days;
            if (isUrgentable==1 && dateRangeDays >= 1)
            {
                    return true;
            }
            else
            {
                if (isUrgentable == 0 && dateRangeDays >= 3)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
           

        }

        //Отмена выполнения просроченного задания
        public async Task<IActionResult> UncompletedTaskCancel(string id, string userId, bool activeORclose)
        {
            PchelaMapUser UserWithDoneReport = await _userManager.FindByIdAsync(userId);
            PchelaMapTask CurrTask = _context.Tasks.FirstOrDefault(x => x.id == id);
            PchelaMapUserTasks TaskUserConnection = _context.UsersTasks.FirstOrDefault(x => x.TaskID == id && x.UserID == UserWithDoneReport.Id);
            if (TaskUserConnection != null)
            {
                var webRoot = _env.WebRootPath;
                string FullDirpath = System.IO.Path.Combine(webRoot, TaskUserConnection.MediaFolder);
                if (System.IO.Directory.Exists(FullDirpath))
                {
                    System.IO.Directory.Delete(FullDirpath, true);
                }
                if (activeORclose)
                {
                    CurrTask.Status = "active";
                }
                else
                {
                    CurrTask.Status = "closed";
                }
                UserWithDoneReport.uncompletedTasks++;
                await _userManager.UpdateAsync(UserWithDoneReport);
                _context.UsersTasks.Remove(TaskUserConnection);
                _context.SaveChanges();

                EmailService emailSender = new EmailService();
                string mailMessage = EmailService._onReportClose["bodyPrt1"] +
                         CurrTask.Description.Substring(0, Math.Min(CurrTask.Description.Length, 50)) + "..." +
                         EmailService._onReportClose["bodyPrt2"];
                await emailSender.SendAsync(UserWithDoneReport.Email, EmailService._onReportClose["header"], mailMessage);
            }
            return RedirectToAction("UsersTasks");
        }
        //Подсчет и передача в отображение в меню админки количества элементов в разделах 
        private void MenuShowElementsCount()
        {
           List<PchelaMapUser> AllUsers = _context.Users.ToList();
            List<PchelaMapTask> AllTasks = _context.Tasks.ToList();
            List<PchelaMapUserTasks> AllUsersTasks = _context.UsersTasks.ToList();
            List<IdentityUserRole<string>> UsersRoles = _context.UserRoles.ToList();
            List<string> UsersIds = new List<string>();
            ViewBag.UsersCount = AllUsers.Count();
            UsersIds = UsersRoles.Where(r => r.RoleId == "5888d3c0-eec7-4e2b-8481-e813089a3c16").Select(b => b.UserId).ToList();
            ViewBag.UsersUserCount = AllUsers.Where(x => UsersIds.Contains(x.Id)).Count();
            UsersIds = UsersRoles.Where(r => r.RoleId == "d258c4e4-a974-466b-9189-83fc350a96c8").Select(b => b.UserId).ToList();
            ViewBag.UsersAdminCount = AllUsers.Where(x => UsersIds.Contains(x.Id)).Count();
            UsersIds = UsersRoles.Where(r => r.RoleId == "7d8a85a4-6a8a-4d26-9094-23148c2c2abe").Select(b => b.UserId).ToList();
            ViewBag.UsersModerCount = _context.Users.Where(x => UsersIds.Contains(x.Id)).Count();
            UsersIds = UsersRoles.Where(r => r.RoleId == "24911f26-3b21-47ae-be22-70666990aa05").Select(b => b.UserId).ToList();
            ViewBag.UsersBanCount = _context.Users.Where(x => UsersIds.Contains(x.Id)).Count();
            ViewBag.TasksCount = AllTasks.Count();
            ViewBag.TasksNYCount = AllTasks.Where(x => x.NY_task == 1).Count();
            ViewBag.TasksOnModerationCount = AllTasks.Where(x => x.Status == "moderating" || x.Status == "stoped").Count();
            ViewBag.ReportsTotalCount = AllUsersTasks.Where(x => x.Status == "complite_moderation" || x.Status == "StopedOnModeration").Count();
            ViewBag.ReportsOnModerationCount = AllUsersTasks.Where(x => x.Status == "complite_moderation").Count();
            ViewBag.ReportsRefusedCount = AllUsersTasks.Where(x => x.Status == "StopedOnModeration").Count();
            ViewBag.TasksActiveCount = AllTasks.Where(x => x.Status == "active").Count();
            List<PchelaMapUserTasks> activeTasks = AllUsersTasks.Where(x => x.Status == "active").ToList();
            List<PchelaMapTask> in_progressTasks = AllTasks.Where(x => x.Status == "in_progress").ToList();
            ViewBag.TasksInProcessCount = (from PchelaMapTask bdvar in in_progressTasks
                                           join x in activeTasks on bdvar.id equals x.TaskID
                                           select new UserWithTasks
                                           {
                                               id = bdvar.id
                                           }).Count();
          
            List<PchelaMapUserTasks> doneTasks = AllUsersTasks.Where(x => x.Status == "done").ToList();
            List<string> doneTasksIds = new List<string>();
            doneTasksIds = doneTasks.Select(x => x.TaskID).ToList();
            ViewBag.TasksDoneCount = doneTasks.Count();
            ViewBag.TasksStopedCount  = AllTasks.Where(x =>  x.Status == "closed" && !doneTasksIds.Contains(x.id)).Count();

            List<PromoBd> promoBds = _context.PromoCode.Where(x => x.status == 0).ToList();
            ViewBag.OpenPromosCount = promoBds.Count();
        
        }

        //Страница выгрузки БД
        public IActionResult ExportData()
        {
            AdminModal _modal = _modalData;
            MenuShowElementsCount();
            ViewBag.PageActive = "export";
            var webRoot = _env.WebRootPath;
            string path = System.IO.Path.Combine(webRoot, "Images/TasksReports/");
            long _size = 0;
            var dirInfo = new DirectoryInfo(path);
            foreach (FileInfo fi in dirInfo.GetFiles("*", SearchOption.AllDirectories))
            {
                _size = _size + fi.Length;
            }
            decimal decSize = (decimal)_size;
            for (int i = 0; i <= 1; i++)
            {
                decSize = decSize / 1024 ;
            }
            ViewBag.ReportsFilesVolume = Math.Round(decSize, 0);

            List<PchelaMapUserTasks> Tasks = _context.UsersTasks.Where(x => x.Status == "done").ToList();
            List<PchelaMapTask> DBUtasksList = _context.Tasks.Where(x => x.Status == "closed").ToList();
            List<ReportsForDownload> TasksList = (from PchelaMapTask bdvar in DBUtasksList
                                                  join x in Tasks on bdvar.id equals x.TaskID
                                                  select new ReportsForDownload
                                                  {
                                                      ResultMediaFolder = x.MediaFolder,
                                                       ResultFiles = ZipResultFiles(x.MediaFolder)
                                                  }).ToList();
            decimal DoneTasksdecSizeTotal = 0;
            foreach (ReportsForDownload reports in TasksList)
            {
                long DoneTasks_size = 0;
                string DoneTask_path = System.IO.Path.Combine(webRoot, reports.ResultMediaFolder);
                var DoneTasksdirInfo = new DirectoryInfo(DoneTask_path);
                foreach (FileInfo fi in DoneTasksdirInfo.GetFiles("*", SearchOption.AllDirectories))
                {
                    DoneTasks_size = DoneTasks_size + fi.Length;
                }
                decimal DoneTasksdecSize = (decimal)DoneTasks_size;
                for (int i = 0; i <= 1; i++)
                {
                    DoneTasksdecSize = DoneTasksdecSize / 1024;
                }
                DoneTasksdecSizeTotal = DoneTasksdecSizeTotal + DoneTasksdecSize;
            }
            ViewBag.DoneTasksReportFilesVolume = Math.Round(DoneTasksdecSizeTotal, 0);

            var contentRoot = _env.ContentRootPath;
            List<string> _mailLogFiles = new List<string>();
            DirectoryInfo _MaillogDirInfo = new DirectoryInfo(System.IO.Path.Combine(contentRoot, "mailLogs"));
            long MailLogs_size = 0;
            foreach (FileInfo _logFile in _MaillogDirInfo.GetFiles())
            {
                _mailLogFiles.Add(_logFile.FullName);
                MailLogs_size = MailLogs_size + _logFile.Length;
            }
            decimal MailLogsdecSize = (decimal)MailLogs_size;
            for (int i = 0; i <= 1; i++)
            {
                MailLogsdecSize = MailLogsdecSize / 1024;
            }
            ViewBag.MailLogsFiles = _mailLogFiles;
            ViewBag.MailLogsFilesVolume = Math.Round(MailLogsdecSize, 0);

            List<string> _ReportZipFiles = new List<string>();
            DirectoryInfo _ReportsZipDirInfo = new DirectoryInfo(System.IO.Path.Combine(webRoot, "Images/ReportsZip"));
            foreach (FileInfo _repZipFile in _ReportsZipDirInfo.GetFiles().Where(x=>x.Extension!=".txt"))
            {
                _ReportZipFiles.Add(_repZipFile.FullName);
            }
            ViewBag.ZipReportsFiles = _ReportZipFiles;
            return View(_modal);
        }

        //отправка отчёта на почту или загрузка на комп
        [HttpPost]
        public async Task<IActionResult> GenerateUsersTable(bool DownloadOrMail, string UsersTimeRangeStart, string UsersTimeRangeEnd)
        {
            List<PchelaMapUser> _users = _context.Users.Include(c => c.Tasks).ToList();
            List<PchelaMapUserTasks> _userstasks = _context.UsersTasks.ToList();
            if (UsersTimeRangeStart != null && UsersTimeRangeEnd != null)
            {
                List<PchelaMapUser> TR_users = new List<PchelaMapUser>();
              
                TR_users = (from PchelaMapUser user in _users
                                                where DateTime.ParseExact(user.CreatedDateUtc, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture) >= DateTime.ParseExact(UsersTimeRangeStart, "dd.MM.yyyy", CultureInfo.InvariantCulture) &&
                                                DateTime.ParseExact(user.CreatedDateUtc, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture) <= DateTime.ParseExact(UsersTimeRangeEnd, "dd.MM.yyyy", CultureInfo.InvariantCulture)
                                                select user).ToList();
                _users = TR_users;
            }


            List<string> roles = new List<string>();
            int j = 0;
            foreach (PchelaMapUser user in _users)
            {
                string str = "";
                var RoleManage = await _userManager.GetRolesAsync(user);
                foreach (var role in RoleManage)
                { str = (str == "") ? role.ToString() : str + " , " + role.ToString(); }
                roles.Add(str);
                j++;
            }
            var contentRoot = _env.ContentRootPath;
            string _path = System.IO.Path.Combine(contentRoot, "reports/PchlmapUsers.xlsx");

            XlxsCreation _CreateFile = new XlxsCreation();

            _CreateFile.XlxsCreationUsers(_users, roles, _userstasks, _path);

            if (DownloadOrMail)
            {
                string mailHeader = "Таблица пользователей";
                string mailMessage = "Таблица пользователей на " + TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _TimeZoneinfo).ToString("dd.MM.yyyy HH:mm:ss");
                if (UsersTimeRangeStart != null && UsersTimeRangeEnd != null)
                {
                    mailMessage = "Таблица пользователей с " + UsersTimeRangeStart + " по " + UsersTimeRangeEnd;
                }
                EmailService emailSender = new EmailService();
                var userMail = _userManager.GetUserAsync(User).Result.Email;
                await emailSender.SendWithAttachmentsAsync(userMail, mailHeader, mailMessage, _path);
                _modalData = new AdminModal
                {
                    Header = "Success",
                    Message = "Таблица пользователей отправлена на " + userMail + ", ожидайте."
                };
                return RedirectToAction("ExportData");
            }
            else
            {
                //_modalData = new AdminModal
                //{
                //    Header = "Success",
                //    Message = "Таблица пользователей сохранена в папку downloads."
                //};
                return DownloadFile(_path);
            }

        }

        //загрузка на комп
        [HttpGet("download")]
        public FileResult DownloadFile(string path)
        {
            var net = new System.Net.WebClient();
            var data = net.DownloadData(path);
            var content = new System.IO.MemoryStream(data);
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = System.IO.Path.GetFileName(path);
            return File(content, contentType, fileName);
        }

        //отправка отчёта на почту или загрузка на комп
        [HttpPost]
        public async Task<IActionResult> GenerateTasksTable(bool DownloadOrMail, string TasksTimeRangeStart, string TasksTimeRangeEnd)
        {
            List<PchelaMapUser> _users = _context.Users.ToList();
            List<PchelaMapTask> _tasks = _context.Tasks.ToList();
            List<PchelaMapUserTasks> _userstasks = _context.UsersTasks.ToList();
            if (TasksTimeRangeStart != null && TasksTimeRangeEnd != null)
            {
                List<PchelaMapTask> TR_tasks = new List<PchelaMapTask>();
                TR_tasks = (from PchelaMapTask task in _tasks
                                                where DateTime.ParseExact(task.CreatedDateUtc, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture) >= DateTime.ParseExact(TasksTimeRangeStart, "dd.MM.yyyy", CultureInfo.InvariantCulture) &&
                                                DateTime.ParseExact(task.CreatedDateUtc, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture) <= DateTime.ParseExact(TasksTimeRangeEnd, "dd.MM.yyyy", CultureInfo.InvariantCulture)
                                                select task).ToList();
                
                _tasks = TR_tasks;
            }
            var contentRoot = _env.ContentRootPath;
            string _path = System.IO.Path.Combine(contentRoot, "reports/PchlmapTasks.xlsx");

            XlxsCreation _CreateFile = new XlxsCreation();
            _CreateFile.XlxsCreationTasks(_tasks, _userstasks, _users, _path);
            if (DownloadOrMail)
            {
                string mailHeader = "Таблица заданий";
                string mailMessage = "Таблица заданий на " + TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _TimeZoneinfo).ToString("dd.MM.yyyy HH:mm:ss");
                if (TasksTimeRangeStart != null && TasksTimeRangeEnd != null)
                {
                    mailMessage = "Таблица заданий с " + TasksTimeRangeStart + " по " + TasksTimeRangeEnd;
                }
                EmailService emailSender = new EmailService();
                var userMail = _userManager.GetUserAsync(User).Result.Email;
                await emailSender.SendWithAttachmentsAsync(userMail, mailHeader, mailMessage, _path);
                _modalData = new AdminModal
                {
                    Header = "Success",
                    Message = "Таблица заданий отправлена на " + userMail + ", ожидайте."
                };
                return RedirectToAction("ExportData");
            }
            else
            {
                //_modalData = new AdminModal
                //{
                //    Header = "Success",
                //    Message = "Таблица заданий сохранена в папку downloads."
                //};
                return DownloadFile(_path);
            }
        }

        //Загрузка Zip архива с файлами выполненных заданий 
        [HttpPost]
        public FileResult downloadDoneReports(string ZipPath)
        {
            var contentType = "application/zip";
            var fileName = System.IO.Path.GetFileName(ZipPath);
            var fs = new FileStream(ZipPath, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.None);
            return File(fs, contentType, fileName);
        }

        //Генерация текстового файла с общей картой папок отчётов (генерится в папке zip)
        private void ReportsZipMapGenerator(ReportsForDownload report)
        {
            var webRoot = _env.WebRootPath;
            string path = System.IO.Path.Combine(webRoot, ("Images/ReportsZip/Map.txt"));
            if (!System.IO.File.Exists(path))
            {
                System.IO.File.CreateText(path);
            }
            using (System.IO.StreamWriter fileDescription = System.IO.File.AppendText(path))
            {
                if (report.ResultFiles.Count() > 1)
                {
                    string lineIdsFolder = "Папка в архиве: " + report.id + "/" + report.UserTakenID;
                    string lineNamesFolder = "Имя нуждающегося/взял задание: " + report.Name + "/" + report.UserTakenName;
                    string lineDescription = "Описание задания: " + report.description;
                    string lineReportFiles = "Файлы отчёта: ";
                    string EmptyLine = " ";
                    int i = 0;
                    foreach (string fileName in report.ResultFiles)
                    {
                        if (i == 0)
                        {
                            lineReportFiles = lineReportFiles + fileName.Substring(fileName.LastIndexOf('/') + 1);
                        }
                        else
                        {
                            lineReportFiles = (lineReportFiles == "Файлы отчёта: " ? "Файлы отчёта: " : lineReportFiles + ", ") + fileName.Substring(fileName.LastIndexOf('/') + 1);
                        }
                        i++;
                    }

                    string[] lineToWrite = new string[] {
                    lineIdsFolder,
                    lineNamesFolder,
                    lineDescription,
                    lineReportFiles,
                    EmptyLine
                    };
                    foreach (string line in lineToWrite)
                    {
                        fileDescription.WriteLine(line);
                    }
                }
            }
   
        }

        //Генерация текстового файла с описанием задания (генерится в папке файлов отчёта задания)
        private void ReportDescriptionGenerator(ReportsForDownload report)
        {
            var webRoot = _env.WebRootPath;
            string lineID = "ID задания: " + report.id;
            string lineCreatorID = "ID создателя задания: " + report.CreatorID;
            string lineName = "Имя нуждающегося: " + report.Name;
            string lineDescription = "Описание задания: " + report.description;
            string linePhone = "Телефон нуждающегося: " + report.Phone;
            string lineEmail = "Почта нуждающегося: " + report.Email;
            string lineAdress = "Адрес: " + report.Adress;
            string lineUrgentable = "Срочность: " + report.UrgentStatus == "1" ? "срочное" : "обычное";
            string lineUserMessage = "Комментарий пользователя: " + report.FromUserMessage;
            string lineAdminMessage = "Комментарий администратора: " + report.FromAdminMessage;
            string lineReportFiles = "Файлы отчёта: ";
            int i = 0;
            foreach (string fileName in report.ResultFiles)
            {
                if (i == 0)
                {
                    lineReportFiles = lineReportFiles + fileName.Substring(fileName.LastIndexOf('/') + 1);
                }
                else
                {
                    lineReportFiles = (lineReportFiles == "Файлы отчёта: " ? "Файлы отчёта: " : lineReportFiles + ", ") + fileName.Substring(fileName.LastIndexOf('/') + 1);
                }
                i++;
            }
            string lineUserTakenID = "ID волонтёра: " + report.UserTakenID;
            string lineUserTakenName = "Имя волонтёра: " + report.UserTakenName;
            string lineCreatedDate = "Дата создания: " + report._CreatedDateUtc;
            string lineTakenDate = "Дата всзятия: " + report._DateTaken;
            string lineDoneDate = "Дата выполнения: " + report._DateDone;
            string lineTimeInProcess = "Длительность выполнения: " + report._TimeInProcess;
            string[] lineToWrite = new string[] {
                    lineID,
                    lineCreatorID,
                    lineName,
                    lineDescription,
                    linePhone,
                    lineEmail,
                    lineAdress,
                    lineUrgentable,
                    lineUserMessage,
                    lineAdminMessage,
                    lineReportFiles,
                    lineUserTakenID,
                    lineUserTakenName,
                    lineCreatedDate,
                    lineTakenDate,
                    lineDoneDate,
                    lineTimeInProcess
                };
            string path = System.IO.Path.Combine(webRoot, (report.ResultMediaFolder + "Info.txt"));
            using (System.IO.StreamWriter fileDescription = new StreamWriter(path))
            {
                foreach (string line in lineToWrite)
                {
                    fileDescription.WriteLine(line);
                }


            }
        }

        //Генерация zip архива для каждого отдельного задания при одобрении отчёта
        private string ReportAddToZip(ReportsForDownload report)
        {
            var webRoot = _env.WebRootPath;
            string ZipPath = System.IO.Path.Combine(webRoot, "Images/ReportsZip/reports.zip");
            string MapPath = System.IO.Path.Combine(webRoot, ("Images/ReportsZip/Map.txt"));
            if (!System.IO.File.Exists(ZipPath))
            {
                using (FileStream zipToOpen = new FileStream(ZipPath, FileMode.Create))
                {
                    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                    {
                        ReportsZipMapGenerator(report);
                        ZipArchiveEntry _mapEntry = archive.CreateEntryFromFile(MapPath, "Map.txt");
                        foreach (string fileToZip in report.ResultFiles)
                        {
                            string fileToZipFullPath = System.IO.Path.Combine(webRoot, fileToZip);
                            string FileName = new System.IO.FileInfo(fileToZip).Name;//fileToZip.Substring(fileToZip.LastIndexOf("/") + 1);
                            ZipArchiveEntry _entry = archive.CreateEntry(report.id + "/" + report.UserTakenID + "/" + FileName);
                            using (FileStream sourceFileStream = new FileStream(fileToZipFullPath, mode: FileMode.Open))
                            using (Stream reader = _entry.Open())
                            {
                                sourceFileStream.CopyTo(reader);
                            }
                        }
                    }
                }
            }
            else
            {
                
                System.IO.FileInfo _fileInfo = new FileInfo(ZipPath);
                if (_fileInfo.Length >= 250000000)
                {
                    string ZipTempPath = System.IO.Path.Combine(webRoot, "Images/ReportsZip/reports" + TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _TimeZoneinfo).ToString("dd-MM-yyyy_HH-mm") + ".zip");
                    System.IO.File.Move(ZipPath, ZipTempPath);
                    System.IO.File.Delete(MapPath);
                }
                //using (FileStream zipToOpen = new FileStream(ZipPath, FileMode.OpenOrCreate))
                //{
                    //using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                using (ZipArchive archive = ZipFile.Open(ZipPath, ZipArchiveMode.Update))
                {
                        ReportsZipMapGenerator(report);
                        ZipArchiveEntry _mapEntry = archive.CreateEntryFromFile(MapPath, "Map.txt");
                        foreach (string fileToZip in report.ResultFiles)
                        {
                            string fileToZipFullPath = System.IO.Path.Combine(webRoot, fileToZip);
                            string FileName = new System.IO.FileInfo(fileToZip).Name;
                            ZipArchiveEntry _entry = archive.CreateEntry(report.id + "/" + report.UserTakenID + "/" + FileName);
                            using (FileStream sourceFileStream = new FileStream(fileToZipFullPath, mode: FileMode.Open))
                            using (Stream reader = _entry.Open())
                            {
                                sourceFileStream.CopyTo(reader);
                            }
                        }
                    }
                //}
            }
            return ZipPath;
        }

        //Возвращает все файлы из директории файлов отчёта задания (включая .txt с описанием)
        private string[] ZipResultFiles(string path)
        {
            var webRoot = _env.WebRootPath;
            string[] Outfiles = new string[] { }; ;
            Outfiles = Directory.GetFiles(System.IO.Path.Combine(webRoot, path)).ToArray();
            Outfiles = FolderShitCleaner(Outfiles);
            for (int t = 0; t < Outfiles.Length; t++)
            {
                Outfiles[t] = path + Outfiles[t];
            }
            return Outfiles;
        }

        //Удаление файлов выполненных заданий
        [HttpPost]
        public IActionResult DeleteDoneReportFiles()
        {
            var webRoot = _env.WebRootPath;
            List<PchelaMapUserTasks> Tasks = new List<PchelaMapUserTasks>();
            List<PchelaMapTask> DBUtasksList = new List<PchelaMapTask>();
            Tasks = _context.UsersTasks.Where(x => x.Status == "done").ToList();
            DBUtasksList = _context.Tasks.Where(x => x.Status == "closed").ToList();
            List<ReportsForDownload> TasksList = (from PchelaMapTask bdvar in DBUtasksList
                                                  join x in Tasks on bdvar.id equals x.TaskID
                                                  select new ReportsForDownload
                                                  {
                                                      id = bdvar.id,
                                                      CreatorID = bdvar.UserId,
                                                      Name = bdvar.Name,
                                                      description = bdvar.Description,
                                                      Phone = bdvar.Phone,
                                                      Email = bdvar.UserMail,
                                                      Adress = bdvar.Adress,
                                                      UrgentStatus = bdvar.Urgentable,
                                                      FromUserMessage = x.UserComment,
                                                      FromAdminMessage = x.AdminComment,
                                                      ResultMediaFolder = x.MediaFolder,
                                                      ResultFiles = ZipResultFiles(x.MediaFolder),
                                                      UserTakenID = x.UserID,
                                                      UserTakenName = _userManager.FindByIdAsync(x.UserID).Result.Name,
                                                      _CreatedDateUtc = bdvar.CreatedDateUtc,
                                                      _DateTaken = x.DateTaken,
                                                      _DateDone = x.DateDone,
                                                      _TimeInProcess = timeInProcess(x.DateTaken, x.DateDone)
                                                  }).ToList();


            foreach (ReportsForDownload report in TasksList)
            {
                string FullDirpath = System.IO.Path.Combine(webRoot, report.ResultMediaFolder);
                if (System.IO.Directory.Exists(FullDirpath))
                {
                    string[] _files = System.IO.Directory.GetFiles(FullDirpath);
                    foreach (string _file in _files)
                    {
                        System.IO.File.Delete(_file);
                    }
                }
            }
            string ZipPath = System.IO.Path.Combine(webRoot, "Images/ReportsZip");
            System.IO.DirectoryInfo _ZipDir = new DirectoryInfo(ZipPath);
            foreach (System.IO.FileInfo _file in _ZipDir.GetFiles())
            {
                System.IO.File.Delete(_file.FullName);
            }
            return RedirectToAction("ExportData");
        }

        //Скачивание почтовых лог файлов 
        [HttpPost]
        public FileResult downloadMailLogs(string fileFullName)
        {
            var contentType = "application/zip";
            var fileName = System.IO.Path.GetFileName(fileFullName);
            var fs = new FileStream(fileFullName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.None);
            return File(fs, contentType, fileName);
        }

        //Удаление почтовых лог файлов 
        [HttpPost]
        public IActionResult DeleteMailLogs()
        {
            var webRoot = _env.WebRootPath;
            var contentRoot = _env.ContentRootPath;
            List<string> _mailLogFiles = new List<string>();
            DirectoryInfo _MaillogDirInfo = new DirectoryInfo(System.IO.Path.Combine(contentRoot, "mailLogs"));
            foreach (FileInfo _logFile in _MaillogDirInfo.GetFiles())
            {
                System.IO.File.Delete(_logFile.FullName);
            }
            return RedirectToAction("ExportData");
        }

        //обработка списка ссылок на аватарки из папки
        // очистка входной папки от файлов ".DS_Store" и упорядочивание файлов с числовым именем по возрастанию
        // + очитска имени файла от пути
        private string[] FolderShitCleaner(string[] folderContent)
        {

            for (var i = 0; i < folderContent.Length; i++)
            {
                if (folderContent[i].Contains(".DS_Store"))
                {
                    folderContent = Array.FindAll(folderContent, val => val != folderContent[i]);
                    i--;
                }
            }
            for (var i = 0; i < folderContent.Length; i++)
            {
                var iicur = folderContent[i].Substring(folderContent[i].LastIndexOf("/", StringComparison.CurrentCulture) + 1);
                folderContent[i] = iicur;
            }
            folderContent = folderContent.OrderBy(x => x.Length).ToArray();
            return folderContent;
        }

       public IActionResult Promo(SortSearchPagin.PromoSortState sortOrder)
        {
            List<PchelaMapUserTasks> TasksWithPromo = _context.UsersTasks.Where(x => x.Promo != null).ToList();
            List<string> TasksWithPromoIds = TasksWithPromo.Select(i => i.TaskID).ToList();
            List<PchelaMapTask> Tasks = _context.Tasks.Where(x => TasksWithPromoIds.Contains(x.id)).ToList();
            List<PromoBd> PromoList = _context.PromoCode.ToList();
            int i = 1;
            List<Promo> PromoViewList = (from PromoBd _promo in PromoList
                                         select new Promo
                                                  {
                                                     indx = i++,
                                                     code=_promo.code,
                                                     userId=_promo.userId,
                                                     userName = _promo.userId==null?"": PromoUserExistCheck("name", _promo.userId),
                                                     userPhoto = _promo.userId == null ? "" : PromoUserExistCheck("photo", _promo.userId),
                                                     taskId = _promo.taskId,
                                                     taskInfo = _promo.taskId == null ? "" : PromoTaskExistCheck(Tasks, _promo.taskId),
                                                     status = _promo.status
                                                  }).ToList();

            if (PromoViewList.Count() == 0)
            {
                ViewBag.TasksCountMessage = "В базе данных нет промокодов";
            }
            MenuShowElementsCount();
            ViewBag.PageActive = "promo";
            PromoViewList = PromoSorting(sortOrder, PromoViewList);
            return View(PromoViewList);
        }
        private string PromoUserExistCheck(string info, string userId)
        {
            var _userResult = _userManager.FindByIdAsync(userId).Result;
            if (_userResult != null)
            {
                if (info == "name")
                {
                    return _userResult.Name;
                }
                else
                {
                    return _userResult.UserPhoto;
                }
            }
            else
            {
                return "пользователь не найден";
            }
        }
            private string PromoTaskExistCheck(List<PchelaMapTask> Tasks, string taskId)
        {
            if (Tasks.Where(x => x.id == taskId).Count() > 0)
            {
                return Tasks.First(x => x.id == taskId).Description;
            }
            else
            {
                return "задание не найдено";
            }
        }
        [HttpPost]
        public async Task<IActionResult> Promo(IFormFile _csvFile)
        {
            var webRoot = _env.WebRootPath;
            string CsvPath = System.IO.Path.Combine(webRoot, "TempFiles/Codes.csv");
            if (_csvFile != null)
            {
                
                using (var fileStream = new FileStream(CsvPath, FileMode.Create))
                {
                    await _csvFile.CopyToAsync(fileStream);
                }
                IEnumerable<PromoCsv> _promoCsvList = null;
                using (var reader = new StreamReader(CsvPath))
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                   _promoCsvList =  csv.GetRecords<PromoCsv>().ToList();
                }
                if (_promoCsvList!=null)
                {
                    List<PromoCsv> distelems = _promoCsvList.Distinct(new PromoItemComparer()).ToList();
                    List<PromoBd> PromoBDList = (from PromoCsv _code in distelems
                                                 select new PromoBd
                                                 {
                                                     code = _code.code ,
                                                     status = 0
                                                 }).ToList();
                    await _context.Database.ExecuteSqlRawAsync("DELETE FROM PromoCode");
                    _context.PromoCode.AddRange(PromoBDList);
                    _context.SaveChanges();
                }
            }
            if (System.IO.File.Exists(CsvPath))
            {
                System.IO.File.Delete(CsvPath);
            }
            return RedirectToAction("Promo");
        }
        public async Task<IActionResult> ChangePromoStatus(string code, int status)
        {
            PromoBd CodeRow = _context.PromoCode.FirstOrDefault(x => x.code == code);
            CodeRow.status = status;
            CodeRow.taskId = null;
            CodeRow.userId = null;
            await _context.SaveChangesAsync();
            return RedirectToAction("Promo");
        }
        [HttpPost]
        public async Task<IActionResult> DeletePromoCode (string code)
        {
            PromoBd CodeRow = _context.PromoCode.FirstOrDefault(x => x.code == code);
            _context.PromoCode.Remove(CodeRow);
            await _context.SaveChangesAsync();
            return RedirectToAction("Promo");
        }
        //сорировка промокодов
        private List<Promo> PromoSorting(SortSearchPagin.PromoSortState sortOrder, List<Promo> PromoList)
        {
            ViewData["IndexSort"] = (sortOrder == SortSearchPagin.PromoSortState.IndexDesc ?
               SortSearchPagin.PromoSortState.IndexAsc : SortSearchPagin.PromoSortState.IndexDesc);
            ViewData["StatusSort"] = (sortOrder == SortSearchPagin.PromoSortState.StatusAsc ?
                  SortSearchPagin.PromoSortState.StatusDesc : SortSearchPagin.PromoSortState.StatusAsc);

            PromoList = sortOrder switch
            {
                SortSearchPagin.PromoSortState.IndexAsc => PromoList.OrderBy(s => s.indx).ToList(),
                SortSearchPagin.PromoSortState.IndexDesc => PromoList.OrderByDescending(s => s.indx).ToList(),
                SortSearchPagin.PromoSortState.StatusAsc => PromoList.OrderBy(s => s.status).ToList(),
                SortSearchPagin.PromoSortState.StatusDesc => PromoList.OrderByDescending(s => s.status).ToList(),
                
                _ => PromoList,
            };

            return PromoList;
        }
    }
}
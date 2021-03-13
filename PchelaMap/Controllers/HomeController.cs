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
using System.IO;
using Microsoft.AspNetCore.Identity;
using PchelaMap.Areas.Identity.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using PchelaMap.Data;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using System.Drawing;
using Microsoft.AspNetCore.Authorization;
using TimeZoneConverter;
using System.Threading;

namespace PchelaMap.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<PchelaMapUser> _signInManager;
        private readonly UserManager<PchelaMapUser> _userManager;
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _env;
        static List<UserWithTasks> _UsersTaskList = new List<UserWithTasks>();
        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger, IWebHostEnvironment env, UserManager<PchelaMapUser> userManager, SignInManager<PchelaMapUser> signInManager)
        {
            _context = context;
            _logger = logger;
            _env = env;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        XElement root;
        TimeZoneInfo _TimeZoneinfo = TZConvert.GetTimeZoneInfo("Russian Standard Time");
        private static UserModal _modalData = new UserModal();
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            AllUsers allusersList = new AllUsers();
            UserModal _modal = new UserModal();
            List<PchelaMapUser> BDusersList = _userManager.Users.ToList();
            allusersList.UsersList = (from PchelaMapUser bdvar in BDusersList
                                      where bdvar.UserCoordinateX != null && bdvar.UserCoordinateY != null && bdvar.UserCoordinateX != "" && bdvar.UserCoordinateY != ""
                                      select new User
                                      {
                                          id = bdvar.Id,
                                          CoordinateX = bdvar.UserCoordinateX,
                                          CoordinateY = bdvar.UserCoordinateY,
                                          Name = bdvar.Name,
                                          Phone = bdvar.PhoneNumber,
                                          PhotoUrl = bdvar.UserPhoto,
                                          Adress = bdvar.UserAdress
                                      }).ToList();
            List<PchelaMapTask> BDtasksList = new List<PchelaMapTask>();
            int UserTasksCount = 0;
            BDtasksList = _context.Tasks.Where(x => x.Status == "active" || x.Status == "closed").ToList();
            List<string> RefusedTasksIds = new List<string>();
            if (_signInManager.IsSignedIn(User))
            {
                List<UsersRefusedTasks> RefusedTasks = _context.UsersRefusedFromTasks.Where(x => x.UserID == user.Id).ToList();
                RefusedTasksIds = RefusedTasks.Select(x => x.TaskID).ToList();

                if (user.EmailConfirmed)
                {
                    if (user.SystemMessageForUser != "" && user.SystemMessageForUser != null)
                    {
                        _modalData = new UserModal
                        {
                            Header = "Success",
                            Message = user.SystemMessageForUser
                        };
                        _modal = _modalData;
                        user.SystemMessageForUser = "";
                        await _userManager.UpdateAsync(user);
                    }
                }
                else
                {
                    _modalData = new UserModal
                    {
                        Header = "Error",
                        Message = "Подтвердите электронный адрес. Перейдите по ссылке в письме на указанной при регистрации почте."
                    };
                    _modal = _modalData;
                }
               
            }
            allusersList.UsersTaskList = (from PchelaMapTask bdvar in BDtasksList
                                          where bdvar.Urgentable == 0 && bdvar.Status == "active" && !RefusedTasksIds.Contains(bdvar.id) && bdvar.NY_task == 0
                                          select new UserWithTasks
                                          {
                                              id = bdvar.id,
                                              CoordinateX = bdvar.CoordinateX,
                                              CoordinateY = bdvar.CoordinateY,
                                              description = bdvar.Description,
                                              Name = bdvar.Name,
                                              Phone = bdvar.Phone,
                                              PhotoUrl = bdvar.Photo,
                                              Adress = bdvar.Adress
                                          }).ToList();
            allusersList.UrgentTasksList = (from PchelaMapTask bdvar in BDtasksList
                                            where bdvar.Urgentable == 1 && bdvar.Status == "active" && !RefusedTasksIds.Contains(bdvar.id) && bdvar.NY_task == 0
                                            select new UserWithTasks
                                            {
                                                id = bdvar.id,
                                                CoordinateX = bdvar.CoordinateX,
                                                CoordinateY = bdvar.CoordinateY,
                                                description = bdvar.Description,
                                                Name = bdvar.Name,
                                                Phone = bdvar.Phone,
                                                PhotoUrl = bdvar.Photo,
                                                Adress = bdvar.Adress
                                            }).ToList();
            allusersList.DoneTasksList = (from PchelaMapTask bdvar in BDtasksList
                                          where bdvar.Status == "closed"
                                          select new UserWithTasks
                                          {
                                              id = bdvar.id,
                                              CoordinateX = bdvar.CoordinateX,
                                              CoordinateY = bdvar.CoordinateY,
                                              description = bdvar.Description,
                                              Name = bdvar.Name,
                                              Phone = bdvar.Phone,
                                              PhotoUrl = bdvar.Photo,
                                              Adress = bdvar.Adress
                                          }).ToList();
            //генератор json файлов для отображения элементов на карте
            JSONgenerator jSONgenerator = new JSONgenerator(user, _signInManager.IsSignedIn(User), _env.WebRootPath);
            jSONgenerator.jsonGenerator(allusersList);
            _modal = _modalData;
            
            MainPageClass _MainPageModel = new MainPageClass()
            {
                UsersCount = allusersList.UsersList.Count(),
                TasksCount = allusersList.UsersTaskList.Count() + allusersList.UrgentTasksList.Count(),
                ModalData = _modal,
                CurrUserTasksCount = UserTasksCount
            };
            _modalData = new UserModal();
            return View("Index", _MainPageModel);
          
            
        }
        //Новогодний список
        public async Task<IActionResult> NY_Page()
        {
            var CurrentUser = await _userManager.GetUserAsync(User);
            List<PchelaMapTask> AllTaskList = _context.Tasks.Where(x=> x.Status == "active" && x.NY_task == 1).ToList();
            List<UserWithTasks> TasksList = new List<UserWithTasks>();
            bool _LimitOfTasksReached = false;
            bool _LimitOfUrgentTasksReached = false;
            List<PchelaMapTask> UrgentTasks = _context.Tasks.Where(x => x.Urgentable == 1).ToList();
            List<string> UrgentTasksIds = UrgentTasks.Select(x => x.id).ToList();
            int Tcount = _context.UsersTasks.Count(x => x.UserID == CurrentUser.Id && x.Status != "done");
            int TUrgcount = _context.UsersTasks.Count(x => x.UserID == CurrentUser.Id && UrgentTasksIds.Contains(x.TaskID) && x.Status != "done");
            if (Tcount >= 2 && !User.IsInRole("admin"))
            {
                _LimitOfTasksReached = true;
            }
            if (TUrgcount >= 1 && !User.IsInRole("admin"))
            {
                _LimitOfUrgentTasksReached = true;
            }
            TasksList = (from PchelaMapTask bdvar in AllTaskList
                         select new UserWithTasks
                         {
                             id = bdvar.id,
                             description = bdvar.Description,
                             Name = bdvar.Name,
                             PhotoUrl = bdvar.Photo,
                             Adress = bdvar.Adress,
                             LimitOfTasksReached = _LimitOfTasksReached,
                             LimitOfUrgentTasksReached = _LimitOfUrgentTasksReached
                         }).ToList();
            ViewBag.Is_NY_SearchPage = "";
            ViewData["searchFieldValue"] = "";
            ViewData["MainHeader"] = "СПИСОК НОВОГОДНИХ ЖЕЛАНИЙ";
            return View(TasksList);
        }

        [HttpPost]
        public IActionResult NY_TasksSearch(string AdressString)
        {
            List<PchelaMapTask> AllTaskList = _context.Tasks.Where(x => x.Status == "active" && x.NY_task == 1).ToList();
            List<UserWithTasks> TasksList = new List<UserWithTasks>();
            TasksList = (from PchelaMapTask bdvar in AllTaskList
                         select new UserWithTasks
                         {
                             id = bdvar.id,
                             description = bdvar.Description,
                             Name = bdvar.Name,
                             PhotoUrl = bdvar.Photo,
                             Adress = bdvar.Adress
                         }).ToList();
            if (AdressString != null && AdressString != "")
            {
                TasksList = TasksList.Where(x => x.Adress?.IndexOf(AdressString, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                ViewBag.Is_NY_SearchPage = "Yes";
            }
            else
            {
                ViewBag.Is_NY_SearchPage = "";
            }
            if (TasksList.Count() == 0)
            {
                ViewBag.UsersCountMessage = "Задания, попадающие под критерии поиска не найдены.";
            }
            ViewData["searchFieldValue"] = AdressString;
            ViewData["MainHeader"] = "СПИСОК НОВОГОДНИХ ЖЕЛАНИЙ";
            return View("NY_Page", TasksList);
        }
        //Просмотр информации о задании на карте
        public async Task<PartialViewResult> TaskView(string id)
        {
            PchelaMapUser user = await _userManager.GetUserAsync(User);
            bool _TaskAlreadyTaken = _context.UsersTasks.Any(x => x.UserID == user.Id && x.TaskID == id);
            bool _LimitOfTasksReached = false;
            bool _LimitOfUrgentTasksReached = false;
            PchelaMapTask taskInfo = _context.Tasks.FirstOrDefault(_task => _task.id == id);
            List<PchelaMapTask> UrgentTasks = _context.Tasks.Where(x => x.Urgentable == 1).ToList();
            List<string> UrgentTasksIds = UrgentTasks.Select(x => x.id).ToList();
            int Tcount = _context.UsersTasks.Count(x => x.UserID == user.Id && x.Status != "done");
            int TUrgcount = _context.UsersTasks.Count(x => x.UserID == user.Id && UrgentTasksIds.Contains(x.TaskID) && x.Status != "done");
            if (Tcount >= 2 && !User.IsInRole("admin"))
            {
                _LimitOfTasksReached = true;
            }
            if (TUrgcount >= 1 && !User.IsInRole("admin"))
            {
                _LimitOfUrgentTasksReached = true;
            }
            UserWithTasks _userTask = new UserWithTasks
            {
                id = taskInfo.id,
                description = taskInfo.Description,
                CoordinateX = taskInfo.CoordinateX,
                CoordinateY = taskInfo.CoordinateY,
                Adress = taskInfo.Adress,
                PhotoUrl = taskInfo.Photo,
                Name = taskInfo.Name,
                Phone = taskInfo.Phone,
                NeedsCar = taskInfo.NeedsCar,
                UrgentStatus = taskInfo.Urgentable,
                CreatorID = taskInfo.UserId,
                TaskAlreadyTaken = _TaskAlreadyTaken,
                LimitOfTasksReached = _LimitOfTasksReached,
                LimitOfUrgentTasksReached = _LimitOfUrgentTasksReached
            };
            return PartialView(_userTask);
        }

        //Форма добавления задания
        [Authorize(Roles = "admin, moderator")]
        public async Task<PartialViewResult> AddHelpPoint()
        {
            var user = await _userManager.GetUserAsync(User);
            var createdTasksCount = _context.Tasks.Where(x=> x.UserId == user.Id && x.Status!= "closed").Count();
            CreateTaskModel CreatingTaskInfo = new CreateTaskModel
            {
                UserAdress = user.UserAdress,
                UserCoordX = user.UserCoordinateX,
                UserCoordY = user.UserCoordinateY,
                CreatedTasksCount = createdTasksCount
            };
            return PartialView(CreatingTaskInfo);
        }
        //Добавление задания в общую таблицу заданий при создании
        [HttpPost]
        [Authorize(Roles = "admin, moderator")]
        public async Task<IActionResult> AddHelpPointPost(CreateTaskModel TaskAddingResult)
        {
            var webRoot = _env.WebRootPath;
            var user = await _userManager.GetUserAsync(User);
            string TaskCoordinateX = "";
            string TaskCoordinateY = "";
            string TaskAdress = "";
            string TaskDescription = "";
            int TaskCarNeed = 0;
            int Urgentability = 0;
            int NyTask = 0;
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
            Urgentability = TaskAddingResult.Urgentability ? 1 : 0;
            NyTask = TaskAddingResult.NYTaskBool ? 1 : 0;
            string TaskID = Guid.NewGuid().ToString();
            if (_context.Tasks.Any(x => x.id == TaskID))
            {
                TaskID = Guid.NewGuid().ToString();
            }
            string TaskReportFolder = "Images/TasksReports/" + TaskID + "/";
            string path = System.IO.Path.Combine(webRoot, TaskReportFolder);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            //добавление задания
            string TaskStatus = "";
            if (!User.IsInRole("admin"))
            {
                TaskStatus = "moderating";
            }
            else
            {
                TaskStatus = "active";
            }
            
            PchelaMapTask _userTask = new PchelaMapTask
            {
                id = TaskID,
                Description = TaskDescription,
                CoordinateX = TaskCoordinateX,
                CoordinateY = TaskCoordinateY,
                Adress = TaskAdress,
                Photo = "/Images/TaskImage.jpg",
                Name = TaskAddingResult.UserName,
                Phone = TaskAddingResult.UserPhone,
                UserMail = TaskAddingResult.UserMail,
                Status = TaskStatus,
                NeedsCar = TaskCarNeed,
                ResultMediaFolder = TaskReportFolder,
                User = user,
                CreatedDateUtc = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _TimeZoneinfo).ToString("dd.MM.yyyy HH:mm:ss"),
                Urgentable = Urgentability,
                NY_task = NyTask
            };
            _context.Tasks.Add(_userTask);
            await _userManager.UpdateAsync(user);

            EmailService emailSender = new EmailService();
            var userMail = TaskAddingResult.UserMail;
            string mailHeader = "";
            string mailMessage = "";
            mailHeader = EmailService._onTaskCreation["header"];
            mailMessage = EmailService._onTaskCreation["bodyPrt1"] +
                _userTask.Description.Substring(0, Math.Min(_userTask.Description.Length, 50)) + "..." + EmailService._onTaskCreation["bodyPrt2"];
            await emailSender.SendAsync(userMail, mailHeader, mailMessage);

            //_modalData = new UserModal
            //{
            //    Header = "Success",
            //    Message = "Задание успешно добавлено и ожидает модерации. По окончании модерации Вы увидите его на карте и во вкладке 'Мои задания'"
            //};
            return RedirectToAction("Index");
        }
        //Форма редактирования задания
        public IActionResult EditHelpPoint(string id)
        {
            var task = _context.Tasks.FirstOrDefault(x => x.id == id);
            bool Urgentable = task.Urgentable == 1 ? true : false;
            bool Car = task.NeedsCar == 1 ? true : false;
            CreateTaskModel CreatingTaskInfo = new CreateTaskModel
            {
                UserAdress = task.Adress,
                UserCoordX = task.CoordinateX,
                UserCoordY = task.CoordinateY,
                Urgentability = Urgentable,
                NeedsCar = Car,
                TaskDescription = task.Description,
                Status = task.Status
            };
            return View("EditHelpPoint", CreatingTaskInfo);
        }
        //Редактирование задания 
        [HttpPost]
        public async Task<IActionResult> EditHelpPoint(CreateTaskModel TaskAddingResult)
        {
            var webRoot = _env.WebRootPath;
            var user = await _userManager.GetUserAsync(User);
            string TaskCoordinateX = "";
            string TaskCoordinateY = "";
            string TaskAdress = "";
            string TaskDescription = "";
            int TaskCarNeed = 0;
            int TaskUrgentable = 0;
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
            TaskUrgentable = TaskAddingResult.Urgentability ? 1 : 0;
            string TaskStatus = "moderating";
            PchelaMapTask _task = _context.Tasks.FirstOrDefault(x => x.id == TaskAddingResult.id);
            _task.Description = TaskDescription;
            _task.CoordinateX = TaskCoordinateX;
            _task.CoordinateY = TaskCoordinateY;
            _task.Adress = TaskAdress;
            _task.NeedsCar = TaskCarNeed;
            _task.Urgentable = TaskUrgentable;
            _task.Status = TaskStatus;
            _context.SaveChanges();
            return RedirectToAction("MyTasks");
        }
        //Нажатие кнопки Взять Задание (Помочь)
        public async Task<IActionResult> TakeTask(string id, string SberTask = "")
        {
            
            var webRoot = _env.WebRootPath;
            var _user = await _userManager.GetUserAsync(User);
            var userID = _user.Id;
            PchelaMapTask taskInfo = _context.Tasks.FirstOrDefault(_task => _task.id == id);
            
            taskInfo.Status = "in_progress";
            var _UserMediaFolder = taskInfo.ResultMediaFolder + userID + "/";
            string path = System.IO.Path.Combine(webRoot, _UserMediaFolder);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string taskpromo = "";
            if (SberTask != "")
            {
                PromoBd promoRow = _context.PromoCode.FirstOrDefault(x => x.status == 0);
                if (promoRow != null)
                {
                    taskpromo = promoRow.code;
                    promoRow.status = 1;
                    promoRow.userId = userID;
                    promoRow.taskId = id;
                }
            }
            PchelaMapUserTasks _SingleUserTask = new PchelaMapUserTasks
            {
                TaskID = id,
                UserID = userID,
                Status = "active",
                MediaFolder = _UserMediaFolder,
                Promo = taskpromo,
                DateTaken = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _TimeZoneinfo).ToString("dd.MM.yyyy HH:mm:ss")
            };
            
            if (!_context.UsersTasks.Any(x => x.TaskID == id))
            {

                await _context.UsersTasks.AddAsync(_SingleUserTask);
                await _context.SaveChangesAsync();
                await _userManager.UpdateAsync(_user);

                EmailService emailSender = new EmailService();
                var userMail = _user.Email;
                string mailHeader = "";
                string mailMessage = "";
                if (taskInfo.Urgentable == 1)
                {
                    mailHeader = EmailService._onTaskUrgentTaken["header"];
                    mailMessage = EmailService._onTaskUrgentTaken["bodyPrt1"] +
               taskInfo.Description.Substring(0, Math.Min(taskInfo.Description.Length, 50)) + "..." +
               EmailService._onTaskUrgentTaken["bodyPrt2"];
                }
                else
                {
                    mailHeader = EmailService._onTaskTaken["header"];
                    mailMessage = EmailService._onTaskTaken["bodyPrt1"] +
                taskInfo.Description.Substring(0, Math.Min(taskInfo.Description.Length, 50)) + "..." +
                EmailService._onTaskTaken["bodyPrt2"];
                }
                await emailSender.SendAsync(userMail, mailHeader, mailMessage);

                userMail = taskInfo.UserMail;
                mailHeader = EmailService._onTaskTakenCreatorMsg["header"];
                mailMessage = EmailService._onTaskTakenCreatorMsg["bodyPrt1"] +
               taskInfo.Description.Substring(0, Math.Min(taskInfo.Description.Length, 50)) + "..." +
               EmailService._onTaskTakenCreatorMsg["bodyPrt2"];
                await emailSender.SendAsync(userMail, mailHeader, mailMessage);

                _modalData = new UserModal
                {
                    Header = "Success",
                    Message = "Задание взято. Перейдите во вкладку 'Мои задания' для дальнейшей работы."
                };

            }
            else
            {
                _modalData = new UserModal
                {
                    Header = "Error",
                    Message = "Задание уже кем-то взято. Мы вскоре решим проблему с отображением взятых заданий на карте."
                };
                
            }
            if (SberTask =="")
            {
                return RedirectToAction("Index");
            }
            else
            {
                if(_context.PromoCode.Count()>0)
                {
                    return RedirectToAction("SberModal", new { id = id });
                }
                else
                {
                    //return Json(new { url = "https://sbermarket.ru/" });
                    return RedirectToAction("SberModal", new { id = id });
                }
                
            }
            
        }

        //Нажатие кнопки Помочь через сбер
        public PartialViewResult SberModal(string id, int mytasks = 0 )
        {
            string promoCode = _context.UsersTasks.FirstOrDefault(x => x.TaskID == id).Promo;
            bool _MyTasksButtonCall = false;
            if (mytasks == 1)
            {
                //- нажатие сбер кнопки из "моих заданий"
                _MyTasksButtonCall = true;
            }
            SberModel _sberModel = new SberModel()
            {
                PromoCode = promoCode,
                MyTasksButtonCall= _MyTasksButtonCall
            };
            return PartialView(_sberModel);
        }
        ////Сообщение пользователю по завершении к.л. действия
        public async Task<PartialViewResult> UserModal(string Header, string Message)
        {
            var user = await _userManager.GetUserAsync(User);
            UserModal _modal = new UserModal
            {
                Header = Header,
                Message = Message
            };
            return PartialView("UserModal", _modal);
        }

        //Страница заданий пользователя
        public async Task<IActionResult> MyTasks()
        {
            var CurrentUser = await _userManager.GetUserAsync(User);
            List<PchelaMapTask> tasks = new List<PchelaMapTask>();
            if (User.IsInRole("admin") || User.IsInRole("moderator"))
            {
                tasks = _context.Tasks.Where(x => x.UserId == CurrentUser.Id).ToList();
            }
            else
            {
                tasks = _context.Tasks.Where(x => x.UserMail == CurrentUser.Email).ToList();
            }
            List<UserWithTasks> TasksList = new List<UserWithTasks>();
            TasksList = (from PchelaMapTask bdvar in tasks
                         where bdvar.Status != "closed"
                         select new UserWithTasks
                         {
                             id = bdvar.id,
                             CoordinateX = bdvar.CoordinateX,
                             CoordinateY = bdvar.CoordinateY,
                             description = bdvar.Description,
                             Name = bdvar.Name,
                             Phone = bdvar.Phone,
                             PhotoUrl = bdvar.Photo,
                             Adress = bdvar.Adress,
                             UrgentStatus = bdvar.Urgentable,
                             GlobalStatus = bdvar.Status,
                             FromAdminMessage = bdvar.AdminComment
                         }).ToList();

            ViewBag.UserDoneTasksCount = _context.UsersTasks.Where(x => x.UserID == CurrentUser.Id && x.Status == "done").Count() +
                _context.Tasks.Where(x => x.UserId == CurrentUser.Id && x.Status == "closed").Count();
            ViewBag.UserTakenTasksCount = _context.UsersTasks.Where(x => x.UserID == CurrentUser.Id && x.Status != "done").Count();
            return View("MyTasks", TasksList);
        }
        //Страница Выполненных заданий пользователя
        public async Task<IActionResult> MyDoneTasks()
        {

            var CurrentUser = await _userManager.GetUserAsync(User);
            List<PchelaMapTask> AllTaskList = _context.Tasks.ToList();
            List<PchelaMapUserTasks> Tasks = _context.UsersTasks.Where(x => x.UserID == CurrentUser.Id).ToList();
            List<UserWithTasks> TasksList = new List<UserWithTasks>();
            TasksList = (from PchelaMapTask bdvar in AllTaskList
                         join x in Tasks on bdvar.id equals x.TaskID
                         where x.Status == "done"
                         select new UserWithTasks
                         {
                             id = bdvar.id,
                             CoordinateX = bdvar.CoordinateX,
                             CoordinateY = bdvar.CoordinateY,
                             description = bdvar.Description,
                             Name = bdvar.Name,
                             Phone = bdvar.Phone,
                             PhotoUrl = bdvar.Photo,
                             GlobalStatus = bdvar.Status,
                             UrgentStatus = bdvar.Urgentable,
                             Adress = bdvar.Adress
                         }).ToList();
            List<UserWithTasks> CloseTasksList = new List<UserWithTasks>();
            CloseTasksList = (from PchelaMapTask bdvar in AllTaskList
                              where bdvar.UserId == CurrentUser.Id && bdvar.Status == "closed"
                              select new UserWithTasks
                              {
                                  id = bdvar.id,
                                  CoordinateX = bdvar.CoordinateX,
                                  CoordinateY = bdvar.CoordinateY,
                                  description = bdvar.Description,
                                  Name = bdvar.Name,
                                  Phone = bdvar.Phone,
                                  PhotoUrl = bdvar.Photo,
                                  Adress = bdvar.Adress,
                                  NeedsCar = bdvar.NeedsCar,
                                  UrgentStatus = bdvar.Urgentable,
                              }).ToList();
            MyDoneTasksView _myDoneTasksView = new MyDoneTasksView()
            {
                TakenDoneTasks = TasksList,
                CreatedDoneTasks = CloseTasksList
            };
          
            if (User.IsInRole("admin") || User.IsInRole("moderator"))
            {
                ViewBag.UsersTasksCount = _context.Tasks.Where(x => x.UserId == CurrentUser.Id && x.Status != "closed").Count();
            }
            else
            {
                ViewBag.UsersTasksCount = _context.Tasks.Where(x => x.UserMail == CurrentUser.Email && x.Status != "closed").Count();
            }
           
            ViewBag.UserTakenTasksCount = _context.UsersTasks.Where(x => x.UserID == CurrentUser.Id && x.Status != "done").Count();
            return View("MyDoneTasks", _myDoneTasksView);
        }
        //Страница ВЗЯТЫХ(чужих) заданий пользователя
        public async Task<IActionResult> MyTakenTasks()
        {
            var CurrentUser = await _userManager.GetUserAsync(User);
            List<PchelaMapUserTasks> Tasks = _context.UsersTasks.Where(x => x.UserID == CurrentUser.Id).ToList();
            List<PchelaMapTask> AllTaskList = _context.Tasks.ToList();
            List<UserWithTasks> TasksList = new List<UserWithTasks>();
            TasksList = (from PchelaMapTask bdvar in AllTaskList
                         join x in Tasks on bdvar.id equals x.TaskID
                         where (bdvar.Status == "in_progress") && (x.Status == "active" || x.Status == "complite_moderation" || x.Status == "StopedOnModeration")
                         select new UserWithTasks
                         {
                             id = bdvar.id,
                             CoordinateX = bdvar.CoordinateX,
                             CoordinateY = bdvar.CoordinateY,
                             description = bdvar.Description,
                             Name = bdvar.Name,
                             Phone = bdvar.Phone,
                             PhotoUrl = bdvar.Photo,
                             Adress = bdvar.Adress,
                             NeedsCar = bdvar.NeedsCar,
                             UrgentStatus = bdvar.Urgentable,
                             Status = x.Status,
                             GlobalStatus = bdvar.Status,
                             FromAdminMessage = x.AdminComment,
                             promo = x.Promo==null?"":x.Promo
                         }).ToList();
            if (User.IsInRole("admin") || User.IsInRole("moderator"))
            {
                ViewBag.UsersTasksCount = _context.Tasks.Where(x => x.UserId == CurrentUser.Id && x.Status != "closed").Count();
            }
            else
            {
                ViewBag.UsersTasksCount = _context.Tasks.Where(x => x.UserMail == CurrentUser.Email && x.Status != "closed").Count();
            }
            ViewBag.UserDoneTasksCount = _context.UsersTasks.Where(x => x.UserID == CurrentUser.Id && x.Status == "done").Count() +
                _context.Tasks.Where(x => x.UserId == CurrentUser.Id && x.Status == "closed").Count();
            return View("MyTakenTasks", TasksList);
        }
        //Страница расширенного просмотра/отказа/составления отчёта о ВЗЯТЫХ(чужих) заданиях пользователя
        public async Task<IActionResult> UserTakenTaskEdit(string id, bool FileSizeLimit, string ActiveMenuButton="")
        {
            if (FileSizeLimit)
            {
                @ViewBag.message = "Не удалось загрузить файл. Файл слишком большой или превышен лимит размера отчёта.";
            }
            var webRoot = _env.WebRootPath;
            var CurrentUser = await _userManager.GetUserAsync(User);
            PchelaMapTask __Task = _context.Tasks.SingleOrDefault(x => x.id == id);
            string GlobalTaskStatus = "";
            string TaskStatus = "";
            string TaskCreator = "";
            string MediaFolder = "";
            string FromUserComment = "";
            if (_context.UsersTasks.Any(x => x.UserID == CurrentUser.Id && x.TaskID == id))
            {
                PchelaMapUserTasks TaskRecord = _context.UsersTasks.SingleOrDefault(x => x.UserID == CurrentUser.Id && x.TaskID == id);
                TaskStatus = TaskRecord.Status;
                GlobalTaskStatus = __Task.Status;
                TaskCreator = "other";
                MediaFolder = TaskRecord.MediaFolder;
                FromUserComment = TaskRecord.UserComment;
            }
            else
            {
                if (CurrentUser.Id == __Task.UserId)
                {
                    TaskStatus = __Task.Status;
                    GlobalTaskStatus = __Task.Status;
                    TaskCreator = "user";
                    MediaFolder = "";
                }
            }

            string[] TaskresultFiles = new string[] { };
            if (MediaFolder != "")
            {
                string path = System.IO.Path.Combine(webRoot, MediaFolder);
                long _size = 0;
                var dirInfo = new DirectoryInfo(path);
                foreach (FileInfo fi in dirInfo.GetFiles("*", SearchOption.AllDirectories))
                {
                    _size = _size + fi.Length;
                }
                decimal decSize = (decimal)_size;
                for (int i = 0; i <= 1; i++)
                {
                    decSize = decSize / 1024;
                }
                ViewBag.foldersize = Math.Round(decSize,1);

                TaskresultFiles = Directory.GetFiles(System.IO.Path.Combine(webRoot, MediaFolder)).Where(x => !x.Contains("Info.txt")).ToArray();


                TaskresultFiles = FolderShitCleaner(TaskresultFiles);
                for (int t = 0; t < TaskresultFiles.Length; t++)
                {
                        TaskresultFiles[t] = MediaFolder + TaskresultFiles[t];
                }
            }
            UserWithTasks ChoosenTask = new UserWithTasks
            {
                id = __Task.id,
                CoordinateX = __Task.CoordinateX,
                CoordinateY = __Task.CoordinateY,
                description = __Task.Description,
                Name = __Task.Name,
                Phone = __Task.Phone,
                Email = __Task.UserMail,
                PhotoUrl = __Task.Photo,
                Adress = __Task.Adress,
                NeedsCar = __Task.NeedsCar,
                UrgentStatus = __Task.Urgentable,
                Status = TaskStatus,
                GlobalStatus = GlobalTaskStatus,
                ResultFiles = TaskresultFiles,
                TaskOwner = TaskCreator,
                FromUserMessage = FromUserComment
            };
            if (User.IsInRole("admin") || User.IsInRole("moderator"))
            {
                ViewBag.UsersTasksCount = _context.Tasks.Where(x => x.UserId == CurrentUser.Id && x.Status != "closed").Count();
            }
            else
            {
                ViewBag.UsersTasksCount = _context.Tasks.Where(x => x.UserMail == CurrentUser.Email && x.Status != "closed").Count();
            }
            ViewBag.UserDoneTasksCount = _context.UsersTasks.Where(x => x.UserID == CurrentUser.Id && x.Status == "done").Count() +
                _context.Tasks.Where(x => x.UserId == CurrentUser.Id && x.Status == "closed").Count();
            ViewBag.UserTakenTasksCount = _context.UsersTasks.Where(x => x.UserID == CurrentUser.Id && x.Status != "done").Count();
           
            ViewBag.ActiveMenuButtonName = ActiveMenuButton;
        
            return View("UserTakenTaskEdit", ChoosenTask);
        }
        //Загрузка медиа для составления отчёта о ВЗЯТЫХ(чужих) заданиях пользователя
        [RequestFormLimits (MultipartBodyLengthLimit = 104857600)]
        [RequestSizeLimit(104857600)]
        [HttpPost]
        public async Task<IActionResult> UserTakenTaskEdit(string id, IFormFile uploadedFile)
        {
            var CurrentUser = await _userManager.GetUserAsync(User);
            var webRoot = _env.WebRootPath;
            bool SizeLimit = false;
            if (uploadedFile != null)
            {
                string MediaFolder = "";
                if (_context.UsersTasks.Any(x => x.UserID == CurrentUser.Id && x.TaskID == id))
                {
                    MediaFolder = _context.UsersTasks.SingleOrDefault(x => x.UserID == CurrentUser.Id && x.TaskID == id).MediaFolder;

                    long _size = 0;
                    var dirInfo = new DirectoryInfo(System.IO.Path.Combine(webRoot, MediaFolder));
                    foreach (FileInfo fi in dirInfo.GetFiles("*", SearchOption.AllDirectories))
                    {
                        _size = _size + fi.Length;
                    }
                    _size = _size + uploadedFile.Length;
                    
                    if (_size < 209715200)
                    {
                        string ReportFileCount = (dirInfo.GetFiles("").Where(x=>!x.Name.EndsWith(".txt")).Count()+1).ToString();
                        string ReportFileExtention = new FileInfo(uploadedFile.FileName).Extension;
                        string fileName = "_ReportFile"+ ReportFileCount + ReportFileExtention;
                        string Filepath = MediaFolder + fileName;
                        string _filepath = System.IO.Path.Combine(webRoot, Filepath);
                        using (var fileStream = new FileStream(_filepath, FileMode.Create))
                        {
                            await uploadedFile.CopyToAsync(fileStream);
                        }
                        SizeLimit = false;
                    }
                    else
                    {
                        SizeLimit = true;
                    }
                    //PchelaMapUserTasks CurrTaskUserLink = _context.UsersTasks.SingleOrDefault(x => x.UserID == CurrentUser.Id && x.TaskID == id);
                    //CurrTaskUserLink.MediaFolder = MediaFolder;
                    //_context.SaveChanges();
                }
            }
           
            return RedirectToAction("UserTakenTaskEdit", new { FileSizeLimit = SizeLimit });
        }

        //Сохранение отзыва волонтёра о выполненном задании
        [HttpPost]
        public async Task<IActionResult> SaveUserCommentOnTaskReport (string id, string UserTaskDoneComment)
        {
            var CurrentUser = await _userManager.GetUserAsync(User);
            if (UserTaskDoneComment != "" && UserTaskDoneComment != null)
            {
                if (_context.UsersTasks.Any(x => x.UserID == CurrentUser.Id && x.TaskID == id))
                {
                    _context.UsersTasks.SingleOrDefault(x => x.UserID == CurrentUser.Id && x.TaskID == id).UserComment = UserTaskDoneComment;
                    _context.SaveChanges();
                }
            }
            return RedirectToAction("UserTakenTaskEdit", new { id = id });
        }

        //Удаление медиа для составления отчёта о ВЗЯТЫХ(чужих) заданиях пользователя
        public IActionResult DeleteTaskMedia(string id, string MediaPath)
        {
            var webRoot = _env.WebRootPath;
            string FullFilepath = System.IO.Path.Combine(webRoot, MediaPath);
            if (System.IO.File.Exists(FullFilepath))
            {
                System.IO.File.Delete(FullFilepath);
            }
            return RedirectToAction("UserTakenTaskEdit", new { id = id });
        }
        //Отправка отчёта о ВЗЯТЫХ(чужих) заданиях пользователя
        public IActionResult CompliteTheTask(string id)
        {
            var CurrentUserID = _userManager.GetUserId(User);
            PchelaMapUserTasks _Task = new PchelaMapUserTasks();
            _Task = _context.UsersTasks.SingleOrDefault(x => x.TaskID == id && x.UserID == CurrentUserID);
            if (_Task != null)
            {
                _Task.Status = "complite_moderation";
                _Task.DateDone = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _TimeZoneinfo).ToString("dd.MM.yyyy HH:mm:ss");
                try
                {
                    //_context.UsersTasks.Update(_Task);

                    _context.SaveChanges();

                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var inf = ex.Entries;
                    foreach (var entry in ex.Entries)
                    {
                        if (entry.Entity is PchelaMapUserTasks)
                        {
                            var proposedValues = entry.CurrentValues;
                            var databaseValues = entry.GetDatabaseValues();
                            foreach (var property in proposedValues.Properties)
                            {
                                var proposedValue = proposedValues[property];
                                var databaseValue = databaseValues[property];
                            }
                            entry.OriginalValues.SetValues(databaseValues);
                        }
                        else
                        {
                            throw new NotSupportedException("??" + entry.Metadata.Name);
                        }
                    }
                }
            }
            return RedirectToAction("MyTakenTasks");
        }
        public PartialViewResult TaskRefuseModal(string id)
        {
            UserWithTasks ChoosenTaskID = new UserWithTasks
            {
                id = id
            };
            return PartialView(ChoosenTaskID);
        }
        //Отказаться от взятого задания
        public IActionResult RefuseFromTask(string id, string ReasonValue, string CustomReasonText)
        {
            string RefuseReasonText = "";
            if (!RefuseReasons.RefuseReasonsDict.Values.Contains(ReasonValue) && CustomReasonText != null)
            {
                RefuseReasonText = ReasonValue + " " + CustomReasonText;
            }
            else
            {
                RefuseReasonText = ReasonValue;
            }

            string CurrentUserID = _userManager.GetUserId(User);
            PchelaMapUserTasks _Task = _context.UsersTasks.SingleOrDefault(x => x.TaskID == id && x.UserID == CurrentUserID);
            if (_Task != null)
            {
                UsersRefusedTasks RefusedRecord = _context.UsersRefusedFromTasks.FirstOrDefault(x => x.TaskID == id && x.UserID == CurrentUserID);
                if (RefusedRecord != null)
                {
                    RefusedRecord.Reason = RefuseReasonText;
                    RefusedRecord.AdminComment = _Task.AdminComment;
                    RefusedRecord.DateRefused = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _TimeZoneinfo).ToString("dd.MM.yyyy HH:mm:ss");
                    RefusedRecord.RefuseCount++;
                }
                else
                {
                    RefusedRecord = new UsersRefusedTasks()
                    {
                        TaskID = _Task.TaskID,
                        UserID = _Task.UserID,
                        Reason = RefuseReasonText,
                        MediaFolder = _Task.MediaFolder,
                        AdminComment = _Task.AdminComment,
                        DateTaken = _Task.DateTaken,
                        DateRefused = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _TimeZoneinfo).ToString("dd.MM.yyyy HH:mm:ss"),
                        RefuseCount = 0
                    };
                    _context.UsersRefusedFromTasks.Add(RefusedRecord);
                }
                _context.UsersTasks.Remove(_Task);
                _context.Tasks.FirstOrDefault(x => x.id == id).Status = "active";
                _context.SaveChanges();
            }
            return RedirectToAction("MyTakenTasks");
        }
        //Закрыть созданное пользователем задание
        public async Task<IActionResult> CloseMyTask(string id)
        {
            var CurrentUserID = _userManager.GetUserId(User);
            PchelaMapTask _Task = _context.Tasks.SingleOrDefault(x => x.id == id && x.UserId == CurrentUserID);
            if (_Task != null)
            {
                _Task.ClosedDateUtc = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _TimeZoneinfo).ToString("dd.MM.yyyy HH:mm:ss");
                _Task.Status = "closed";
                _context.SaveChanges();
                var TaskReport = _context.UsersTasks.FirstOrDefault(x => x.TaskID == _Task.id && (x.Status == "in_progress" || x.Status == "moderating"));
                if (TaskReport != null)
                {
                    PchelaMapUser _userTaken = await _userManager.FindByIdAsync(TaskReport.UserID);
                    EmailService emailSender = new EmailService();
                    var userMail = _userTaken.Email;
                    string mailHeader = EmailService._onTaskCloseByCreator["header"];
                    string mailMessage = EmailService._onTaskCloseByCreator["bodyPrt1"] +
              _Task.Description.Substring(0, Math.Min(_Task.Description.Length, 50)) + "..." +
              EmailService._onTaskCloseByCreator["bodyPrt2"];
                    await emailSender.SendAsync(userMail, mailHeader, mailMessage);
                }

            }
            return RedirectToAction("MyTasks");
        }
        //Удаление своего задания (возможно только при непрохождении модерации)
        public async Task<IActionResult> DeleteTask(string id)
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
            return RedirectToAction("MyTasks");
        }

        //Отображение страницы "приватность"
        public IActionResult Privacy()
        {
            List<Privacy> textBlocks = new List<Privacy>();

            var webRoot = _env.WebRootPath;
            root = XElement.Load(System.IO.Path.Combine(webRoot, "xml/Pchela.xml"));

            textBlocks =
                 (from node in root.Element("Privacy").Elements("textBlock")
                  select new Privacy
                  {
                      header = node.Element("header").Value,
                      text = node.Element("text").Value.Split('/').ToList()
                  }).ToList();

            ViewData["MainHeader"] = root.Element("Privacy").Element("mainheader").Value;
            ViewData["Text"] = textBlocks;

            return View();
        }
        //Отображение страницы "политика"
        public IActionResult Policy()
        {
            List<Policy> textBlocks = new List<Policy>();

            var webRoot = _env.WebRootPath;
            root = XElement.Load(System.IO.Path.Combine(webRoot, "xml/Pchela.xml"));

            textBlocks =
                 (from node in root.Element("Policy").Elements("textBlock")
                  select new Policy
                  {
                      header = node.Element("header").Value,
                      text = sublistGenerator(node.Element("text").Value.Split('*').ToList())
                  }).ToList();

            ViewData["MainHeader"] = root.Element("Policy").Element("mainheader").Value;
            ViewData["Text"] = textBlocks;

            return View();
        }
        private List<List<string>> sublistGenerator(List<string> inputList)
        {
            List<List<string>> outList = new List<List<string>>();
            foreach (string val in inputList)
            {
                outList.Add(FromXMLLinkParser(val).Split('^').ToList());
            }
            return outList;
        }
        //Отображение страницы "о проекте"
        public IActionResult About()
        {
            List<About> textBlocks = new List<About>();

            var webRoot = _env.WebRootPath;
            root = XElement.Load(System.IO.Path.Combine(webRoot, "xml/Pchela.xml"));

            textBlocks =
                 (from node in root.Element("About").Elements("textBlock")
                  select new About
                  {
                      text = FromXMLLinkParser(node.Element("text").Value)
                  }).ToList();

            ViewData["MainHeader"] = root.Element("About").Element("mainheader").Value;
            ViewData["Text"] = textBlocks;

            return View();
        }
        private string FromXMLLinkParser(string InString)
        {

            while (InString.Contains("'aS'"))
            {
                InString = InString.Replace("'aS'", "<a class='btn InTextLink' href='");
                InString = InString.Replace("'aTs'", "'>");
                InString = InString.Replace("'aE'", ("</a>"));
            }
            return InString;

        }
        //Отображение страницы "инструкции"
        public IActionResult Instructions()
        {
            List<Privacy> textBlocks = new List<Privacy>();

            var webRoot = _env.WebRootPath;
            root = XElement.Load(System.IO.Path.Combine(webRoot, "xml/Pchela.xml"));

            textBlocks =
                 (from node in root.Element("Instructions").Elements("textBlock")
                  select new Privacy
                  {
                      header = node.Element("header").Value,
                      text = FromXMLLinkParser(node.Element("text").Value).Split('*').ToList(),
                      alert = FromXMLLinkParser(node.Element("alert").Value).Split('*').ToList()
                  }).ToList();

            ViewData["MainHeader"] = root.Element("Instructions").Element("mainheader").Value;
            ViewData["Disclaimer"] = FromXMLLinkParser(root.Element("Instructions").Element("Disclaimer").Value).Split('*').ToList();
            ViewData["Text"] = textBlocks;

            return View();
        }
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
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

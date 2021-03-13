using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Authentication;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Smtp;
using MimeKit;

namespace PchelaMap.Areas.Identity.Data
{

    public class EmailService
    {
        bool ServerOrLocal = true; //true if server (из-за разницы формата datetime на сервере и на компе разработчика
        string SenderMail = "YourMail@mail.ru";

        private int portNumber = 25;
        private string ServerName = "your smtp server name";
        private string ServPassw ;
        private string ServLogin ;

        private string logFileName = "imap.log";
        private string logFilePath = "mailLogs/imap.log";

        public async Task SendAsync(string to, string subject, string MailText, int isHtml = 0)
        {


            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(SenderMail);
            email.Sender.Name = "Админ pchl-map";
            email.From.Add(new MailboxAddress("Админ pchlmap", "YourMail@mail.ru"));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            if (isHtml==1)
            {
                email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = MailText };
            }
            else
            {
                email.Body = new TextPart(MimeKit.Text.TextFormat.Plain) { Text = MailText };
            }
            

            //FileInfo _logInfo = new FileInfo(logFilePath);
            //if (_logInfo.Length > 1000000)
            //{
            //    string LogNewName = "mailLogs/"+DateTime.UtcNow.ToString("dd-MM-yyyy_HH-mm")+logFileName;
            //    File.Move(logFilePath, LogNewName);
            //}
            using (SmtpClient smtp = new SmtpClient(new ProtocolLogger(logFilePath)))
            {
                if (ServerOrLocal)
                {
                    await smtp.ConnectAsync(ServerName, portNumber, MailKit.Security.SecureSocketOptions.None);
                }
                else
                {
                    await smtp.ConnectAsync(ServerName, portNumber, MailKit.Security.SecureSocketOptions.Auto);
                    await smtp.AuthenticateAsync(ServLogin, ServPassw);
                }
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
        }
        public async Task SendWithAttachmentsAsync(string to, string subject, string MailText, string path)
        {
            
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(SenderMail);
            email.Sender.Name = "Админ pchl-map";
            email.From.Add(new MailboxAddress("Админ pchl-map", "YourMail@mail.ru"));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            TextPart _body = new TextPart(MimeKit.Text.TextFormat.Plain) { Text = MailText };
            System.IO.FileStream fileStream = new System.IO.FileStream(path, System.IO.FileMode.Open);
            MimePart attachment = new MimePart("application", "vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                Content = new MimeContent(fileStream, ContentEncoding.Default),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = System.IO.Path.GetFileName(path)
            };

            Multipart _multipart = new Multipart("multipartMix");
            _multipart.Add(_body);
            _multipart.Add(attachment);
            email.Body = _multipart;

            
            //FileInfo _logInfo = new FileInfo(logFilePath);
            //if (_logInfo.Length > 1000000)
            //{
            //    string LogNewName = "mailLogs/" + DateTime.UtcNow.ToString("dd -MM-yyyy_HH-mm") + logFileName;
            //    File.Move(logFilePath, LogNewName);
            //}
            using (var smtp = new SmtpClient(new ProtocolLogger(logFilePath)))
            {
                if (ServerOrLocal)
                {
                    await smtp.ConnectAsync(ServerName, portNumber, MailKit.Security.SecureSocketOptions.None);
                }
                else
                {
                    await smtp.ConnectAsync(ServerName, portNumber, MailKit.Security.SecureSocketOptions.Auto);
                    await smtp.AuthenticateAsync(ServLogin, ServPassw);
                }

                await smtp.SendAsync(email);

                foreach (var part in email.BodyParts)
                {
                    await (part as MimePart).Content.Stream.DisposeAsync();
                }
                await smtp.DisconnectAsync(true);

            }

        }

        public static readonly Dictionary<string, string> _onRegistration = new Dictionary<string, string>()
        {
             { "header","Вы успешно зарегистрировались" },
             { "body" ,"Вы успешно зарегистрировались на карте pchlmap.ru . " +
                "Теперь Вы можете создать задание или взять задние."}
        };
        public static readonly Dictionary<string, string> _onTaskCreation = new Dictionary<string, string>()
        {
             { "header","Задание добавлено и ожидает модерации." },
              { "bodyPrt1","Задание '" },
             { "bodyPrt2" ,"' успешно добавлено и ожидает модерации. " +
                "По окончании модерации Вы увидите его на карте."}
        };
        public static readonly Dictionary<string, string> _onTaskModerationStop = new Dictionary<string, string>()
        {
             { "header","Задание не прошло модерацию." },
              { "bodyPrt1","Задание '" },
             { "bodyPrt2" ,"' не прошло модерацию. " +
                "Причина: "}
        };
        public static readonly Dictionary<string, string> _onTaskModerationActive = new Dictionary<string, string>()
        {
             { "header","Задание прошло модерацию." },
              { "bodyPrt1","Задание '" },
             { "bodyPrt2" ,"'прошло модерацию и отображается на карте. " +
                "Ожидайте взятия задания волонтёром."}
        };
        public static readonly Dictionary<string, string> _onTaskTaken = new Dictionary<string, string>()
        {
             { "header","Вы зяли задание" },
              { "bodyPrt1","Вы взяли задание '" },
             { "bodyPrt2" ,"'. Обязательно снимите видео или фото отчёт о выполнении. " +
                "Срок выполнения задания 3 дня."}
        };
        public static readonly Dictionary<string, string> _onTaskUrgentTaken = new Dictionary<string, string>()
        {
             { "header","Вы зяли срочное задание" },
              { "bodyPrt1","Вы взяли срочное задание '" },
             { "bodyPrt2" ,"'. Обязательно снимите видео или фото отчёт о выполнении. " +
                "Срок выполнения задания 24 часа."}
        };
        public static readonly Dictionary<string, string> _onTaskTakenCreatorMsg = new Dictionary<string, string>()
        {
             { "header","Ваше задание взято" },
              { "bodyPrt1","Волонтёр взял Ваше задание '" },
             { "bodyPrt2" , "' . Ожидайте выполнения."}
        };
        public static readonly Dictionary<string, string> _onReportClose = new Dictionary<string, string>()
        {
             { "header","Задание закрыто администратором." },
             { "bodyPrt1" ,"Возможно истек максимальный срок ожидания выполнения задания '"},
            { "bodyPrt2" , "'. "}
        };
        public static readonly Dictionary<string, string> _onTaskDone = new Dictionary<string, string>()
        {
             { "header","Вы выполнили задание." },
              { "bodyPrt1" ,"Вы выполнили задание '"},
             { "bodyPrt2" ,"' и Вам начислено 3 балла. " +
                ""}
        };
        public static readonly Dictionary<string, string> _onTaskUrgentDone = new Dictionary<string, string>()
        {
             { "header","Вы выполнили срочное задание." },
              { "bodyPrt1" ,"Вы выполнили срочное задание '"},
             { "bodyPrt2" ,"' и Вам начислено 5 баллов. " +
                ""}
        };
        public static readonly Dictionary<string, string> _onReportModerationStop = new Dictionary<string, string>()
        {
             { "header","Отчёт не прошёл модерацию." },
             { "bodyPrt1" ,"Отчёт о выполнении задания '"},
             { "bodyPrt2" ,"' не прошёл модерацию. " +
                "Причина: "}
        };
        public static readonly Dictionary<string, string> _onTaskCloseByCreator = new Dictionary<string, string>()
        {
             { "header","Задание закрыто по инициативе создателя задания." },
             { "bodyPrt1" ,"Задание '"},
             { "bodyPrt2" ,"' закрыто по инициативе создателя задания." +
                ""}
        };
    }
}

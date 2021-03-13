# WebMap
An interactive map of volunteers and volunteer tasks. Asp.net core project with the connection of Yandex Maps API and ability to register users through social networks.
The user registers via Facebook or Vkontakte.
There are four user roles - administrator, user, moderator, ban.
The user fills out the profile, marks himself on the map and can take tasks. The taken, created and completed tasks are displayed on the user's tasks page.
User can refuse the taken task. When the task is completed, the user generates a report on the completion of the task - attaches a photo / video or pdf file and sends the report for moderation.
The task is considered completed when the administrator approves the task report.
The user has points that are added when completing task and deducted when they refuse from taken task.
Tasks can only be created by a user in the administrator role. Users can create tasks, but this function disabled.
The administrator sees the "Database" tab on the main page - this is the administrative part, it displays a database with the ability to change. The administrative part allows to view and reject or approve reports of volunteers on completed tasks, view and edit tasks, edit user roles, download a database (in xlsx format), batch download and deletion of report files, delete mail logs from the server, download a .csv file from with promotional codes from SberMarket.

For project normal work add:
1) Your smtp server name in PchelaMap/Areas/Identity/Data/EmailService.cs 19str
2) Your Facebook AppId and AppSecret in PchelaMap/Startup.cs 80,81 str
3) Your VKontakte ClientId and ClientSecret in PchelaMap/Startup.cs 86,87 str

Home page must look like this:
<img src="MainPage.png"/>
There are not much comments in the code and they are in Russian. Will be corrected soon.

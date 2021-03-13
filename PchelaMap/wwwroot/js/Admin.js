function ShowOnlyADMINuser()
{
    $('.AdminUserRow').each(function (index) {
        $(this).show();
    });
    $('.UserUserRow').each(function (index) {
        $(this).hide();
    });
    $('.ModerUserRow').each(function (index) {
        $(this).hide();
    });
    $('.BanUserRow').each(function (index) {
        $(this).hide();
    });
}
function ShowOnlyMODERuser() {
    $('.AdminUserRow').each(function (index) {
        $(this).hide();
    });
    $('.UserUserRow').each(function (index) {
        $(this).hide();
    });
    $('.ModerUserRow').each(function (index) {
        $(this).show();
    });
    $('.BanUserRow').each(function (index) {
        $(this).hide();
    });
}
function ShowOnlyUSERuser() {
    $('.AdminUserRow').each(function (index) {
        $(this).hide();
    });
    $('.UserUserRow').each(function (index) {
        $(this).show();
    });
    $('.ModerUserRow').each(function (index) {
        $(this).hide();
    });
    $('.BanUserRow').each(function (index) {
        $(this).hide();
    });
}
function ShowOnlyBANuser() {
    $('.AdminUserRow').each(function (index) {
        $(this).hide();
    });
    $('.UserUserRow').each(function (index) {
        $(this).hide();
    });
    $('.ModerUserRow').each(function (index) {
        $(this).hide();
    });
    $('.BanUserRow').each(function (index) {
        $(this).show();
    });
}
function ShowALLuser() {
    $('.AdminUserRow').each(function (index) {
        $(this).show();
    });
    $('.UserUserRow').each(function (index) {
        $(this).show();
    });
    $('.ModerUserRow').each(function (index) {
        $(this).show();
    });
    $('.BanUserRow').each(function (index) {
        $(this).show();
    });
}
$(function () {
    var resultDiv = $('#SelectedIds');
    var resultJSON = [];
    if ($('#selectable').length != 0) {
        $('#selectable').selectable(
            {
                filter: 'tr',
                selected: function (event, ui) {
                    var idText = $(ui.selected).find(':hidden').val();
                    if (!resultJSON.includes(idText)) {
                        resultJSON.push(idText);
                        resultDiv.val(JSON.stringify(resultJSON));
                    }
                    $('#RecordControls').show();
                },
                unselected: function (event, ui) {
                    var idText = $(ui.unselected).find(':hidden').val();
                    var index = resultJSON.indexOf(idText);
                    resultJSON.splice(index, 1);
                    var JSONstring = JSON.stringify(resultJSON);
                    resultDiv.val(JSONstring);
                    if (resultJSON.length == 0) {
                        $('#RecordControls').hide();
                    }
                }
            }
        );
    }
});

function confirmDelete() {
    if (confirm("Удалить выбранных пользователей?")) {
        return true;
    }
    else {
        return false;
    }
}

function confirmAvaReset()
{
    if (confirm("Очистить аватарки выбранных пользователей?")) {
        return true;
    }
    else {
        return false;
    }
}

function confirmRoleChange() {
    if (confirm("Изменить роли выбранных пользователей?")) {
        return true;
    }
    else {
        return false;
    }
}
function confirmDeleteTask() {
    if (confirm("Удалить задание?")) {
        return true;
    }
    else {
        return false;
    }
}
function OpenUsersSearchModalClick() {

    $.get('/Admin/_UsersSearch', function (data) {
        $('#SearchModContent').html(data);
        $('#SearchModDialog').modal('show');
    });
}
function UsersSearchConfirm() {
    var NameF = $('#UserName').val();
    var PhoneF = $('#UserPhoneNumber').val();
    var MailF = $('#UserMail').val();
    var AdressF = $('#UserAdress').val();
    if (NameF == "" && PhoneF == "" && MailF == "" && AdressF == "") {
        alert("Введите значение хотя бы в одно из полей.");
        return false;
    }
}
function OpenTasksSearchModalClick() {

    $.get('/Admin/_TasksSearch', function (data) {
        $('#SearchModContent').html(data);
        $('#SearchModDialog').modal('show');
    });
}
function TasksSearchConfirm() {
    var DescripF = $('#TaskDescription').val();
    var NameF = $('#UserName').val();
    var PhoneF = $('#UserPhoneNumber').val();
    var MailF = $('#UserMail').val();
    var AdressF = $('#UserAdress').val();
    if (NameF == "" && PhoneF == "" && MailF == "" && AdressF == "" && DescripF == "") {
        alert("Введите значение хотя бы в одно из полей.");
        return false;
    }
}
function OpenReportFilesModalClick(_folder) {
    $.get('/Admin/_ReportFilesPartialView', { _reportFolder: _folder }, function (data) {
        $('#ReportsModContent').html(data);
        $('#ReportsModDialog').modal('show');
    });
}
function confirmDoneReportsFilesDelete() {
    if (confirm("Удалить файлы отчётов выполненных заданий?")) {
        //window.location.href = 'DeleteDoneReportFiles/';
        return true;
    }
    else {
        return false;
    }
}
function confirmMailLogsFilesDelete() {
    if (confirm("Удалить лог файлы почтовых отправлений?")) {
        return true;
    }
    else {
        return false;
    }
}
function ChangeStatus(taskId, UserTakenId, Status) {
    var textboxId = "MessageForUser" + UserTakenId;
    var test = $('textarea#' + textboxId + '').length;
    var AdminMessageText = $('textarea#' + textboxId + '').val();
    window.location.href = 'EditStatus/?id=' + taskId + '&userId=' + UserTakenId + '&status=' + Status + '&MessageForUser=' + AdminMessageText;
}
function MakeTaskActiveAgain(taskId, UserTakenId, Status, MakeActiveAgain) {
    var textboxId = "MessageForUser" + UserTakenId;
    var test = $('textarea#' + textboxId + '').length;
    var AdminMessageText = $('textarea#' + textboxId + '').val();
    window.location.href = 'EditStatus/?id=' + taskId + '&userId=' + UserTakenId + '&status=' + Status + '&MessageForUser=' + AdminMessageText + '&MakeActiveAgain=' + MakeActiveAgain;
}
var True = true, False = false;
function ChangeGlobalStatus(taskId, UserCreatedId, Status, SelectedTasksOrAll, OwnTasksOrTaken, UserTakenId) {
    var textboxId = "MessageForUser" + UserCreatedId;

    var AdminMessageText = $('textarea#' + textboxId + '').val();
    window.location.href = '/Admin/EditGlobalStatus/?id=' + taskId + '&status=' + Status + '&MessageForUser=' + AdminMessageText + '&userId=' + UserCreatedId + '&UserTakenId=' + UserTakenId + '&SelectedTasksOrAll=' + SelectedTasksOrAll + '&OwnTasksOrTaken=' + OwnTasksOrTaken;
}
function ChangeModerateGlobalStatus(taskId, UserCreatedId, Status) {
    var textboxId = "MessageForUser" + UserCreatedId;
    var test = $('textarea#' + textboxId + '').length;
    var AdminMessageText = $('textarea#' + textboxId + '').val();
    window.location.href = 'ChangeModerateGlobalStatus/?id=' + taskId + '&status=' + Status + '&MessageForUser=' + AdminMessageText;
}
function confirmPromoDelete() {
    if (confirm("Удалить промокод?")) {
        return true;
    }
    else {
        return false;
    }
}
function ChangePromoStatus(code, status) {
    window.location.href = '/Admin/ChangePromoStatus/?code=' + code + '&status=' + status;
}
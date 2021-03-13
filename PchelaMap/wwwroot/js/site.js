// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function NYTakeTaskClick(taskId) {
    $(".LoaderContainer").show();
    window.location.href = '/Home/TakeTask/?id=' + taskId;
}
function NYLoaderContainerClick(event) {
    //event.stopPropagotion();
    event.preventDefault();
}
function SberDirectLink(event) {

    //event.stopPropagation();
    event.preventDefault();
    window.location.href = 'https://sbermarket.ru/';
}
function CopyPromoToClipboard() {
    var CopyText = document.getElementById('Promo');
    CopyText.select();
    CopyText.setSelectionRange(0, 50);
    document.execCommand("copy");

    var CopyInfo = document.getElementById('PromoInfoString');
    CopyInfo.innerText = 'промокод скопирован';
    CopyInfo.style.color = 'red';
}
function SberMyTasksDirectLink(event) {
   
    //event.stopPropagation();
    event.preventDefault();
    window.location.href = 'https://sbermarket.ru/';
}
function SberMyTasksClick(event, taskId) {
    $.get('/Home/SberModal', { id: taskId, mytasks: 1 }, function (data) {
        $('#TaskModContent').html(data);
        $('#TaskModDialog').modal('show');
    });
    //event.stopPropagation();
    event.preventDefault();
}
function TaskImageClick() {
    $('#file').click();
}
function ZoomPicture(el) {
    $(el).toggleClass("TaskImage TaskImageZoomed");
}
function ZoomAvatar(el) {
    $(el).toggleClass("UserAvatar UserAvatarZoomed");
}
function ZoomPdf(id) {
    $('#'+id+'').toggleClass("TaskImage TaskImageZoomed");
}
function OpenTextBox(reason) {
    var _reason = reason;
    $('#alert').hide();
    $("#ReasonValue").val(_reason);
    $("#CustomReasonTextField").show();
    $("#CustomReasonTextField").prop('required', true);
}
function HideTextBox(reason) {
    var _reason = reason;
    $('#alert').hide();
    $("#ReasonValue").val(_reason);
    $("#CustomReasonTextField").hide();
    $("#CustomReasonTextField").prop('required', null);
}
$("input[type=submit]").click(function () {
    var anyChecked = false;
    $('.ReasonLabel').each(function () {
        var ClassName = $(this).attr('class');
        if (ClassName.includes("active")) {
            anyChecked = true;
        }
    });
    if (!anyChecked) {
        $('#alert').show();
    }
});


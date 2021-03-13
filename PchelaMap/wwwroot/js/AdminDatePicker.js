$(function () {
    $("#TR1").daterangepicker({
        opens: 'left',
        startDate: moment().subtract(30, 'days'),
        endDate: moment(),
        minYear: 2020,
        ranges:
        {
            'Сегодня': [moment(), moment()],
            'Последние 7 дней': [moment().subtract(6, 'days'), moment()],
            'Последние 30 дней': [moment().subtract(29, 'days'), moment()],
            'Этот месяц': [moment().startOf('month'), moment().endOf('month')],
            'Прошлый месяц': [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')],
        },
        locale:
        {
            "customRangeLabel": "Произвольный диапазон",
            "applyLabel": "Применить",
            "cancelLabel": "Отмена",
            "daysOfWeek":
                [
                    "Вс",
                    "Пн",
                    "Вт",
                    "Ср",
                    "Чт",
                    "Пт",
                    "Сб"
                ],
            "monthNames":
                [
                    "Январь",
                    "Февраль",
                    "Март",
                    "Апрель",
                    "Май",
                    "Июнь",
                    "Июль",
                    "Август",
                    "Сентабрь",
                    "Октябрь",
                    "Ноябрь",
                    "Декабрь"
                ]
        }
    });
    $("#TR2").daterangepicker({
        opens: 'left',
        drops: 'up',
        startDate: moment().subtract(30, 'days'),
        endDate: moment(),
        minYear: 2020,
        ranges:
        {
            'Сегодня': [moment(), moment()],
            'Последние 7 дней': [moment().subtract(6, 'days'), moment()],
            'Последние 30 дней': [moment().subtract(29, 'days'), moment()],
            'Этот месяц': [moment().startOf('month'), moment().endOf('month')],
            'Прошлый месяц': [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')],
        },
        locale:
        {
            "customRangeLabel": "Произвольный диапазон",
            "applyLabel": "Применить",
            "cancelLabel": "Отмена",
            "daysOfWeek":
                [
                    "Вс",
                    "Пн",
                    "Вт",
                    "Ср",
                    "Чт",
                    "Пт",
                    "Сб"
                ],
            "monthNames":
                [
                    "Январь",
                    "Февраль",
                    "Март",
                    "Апрель",
                    "Май",
                    "Июнь",
                    "Июль",
                    "Август",
                    "Сентабрь",
                    "Октябрь",
                    "Ноябрь",
                    "Декабрь"
                ]
        }
    });
});
$('#TR1').on('apply.daterangepicker', function (ev, picker) {

    //server variant:
    var startdate = picker.startDate.format('DD.MM.YYYY');
    //local variant:
    //var startdate = picker.startDate.format('MM/DD/YYYY');
    $('#UsersTimeRangeStart').val(startdate);
    //server variant:
    var enddate = picker.endDate.format('DD.MM.YYYY');
    //local variant:
    // var enddate = picker.endDate.format('MM/DD/YYYY');
    $('#UsersTimeRangeEnd').val(enddate);
    var rangeString = 'с ' + startdate + ' по ' + enddate + ' ';
    $('#TR1text').text(rangeString)
});
$('#TR2').on('apply.daterangepicker', function (ev, picker) {
    //server variant:
    //var startdate = picker.startDate.format('DD.MM.YYYY');
    //local variant:
    var startdate = picker.startDate.format('DD.MM.YYYY');
    $('#TasksTimeRangeStart').val(startdate);
    //server variant:
    //var enddate = picker.endDate.format('DD.MM.YYYY');
    //local variant:
    var enddate = picker.endDate.format('DD.MM.YYYY');
    $('#TasksTimeRangeEnd').val(enddate);
    var rangeString = 'с ' + startdate + ' по ' + enddate + ' ';
    $('#TR2text').text(rangeString)
});

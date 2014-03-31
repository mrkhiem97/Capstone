
var status = 0;
var tempDate;
var datetimeController = {
    initializeDatePicker: function (datefieldId, minDate, maxDate, defaultDate) {
        var yearRange = minDate.getFullYear() + ":" + maxDate.getFullYear();
        datefield = "#" + datefieldId;
        $(datefield).datepicker({
            changeMonth: true,
            changeYear: true,
            formatDate: "mm/dd/yy",
            yearRange: yearRange,
            inline: true,
            minDate: minDate,
            maxDate: maxDate
        });
        $(datefield).datepicker("setDate", defaultDate);
    },

    selectDatePicker: function (datefieldId, minDate, maxDate, defaultDate) {
        var yearRange = minDate.getFullYear() + ":" + maxDate.getFullYear();
        datefield = "#" + datefieldId;

        $(datefield).datepicker({

            async: false,
            changeMonth: true,
            changeYear: true,
            formatDate: "mm/dd/yy",
            yearRange: yearRange,
            inline: true,
            minDate: minDate,
            maxDate: maxDate,

            beforeShow: function () {
                $(".ui-datepicker").css('font-size', 14)
            },

            onSelect: function (dateText) {
                //alert(status + " : " + tempDate)
                ajaxGetLocationListByDate(dateText);

                var date1 = $.datepicker.parseDate($.datepicker._defaults.dateFormat, $("#date-picker1").val());
                var date2 = $.datepicker.parseDate($.datepicker._defaults.dateFormat, $("#date-picker2").val());

                // If status == 0  mean first select
                if (status == 0) {
                    if (tempDate == dateText) {
                        status = 1;
                        $("#date-picker1").val(dateText);
                        
                    }
                    $("#date1").val(dateText);
                    $("#date2").val("");
                    tempDate = dateText;
                } else
                    // If status == 2 mean already selected range. Reset status to select new range
                    if (status == 2) {
                        status = 0;
                        $("#date-picker1").val("");
                        $("#date1").val(dateText);
                        $("#date-picker2").val("");
                        $("#date2").val("");
                    } else
                        // Mean that the 2nd time click on a day -> 
                        if (status == 1) {
                            if (!date1) {
                                $("#date-picker1").val(dateText);
                                $("#date1").val(dateText);
                            } else
                                if (tempDate != dateText) {
                                    $("#date-picker2").val(dateText);
                                    $("#date2").val(dateText);
                                    status = 2;

                                    var fromDate = $("#date1").val();
                                    var toDate = $("#date2").val();

                                    locationFilter(fromDate, toDate);
                                }

                        }

                tempDate = dateText;
            },
            beforeShowDay: function (date) {
                var date1 = $.datepicker.parseDate($.datepicker._defaults.dateFormat, $("#date-picker1").val());
                var date2 = $.datepicker.parseDate($.datepicker._defaults.dateFormat, $("#date-picker2").val());

                var dmy = ((date.getMonth() + 1) < 10 ? "0" + (date.getMonth() + 1) : (date.getMonth() + 1)) + "/" + (date.getDate() < 10 ? "0" + date.getDate() : date.getDate()) + "/" + date.getFullYear();
                var a = $.inArray(dmy, SelectedDates);
                if ($.inArray(dmy, SelectedDates) >= 0) {
                    return [true, "Highlighted", "You have event this day"];
                } else {
                    return [true, (date2 && date <= date1 && date >= date2) || (date >= date1 && date <= date2) || (date1 && date1.getTime() == date.getTime()) ? "dp-highlight" : ""];
                    status = 0;
                }



            }
        });

        $(datefield).datepicker("setDate", defaultDate);
    }
}
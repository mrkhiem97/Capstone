

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
                ajaxGetLocationListByDate(dateText);
            },
            beforeShowDay: function (date) {

                for (var i = 0; i < locationList.length; i++) {
                    if (SelectedDates[i]) {
                        return [true, "Highlighted", "You have event this day"];
                    } else {
                        return [true, '', 'No event this day'];
                    }
                }


            }
        });

        $(datefield).datepicker("setDate", defaultDate);
    }
}
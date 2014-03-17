var datetimeController = {
    initializeDatePicker: function (datefieldId, minDate, maxDate, defaultDate) {
        var yearRange = minDate.getFullYear()+ ":" + maxDate.getFullYear();
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
    }
}
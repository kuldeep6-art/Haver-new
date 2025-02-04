
function refreshDDL(ddl_ID, URI, showNoDataMsg, noDataMsg, addDefault, defaultText, fadeOutIn) {
    var theDDL = $("#" + ddl_ID);
    $(function () {
        $.getJSON(URI, function (data) {
            if (data !== null && !jQuery.isEmptyObject(data)) {
                theDDL.empty();
                if (addDefault) {
                    if (defaultText == null || jQuery.isEmptyObject(defaultText)) {
                        defaultText = 'Select'
                    };
                    theDDL.append($('<option/>', {
                        value: "",
                        text: defaultText
                    }));
                }
                $.each(data, function (index, item) {
                    theDDL.append($('<option/>', {
                        value: item.value,
                        text: item.text,
                        selected: item.selected
                    }));
                });
                theDDL.trigger("chosen:updated");
            } else {
                if (showNoDataMsg) {
                    theDDL.empty();
                    if (noDataMsg == null || jQuery.isEmptyObject(noDataMsg)) {
                        noDataMsg = 'No Matching Data'
                    };
                    theDDL.append($('<option/>', {
                        value: null,
                        text: noDataMsg
                    }));
                    theDDL.trigger("chosen:updated");
                }
            }
        });
    });
    if (fadeOutIn) {
        theDDL.fadeToggle(400, function () {
            theDDL.fadeToggle(400);
        });
    }
    return;
}


$(document).ready(function () {
    $("#btnloadreport").click(function () {
        ReportManager.loadreport();
    });
});

var ReportManager = {
    loadreport: function () {
        var jsonParm = "";
        var serviceUrl = "../Report/GenerateReport";
    },

    GetReport: function (serviceUrl, jsonParm, errorCallback) {
        jQuery.ajax({
            url: serviceUrl,
            async: false,
            type: "POST",
            data: "{" + jsonParm + "}",
            contentType: "application/json; charset-utf-8",
            success: function () {
                window.open('../Report/')
            }
        })
    }
}
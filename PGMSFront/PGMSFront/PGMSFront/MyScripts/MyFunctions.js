
/* ///////////////// Alert /////////////////////////////////*/
function alertError(msg) {
    $.confirm({
        title: 'Error!',
        content: msg,
        icon: 'fa fa-warning',
        closeIcon: false,
        animation: 'scale',
        type: 'red',
        buttons: {
            deleteUser: {
                text: 'Ok',
                action: function () {
                    //$.alert('Deleted the user!');
                }
            }
        }
    });
}

function alert(msg) {
    $.confirm({
        title: 'Information!',
        content: msg,
        icon: 'fa fa-check',
        closeIcon: true,
        animation: 'zoom',
        animationBounce: 2,
        closeAnimation: 'scale',
        type: 'green',
        autoClose: 'MyFunction|10000',
        escapeKey: 'MyFunction',
        buttons: {
            MyFunction: {
                text: 'Ok',
                action: function () {
                    //$.alert('Deleted the user!');
                }
            }
        }
    });
}

/////////////////////// Loader Show & Hide Start ////////////////////////////////
function LoaderShow() {
    waitingDialog.show('', {
        dialogSize: 'sm', progressType: 'warning'
    });
}

function LoaderHide() {
    waitingDialog.hide();
}
     /////////////////////// Loader Show & Hide End ////////////////////////////////

/* Check Numeric*/
function CheckIsNumeric(e, tx) {
    var AsciiCode = e.keyCode ? e.keyCode : e.which ? e.which : e.charCode;
    if ((AsciiCode < 47 && AsciiCode != 8 && AsciiCode != 9) || (AsciiCode > 57 || (AsciiCode == 47))) {

        // alert('Please enter only numbers !');
        return false;
    }

}

/* Check Decimal*/
function CheckIsDecimal(e, tx) {
    var AsciiCode = e.keyCode ? e.keyCode : e.which ? e.which : e.charCode;
    // alert(AsciiCode);
    if ((AsciiCode < 46 && AsciiCode != 8 && AsciiCode != 9) || (AsciiCode > 57) || (AsciiCode == 47)) {
        //Only Numbers Allowed
        return false;
    }
    var num = tx.value;
    var len = num.length;
    var indx = -1;
    indx = num.indexOf('.');
    if (indx != -1) {
        if ((AsciiCode == 46)) {
            // (.) Not Allowed More than One
            return false;
        }
        var dgt = num.substr(indx, len);
        var count = dgt.length;
        //alert (count);
        if (count > 2 && AsciiCode != 8 && AsciiCode != 9) {
            //Only 2 Decimal Digits Allowed
            // return false;

        }
    }
}

/* Validate Decimal */
function ValidateDecimal(tx) {
    var txval = $(tx).val().trim();
    try {
        if (!parseFloat(txval)) {
            txval = 0.00;
        }
    } catch (e) {
        txval = 0.00;
    }
    $(tx).val(parseFloat(txval).toFixed(2));
}

function ValidateEmail(tx) {
    var testEmail = /^[A-Z0-9._%+-]+@([A-Z0-9-]+\.)+[A-Z]{2,4}$/i;
    var txval = $(tx).val().trim();
    try {
        if (testEmail.test(txval))
            return true;
        else {
            $(tx).val("");
            alert("Input text for Email is not valid!");
            $(tx).focus();
            return false;
        }
    } catch (e) {
        $(tx).val("");
        alert("Input text for Email is not valid!");
        $(tx).focus();
        return false;
    }
}

function CheckIsAlpha(e, tx) {
    var AsciiCode = e.keyCode ? e.keyCode : e.which ? e.which : e.charCode;
    if (AsciiCode == 8 || AsciiCode == 9 || AsciiCode == 32 || AsciiCode == 127 || (AsciiCode >= 65 && AsciiCode <= 90) || (AsciiCode >= 97 && AsciiCode <= 122)) {
        //Only for Alphbets
        return true;
    }
    else {
        return false;
    }

}

function CheckIsAlphaNumeric(e, tx) {
    var AsciiCode = e.keyCode ? e.keyCode : e.which ? e.which : e.charCode;
    if (AsciiCode == 8 || AsciiCode == 9 || AsciiCode == 32 || AsciiCode == 46 || AsciiCode == 127 || (AsciiCode >= 48 && AsciiCode <= 57) || (AsciiCode >= 65 && AsciiCode <= 90) || (AsciiCode >= 97 && AsciiCode <= 122)) {
        //Only for Alphanumeric
        return true;
    }
    else {
        return false;
    }
}

function CheckIsAlphaNumericRestrictSpacePercent(e, tx) {
    var AsciiCode = e.keyCode ? e.keyCode : e.which ? e.which : e.charCode;
    if ((AsciiCode == 8 || AsciiCode == 9 || AsciiCode == 46 || AsciiCode == 127 || (AsciiCode >= 48 && AsciiCode <= 57) || (AsciiCode >= 65 && AsciiCode <= 90) || (AsciiCode >= 97 && AsciiCode <= 122)) && AsciiCode != 37) {
        //Only for Alphanumeric
        return true;
    }
    else {
        return false;
    }
}

jQuery.fn.IsEmailOnly =
    function () {
        return this.each(function () {

            $(this).change(function (e) {

                var mailformat = /^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$/;

                if (mailformat.test(this.value) == false) {
                    alert('Invalid Email Format');
                    $(this).val("");
                    this.focus();
                }

            });
        });
    };

function PrintElem(elem) {
    try {
        var data = $("#" + elem).html();
        var printWindow = window.open(null, 'Print_Page', 'height=600px,width=982px,align=center');//window.open('', '', 'height=400,width=740');
        printWindow.document.write('<html><head></head>');
        printWindow.document.write("<body style='width:740px'>");
        printWindow.document.write(data);
        printWindow.document.write('</body></html>');
        printWindow.document.close();
        printWindow.print();
        printWindow.close();
    }
    catch (e) {
    }
}

function GetDateMMDDYYYY(Date) {
    try {
        //Get Day, Month & Year from Input Date
        var dd = (Date.indexOf('/') > 0 ? Date.substr(0, Date.indexOf('/')) : (Date.indexOf('-') > 0 ? Date.substr(0, Date.indexOf('-')) : Date.substr(0, 2)));
        Date = (Date.indexOf('/') > 0 ? Date.substr(Date.indexOf('/') + 1, Date.length) : (Date.indexOf('-') > 0 ? Date.substr(Date.indexOf('-') + 1, Date.length) : Date.substr(2, Date.length)));
        var mm = (Date.indexOf('/') > 0 ? Date.substr(0, Date.indexOf('/')) : (Date.indexOf('-') > 0 ? Date.substr(0, Date.indexOf('-')) : Date.substr(0, 2)));
        var yyyy = (Date.indexOf('/') > 0 ? Date.substr(Date.indexOf('/') + 1, Date.length) : (Date.indexOf('-') > 0 ? Date.substr(Date.indexOf('-') + 1, Date.length) : Date.substr(2, Date.length)));

        return mm + "/" + dd + "/" + yyyy;
    }
    catch (e) {
    }
}

function ValidateDateDDMMYYY(Date) {
    try {
        //Get Day, Month & Year from Input Date
        var dd = (Date.indexOf('/') > 0 ? Date.substr(0, Date.indexOf('/')) : (Date.indexOf('-') > 0 ? Date.substr(0, Date.indexOf('-')) : Date.substr(0, 2)));
        Date = (Date.indexOf('/') > 0 ? Date.substr(Date.indexOf('/') + 1, Date.length) : (Date.indexOf('-') > 0 ? Date.substr(Date.indexOf('-') + 1, Date.length) : Date.substr(2, Date.length)));
        var mm = (Date.indexOf('/') > 0 ? Date.substr(0, Date.indexOf('/')) : (Date.indexOf('-') > 0 ? Date.substr(0, Date.indexOf('-')) : Date.substr(0, 2)));
        var yyyy = (Date.indexOf('/') > 0 ? Date.substr(Date.indexOf('/') + 1, Date.length) : (Date.indexOf('-') > 0 ? Date.substr(Date.indexOf('-') + 1, Date.length) : Date.substr(2, Date.length)));


        //Check Day of Input Date
        if (dd < 1 || dd > 31 || dd == '__' || dd == '') {
            alert('Date is Invalid Format! Please Enter Date in dd/mm/yyyy');
            return false;
        }

        //Check Month of Input Date
        if (mm < 1 || mm > 12 || mm == '__' || mm == '') {
            alert('Month is Invalid Format! Please Enter Date in dd/mm/yyyy');
            return false;
        }
        if (yyyy < 1900 || yyyy == '____' || yyyy == '') {
            alert('Year Between 1900 and Current Year');
            return false;
        }
        return true;
    }
    catch (e) {
        return false;
    }
}

////////////////////////////// Null Val Handle ///////////////////////////////////
function NullHandle(val) {
    var rVal = '';
    if (val != null && val != undefined) {
        rVal = val;
    }
    return rVal;
}
function NullDecimalHandleSpace(val) {
    var rVal = '';
    if (val != null && val != undefined && val != "0" && val != "0.00") {
        rVal = parseFloat(val).toFixed(2);
    }
    return rVal;
}
function NullZeroHandle(val) {
    var rVal = '';
    if (val != null && val != undefined && val != "0") {
        rVal = val;
    }
    return rVal;
}
function NullHandleFloat(val) {
    var rVal = '0.00';
    if (val != null && val != undefined) {
        rVal = parseFloat(val).toFixed(2);
    }
    return rVal;
}
function NullDecimalHandle(val) {
    var rVal = '&nbsp;';
    if (val != null && val != undefined && val != "0" && val != "0.00") {
        rVal = parseFloat(val).toFixed(2);
    }
    return rVal;
}
function NullDecimalRoundHandle(val) {
    var rVal = '&nbsp;';
    if (val != null && val != undefined && val != "0" && val != "0.00") {
        rVal = parseFloat(val).toFixed(0);
    }
    return rVal;
}

function NullDecimalRoundHandleZero(val) {
    var rVal = '0';
    if (val != null && val != undefined) {
        rVal = parseFloat(val).toFixed(0);
    }
    return rVal;
}
function ThousandSeperator(num) {
    try {
        if (num == null || num == undefined || num == '' || num == "0") {
            num = " ";
        }
        else {
            num = Math.round(num);
            if (num.toString().length > 3) {
                var num1 = num.toString().substring(num.toString().length - 3, num.toString().length);
                num = num.toString().substring(0, num.toString().length - 3);
                num = num.toString().replace(/\B(?=(\d{2})+(?!\d))/g, ",");
                num = num + ',' + num1;
            }
        }
    }
    catch (e) {
    }
    return num;
}
function ThousandSeperatorDecimal(decVal) {
    var num = decVal;
    try {
        if (num == null || num == undefined || num == '' || num == "0") {
            num = " ";
        }
        else {
            num = parseFloat(num).toFixed(2);
            var parts = num.toString().split(".");
            num = parts[0];
            if (num.toString().length > 3) {
                var num1 = num.toString().substring(num.toString().length - 3, num.toString().length);
                num = num.toString().substring(0, num.toString().length - 3);
                num = num.toString().replace(/\B(?=(\d{2})+(?!\d))/g, ",");
                num = num + ',' + num1 + '.' + parts[1];
            }
        }
    }
    catch (e) {
    }
    return num;
}

//////////////////// Format Part No With Space //////////////////////////////////////////////
function FormatPartNo(partNo, MRPCode) {
    var returnPartNo = partNo
    //if (partNo.length >= 10) {
    //    returnPartNo = returnPartNo + partNo.substring(0, 1) + ' ' + partNo.substring(1, 4) + ' ' + partNo.substring(4, 7) + ' ' + partNo.substring(7, 10);

    //}
    if (MRPCode != null && MRPCode.length == 3) {
        returnPartNo = returnPartNo + ' ' + MRPCode;
    }
    return returnPartNo;
}

/////////////////// Deformat Part No without Space /////////////////////////////////////////////////
function DeFormatPartNo(partNo) {

    var returnPartNo = ''
    if (partNo.length >= 13) {
        //returnPartNo = returnPartNo + partNo.substring(0, 1) + partNo.substring(2, 5) + partNo.substring(6, 9) + partNo.substring(10, 13);
        returnPartNo = partNo.substring(0, 13);

    }
    return returnPartNo;
}

////////////////////////////// Deformat MRP Code ////////////////////////////////////////////////////////////
function DeFormatMRPCode(partNo) {

    var returnMRPCode = ''
    if (partNo.length >= 16) {
        returnMRPCode = partNo.substring(14);
    }
    return returnMRPCode;
}


/* JavaScript For Disable Back Button */
function noBack() { window.history.forward(); }
noBack();
window.onload = noBack;
window.onpageshow = function (evt) { if (evt.persisted) noBack(); }
window.onunload = function () { void (0); }



/* Start of JavaScript For Disable Right Click */

//var message = "Right Click Disabled!";
//function clickIE4() {
//    if (event.button == 2) {
//        alert(message);
//        return false;
//    }
//}
//function clickNS4(e) {
//    if (document.layers || document.getElementById && !document.all) {
//        if (e.which == 2 || e.which == 3) {
//            alert(message);
//            return false;
//        }
//    }
//}
//if (document.layers) {
//    document.captureEvents(Event.MOUSEDOWN);
//    document.onmousedown = clickNS4;
//}
//else if (document.all && !document.getElementById) {
//    document.onmousedown = clickIE4;
//}
////document.oncontextmenu = new Function("alert(message);return false")
//document.oncontextmenu = new Function("return false")

/* End of JavaScript For Disable Right Click */

document.onkeypress = function (event) {
    event = (event || window.event);
    if (event.keyCode == 123) {
        return false;
    }
}
document.onmousedown = function (event) {
    event = (event || window.event);
    if (event.keyCode == 123) {
        return false;
    }
}
document.onkeydown = function (event) {
    event = (event || window.event);
    if (event.keyCode == 123) {
        return false;
    }
}

delete console;



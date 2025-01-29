function validatePassword() {
    debugger;
    var password = $("#txtNewPassword").val();
    const re = /(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&*()+=-\?;,./{}|\":<>\[\]\\\' ~_]).{8,}/
    if (re.test(password)) {
        return true;
    } else {
        alert('Password must contains at least 8 characters long, at least 1 numeric character, at least 1 lowercase letter, at least 1 uppercase letter, at least 1 special character');
    }
}
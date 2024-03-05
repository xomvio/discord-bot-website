function addBot() {
    var myw = window.open('https://discord.com/oauth2/authorize?client_id=1143438057907961967&scope=bot&permissions=381585271830', 'Add%20Bitguard', 'menubar=no,width=500,height=777,location=no,resizable=no,scrollbars=yes,status=no');
    var timer = setInterval(function () {
        if (myw.closed) {
            clearInterval(timer);
            location.reload();
        }
    }, 1000)

}

function serverSelect(el) {
    if (el.options[el.selectedIndex].value == "invite") {
        var myw = window.open('https://discord.com/oauth2/authorize?client_id=1143438057907961967&scope=bot&permissions=381585271830', 'Add%20Bitguard', 'menubar=no,width=500,height=777,location=no,resizable=no,scrollbars=yes,status=no');
        var timer = setInterval(function () {
            if (myw.closed) {
                clearInterval(timer);
                location.reload();
            }
        }, 1000)
    }
    else {
        window.location.href = "?s=" + el.options[el.selectedIndex].value;
    }
}

function showData(e) {
    var c = document.getElementById(e);
    if (c.style.maxHeight.length < 4) {
        c.style.maxHeight = c.scrollHeight + "px";
    }
    else {
        c.style.maxHeight = "0px";
    }
}

//İLETİŞİM ACTIONS

var modal = document.getElementById("commModal");
var commForm = document.getElementById("commForm");
var commSendBtn = document.getElementById("commSendBtn");
var commSuccess = document.getElementById("commSuccess");
var commFail = document.getElementById("commFail");
var sending = 0;

var commBtn = document.getElementById("commBtn");
var succ = document.getElementById("commSuccess");
var spanClose = document.getElementById("closeModal");
var invalidMsg = document.getElementById("invalidMsg");
let mail = document.getElementById("mail");
let message = document.getElementById("message");

commBtn.onclick = function () {
    modal.style.display = "flex";
    commSuccess.style.display = "none";
    commFail.style.display = "none";
    commForm.style.display = "block";
    invalidMsg.style.display = "none";
    mail.value = "";
    message.value = "";
    mail.style.border = "1px inset #212529";
}

spanClose.onclick = function () {
    modal.style.display = "none";
}

window.onclick = function (event) {
    if (event.target == modal) {
        modal.style.display = "none";
    }
}


commSendBtn.onclick = async function () {
    mail = document.getElementById("mail");
    message = document.getElementById("message");

    var validRegex = /^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$/;

    if (mail.value.match(validRegex)) {
        if (sending == 0) {
            sending = 1;
            let qqq = await fetch("/communication?mail=" + mail.value + "&message=" + message.value);

            commForm.style.display = "none";

            if (qqq.status == 204) {
                commSuccess.style.display = "block";
            }
            else {
                commFail.style.display = "block";
            }
            sending = 0;
        }
    } else {
        mail.style.borderColor = "red";
        invalidMsg.style.display = "block";
    }
}
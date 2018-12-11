window.addEventListener('DOMNodeInserted', function (e) {
    if (e.target.className == 'ac-container') {
        setTimeout(function () {
            try {
                //e.target.scrollIntoView();
                e.target.scrollIntoViewIfNeeded();
            }
            catch (e) {
                console.log(e);
            }
        }, 300);
    }
});

window.onload = function () {
    BotChat.App({
        directLine: { secret: 'xcBwSd2aeaA.cwA.s9A.yB_PKkHMAQp5kRtwweps13e7qEYFtUN6YUUj0UnVMZM' },
        user: { id: 'userid', name: 'You' },
        bot: { id: 'botid' },
        resize: 'detect'
    }, document.getElementById("bot"));

    var legalText = "The information detailed herein, including that with regard to active pharmaceutical ingredients , their use in treatments and possible mechanism(s) of action, was all obtained from public sources. For more information on our services and products – including product suitability, intended uses and current availability – please contact TAPI Customer Service. CAS Registry Number is a Registered Trademark of the American Chemical Society. This webpage and any reference to the above product is to be read within the meaning and terms of the Legal Notes (https://www.tapi.com/LegalNotes/).";
    var legaluri = "https://www.tapi.com/LegalNotes/";
    var legalNote = "<a href=\"" + legaluri + "\"> Legal Notes </a>";
    document.getElementsByClassName("wc-header")[0].lastChild.innerText = legalText;

    var innerDiv = document.createElement('div');
    innerDiv.innerText = "Please note that this is a beta version of our API sourcing tool and you may experience some bugs while in use.";
    innerDiv.className = "wc-header-beta-msg";
    document.getElementsByClassName("wc-header")[0].appendChild(innerDiv)

    var btnDiv = document.createElement('div');
    btnDiv.className = "wc-header-div-btn";

    var btnMinimize = document.createElement('button');
    btnMinimize.addEventListener("click", function () {
        this.parentElement.parentElement.lastChild.previousSibling.previousSibling.innerText = "";
        this.parentElement.parentElement.removeChild(this.parentElement.parentElement.lastChild);
    });
    btnMinimize.innerText = "I Understand, Please minimized";
    btnDiv.appendChild(btnMinimize);

    document.getElementsByClassName("wc-header")[0].appendChild(btnDiv)
};
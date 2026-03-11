let addmanager = document.getElementById('addpopup');
let popup = document.getElementById('popup')
let overlay = document.getElementById('overlay')
let cancel = document.getElementsByClassName('cancelPopup')
let changebtn = document.getElementsByClassName('change')
let changepopup = document.getElementById('changepopup')

let deletebtn = document.getElementsByClassName('delete')
let deletepopup = document.getElementById('deletepopup')
function add(popup,overlay) {

    popup.style.display = 'block';
    overlay.style.display = 'block';
}
addmanager.addEventListener('click', function () {
    add(popup, overlay);
});

for (let i = 0; i < deletebtn.length; i++) {//chi dung dc voi class( class=change y)
    deletebtn[i].addEventListener('click', function () {
        add(overlay, deletepopup);
    });
}

for (let i = 0; i < changebtn.length; i++) {//chi dung dc voi class( class=change y)
    changebtn[i].addEventListener('click', function () {
        add(overlay, changepopup);
    });
}



for (let i = 0; i < cancel.length; i++) {
    cancel[i].addEventListener('click', function () {
        overlay.style.display = 'none';
        popup.style.display = 'none';
        changepopup.style.display = 'none';
        deletepopup.style.display = 'none';
    });
}

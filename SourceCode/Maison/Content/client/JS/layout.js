const button = document.getElementById('buttondropmenu');
const dropmenu = document.getElementById('dropmenu');
const overlay = document.getElementById('overlay');
let checktk = document.getElementById('checktk')
let clicktk = document.getElementById('clicktk')
function openMenu(dropmenu,overlay) {
    dropmenu.style.display = 'block';
    overlay.style.display = 'block';
}
function closeMenu(dropmenu, overlay) {
    dropmenu.style.display = 'none';
    overlay.style.display = 'none';
}

button.addEventListener('click', function () {
    const isOpen = dropmenu.style.display === 'block';

    if (isOpen) {
        closeMenu(dropmenu, overlay);
    } else {
        openMenu(dropmenu, overlay);
    }
});


overlay.addEventListener('click', function () {
    closeMenu(dropmenu, overlay);
    closeMenu(checktk, overlay);
});
//function add(checktk, overlay) {
//    checktk.style.display = 'block';
//    overlay.style.display = 'block';

//}
clicktk.addEventListener('click', function () {
    const isOpen = checktk.style.display === 'block';
    if (isOpen) {
        closeMenu(checktk, overlay);
    } else {
        openMenu(checktk, overlay);
    }

});
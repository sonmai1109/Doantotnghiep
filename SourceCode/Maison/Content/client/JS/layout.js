const button = document.getElementById('buttondropmenu');
const dropmenu = document.getElementById('dropmenu');
const overlay = document.getElementById('overlay');

function openMenu() {
    dropmenu.style.display = 'block';
    overlay.style.display = 'block';
}

function closeMenu() {
    dropmenu.style.display = 'none';
    overlay.style.display = 'none';
}

button.addEventListener('click', function () {
    const isOpen = dropmenu.style.display === 'block';

    if (isOpen) {
        closeMenu();
    } else {
        openMenu();
    }
});


overlay.addEventListener('click', function () {
    closeMenu();
});

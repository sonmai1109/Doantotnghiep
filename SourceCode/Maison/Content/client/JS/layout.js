const button = document.getElementById('buttondropmenu');
const dropmenu = document.getElementById('dropmenu');
const overlay = document.getElementById('overlay');

const clicktk = document.getElementById('clicktk');
const checktk = document.getElementById('checktk');

const clicktk2 = document.getElementById('clicktk2');
const checktk2 = document.getElementById('checktk2');

function show(el) {
    if (!el) return;
    el.style.display = 'block';
}

function hide(el) {
    if (!el) return;
    el.style.display = 'none';
}

function isVisible(el) {
    if (!el) return false;
    return window.getComputedStyle(el).display !== 'none';
}

// đóng tất cả
function closeAll() {
    hide(dropmenu);
    hide(checktk);
    hide(checktk2);
    hide(overlay);
}

// toggle chuẩn (chỉ mở 1 cái)
function toggle(target) {
    if (!target) return;

    const currentlyOpen = isVisible(target);

    closeAll();

    if (!currentlyOpen) {
        show(target);
        show(overlay);
    }
}

// ===== EVENT =====

// menu chính
if (button && dropmenu) {
    button.addEventListener('click', () => toggle(dropmenu));
}

// chưa đăng nhập
if (clicktk && checktk) {
    clicktk.addEventListener('click', () => toggle(checktk));
}

// đã đăng nhập
if (clicktk2 && checktk2) {
    clicktk2.addEventListener('click', () => toggle(checktk2));
}

// click overlay để đóng
if (overlay) {
    overlay.addEventListener('click', closeAll);
}
function toggleChat() {
    // Đổi lệnh này: Tự động thêm/xóa class show-chat
    $('#chat-window').toggleClass('show-chat');
}
function handleEnter(e) {
    if (e.keyCode === 13) { sendChatMessage(); }
}

function sendChatMessage() {
    let inputField = $('#txtChatInput');
    let msg = inputField.val().trim();
    if (msg === "") return;

    let chatBody = $('#chat-body');

    // 1. In tin nhắn của User lên màn hình (KHÔNG DÙNG .clearfix NỮA)
    chatBody.append(`<div class="msg-box msg-user">${msg}</div>`);
    inputField.val(''); // Xóa ô nhập
    chatBody.scrollTop(chatBody[0].scrollHeight); // Cuộn xuống dưới cùng

    // Hiển thị trạng thái "Bot đang gõ..."
    let typingId = "typing_" + Date.now();
    chatBody.append(`<div class="msg-box msg-bot font-italic text-muted" id="${typingId}">Đang gõ chữ...</div>`);
    chatBody.scrollTop(chatBody[0].scrollHeight);

    // 2. Gửi AJAX lên C#
    $.ajax({
        url: '/Chatbot/GiaoTiepGemini',
        type: 'POST',
        data: { tinNhanKhachHang: msg },
        success: function (res) {
            $('#' + typingId).remove(); // Xóa chữ Đang gõ

            if (res.status) {
                // In tin nhắn của Bot
                chatBody.append(`<div class="msg-box msg-bot">${res.reply}</div>`);
            } else {
                chatBody.append(`<div class="msg-box msg-bot text-danger">⚠️ ${res.message}</div>`);
            }
            chatBody.scrollTop(chatBody[0].scrollHeight);
        },
        error: function () {
            $('#' + typingId).remove();
            chatBody.append(`<div class="msg-box msg-bot text-danger">⚠️ Mất kết nối tới máy chủ.</div>`);
            chatBody.scrollTop(chatBody[0].scrollHeight);
        }
    });
}
// CHẠY NGAY KHI LOAD TRANG
$(document).ready(function () {
    loadChatHistory();
});

// Hàm khôi phục trí nhớ từ Session
function loadChatHistory() {
    $.ajax({
        url: '/Chatbot/GetChatHistory',
        type: 'GET',
        success: function (data) {
            let chatBody = $('#chat-body');
            chatBody.empty(); // Xóa sạch giao diện rỗng hiện tại

            // Vẽ lại từng tin nhắn từ C# Session đẩy xuống
            data.forEach(function (msg) {
                if (msg.Type === "user") {
                    chatBody.append(`<div class="msg-box msg-user">${msg.Content}</div>`);
                } else {
                    chatBody.append(`<div class="msg-box msg-bot">${msg.Content}</div>`);
                }
            });
            // Tự cuộn xuống đáy
            chatBody.scrollTop(chatBody[0].scrollHeight);
        }
    });
}

function toggleChat() {
    $('#chat-window').toggleClass('show-chat');
}

function handleEnter(e) {
    if (e.keyCode === 13) { sendChatMessage(); }
}

function sendChatMessage() {
    let inputField = $('#txtChatInput');
    let msg = inputField.val().trim();
    if (msg === "") return;

    let chatBody = $('#chat-body');

    // In tin nhắn User
    chatBody.append(`<div class="msg-box msg-user">${msg}</div>`);
    inputField.val('');
    chatBody.scrollTop(chatBody[0].scrollHeight);

    // Hiển thị trạng thái "Bot đang gõ..."
    let typingId = "typing_" + Date.now();
    chatBody.append(`<div class="msg-box msg-bot font-italic text-muted" id="${typingId}">Đang gõ chữ...</div>`);
    chatBody.scrollTop(chatBody[0].scrollHeight);

    // Gửi AJAX
    $.ajax({
        url: '/Chatbot/GiaoTiepGemini',
        type: 'POST',
        data: { tinNhanKhachHang: msg },
        success: function (res) {
            $('#' + typingId).remove();
            if (res.status) {
                chatBody.append(`<div class="msg-box msg-bot">${res.reply}</div>`);
            } else {
                chatBody.append(`<div class="msg-box msg-bot text-danger">⚠️ ${res.message}</div>`);
            }
            chatBody.scrollTop(chatBody[0].scrollHeight);
        },
        error: function () {
            $('#' + typingId).remove();
            chatBody.append(`<div class="msg-box msg-bot text-danger">⚠️ Mất kết nối tới máy chủ.</div>`);
            chatBody.scrollTop(chatBody[0].scrollHeight);
        }
    });
}
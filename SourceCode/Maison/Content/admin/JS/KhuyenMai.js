$(function () {
    // Mở popup Thêm
    $('#addPopup').on('click', function () {
        $('#add-form')[0].reset();
        $('#add-message').text('');
        $('#overlay').fadeIn(200);
        $('#popUp').fadeIn(200);
    });

    // Đóng tất cả popup
    $('.cancelPopup, #overlay').on('click', function () {
        $('.popup, #overlay').fadeOut(200);
    });
});

// Load dữ liệu đổ vào form Sửa
function loadData(id) {
    $.post('/Admin/KhuyenMais/Get', { id: id }, function (res) {
        if (res.status == true) {
            $("#makm").val(res.MaKM);
            $("#tenkm").val(res.TenKM);
            $("#mota").val(res.MoTa);
            $("#ngaybatdau").val(res.NgayBatDau ? res.NgayBatDau : '');
            $("#ngayketthuc").val(res.NgayKetThuc ? res.NgayKetThuc : '');

            $('#update-message').text('');
            $('#overlay').fadeIn(200);
            $('#changePopUp').fadeIn(200);
        } else {
            alert('Không tìm thấy dữ liệu khuyến mãi!');
        }
    }).fail(function () {
        alert('Lỗi hệ thống khi tải dữ liệu!');
    });
}

// Gọi API Thêm
function themKhuyenMai() {
    var formData = $('#add-form').serialize();
    $.post('/Admin/KhuyenMais/Create', formData, function (res) {
        if (res.status == true) {
            window.location.reload();
        } else {
            $('#add-message').text(res.message);
        }
    }).fail(function () {
        $('#add-message').text('Lỗi kết nối tới Server!');
    });
    return false; // Chặn load lại trang của form
}

// Gọi API Sửa
function suaKhuyenMai() {
    var formData = $('#update-form').serialize();
    $.post('/Admin/KhuyenMais/Edit', formData, function (res) {
        if (res.status == true) {
            window.location.reload();
        } else {
            $('#update-message').text(res.message);
        }
    }).fail(function () {
        $('#update-message').text('Lỗi kết nối tới Server!');
    });
    return false;
}

// Xóa (Dùng luôn confirm mặc định cho nhanh và an toàn)
function deleteData(id) {
    if (confirm("Cảnh báo: Nếu xóa chương trình này, toàn bộ sản phẩm đang được gán sẽ bị mất khuyến mãi. Bạn có chắc chắn muốn xóa?")) {
        $.post('/Admin/KhuyenMais/Delete', { id: id }, function (res) {
            if (res.status == true) {
                $("#row-" + id).fadeOut(300, function () { $(this).remove(); });
            } else {
                alert('Xóa thất bại!');
            }
        });
    }
}

// Bật tắt trạng thái (Ẩn/Hiện)
function toggleStatus(id) {
    $.post('/Admin/KhuyenMais/ToggleStatus', { id: id }, function (res) {
        if (res.status == true) {
            window.location.reload();
        } else {
            alert('Thay đổi trạng thái thất bại!');
        }
    });
}
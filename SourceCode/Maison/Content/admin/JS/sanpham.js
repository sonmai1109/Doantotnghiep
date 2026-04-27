// ==============================================================
// 1. KHỞI TẠO CKEDITOR KHI TRANG LOAD XONG
// ==============================================================
$(document).ready(function () {
    // Kích hoạt CKEditor cho Popup Thêm Mới
    if ($('#txtMoTaThem').length) {
        CKEDITOR.replace('txtMoTaThem', {
            height: 250,
            versionCheck: false // Ẩn cảnh báo phiên bản cũ
        });
    }

    // Kích hoạt CKEditor cho Popup Sửa
    if ($('#txtMoTaSua').length) {
        CKEDITOR.replace('txtMoTaSua', {
            height: 250,
            versionCheck: false
        });
    }
});

// ==============================================================
// 2. HÀM THÊM SẢN PHẨM MỚI (CÓ XỬ LÝ CKEDITOR)
// ==============================================================
function themSanPham() {
    // Ép dữ liệu từ giao diện CKEditor vào thẻ <textarea> thật trước khi gửi đi
    for (instance in CKEDITOR.instances) {
        CKEDITOR.instances[instance].updateElement();
    }

    let formData = new FormData(document.getElementById('add-form'));

    $.ajax({
        url: '/Sanphams/Create',
        type: 'POST',
        data: formData,
        contentType: false,
        processData: false,
        success: function (res) {
            if (res.status == true) {
                $("#add-message").removeClass("text-danger").addClass("text-success").html(res.message);
                setTimeout(function () { window.location.reload(); }, 1000);
            } else {
                $("#add-message").removeClass("text-success").addClass("text-danger").html(res.message);
            }
        },
        error: function (xhr) { console.log(xhr); }
    });
    return false; // Chặn hành vi submit mặc định của Form
}

// ==============================================================
// 3. HÀM LOAD DỮ LIỆU ĐỂ SỬA (LẤY DỮ LIỆU CŨ TỪ DATABASE LÊN GIAO DIỆN)
// ==============================================================
function loaddata(id) {
    $("#update-message").removeClass("text-success text-danger").html("");

    $.ajax({
        type: 'POST',
        data: { "id": id },
        url: '/Sanphams/Loaddata',
        success: function (res) {
            // Đổ dữ liệu vào các ô Input thông thường
            $("#masp").val(res.MaSP);
            $("#tensp").val(res.TenSP);
            $("#madm").val(res.MaDM);
            $("#mabrand").val(res.MaBrand);
            $("#baohanh").val(res.ThoiHanBaoHanh);

            // ---> ĐỔ DỮ LIỆU VÀO CKEDITOR CHUẨN XÁC <---
            // Phải kiểm tra xem đối tượng CKEditor đã khởi tạo thành công chưa rồi mới setData
            if (CKEDITOR.instances['txtMoTaSua']) {
                CKEDITOR.instances['txtMoTaSua'].setData(res.MoTa);
            } else {
                // Nếu CKEditor lỗi không load được, đổ tạm vào textarea gốc
                $("#txtMoTaSua").val(res.MoTa);
            }
        },
        error: function (xhr) { alert("Lỗi tải dữ liệu..."); }
    });
}

// ==============================================================
// 4. HÀM CẬP NHẬT THÔNG TIN SẢN PHẨM (CÓ XỬ LÝ CKEDITOR)
// ==============================================================
function suaSanPham() {
    // Ép dữ liệu từ giao diện CKEditor vào thẻ <textarea> thật trước khi gửi đi
    for (instance in CKEDITOR.instances) {
        CKEDITOR.instances[instance].updateElement();
    }

    let formData = new FormData(document.getElementById('update-form'));

    $.ajax({
        url: '/Sanphams/Update',
        type: 'POST',
        data: formData,
        contentType: false,
        processData: false,
        success: function (res) {
            if (res.status == true) {
                $("#update-message").removeClass("text-danger").addClass("text-success").html(res.message);
                setTimeout(function () { window.location.reload(); }, 1000);
            } else {
                $("#update-message").removeClass("text-success").addClass("text-danger").html(res.message);
            }
        },
        error: function (xhr) { console.log(xhr); }
    });
    return false; // Chặn hành vi submit mặc định của Form
}

// ==============================================================
// 5. CÁC HÀM XÓA SẢN PHẨM
// ==============================================================
function deleteData(id) {
    $("#delete-masp").val(id);
}

function xoaSanPham() {
    let id = $("#delete-masp").val();
    $.ajax({
        type: 'POST',
        data: { "id": id },
        url: '/Sanphams/Delete',
        success: function (res) {
            if (res.status == true) {
                $(".cancelPopup").click();
                $(".row-" + id).fadeOut(300, function () { $(this).remove(); });
            } else {
                alert(res.message);
                $(".cancelPopup").click();
            }
        },
        error: function (xhr) { alert("Lỗi xóa dữ liệu..."); }
    });
}
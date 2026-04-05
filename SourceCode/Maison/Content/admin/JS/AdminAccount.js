//Ajax thêm tài khoản
function themTaiKhoanAdmin() {
    let data = {};
    let formData = $('#add-form').serializeArray({
    });
    $.each(formData, function (index, value) {
        data["" + value.name + ""] = value.value;
    });
    if (data["matkhau"] != data["nhaplaimatkhau"]) {
        $("#add-message").html("Mật khẩu không khớp");
        return false;
    }
    $.ajax({
        url: '/AdminUser/Create',
        type: 'post',
        contentType: 'application/json',
        data: JSON.stringify(data),
        dataType: 'json',
        success: function (respone) {
            $("#add-message").html(respone.message);
            if (respone.status == true) {
                $("#add-message").addClass("text-warning");
                setTimeout(function () {
                    window.location.replace("/Admin/AdminUser");
                }, 1000)
            } else {
                $("#add-message").addClass("text-danger");
            }
        },
        error: function (respone) {
            console.log(respone);
        }
    });
    return false;
}

//load dữ liệu lên form sửa
function loadData(id) {
    $.ajax({
        type: 'POST',
        data: { "id": id },
        url: '/AdminUser/Index',
        success: function (response) {
            $("#matk").val(response.ID);
            $("#tendangnhap").val(response.TenDangNhap);
            $("#hoten").val(response.HoTen);
            if (response.LoaiTaiKhoan == true) {
                $("#admin-role").prop("checked", true);
            } else {
                $("#manager-role").prop("checked", true);
            }
            if (response.TrangThai == true) {
                $("#actived").prop("checked", true);
            } else {
                $("#blocked").prop("checked", true);
            }
        },
        error: function (response) {
            //debugger;  
            console.log(xhr.responseText);
            alert("Error has occurred..");
        }
    });
}

//ajax sửa tài khoản quản trị
function suaTaiKhoanQuanTri() {
    let data = {};
    let formData = $('#update-form').serializeArray({
    });
    $.each(formData, function (index, value) {
        data["" + value.name + ""] = value.value;
    });
    $.ajax({
        url: '/AdminUser/Update',
        type: 'post',
        contentType: 'application/json',
        data: JSON.stringify(data),
        dataType: 'json',
        success: function (respone) {
            $("#update-message").html(respone.message);
            if (respone.status == true) {
                $("#update-message").addClass("text-warning");
                setTimeout(function () {
                    window.location.replace("/Admin/AdminUser");
                }, 1000)
            } else {
                $("#update-message").addClass("text-danger");
            }
        },
        error: function (respone) {
            console.log(respone);
        }
    });
    return false;
}

//load data lên form xóa
function deleteData(id) {
    $("#delete-message").html("");
    $("#delete-adminuser-id").val(id);
}

function xoaTaiKhoan() {
    let id = $("#delete-adminuser-id").val();
    $.ajax({
        type: 'POST',
        data: { "id": id },
        url: '/AdminUser/Delete',
        success: function (response) {
            if (response.status == true) {
                $(".cancelPopup").click();
                $("#row-" + id).remove();
            } else {
                $("#delete-message").html(response.message);
            }
        },
        error: function (response) {
            console.log(xhr.responseText);
            alert("Error has occurred..");
        }
    });
}
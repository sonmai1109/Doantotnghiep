function themThuocTinh() {
    let data = {};
    let formdata = $('#add-form').serializeArray();
    $.each(formdata, function (index, value) {
        data[value.name] = value.value;
    });

    $.ajax({
        url: '/ThuocTinhs/Create',
        type: 'post',
        contentType: 'application/json',
        data: JSON.stringify(data),
        dataType: 'json',
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
    return false;
}

function loaddata(id) {
    $("#update-message").removeClass("text-success text-danger").html("");
    $.ajax({
        type: 'POST',
        data: { "id": id },
        url: '/ThuocTinhs/Loaddata',
        success: function (res) {
            $("#matt").val(res.MaTT);
            $("#tentt").val(res.TenTT);
            $("#madm").val(res.MaDM);

            // Ép kiểu boolean thành string chữ thường để match với thẻ <option value="true">
            if (res.LaThuocTinhChinh !== undefined) {
                $("#edit_LaThuocTinhChinh").val(res.LaThuocTinhChinh.toString().toLowerCase());
            }
        },
        error: function (xhr) { alert("Lỗi tải dữ liệu..."); }
    });
}

function suaThuocTinh() {
    let data = {};
    let formdata = $('#update-form').serializeArray();
    $.each(formdata, function (index, value) {
        data[value.name] = value.value;
    });

    $.ajax({
        url: '/ThuocTinhs/Update',
        type: 'post',
        data: JSON.stringify(data),
        contentType: 'application/json',
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
    return false;
}

function deleteData(id) {
    $("#delete-matt").val(id);
}

function xoaThuocTinh() {
    let id = $("#delete-matt").val();
    $.ajax({
        type: 'POST',
        data: { "id": id },
        url: '/ThuocTinhs/Delete',
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
function themSanPham() {
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
    return false;
}

function loaddata(id) {
    $("#update-message").removeClass("text-success text-danger").html("");
    $.ajax({
        type: 'POST',
        data: { "id": id },
        url: '/Sanphams/Loaddata',
        success: function (res) {
            $("#masp").val(res.MaSP);
            $("#tensp").val(res.TenSP);
            $("#madm").val(res.MaDM);
            $("#mabrand").val(res.MaBrand);
            $("#baohanh").val(res.ThoiHanBaoHanh);
            $("#mota").val(res.MoTa);
        },
        error: function (xhr) { alert("Lỗi tải dữ liệu..."); }
    });
}

function suaSanPham() {
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
    return false;
}

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
function thembrand() {
    // Tự động lấy toàn bộ dữ liệu (kể cả file) từ form
    let formData = new FormData(document.getElementById('add-form'));

    $.ajax({
        url: '/Brands/Create',
        type: 'POST', // Phải viết hoa
        data: formData,
        contentType: false, // Bắt buộc phải là false khi dùng FormData
        processData: false, // Bắt buộc phải là false khi dùng FormData
        success: function (respone) {
            if (respone.status == true) {
                $("#add-mess").removeClass("text-danger").addClass("text-warning");
                setTimeout(function () {
                    window.location.replace("/Admin/Brands");
                }, 1000);
            } else {
                $("#add-mess").removeClass("text-warning").addClass("text-danger");
            }
            $("#add-mess").html(respone.message);
        },
        error: function (respone) { console.log(respone); }
    });
    return false;
}

function loaddata(id) {
    $("#update-mess").removeClass("text-warning text-danger").html("");
    $.ajax({
        type: 'POST',
        data: { "id": id },
        url: '/Brands/Loaddata',
        success: function (respone) {
            $("#mabrand").val(respone.MaBrand);
            $("#tenbrand").val(respone.TenBrand);
            $("#logo").val(respone.Logo);
            $("#mota").val(respone.MoTa);
        },
        error: function (xhr) {
            console.log(xhr.responseText);
            alert("Lỗi tải dữ liệu...");
        }
    });
}

function suabrand() {
    let formData = new FormData(document.getElementById('update-form'));

    $.ajax({
        url: '/Brands/Update',
        type: 'POST',
        data: formData,
        contentType: false, // Bắt buộc phải là false
        processData: false, // Bắt buộc phải là false
        success: function (respone) {
            $("#update-mess").html(respone.message);
            if (respone.status == true) {
                $("#update-mess").removeClass("text-danger").addClass("text-warning");
                setTimeout(function () {
                    window.location.replace("/Admin/Brands");
                }, 1000);
            } else {
                $("#update-mess").removeClass("text-warning").addClass("text-danger");
            }
        },
        error: function (respone) { console.log(respone); }
    });
    return false;
}

function deleteData(id) {
    $("#delete-mabrand").val(id);
}

function xoabrand() {
    let id = $("#delete-mabrand").val();
    $.ajax({
        type: 'POST',
        data: { "id": id },
        url: '/Brands/Delete',
        success: function (respone) {
            if (respone.status == true) {
                $(".cancelPopup").click();
                $(".row-" + id).remove(); // Cập nhật đúng selector .row-ID của bạn
                setTimeout(function () {
                    window.location.replace('/Admin/Brands');
                }, 300);
            }
        },
        error: function (xhr) {
            console.log(xhr.responseText);
            alert("Lỗi xóa dữ liệu...");
        }
    });
}
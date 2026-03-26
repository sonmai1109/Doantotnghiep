//Load sản phẩm lên model
//Load sản phẩm lên model


function loadSanPham(id) {
    $.ajax({
        type: 'POST',
        data: { "id": id },
        url: '/Product/Index',
        success: function (response) {

            if (!response) {
                alert("Không tìm thấy sản phẩm");
                return;
            }

            // ================= ẢNH ĐẠI DIỆN BAN ĐẦU =================
            $("#modal-a-hinhanh").attr("href", response.HinhAnh);
            $("#modal-hinhanh").attr("src", response.HinhAnh);

            // ================= ẢNH THEO MÀU (MAP) =================
            let imageMap = {};  // { hexColor : [img1, img2,...] }

            if (response.Images) {
                response.Images.forEach(img => {
                    if (!img.MaMauHex) return; // bỏ ảnh không có màu

                    if (!imageMap[img.MaMauHex])
                        imageMap[img.MaMauHex] = [];

                    imageMap[img.MaMauHex].push(img.LinkAnh);
                });
            }

            // ============= HIỂN THỊ THÔNG TIN TEXT =============
            $("#modal-tensp").html(response.TenSP);
            $("#modal-danhmuc").html(response.DanhMuc.TenDanhMuc);

            if (response.GiaSauKhuyenMai && response.GiaSauKhuyenMai < response.Gia) {
                $("#modal-gia").html(`
                    <span style="text-decoration:line-through;color:gray;">
                        ${response.Gia.toLocaleString('vi-VN')}đ
                    </span>
                    <span style="color:red;font-weight:bold;margin-left:8px;">
                        ${response.GiaSauKhuyenMai.toLocaleString('vi-VN')}đ
                    </span>`);
            } else {
                $("#modal-gia").html(response.Gia.toLocaleString('vi-VN') + "đ");
            }

            // ============= TẠO DANH SÁCH MÀU =============
            let colorHtml = "";
            let colorMap = {};

            response.SanPhamChiTiets.forEach(ct => {
                if (!ct.MauSac) return;

                let hex = ct.MauSac.MaMauHex ? ct.MauSac.MaMauHex : "#" + ct.MauSac.MaMau;

                if (!colorMap[hex]) colorMap[hex] = [];
                colorMap[hex].push(ct);
            });

            Object.keys(colorMap).forEach(hexColor => {
                colorHtml += `
                    <div class="sp-color-item" 
                         data-color="${hexColor}" 
                         style="width:22px;height:22px;border-radius:50%;cursor:pointer;border:1px solid #333;background:${hexColor};">
                    </div>`;
            });

            $("#modal-list-color").html(colorHtml);

            // ============= CLICK CHỌN MÀU =============
            $(".sp-color-item").click(function () {

                $(".sp-color-item").css("outline", "none");
                $(this).css("outline", "2px solid #000");

                let hex = $(this).data("color");
                let list = colorMap[hex];

                // Load size theo màu
                let sizeHTML = "";
                list.forEach(ct => {
                    sizeHTML += `<option value="${ct.IDCTSP}">${ct.KichCoName}</option>`;
                });
                $("#modal-kichco-soluong").html(sizeHTML);


                // ========== ĐỔI ẢNH THEO MÀU – ẢNH ĐẦU TIÊN ==========
                if (imageMap[hex] && imageMap[hex].length > 0) {
                    let img = imageMap[hex][0];

                    // Hiệu ứng fade nhẹ
                    $("#modal-hinhanh").css("opacity", "0");

                    setTimeout(() => {
                        $("#modal-hinhanh").attr("src", img);
                        $("#modal-a-hinhanh").attr("href", img);
                        $("#modal-hinhanh").css("opacity", "1");
                    }, 150);
                }

                // Kích hoạt kiểm kho
                $("#modal-kichco-soluong").trigger("change");
            });

            // Auto chọn màu đầu tiên
            $(".sp-color-item").first().click();
        },
        error: function (xhr) {
            console.log(xhr.responseText);
            alert("Error has occurred..");
        }
    });
}


//function loadSanPham(id) {
//    $.ajax({
//        type: 'POST',
//        data: { "id": id },
//        url: '/Product/Index',
//        success: function (response) {
//            $("#modal-a-hinhanh").attr("href", response.HinhAnh);
//            $("#modal-hinhanh").attr("src", response.HinhAnh);
//            $("#modal-tensp").html(response.TenSP);
//            $("#modal-danhmuc").html(response.DanhMuc.TenDanhMuc);
//            $("#modal-gia").html(response.Gia.toLocaleString('it-IT', { style: 'currency', currency: 'VND' }));
//            $("#modal-mamau").val(response.MaMau.trim());
//            $.each(response.SanPhamChiTiets, function (index) {
//                $("#kichco-soluong-" + response.SanPhamChiTiets[index].MaKichCo).val(response.SanPhamChiTiets[index].IDCTSP);
//            })
//            if (response.SanPhamChiTiets[0].SoLuong == 0) {
//                $("#order-text").html("Hết hàng ! Hãy chọn kích cỡ khác");
//                $("#order-text").attr("disabled", "disabled");
//            }
//        },
//        error: function (response) {
//            //debugger;  
//            console.log(xhr.responseText);
//            alert("Error has occurred..");
//        }
//    });
//}


//Ajax load số lượng theo size
$(document).on("change", "#modal-kichco-soluong", function () {
    let id = $(this).val();
    $.ajax({
        type: 'POST',
        data: { "id": id },
        url: '/Product/Detail',
        success: function (response) {
            if (response.SoLuong > 0) {
                $("#order-text").html("Thêm vào giỏ");
                $("#order-text").removeAttr("disabled");
            } else {
                $("#order-text").html("Hết hàng ! Hãy chọn kích cỡ khác");
                $("#order-text").attr("disabled", "disabled");
            }
        },
        error: function (response) {
            //debugger;  
            console.log(xhr.responseText);
            alert("Error has occurred..");
        }
    });
});

//Ajax thêm sp vào giỏ hàng
function themVaoGioHang() {
 
        // ⭐ SỬA LẠI ĐỂ LẤY ID TỪ ĐÚNG DROPDOWN TRÊN TRANG CHI TIẾT
        // Hoặc nếu hàm này dùng chung cho cả Quick View, bạn có thể ưu tiên lấy theo ID trang chi tiết trước.
        let idctsp = $("#product-size").val() || $("#modal-kichco-soluong").val();

        // Lấy số lượng (ID này đã đúng)
        let soluong = $("#modal-soluong").val();

        // Đảm bảo số lượng phải là số nguyên dương
        if (isNaN(parseInt(soluong)) || parseInt(soluong) < 1) {
            soluong = 1;
        }

        // Đảm bảo IDCTSP đã được chọn
        if (!idctsp || idctsp == 0) {
            swal({
                title: "Thất bại!",
                text: "Vui lòng chọn Kích cỡ/Màu sắc sản phẩm",
                type: "danger",
                icon: "warning",
                timer: 1500,
                button: false
            });
            return;
        }

    $.ajax({
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ "idctsp": idctsp, "soluongmua": soluong }),
        url: '/Cart/AddToCart',
        success: function (response) {
            if (!response.status) {
                swal({
                    title: "Thất bại!",
                    text: "Số lượng hàng trong kho không đủ",
                    type: "danger",
                    icon: "warning",
                    timer: 1500,
                    button: false
                });
            } else {
                $("#product-count").html(response.cart.length);
                $(".close").click();
                swal({
                    title: "Thành công!",
                    text: "Xem chi tiết tại giỏ hàng nhé <3",
                    type: "success",
                    icon: "success",
                    timer: 1500,
                    button: false
                });
            }
        },
        error: function (response) {
            //debugger;  
            console.log(xhr.responseText);
            alert("Error has occurred..");
        }
    });
}

function updateGioHang(insertIdctsp, insertSoluong) {
    let idctsp = insertIdctsp 
    let soluong = insertSoluong

    $.ajax({
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ "idctsp": idctsp, "soluongmua": soluong }),
        url: '/Cart/UpdateFromCart',
        success: function (response) {
            if (!response.status) {
                swal({
                    title: "Thất bại!",
                    text: "Số lượng hàng trong kho không đủ",
                    type: "danger",
                    icon: "warning",
                    timer: 1500,
                    button: false
                });
            } else {
                $("#product-count").html(response.cart.length);
                $(".close").click();
            }
        },
        error: function (response) {
            //debugger;  
            console.log(xhr.responseText);
            alert("Error has occurred..");
        }
    });
}

//Ajax xóa sp trong giỏ hàng
function xoaGioHang(idctsp) {
    console.log("xoa", idctsp);
    
    let total = 0;
    swal({
        title: "Bạn có chắc chắn",
        text: "Xóa sản phẩm này khỏi giỏ hàng",
        icon: "warning",
        buttons: true,
        dangerMode: true,
    })
        .then((willDelete) => {
            if (willDelete) {
                $.ajax({
                    type: 'POST',
                    data: { "idctsp": idctsp },
                    url: '/Cart/DeleteFromCart',
                    success: function (response) {
                        if (response.length == 0) {
                            window.location = "/Cart/Orders";
                        } else {
                            $("#row-order-" + idctsp).remove();
                            $("#product-count").html(response.length);
                            $.each(response, function (index) {
                                total += response[index].SoLuongMua * response[index].GiaMua;
                            })
                            $("#order-total").html(total.toLocaleString('it-IT', { style: 'currency', currency: 'VND' }));
                        }
                    },
                    error: function (response) {
                        //debugger;  
                        console.log(xhr.responseText);
                        alert("Error has occurred..");
                    }
                });
            }
        });
}

//Ajax đặt hàng
function datHang() {
    let data = {};
    let formData = $('#add-bill-form').serializeArray({
    });
    $.each(formData, function (index, value) {
        data["" + value.name + ""] = value.value;
    });
    $.ajax({
        url: '/Bill/CreateBill',
        type: 'post',
        contentType: 'application/json',
        data: JSON.stringify(data),
        dataType: 'json',
        success: function (respone) {
            if (respone.status == true) {
                window.location.replace("/Bill/ListBills");
            } else {
                $("#add-message").addClass("text-danger");
                $("#add-message").html(respone.message);
            }
        },
        error: function (respone) {
            console.log(respone);
        }
    });
    return false;
}

//Ajax Hủy đơn hàng
function huyDonHang(id) {
    swal({
        title: "Cảnh báo",
        text: "Bạn có chắc về việc hủy đơn hàng này!",
        icon: "warning",
        buttons: true,
        dangerMode: true,
    })
        .then((willDelete) => {
            if (willDelete) {
                $.ajax({
                    type: 'POST',
                    data: { "mahd": id, "stt": 0 },
                    url: '/Bill/ChangeStatus',
                    success: function (response) {
                        if (response.status == true) {
                            swal({
                                title: "Thành công!",
                                text: "Hủy đơn hàng thành công !",
                                type: "success",
                                icon: "success",
                                timer: 1500,
                                button: false
                            });
                        } else {
                            swal({
                                title: "Thất bại!",
                                text: "Bạn không thể hủy đơn hàng do đơn hàng đã đang giao",
                                type: "danger",
                                icon: "error",
                                timer: 1500,
                                button: false
                            });
                        }
                        setTimeout(function () {
                            window.location = "/Bill/ListBills";
                        }, 1500);
                    },
                    error: function (response) {
                        //debugger;  
                        console.log(xhr.responseText);
                        alert("Error has occurred..");
                    }
                });
            }
        });
}
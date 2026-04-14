
        // 1. Hàm Hủy Đơn Hàng
        function huyDonHang(id) {
            swal({
                title: "Cảnh báo",
                text: "Bạn có chắc về việc hủy đơn hàng này!",
                icon: "warning",
                buttons: ["Quay lại", "Đồng ý Hủy"],
                dangerMode: true,
            }).then((willDelete) => {
                if (willDelete) {
                    // Hiện popup đang xử lý
                    swal({ title: "Đang xử lý...", icon: "info", buttons: false, closeOnClickOutside: false });

                    $.ajax({
                        type: 'POST',
                        data: { "mahd": id, "stt": 0 },
                        url: '/Admin/Bill/ChangeStatus',
                        success: function (response) {
                            if (response.status == true) {
                                swal("Thành công!", "Đã hủy đơn hàng và hoàn lại tồn kho!", "success")
                                    .then(() => { window.location.reload(); });
                            } else {
                                swal("Thất bại!", response.message ? response.message : "Không thể hủy đơn", "error")
                                    .then(() => { window.location.reload(); });
                            }
                        },
                        error: function (xhr) {
                            console.log("Lỗi Hủy đơn:", xhr.responseText);
                            swal("Lỗi Code Backend!", "Bật F12 -> Console để xem chi tiết lỗi đỏ.", "error");
                        }
                    });
                }
            });
        }

        // 2. Hàm Đổi Trạng Thái (Fix lỗi im ru + Bắt lỗi)
        function doiTrangThai(id) {
            let tt = $("#hd-trangthai-update-" + id).val();

        // Vừa chọn xong là hiện popup thông báo đang xử lý luôn
        swal({
            title: "Đang xử lý...",
        text: "Hệ thống đang cập nhật trạng thái...",
        icon: "info",
        buttons: false,
        closeOnClickOutside: false
            });

        $.ajax({
            type: 'POST',
        data: {"mahd": id, "stt": tt },
        url: '/Admin/Bill/ChangeStatus',
        success: function (response) {
                    if (response.status == true) {
            swal("Thành công!", "Sửa trạng thái thành công!", "success")
                .then(() => { window.location.reload(); });
                    } else {
            // Bắn popup lỗi màu đỏ kèm tin nhắn từ Backend trả về
            swal("Thất bại!", response.message ? response.message : "Cập nhật không thành công", "error")
                .then(() => { window.location.reload(); });
                    }
                },
        error: function (xhr) {
            // Bắn popup nếu Controller bị sập (lỗi màn hình vàng)
            console.log("Lỗi Đổi trạng thái:", xhr.responseText);
                    swal("Lỗi Hệ Thống!", "Đường truyền AJAX thất bại. Mở F12 -> Console xem lỗi.", "error")
                        .then(() => {window.location.reload(); });
                }
            });
        }

        // 3. Hàm Load Dữ Liệu Lên Modal
        function loadDuLieuChiTiet(id) {
            $("#hd-body").empty();
        swal({title: "Đang tải dữ liệu...", icon: "info", buttons: false }); // Feedback loading

        $.ajax({
            type: 'POST',
        data: {"id": id },
        url: '/Admin/Bill/GetDetails',
        success: function (response) {
            swal.close(); // Tắt popup loading
        if (response.error) {
            swal("Lỗi!", response.error, "error");
        return;
                    }

        let hd = response.hoadon;
        $("#hd-mahd").text("#" + hd.MaHD);
        $("#hd-nguoidat").val(hd.TaiKhoanNguoiDung.HoTen);
        $("#hd-nguoinhan").val(hd.HoTenNguoiNhan);
        $("#hd-trangthai").val(hd.TrangThai == 0 ? "Đã hủy" : (hd.TrangThai == 1 ? "Chờ xác nhận" : (hd.TrangThai == 2 ? "Đang giao" : "Thành công")));
        $("#hd-ngaydat").val(hd.NgayDat);
        $("#hd-sdt").val(hd.SoDienThoaiNhan);
        $("#hd-diachi").val(hd.DiaChiNhan);
        $("#hd-nguoisua").val(hd.NguoiSua);
        $("#hd-ngaysua").val(hd.NgaySua);
        $("#hd-ghichu").val(hd.GhiChu);

        let total = 0;
        $.each(response.cthd, function (index, item) {
            $("#hd-body").append(`
                            <tr>
                                <td><img src="${item.HinhAnh}" width="60" style="border-radius: 5px;" onerror="this.src='/Content/Images/no-image.png'" /></td>
                                <td class="text-left">
                                    <strong style="font-size: 15px;">${item.TenSP}</strong><br/>
                                    <small class="text-muted"><i class="fas fa-microchip"></i> ${item.CauHinh}</small>
                                </td>
                                <td>${item.GiaMua.toLocaleString('vi-VN')} đ</td>
                                <td><span class="badge badge-secondary" style="font-size:14px;">${item.SoLuongMua}</span></td>
                                <td class="text-danger font-weight-bold">${item.ThanhTien.toLocaleString('vi-VN')} đ</td>
                            </tr>
                        `);
        total += item.ThanhTien;
                    });

        $("#hd-body").append(`
        <tr class="bg-light">
            <td colspan="4" class="text-right font-weight-bold" style="font-size:16px;">Tổng cộng:</td>
            <td class="text-danger font-weight-bold" style="font-size:18px;">${total.toLocaleString("vi-VN")} đ</td>
        </tr>
        `);

        $('#modalChiTiet').modal('show');
                },
        error: function (xhr) {
            swal("Lỗi!", "Không lấy được dữ liệu hóa đơn", "error");
        console.log(xhr.responseText);
                }
            });
        }
 
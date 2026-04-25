
$(document).ready(function () {
    $('#live-search-input').on('keyup', function () {
        var keyword = $(this).val().trim();

        if (keyword.length > 1) {
            $.ajax({
                url: '/Home/LiveSearch',
                type: 'GET',
                data: { keyword: keyword },
                success: function (res) {
                    // Res bây giờ là một mảng JSON: [{MaSP: 1, TenSP: "..."}, ...]
                    if (res.length > 0) {
                        let html = '<ul class="list-unstyled mb-0">';

                        res.forEach(function (item) {
                            // Tính toán giá tiền
                            let giaKhuyenMai = item.PhanTramGiam > 0 ? item.GiaGoc * (1 - item.PhanTramGiam / 100) : item.GiaGoc;

                            // Format tiền tệ chuẩn VNĐ trong JS
                            let formatGiaGoc = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(item.GiaGoc);
                            let formatGiaKM = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(giaKhuyenMai);

                            html += `
                                    <li class="p-2 border-bottom search-item-row" style="transition: 0.2s;">
                                        <a href="/Product/ProductDetail/${item.MaSP}" class="d-flex align-items-center text-decoration-none text-dark" style="gap: 15px;">
                                            <img src="${item.HinhAnh}" onerror="this.src='/Content/Images/no-image.png'" style="width: 50px; height: 50px; object-fit: contain; border-radius: 4px; border: 1px solid #eaeaea;">
                                            <div style="flex: 1; overflow: hidden;">
                                                <h6 class="mb-1 text-truncate" style="font-size: 14px; font-weight: 600;">${item.TenSP}</h6>
                                                <small class="text-muted d-block" style="font-size: 12px;">${item.TenDM}</small>
                                            </div>
                                            <div class="price-box pr-2 text-right">`;

                            if (item.PhanTramGiam > 0) {
                                html += `
                                                <span class="d-block text-danger font-weight-bold" style="font-size: 14px;">${formatGiaKM}</span>
                                                <div class="d-flex justify-content-end align-items-center mt-1">
                                                    <del class="text-muted mr-1" style="font-size: 11px;">${formatGiaGoc}</del>
                                                    <span class="badge badge-danger" style="font-size: 10px; padding: 2px 4px;">-${item.PhanTramGiam}%</span>
                                                </div>`;
                            } else {
                                html += `   <span class="d-block text-danger font-weight-bold" style="font-size: 14px;">${formatGiaGoc}</span>`;
                            }

                            html += `       </div>
                                        </a>
                                    </li>`;
                        });

                        html += `</ul>
                                     <div class="text-center p-2 bg-light">
                                        <a href="#" class="text-danger text-decoration-none" style="font-size: 13px; font-weight: bold;">Xem tất cả kết quả</a>
                                     </div>`;

                        $('#search-results-container').html(html).show();
                    } else {
                        $('#search-results-container').html('<div class="p-3 text-center text-muted">Không tìm thấy sản phẩm nào.</div>').show();
                    }
                },
                error: function () { console.log("Lỗi gọi AJAX tìm kiếm"); }
            });
        } else {
            $('#search-results-container').hide();
        }
    });

    $(document).on('click', function (e) {
        if (!$(e.target).closest('.flex-grow-1').length) {
            $('#search-results-container').hide();
        }
    });
});
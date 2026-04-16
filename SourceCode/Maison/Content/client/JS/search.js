
    $(document).ready(function () {
        // Khi người dùng gõ vào ô tìm kiếm
        $('#live-search-input').on('keyup', function () {
            var keyword = $(this).val().trim();

            // Chỉ bắt đầu tìm nếu gõ từ 2 ký tự trở lên cho đỡ nặng Server
            if (keyword.length > 1) {
                $.ajax({
                    url: '/Home/LiveSearch',
                    type: 'GET',
                    data: { keyword: keyword },
                    success: function (res) {
                        if (res.trim() !== "") {
                            // Hiện kết quả
                            $('#search-results-container').html(res).show();
                        } else {
                            // Không có sản phẩm nào
                            $('#search-results-container').html('<div class="p-3 text-center text-muted">Không tìm thấy sản phẩm nào.</div>').show();
                        }
                    }
                });
            } else {
                // Nếu xóa trắng thì ẩn khung đi
                $('#search-results-container').hide();
            }
        });

    // Bấm ra ngoài khoảng trắng thì tự động ẩn khung tìm kiếm
    $(document).on('click', function (e) {
            if (!$(e.target).closest('.flex-grow-1').length) {
        $('#search-results-container').hide();
            }
        });
    });

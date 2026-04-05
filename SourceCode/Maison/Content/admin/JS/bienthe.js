// ========================================================
// HÀM DÙNG CHUNG: VẼ MÃ HTML CHO KHU VỰC THÔNG SỐ KỸ THUẬT
// ========================================================
function renderAttributeHTML(attributesData, currentSelected = []) {
    let html = '';
    attributesData.forEach(function (item) {
        let selectedMaGT = '';
        if (currentSelected.length > 0) {
            let matched = currentSelected.find(c => c.MaTT == item.MaTT);
            if (matched) selectedMaGT = matched.MaGT;
        }

        let options = `<option value="">-- Bỏ qua --</option>`;
        item.GiaTris.forEach(function (val) {
            let isSelected = (val.MaGT == selectedMaGT) ? "selected" : "";
            options += `<option value="${val.MaGT}" ${isSelected}>${val.GiaTri}</option>`;
        });

        options += `<option value="NEW" class="text-primary font-weight-bold">➕ Thêm giá trị mới...</option>`;

        html += `
            <div class="form-group mb-3 p-3 border rounded shadow-sm" style="background-color: #f8f9fa;">
                <label class="font-weight-bold text-dark">${item.TenTT}:</label>
                
                <div class="d-flex align-items-center mb-2">
                    <select class="form-control" name="ThuocTinh_Select_${item.MaTT}" id="Select_${item.MaTT}" onchange="toggleNewInput(this, ${item.MaTT})">
                        ${options}
                    </select>
                    
                    <button type="button" class="btn btn-sm btn-outline-info ml-2 px-3" onclick="suaGiaTriNhanh(${item.MaTT})" title="Sửa chính tả">
                        <i class="fas fa-edit"></i>
                    </button>
                </div>

                <div id="DivNewInput_${item.MaTT}" style="display: none; margin-top: 10px;">
                    <div class="input-group">
                        <input type="text" class="form-control border-primary" id="NewInput_${item.MaTT}" placeholder="Nhập ${item.TenTT} mới (VD: RTX 4060)..." />
                        <div class="input-group-append">
                            <button class="btn btn-primary" type="button" onclick="luuGiaTriNhanh(${item.MaTT})">
                                <i class="fas fa-check mr-1"></i> Lưu thông số
                            </button>
                        </div>
                    </div>
                    <small id="MsgNew_${item.MaTT}" class="font-weight-bold mt-1 d-block"></small>
                </div>
            </div>
        `;
    });
    return html;
}

// ===================================================
// HÀM XỬ LÝ LƯU NHANH GIÁ TRỊ VỪA GÕ
// ===================================================
function luuGiaTriNhanh(maTT) {
    let inputVal = $("#NewInput_" + maTT).val();
    let msgBox = $("#MsgNew_" + maTT);

    if (inputVal.trim() === "") {
        msgBox.removeClass("text-success").addClass("text-danger").text("Vui lòng gõ thông số vào ô trống!");
        return;
    }

    // Gọi API lưu vào DB
    $.post('/BienThes/ThemGiaTriNhanh', { maTT: maTT, giaTri: inputVal }, function (res) {
        if (res.status == true) {
            // 1. Hiện thông báo thành công màu xanh lá
            msgBox.removeClass("text-danger").addClass("text-success").text(res.message);

            // 2. Chèn thẻ Option mới đó vào cái Dropdown
            let selectBox = $("#Select_" + maTT);
            let newOption = `<option value="${res.id}">${res.text}</option>`;

            // Chèn nó lên ngay trên cái nút "➕ Thêm giá trị mới"
            selectBox.find("option[value='NEW']").before(newOption);

            // 3. Tự động Chọn luôn cái giá trị vừa thêm
            selectBox.val(res.id);

            // 4. Xóa trắng ô input và từ từ ẩn khu vực thêm mới đi (Nhường chỗ nhập form khác)
            $("#NewInput_" + maTT).val("");
            setTimeout(function () {
                $("#DivNewInput_" + maTT).slideUp();
                msgBox.text("");
            }, 1000); // Đợi 1 giây cho người ta đọc chữ "Đã lưu!" rồi mới giấu đi

        } else {
            msgBox.removeClass("text-success").addClass("text-danger").text(res.message);
        }
    });
}

// Nhớ sửa lại hàm toggle để nó hiện/ẩn nguyên cái cụm `DivNewInput` nhé:
function toggleNewInput(selectObj, maTT) {
    var divInput = document.getElementById("DivNewInput_" + maTT);
    if (selectObj.value === "NEW") {
        divInput.style.display = "block";
        $("#NewInput_" + maTT).focus(); // Tự động trỏ chuột vào ô gõ luôn cho tiện
    } else {
        divInput.style.display = "none";
    }
}

// ========================================================
// NGHIỆP VỤ: THÊM BIẾN THỂ MỚI
// ========================================================
// Bắt sự kiện bấm nút "Thêm" để load giao diện Input động
document.getElementById('addpopup').addEventListener('click', function () {
    let maDM = $("#current_MaDM").val();
    $("#dynamic-attributes-add").html('<span class="text-muted font-italic">Đang tải thông số...</span>');

    $.ajax({
        url: '/BienThes/LoadThuocTinhTheoDanhMuc',
        type: 'POST',
        data: { maDM: maDM },
        success: function (res) {
            if (res.length === 0) {
                $("#dynamic-attributes-add").html('<span class="text-danger">Danh mục này chưa được cài đặt thuộc tính nào. Vui lòng quay lại Quản lý Thuộc Tính.</span>');
            } else {
                $("#dynamic-attributes-add").html(renderAttributeHTML(res));
            }

            // Ép mở Popup (phòng hờ addpopup.js chạy không kịp)
            document.getElementById('popup').style.display = 'block';
            document.getElementById('overlay').style.display = 'block';
        }
    });
});

// Submit Form Thêm
function themBienThe() {
    let formData = new FormData(document.getElementById('add-form'));

    $.ajax({
        url: '/BienThes/Create',
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
        error: function (xhr) { console.log(xhr.responseText); }
    });
    return false;
}

// ========================================================
// NGHIỆP VỤ: SỬA BIẾN THỂ
// ========================================================
// Bắt sự kiện bấm nút "Sửa"
function loaddata(id) {
    $("#update-message").removeClass("text-success text-danger").html("");
    $("#dynamic-attributes-edit").html('<span class="text-muted font-italic">Đang tải thông số...</span>');

    // 1. Lấy thông tin cơ bản của biến thể
    $.ajax({
        type: 'POST',
        data: { "id": id },
        url: '/BienThes/Loaddata',
        success: function (bt) {
            $("#mabt").val(bt.MaBT);
            $("#giaban").val(bt.GiaBan);
            $("#soluongton").val(bt.SoLuongTon);
            loadGallery(id);
            // 2. Lấy bộ khung Thuộc Tính và lồng ghép mảng thông số biến thể đang có vào
            let maDM = $("#current_MaDM").val();
            $.ajax({
                url: '/BienThes/LoadThuocTinhTheoDanhMuc',
                type: 'POST',
                data: { maDM: maDM },
                success: function (attributesData) {
                    if (attributesData.length === 0) {
                        $("#dynamic-attributes-edit").html('<span class="text-danger">Không có thuộc tính.</span>');
                    } else {
                        $("#dynamic-attributes-edit").html(renderAttributeHTML(attributesData, bt.CacThuocTinhDangChon));
                    }

                    // Mở popup Sửa
                    document.getElementById('changepopup').style.display = 'block';
                    document.getElementById('overlay').style.display = 'block';
                }
            });
        },
        error: function () { alert("Lỗi tải dữ liệu biến thể..."); }
    });
}

// Submit Form Sửa
function suaBienThe() {
    let formData = new FormData(document.getElementById('update-form'));

    // =================================================================
    // BƯỚC 2 ĐƯỢC CHÈN VÀO ĐÂY: Gom thứ tự ảnh phụ sau khi kéo thả
    // =================================================================
    $('#gallery-container .gallery-item').each(function () {
        // Hàm này sẽ nhặt từng cái ID của ảnh (từ trái qua phải) 
        // và nhét vào cái hộp mang tên "sortedGalleryIds" để gửi lên C#
        formData.append("sortedGalleryIds", $(this).data('id'));
    });
    // =================================================================

    $.ajax({
        url: '/BienThes/Update',
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
        error: function (xhr) { console.log(xhr.responseText); }
    });
    return false;
}
// ========================================================
// NGHIỆP VỤ: XÓA BIẾN THỂ
// ========================================================
function deleteData(id) {
    $("#delete-mabt").val(id);
    document.getElementById('deletepopup').style.display = 'block';
    document.getElementById('overlay').style.display = 'block';
}

function xoaBienThe() {
    let id = $("#delete-mabt").val();
    $.ajax({
        type: 'POST',
        data: { "id": id },
        url: '/BienThes/Delete',
        success: function (res) {
            if (res.status == true) {
                // Tắt popup
                $(".cancelPopup").click();
                // Bay màu row bằng class (vì trong vòng lặp foreach View bạn dùng class="row-@item.MaBT")
                $(".row-" + id).fadeOut(300, function () { $(this).remove(); });
            } else {
                alert(res.message);
                $(".cancelPopup").click();
            }
        },
        error: function () { alert("Lỗi xóa dữ liệu..."); }
    });
}
// Bấm nút sửa cạnh Dropdown
function suaGiaTriNhanh(maTT) {
    let selectBox = document.getElementById('Select_' + maTT);
    let maGT = selectBox.value;

    // Nếu người dùng chưa chọn gì, hoặc đang chọn chữ "Thêm mới..." thì cấm sửa
    if (!maGT || maGT === "NEW") {
        alert("Vui lòng chọn một giá trị có sẵn (VD: rtx3050) để sửa!");
        return;
    }

    // Lấy chữ cũ đang hiển thị
    let textCu = selectBox.options[selectBox.selectedIndex].text;

    // Bật Popup nhỏ mặc định của trình duyệt để gõ
    let textMoi = prompt("Sửa lỗi chính tả (Nó sẽ tự động cập nhật cho TẤT CẢ các sản phẩm đang dùng):", textCu);

    // Nếu gõ chữ mới và bấm OK
    if (textMoi != null && textMoi.trim() !== "" && textMoi !== textCu) {
        $.ajax({
            url: '/BienThes/SuaGiaTriNhanh',
            type: 'POST',
            data: { maGT: maGT, giaTriMoi: textMoi },
            success: function (res) {
                if (res.status == true) {
                    // Cập nhật giao diện lập tức: Sửa lại chữ hiển thị trong Dropdown luôn
                    selectBox.options[selectBox.selectedIndex].text = textMoi;
                } else {
                    alert(res.message);
                }
            }
        });
    }
}
// Hàm tải danh sách ảnh phụ vào Popup
function loadGallery(maBT) {
    let container = document.getElementById('gallery-container');
    container.innerHTML = '<span class="text-muted">Đang tải ảnh...</span>';

    $.get('/Admin/BienThes/GetGallery', { maBT: maBT }, function (data) {
        container.innerHTML = ''; // Xóa chữ Đang tải
        if (data.length === 0) {
            container.innerHTML = '<span class="text-muted">Cấu hình này chưa có ảnh phụ.</span>';
            return;
        }

        // Vẽ từng bức ảnh ra
        data.forEach(function (img) {
            let html = `
                <div class="gallery-item" data-id="${img.MaAnh}" style="position: relative; cursor: grab;">
                    <img src="${img.DuongDanAnh}" style="width: 80px; height: 80px; object-fit: cover; border-radius: 5px; border: 1px solid #aaa;" onerror="this.src='/Content/Images/no-image.png'" />
                    <button type="button" class="btn btn-sm btn-danger p-0" style="position: absolute; top: -5px; right: -5px; width: 20px; height: 20px; border-radius: 50%;" onclick="deleteGalleryImage(${img.MaAnh}, this)" title="Xóa ảnh này">
                        <i class="fas fa-times" style="font-size: 10px;"></i>
                    </button>
                </div>
            `;
            container.insertAdjacentHTML('beforeend', html);
        });

        // BẬT TÍNH NĂNG KÉO THẢ (SORTABLE)
        new Sortable(container, {
            animation: 150, // Hiệu ứng mượt
            ghostClass: 'bg-light',
            //onEnd: function () {
            //    // Khi thả chuột ra, lưu thứ tự mới ngay lập tức
            //    saveGalleryOrder();
            //}
        });
    });
}

// Hàm Xóa ảnh
function deleteGalleryImage(maAnh, btnElement) {
    if (confirm("Bạn có chắc muốn xóa ảnh phụ này?")) {
        $.post('/Admin/BienThes/DeleteGalleryImage', { maAnh: maAnh }, function (res) {
            if (res.status == true) {
                // Xóa cái cục HTML chứa ảnh đó đi cho mượt (không cần reload trang)
                $(btnElement).parent('.gallery-item').fadeOut(300, function () { $(this).remove(); });
            } else {
                alert("Lỗi khi xóa ảnh!");
            }
        });
    }
}

// Hàm lấy thứ tự hiện tại và gửi lên Server
function saveGalleryOrder() {
    let sortedIds = [];
    // Quét tất cả thẻ có class gallery-item để lấy MaAnh theo thứ tự từ trái sang phải
    $('#gallery-container .gallery-item').each(function () {
        sortedIds.push($(this).data('id'));
    });

    // Gửi mảng ID lên API
    $.post('/Admin/BienThes/UpdateGalleryOrder', { sortedIds: sortedIds }, function (res) {
        if (res.status == true) {
            console.log("Đã lưu thứ tự mới thành công!");
        }
    });
}
function autoUploadGallery() {
    let maBT = $("#mabt").val(); // Lấy ID biến thể đang sửa
    let fileInput = document.getElementById('GalleryInputAjax');
    let files = fileInput.files;

    if (files.length === 0) return;

    // Tạo FormData để gửi file qua Ajax
    let formData = new FormData();
    formData.append("maBT", maBT);
    for (let i = 0; i < files.length; i++) {
        formData.append("files", files[i]);
    }

    // Hiển thị trạng thái đang xử lý cho chuyên nghiệp
    let container = document.getElementById('gallery-container');
    container.insertAdjacentHTML('afterbegin', '<div id="upload-loading" class="text-primary font-weight-bold w-100">🔄 Đang tải ảnh lên...</div>');

    $.ajax({
        url: '/Admin/BienThes/UploadGalleryAjax',
        type: 'POST',
        data: formData,
        contentType: false,
        processData: false,
        success: function (res) {
            if (res.status) {
                // Xóa nội dung cũ của ô input file để có thể chọn tiếp lần sau
                fileInput.value = "";
                // Gọi lại hàm loadGallery để vẽ lại đống ảnh (đã bao gồm ảnh mới)
                loadGallery(maBT);
            } else {
                alert(res.message);
                $("#upload-loading").remove();
            }
        },
        error: function () {
            alert("Lỗi khi tải ảnh lên server!");
            $("#upload-loading").remove();
        }
    });
}
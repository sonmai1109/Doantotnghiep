using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Linq;
using System.Data.Entity;
using Maison.Models;
using Maison.Session;
using System.Text.RegularExpressions;

namespace Maison.Controllers
{
    public class ChatbotController : Controller
    {
        private readonly string _apiKey = "AIzaSyAuXnX5ozLyV27QTThdRNR2hg0BOl7v3kY";
        private readonly string _apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key=";
        private shopdb db = new shopdb();

        // ========================================================
        // 1. API: LẤY LỊCH SỬ CHAT TỪ SESSION
        // ========================================================
        [HttpGet]
        public JsonResult GetChatHistory()
        {
            var history = Session[ConstainBot.BOT_HISTORY] as List<ChatMsg>;
            if (history == null)
            {
                history = new List<ChatMsg>();
                history.Add(new ChatMsg { Type = "bot", Content = "Dạ chào anh/chị, em là trợ lý ảo của MTS Shop. Em có thể giúp gì cho anh/chị ạ?" });
                Session[ConstainBot.BOT_HISTORY] = history;
            }
            return Json(history, JsonRequestBehavior.AllowGet);
        }

        // ========================================================
        // 2. API: GIAO TIẾP VỚI GEMINI
        // ========================================================
        [HttpPost]
        public async Task<JsonResult> GiaoTiepGemini(string tinNhanKhachHang)
        {
            try
            {
                if (string.IsNullOrEmpty(tinNhanKhachHang))
                    return Json(new { status = false, message = "Vui lòng nhập tin nhắn." });

                var history = Session[ConstainBot.BOT_HISTORY] as List<ChatMsg> ?? new List<ChatMsg>();
                history.Add(new ChatMsg { Type = "user", Content = tinNhanKhachHang });
                Session[ConstainBot.BOT_HISTORY] = history;

                DateTime gioHienTai = DateTime.Now;

                // 1. LẤY SẢN PHẨM & DỮ LIỆU LIÊN QUAN
                //CÁCH 1: KHI MẠNG KHỎE
                //var danhSachSP = db.Sanphams
                //    .Include(s => s.BienThes.Select(b => b.ChiTietBTs.Select(c => c.GiaTriTT.ThuocTinh)))
                //    .Include(s => s.SanPhamKhuyenMais.Select(sk => sk.KhuyenMai))
                //    .Take(25).ToList();
                // Cách 2 


                // Đảm bảo tin nhắn đã viết thường
                string msg = tinNhanKhachHang.ToLower();

                var query = db.Sanphams
                    .Include(s => s.BienThes.Select(b => b.ChiTietBTs.Select(c => c.GiaTriTT.ThuocTinh)))
                    .Include(s => s.SanPhamKhuyenMais.Select(sk => sk.KhuyenMai))
                    .AsQueryable();

                // ==========================================================
                // A. NHẬN DIỆN LOẠI SẢN PHẨM (Hỗ trợ không dấu + Đồng nghĩa)
                // ==========================================================
                // Cấu trúc: { "từ khách gõ", "Từ chuẩn có dấu trong Database" }
                Dictionary<string, string> tuKhoaDanhMuc = new Dictionary<string, string>()
{
    { "chuot", "chuột" }, { "chuột", "chuột" },
    { "phim", "phím" }, { "phím", "phím" },
    { "ban phim", "phím" }, { "bàn phím", "phím" },
    { "tai nghe", "tai nghe" },
    { "loa", "loa" },
    { "tay cam", "tay cầm" }, { "tay cầm", "tay cầm" },
    { "man hinh", "màn hình" }, { "màn hình", "màn hình" },
    { "pc", "pc" }, { "laptop", "laptop" },
    { "vga", "vga" }, { "gpu", "gpu" }
};

                string loaiCanTimTrongDB = null;
                foreach (var item in tuKhoaDanhMuc)
                {
                    if (msg.Contains(item.Key))
                    {
                        loaiCanTimTrongDB = item.Value; // Bắt được rồi thì lấy từ chuẩn ra
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(loaiCanTimTrongDB))
                {
                    // Cầm từ chuẩn có dấu chui vào DB tìm, bao trúng!
                    query = query.Where(s => s.TenSP.ToLower().Contains(loaiCanTimTrongDB));
                }

                // ==========================================================
                // B. Nhận diện THƯƠNG HIỆU (Như cũ)
                // ==========================================================
                string[] thuongHieu = { "acer", "asus", "dell", "hp", "lenovo", "macbook", "msi", "logitech", "razer", "corsair", "dareu", "akko" };
                string hangCanTim = thuongHieu.FirstOrDefault(h => msg.Contains(h));

                if (!string.IsNullOrEmpty(hangCanTim))
                {
                    query = query.Where(s => s.TenSP.ToLower().Contains(hangCanTim));
                }

                // ==========================================================
                // C. Xử lý yêu cầu TÌM HÀNG GIẢM GIÁ (Thêm các từ không dấu)
                // ==========================================================
                if (msg.Contains("giảm") || msg.Contains("giam") ||
                    msg.Contains("sale") ||
                    msg.Contains("khuyến mãi") || msg.Contains("khuyen mai") ||
                    msg.Contains("rẻ") || msg.Contains("re"))
                {
                    query = query.Where(s => s.SanPhamKhuyenMais.Any(sk =>
                        sk.KhuyenMai.TrangThai == 1 &&
                        sk.KhuyenMai.NgayBatDau <= gioHienTai &&
                        sk.KhuyenMai.NgayKetThuc >= gioHienTai));
                }

                // D. XUẤT DỮ LIỆU CHÍNH XÁC CHO AI (Take 4-5 sản phẩm để tối ưu Token)
                var danhSachSP = query.OrderByDescending(s => s.MaSP).Take(5).ToList();

                // E. BACKUP: Nếu khách chỉ "Hello" hoặc hỏi mông lung, hoặc lọc không ra món nào
                if (danhSachSP.Count == 0)
                {
                    danhSachSP = db.Sanphams
                        .Include(s => s.BienThes.Select(b => b.ChiTietBTs.Select(c => c.GiaTriTT.ThuocTinh)))
                        .Include(s => s.SanPhamKhuyenMais.Select(sk => sk.KhuyenMai))
                        .OrderByDescending(s => s.MaSP).Take(4).ToList(); // Trả về 4 món mới nhất của shop
                }


                // Cách 2 

                string thongTinKhoHang = "";
                foreach (var sp in danhSachSP)
                {
                    thongTinKhoHang += $"--- SẢN PHẨM: {sp.TenSP} ---\n";

                    var cacBienThe = sp.BienThes.Where(b => b.SoLuongTon > 0).ToList();
                    foreach (var bt in cacBienThe)
                    {
                        // A. Lấy cấu hình chi tiết (CPU, RAM...)
                        var chiTiet = bt.ChiTietBTs
                            .Where(ct => ct.GiaTriTT.ThuocTinh.LaThuocTinhChinh == true)
                            .OrderBy(ct => ct.GiaTriTT.ThuocTinh.ThuTuHienThi)
                            .Select(ct => $"{ct.GiaTriTT.ThuocTinh.TenTT}: {ct.GiaTriTT.GiaTri}");
                        string cauHinhRieng = string.Join(", ", chiTiet);

                        // B. TÍNH KHUYẾN MÃI CHÍNH XÁC CHO TỪNG BIẾN THỂ
                        // B. TÍNH KHUYẾN MÃI CHÍNH XÁC CHO TỪNG BIẾN THỂ
                        decimal giaGoc = bt.GiaBan;
                        decimal giaSale = giaGoc;
                        string tenKM = "Không có";
                        int phanTramGiam = 0;

                        // SỬA LẠI ĐOẠN NÀY: Cho phép nhận khuyến mãi của riêng Biến Thể, HOẶC khuyến mãi chung của cả Sản Phẩm (MaBT == null)
                        var kmDungChoBT = sp.SanPhamKhuyenMais
                            .Where(sk => (sk.MaBT == bt.MaBT || sk.MaBT == null || sk.MaBT == 0) // Mở rộng điều kiện
                                         && sk.KhuyenMai.TrangThai == 1
                                         && sk.KhuyenMai.NgayBatDau <= gioHienTai
                                         && sk.KhuyenMai.NgayKetThuc >= gioHienTai)
                            .OrderByDescending(sk => sk.PhanTramGiam).FirstOrDefault();

                        if (kmDungChoBT != null)
                        {
                            phanTramGiam = (int)kmDungChoBT.PhanTramGiam;
                            giaSale = giaGoc - (giaGoc * (decimal)phanTramGiam / 100);
                            tenKM = kmDungChoBT.KhuyenMai.TenKM;
                        }

                        thongTinKhoHang += $"+ Phiên bản: [{cauHinhRieng}] | Giá gốc: {giaGoc:N0}đ | Giá Sale: {giaSale:N0}đ | KM: {tenKM} | Giam: {phanTramGiam}% | MãSP: {sp.MaSP} | LinkẢnh: {sp.HinhAnh}\n";
                    }
                    thongTinKhoHang += "\n";
                }

                // 2. KỊCH BẢN ÉP AI LÀM ĐỎ TOÀN BỘ THÔNG TIN KM
                string kichBan = $@"Bạn là trợ lý bán hàng MTS Shop. 
         Dữ liệu kho: {thongTinKhoHang}

         QUY TẮC HIỂN THỊ (BẮT BUỘC):
         1. PHÂN BIỆT CẤU HÌNH: Khách hỏi i5 báo đúng bản i5, i7 báo đúng i7. Đừng báo nhầm giá bản này cho bản kia.
         2. LÀM ĐỎ THÔNG TIN: Mọi thông tin về giá và khuyến mãi PHẢI nằm trong thẻ span màu đỏ: <span style=""color:red;font-weight:bold"">nội dung</span>.
            - Làm đỏ Giá gốc (Ví dụ: <span style=""color:red;font-weight:bold"">21,790,000đ</span>).
            - Làm đỏ Giá Sale (Ví dụ: <span style=""color:red;font-weight:bold"">20,264,700đ</span>).
            - Làm đỏ Phần trăm giảm (Ví dụ: <span style=""color:red;font-weight:bold"">Giảm 7%</span>).
            - Làm đỏ Tên chương trình và trong ngoặc kép (Ví dụ: <span style=""color:red;font-weight:bold"">Vui Tết Tràn Ngập</span>).
            -Tự động sắp xếp các chữ cho đẹp mắt,gọn gàng.
         3.  HTML CARD: Khi khách quan tâm mẫu nào , Băt buộc chèn Card này ở cuối câu trả lời. 
         LẤY đúng 'LinkẢnh' và 'MãSP' từ dữ liệu bên trên để điền vào:
         <div class='product-chat-card'>
             <a href='/Product/ProductDetail/[MãSP]' target='_blank'>
                 <img src='[LinkẢnh]' alt='[Tên]' style='width:100%; max-width:200px;' />
                 <p>[Tên]</p>
                 <span class='text-danger font-weight-bold'>[Giá Sale]</span>
             </a>
         </div>
         4. PHONG CÁCH: Dạ, thưa, anh/chị. Ngắn gọn, tập trung vào việc chốt đơn.
        ";

                var payload = new
                {
                    // Sửa chữ 'i' thành in hoa và bỏ dấu gạch dưới
                    systemInstruction = new { parts = new[] { new { text = kichBan } } },
                    contents = new[] { new { role = "user", parts = new[] { new { text = tinNhanKhachHang } } } }
                };

                using (var client = new HttpClient())
                {
                    var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(_apiUrl + _apiKey, content);
                    string responseString = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        dynamic resultData = JsonConvert.DeserializeObject(responseString);
                        string cauTraLoiBot = resultData.candidates[0].content.parts[0].text;
                        cauTraLoiBot = cauTraLoiBot.Replace("**", "").Replace("```html", "").Replace("```", "");

                        history.Add(new ChatMsg { Type = "bot", Content = cauTraLoiBot });
                        Session[ConstainBot.BOT_HISTORY] = history;
                        return Json(new { status = true, reply = cauTraLoiBot });
                    }
                    // ĐOẠN CODE CŨ CỦA BẠN:
                    else { return Json(new { status = false, message = "Hệ thống đang bận ạ!" }); }

                    // SỬA THÀNH để biết lỗi chính:
                    //else
                    //{
                    //    return Json(new { status = false, message = "Lỗi từ API: " + responseString });
                    //}
                }
            }
            catch (Exception ex) { return Json(new { status = false, message = "Lỗi: " + ex.Message }); }
        }
       
    }
}
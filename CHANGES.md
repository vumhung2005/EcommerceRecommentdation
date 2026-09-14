# Các lỗi đã sửa

## 1. Không hiển thị tên thương hiệu / danh mục
- **Nguyên nhân:** Trang chi tiết sản phẩm (`ProductDetail.jsx`) không hề render `p.brand`/`p.category` dù backend đã trả về đầy đủ; bảng sản phẩm trong Admin (`AdminDashboard.jsx`) thiếu hẳn cột "Thương hiệu".
- **Đã sửa:**
  - `ProductDetail.jsx`: thêm dòng hiển thị "Danh mục / Thương hiệu".
  - `AdminDashboard.jsx`: thêm cột "Thương hiệu" vào bảng sản phẩm; tên danh mục/thương hiệu giờ có fallback tra theo `categoryId`/`brandId` từ danh sách `cats`/`brands` đã load, phòng trường hợp navigation property null.
  - `ProductController.cs`: thêm `.AsNoTracking()` cho `GetProducts()`/`GetProduct()` để tránh EF Core dùng chung instance Category/Brand giữa nhiều Product (gây lỗi tham chiếu vòng khi serialize JSON).

## 2. Nút "Trở thành người bán" không hoạt động
- **Nguyên nhân:** `Header.jsx` import hàm `becomeSeller` từ `services/api.js`, nhưng file này **chưa từng export** hàm đó (chỉ có `makeAdmin`). Import một named export không tồn tại làm lỗi module ES, nút bấm không có tác dụng.
- **Đã sửa:** thêm hàm `becomeSeller()` vào `api.js`, gọi đúng endpoint backend sẵn có `POST /api/auth/become-seller`, lưu lại token/user mới sau khi có role Seller.
- **Lưu ý:** cần đảm bảo database đã áp dụng migration `SellerOrderPaymentEnhancements` (cột `SellerStatus`) — chạy `dotnet ef database update` hoặc script `DATABASE_UPDATE_SELLER_PAYMENT.sql` nếu chưa.

## 3. Admin chưa xác nhận đơn hàng Pending → đã mua (Completed) + biểu đồ doanh thu
- Luồng trạng thái đơn hàng nhiều bước (Pending → Confirmed → Shipping → Completed) và biểu đồ doanh thu 30 ngày **đã có sẵn** trong code, chỉ cần chạy đúng bản build này là sẽ thấy.
- **Bổ sung thêm:** cho phép Admin xác nhận **thẳng Pending → Completed** bằng 1 nút "🛍️ Xác nhận đã mua" (không bắt buộc phải đi qua Confirmed/Shipping), đồng thời vẫn giữ nút xác nhận từng bước cho ai muốn theo dõi giao hàng chi tiết. Tồn kho được trừ đúng 1 lần dù đi tắt hay đi từng bước.

## 4. Thanh toán từng sản phẩm trong giỏ hàng + chỉ cho đánh giá sau khi thanh toán xong
- Việc "chỉ cho đánh giá sau khi thanh toán xong" **đã được backend kiểm soát đúng** (`ReviewController`: chỉ cho review khi đơn `Status == Completed` và `Payment.Status == Paid`) — không cần sửa.
- **Đã thêm mới:** chọn từng sản phẩm trong giỏ hàng để thanh toán riêng, sản phẩm chưa chọn vẫn giữ lại trong giỏ:
  - `Cart.jsx`: thêm checkbox từng dòng + "Chọn tất cả", nút "Thanh toán (N) sản phẩm đã chọn".
  - `Checkout.jsx`: chỉ hiển thị/tính tiền các sản phẩm được chọn, gửi kèm `cartItemIds` khi tạo đơn.
  - `OrderController.cs` (`CreateOrder`): nhận thêm `CartItemIds` (không bắt buộc — bỏ trống vẫn thanh toán cả giỏ như cũ), chỉ xóa các CartItem đã thanh toán, giữ nguyên các sản phẩm còn lại trong giỏ.

## 5. Lỗi build migration: `CS0115: no suitable method found to override`
- **Nguyên nhân:** File `Migrations/20260901095000_SellerOrderPaymentEnhancements.Designer.cs` khai báo sai tên phương thức `BuildModel(ModelBuilder)` — tên này chỉ dùng cho `AppDbContextModelSnapshot.cs`. File Designer của **từng migration** phải override `BuildTargetModel(ModelBuilder)`, nên trình biên dịch báo không tìm thấy phương thức ảo nào để override.
- **Đã sửa:** đổi `BuildModel` → `BuildTargetModel` trong file Designer đó. Đã đối chiếu toàn bộ các migration khác — không còn file nào bị lỗi tương tự, và nội dung model trong Designer đã khớp 100% với snapshot.

---

**Về môi trường sandbox này:** và không có kết nối mạng để `npm install`/`dotnet restore`, nên mình không build/chạy thử trực tiếp được — đã rà tay từng file liên quan và kiểm tra cú pháp JS/ngoặc cân bằng. Sau khi tải về, chạy như README:
```
cd Ecommerce.Client && npm install && npm run dev
cd Ecommerce.API && dotnet restore && dotnet ef database update && dotnet run
```
Nếu còn lỗi cụ thể khi chạy (thông báo lỗi, console log), gửi lại cho mình để rà tiếp.

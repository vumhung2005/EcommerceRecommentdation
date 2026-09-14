# Ecommerce Recommendation - Frontend đã hoàn thiện

## Frontend
```powershell
cd Ecommerce.Client
npm install
npm run dev
```
Mở http://localhost:5173

## Backend
```powershell
cd Ecommerce.API
dotnet restore
dotnet run
```
API mặc định: http://localhost:5116

## Chức năng frontend đã bổ sung/kết nối
- Đăng nhập / đăng ký / đăng xuất và lưu JWT.
- Header hiển thị Admin khi tài khoản có role Admin.
- Danh sách sản phẩm, tìm kiếm, chi tiết sản phẩm.
- Click sản phẩm -> gọi `/api/products/{id}/click`.
- Xem sản phẩm -> backend ghi View.
- Thêm vào giỏ -> backend ghi AddToCart.
- Giỏ hàng: xem, tăng/giảm số lượng, xóa.
- Checkout -> tạo đơn và backend ghi Purchase.
- Danh sách đơn hàng và chi tiết đơn hàng.
- Đánh giá sản phẩm.
- Recommendation: ưu tiên recommendation đã lưu, nếu chưa có thì gọi recommendation hiện tại.
- Admin Dashboard: thống kê sản phẩm/tồn kho/đơn hàng, quản lý sản phẩm CRUD, danh mục CRUD, thương hiệu CRUD, cập nhật trạng thái đơn hàng, tạo recommendation.
- Admin thêm sản phẩm có upload ảnh qua `/api/products/upload-image` hoặc nhập đường dẫn ảnh.

## Ảnh sản phẩm
Backend đã có `wwwroot/images/products`. Khi chạy ASP.NET API, `UseStaticFiles()` phục vụ ảnh tại:
`http://localhost:5116/images/products/<ten-file>`

## Lưu ý
Không đóng gói `node_modules`, `venv`, `bin`, `obj` để file nén nhẹ hơn. Sau khi giải nén chạy `npm install` trong `Ecommerce.Client`.

Hệ thống gợi ý sản phẩm cá nhân hóa
Giới thiệu

Đây là dự án website thương mại điện tử được xây dựng nhằm cung cấp các chức năng mua sắm trực tuyến và tích hợp hệ thống gợi ý sản phẩm cá nhân hóa. Website cho phép người dùng xem và tìm kiếm sản phẩm, quản lý giỏ hàng, đặt hàng, đánh giá sản phẩm và nhận các sản phẩm được đề xuất dựa trên hành vi tương tác.
Hệ thống được xây dựng với mục tiêu tạo ra trải nghiệm mua sắm thuận tiện và hỗ trợ người dùng tìm được những sản phẩm phù hợp với nhu cầu.

Chức năng chính
Người dùng
Đăng ký tài khoản
Đăng nhập / đăng xuất
Đăng nhập bằng Google
Xem danh sách sản phẩm
Xem chi tiết sản phẩm
Tìm kiếm và lọc sản phẩm
Thêm sản phẩm vào giỏ hàng
Đặt hàng
Xem lịch sử đơn hàng
Đánh giá sản phẩm
Xem sản phẩm được đề xuất
Quản trị viên
Quản lý sản phẩm
Quản lý danh mục
Quản lý thương hiệu
Quản lý đơn hàng
Quản lý người dùng
Theo dõi dữ liệu hoạt động của hệ thống
Công nghệ sử dụng
Frontend: ReactJS, JavaScript, HTML, CSS
Backend: ASP.NET Core Web API, C#
Database: Microsoft SQL Server
Authentication: ASP.NET Identity, JWT
Recommendation: Python, Collaborative Filtering
API: RESTful API

Dữ liệu được sử dụng để phân tích mức độ tương tác giữa người dùng và sản phẩm. Thuật toán Collaborative Filtering được áp dụng để tính toán độ tương đồng và tạo danh sách sản phẩm đề xuất.

## Giao diện hệ thống

### Trang chủ

![Trang chủ](../Ecommerce.API/wwwroot/images/home.png)

### Sản phẩm mẫu

![Sản phẩm mẫu](../Ecommerce.API/wwwroot/images/products/samsung-galaxy-s26.jpg)

### Chi tiết sản phẩm

![Chi tiết sản phẩm](../Ecommerce.API/wwwroot/images/product-detail.png)

### Giỏ hàng

![Giỏ hàng](../Ecommerce.API/wwwroot/images/cart.png)

### Sản phẩm đề xuất

![Sản phẩm đề xuất](../Ecommerce.API/wwwroot/images/recommendation.png)

Cài đặt
Backend
cd Ecommerce.API
dotnet restore
dotnet run
Frontend
cd Ecommerce.Client
npm install
npm run dev
Recommendation API
cd Recommendation.AI
pip install -r requirements.txt
python app.py
Cơ sở dữ liệu

Website sử dụng Microsoft SQL Server để lưu trữ thông tin người dùng, sản phẩm, danh mục, thương hiệu, giỏ hàng, đơn hàng, đánh giá và dữ liệu hành vi người dùng.

Mã nguồn

GitHub Repository:

https://github.com/vumhung2005/EcommerceRecommentdation

Tác giả

Vũ Mạnh Hùng

Đồ án ngành – 2026
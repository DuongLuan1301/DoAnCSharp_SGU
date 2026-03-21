# DoAnCSharp_SGU
Đồ án môn học ngôn ngữ lập trình C# - 841423

Danh sách thành viên:
Nhữ Dương Luân - 3123411184
Huỳnh Tuấn Tài - 

Tên đồ án: Ứng dụng thuyết minh tự động quảng bá sản phẩm cho du khách du nước ngoài.

1) Giới thiệu:
- Ứng dụng là một hệ thống du lịch thông minh phục vụ du khách nước ngoài đang tham quan tại các địa điểm trung tâm (chợ, khu phố ăn uống, siêu thị,...), Hệ thống giúp du khách hiểu và khám phá các hàng quán địa phương gần vị trí hiện tại một cách tự động, mà không cần hiểu tiếng Việt hay phải hỏi người bán.

2) Ý tưởng:
- Du khách (Customer): họ sử dụng ứng dụng trên điện thoại di động được xây dựng bằng C# và framework .MAUI. Ứng dụng trên điện thoại sẽ định vị vị trí hiện tại của người dùng thông qua GPS, sau đó tìm kiếm các hàng quán trong một phạm vi bán kính nhất định (geofence logic) và hiển thị thông tin trên bản đồ hoặc danh sách (POIs list). Khi du khách chọn một quán, ứng dụng sẽ tự động đọc mô tả quán bằng giọng nói (TTS) theo ngôn ngữ mà họ chọn (ví dụ tiếng Anh, tiếng Pháp, tiếng Nhật…).
- Người bán hàng (Seller): Với mục đích quảng bá hình ảnh của hàng quán, họ sử dụng website để đăng ký và quản lý thông tin quán của mình (tên quán, vị trí, mô tả, hình ảnh, bán kính tiếp cận), gửi yêu cầu lên hệ thống để chờ duyệt.
- Quản trị viên (Admin): Hệ thống cung cấp giao diện quản trị để quản lý (CRUD) các dữ liệu hàng quán, tài khoản người bán. Đảm bảo dữ liệu quán được kiểm soát và chỉ hiển thị trên ứng dụng khi đã được phê duyệt.

3) Mục tiêu:
- Xây dựng một hệ thống đầy đủ chức năng nghiệp vụ cho 3 vai trò: du khách, người bán, admin.
- Thiết kế kiến trúc rõ ràng, chia lớp (UI – Logic – Data) để dễ bảo trì, dễ đọc code và dễ cộng tác trong nhóm.
- Hỗ trợ chạy trên localhost cho mục đích demo, nhưng cấu trúc thư mục, API, database được thiết kế chuẩn để sau này có thể nâng cấp lên môi trường thật.
- Hỗ trợ mở rộng về sau:
    + Thêm nhiều thành phố, nhiều loại địa điểm du lịch (quán ăn, quán nước, điểm tham quan, bảo tàng…).
    + Tăng số lượng ngôn ngữ hỗ trợ (Anh, Pháp, Nhật, Hàn, Trung…).
    + Tích hợp thêm các tính năng như đánh giá, lượt xem, lịch sử truy cập,.v.v

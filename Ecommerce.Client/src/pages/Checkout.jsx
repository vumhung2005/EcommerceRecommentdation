import { useEffect, useMemo, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { getCart, createOrder, getCurrentUser } from "../services/api";
import "./Checkout.css";

export default function Checkout() {
  const nav = useNavigate();
  const location = useLocation();
  const user = getCurrentUser();
  const selectedIds = location.state?.cartItemIds || null; // null = thanh toán toàn bộ giỏ hàng
  const [cart, setCart] = useState(null);
  const [form, setForm] = useState({
    receiverName: user?.fullName || "",
    phone: user?.phoneNumber || "",
    shippingAddress: user?.address || "",
    paymentMethod: "COD",
  });
  const [loading, setLoading] = useState(true);
  const [placing, setPlacing] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    getCart()
      .then(setCart)
      .catch((e) => {
        setError(e.message);
        nav("/login");
      })
      .finally(() => setLoading(false));
  }, [nav]);

  const checkoutItems = useMemo(() => {
    if (!cart?.items) return [];
    if (!selectedIds) return cart.items;
    return cart.items.filter((item) => selectedIds.includes(item.cartItemId));
  }, [cart, selectedIds]);

  const total = useMemo(
    () =>
      checkoutItems.reduce((sum, item) => sum + Number(item.subtotal || 0), 0),
    [checkoutItems],
  );

  const submit = async (e) => {
    e.preventDefault();
    setError("");
    try {
      setPlacing(true);
      const result = await createOrder({ ...form, cartItemIds: selectedIds });
      nav(`/orders/${result.orderId}`);
    } catch (e) {
      setError(e.message);
    } finally {
      setPlacing(false);
    }
  };

  if (loading) {
    return <div className="page-message">Đang tải thông tin thanh toán...</div>;
  }

  if (!checkoutItems.length) {
    return (
      <div className="page-message">
        Không có sản phẩm để thanh toán.{" "}
        <button onClick={() => nav("/cart")}>Quay lại giỏ hàng</button>
      </div>
    );
  }

  return (
    <div className="checkout-page">
      <form className="checkout-card checkout-form" onSubmit={submit}>
        <h1>Thanh toán</h1>
        <p className="checkout-note">
          {selectedIds
            ? `Bạn đang thanh toán ${checkoutItems.length} sản phẩm đã chọn. Các sản phẩm còn lại vẫn giữ trong giỏ hàng.`
            : "Nhập thông tin giao hàng trước khi xác nhận đơn."}
        </p>

        <div className="checkout-fields">
          <label>
            Người nhận
            <input
              required
              value={form.receiverName}
              onChange={(e) =>
                setForm({ ...form, receiverName: e.target.value })
              }
            />
          </label>

          <label>
            Số điện thoại
            <input
              required
              value={form.phone}
              onChange={(e) => setForm({ ...form, phone: e.target.value })}
            />
          </label>

          <label className="full-field">
            Địa chỉ giao hàng
            <textarea
              required
              rows="3"
              value={form.shippingAddress}
              onChange={(e) =>
                setForm({ ...form, shippingAddress: e.target.value })
              }
            />
          </label>

          <label className="full-field">
            Phương thức thanh toán
            <select
              value={form.paymentMethod}
              onChange={(e) =>
                setForm({ ...form, paymentMethod: e.target.value })
              }
            >
              <option value="COD">Thanh toán khi nhận hàng (COD)</option>
              <option value="BankTransfer">Chuyển khoản ngân hàng</option>
            </select>
          </label>
        </div>

        <div className="checkout-summary">
          <h2>Thông tin đơn hàng</h2>
          {checkoutItems.map((item) => (
            <div className="checkout-row" key={item.cartItemId}>
              <span>
                {item.productName} × {item.quantity}
              </span>
              <strong>{Number(item.subtotal).toLocaleString("vi-VN")}₫</strong>
            </div>
          ))}
          <div className="checkout-total">
            <span>Tổng cộng</span>
            <strong>{total.toLocaleString("vi-VN")}₫</strong>
          </div>
        </div>

        {error && <div className="error-message">{error}</div>}
        <button type="submit" disabled={placing}>
          {placing ? "Đang tạo đơn..." : "✓ Xác nhận thanh toán & đặt hàng"}
        </button>
      </form>
    </div>
  );
}

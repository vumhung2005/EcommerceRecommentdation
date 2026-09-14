import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { getCart, updateCartItem, removeCartItem, token } from "../services/api";
import { SERVER_URL } from "../services/api";
import "./Cart.css";
export default function Cart() {
  const nav = useNavigate();
  const [cart, setCart] = useState({ items: [], totalAmount: 0 });
  const [selected, setSelected] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const load = async () => {
    try {
      setLoading(true);
      const c = await getCart();
      setCart(c);
      setSelected((c.items || []).map((i) => i.cartItemId));
    } catch (e) {
      setError(e.message);
      if (!token()) nav("/login");
    } finally {
      setLoading(false);
    }
  };
  useEffect(() => {
    load();
  }, []);

  const change = async (i, q) => {
    try {
      await updateCartItem(i, q);
      await load();
    } catch (e) {
      alert(e.message);
    }
  };
  const remove = async (i) => {
    if (!confirm("Xóa sản phẩm khỏi giỏ hàng?")) return;
    try {
      await removeCartItem(i);
      await load();
    } catch (e) {
      alert(e.message);
    }
  };

  const toggle = (id) =>
    setSelected((s) =>
      s.includes(id) ? s.filter((x) => x !== id) : [...s, id],
    );
  const toggleAll = () =>
    setSelected((s) =>
      s.length === cart.items.length ? [] : cart.items.map((i) => i.cartItemId),
    );

  const selectedItems = (cart.items || []).filter((i) =>
    selected.includes(i.cartItemId),
  );
  const selectedTotal = selectedItems.reduce(
    (s, i) => s + Number(i.subtotal || 0),
    0,
  );

  const goCheckout = () => {
    if (!selectedItems.length) {
      alert("Vui lòng chọn ít nhất 1 sản phẩm để thanh toán.");
      return;
    }
    nav("/checkout", {
      state: { cartItemIds: selectedItems.map((i) => i.cartItemId) },
    });
  };

  if (loading) return <div className="page-message">Đang tải giỏ hàng...</div>;
  return (
    <div className="cart-page">
      <div className="cart-container">
        <h1>🛒 Giỏ hàng</h1>
        {error ? (
          <div className="error-message">{error}</div>
        ) : !cart.items?.length ? (
          <div className="empty-cart">
            <h2>Giỏ hàng đang trống</h2>
            <Link to="/">Tiếp tục mua sắm</Link>
          </div>
        ) : (
          <div className="cart-layout">
            <div className="cart-list">
              <label className="cart-select-all">
                <input
                  type="checkbox"
                  checked={
                    selected.length === cart.items.length &&
                    cart.items.length > 0
                  }
                  onChange={toggleAll}
                />{" "}
                Chọn tất cả ({cart.items.length} sản phẩm)
              </label>
              {cart.items.map((i) => (
                <div className="cart-item" key={i.cartItemId}>
                  <label className="cart-item-check">
                    <input
                      type="checkbox"
                      checked={selected.includes(i.cartItemId)}
                      onChange={() => toggle(i.cartItemId)}
                    />
                  </label>
                  <img
                    src={
                      i.image
                        ? i.image.startsWith("http")
                          ? i.image
                          : `${SERVER_URL}${i.image}`
                        : ""
                    }
                    alt=""
                  />
                  <div className="cart-item-info">
                    <Link to={`/products/${i.productId}`}>{i.productName}</Link>
                    <strong>{Number(i.price).toLocaleString("vi-VN")}₫</strong>
                  </div>
                  <div className="quantity-control">
                    <button
                      disabled={i.quantity <= 1}
                      onClick={() => change(i.cartItemId, i.quantity - 1)}
                    >
                      −
                    </button>
                    <span>{i.quantity}</span>
                    <button
                      onClick={() => change(i.cartItemId, i.quantity + 1)}
                    >
                      +
                    </button>
                  </div>
                  <strong className="subtotal">
                    {Number(i.subtotal).toLocaleString("vi-VN")}₫
                  </strong>
                  <button
                    className="remove-btn"
                    onClick={() => remove(i.cartItemId)}
                  >
                    Xóa
                  </button>
                </div>
              ))}
            </div>
            <aside className="cart-summary">
              <h2>Tóm tắt</h2>
              <div>
                Đã chọn {selectedItems.length}/{cart.items.length} sản phẩm
              </div>
              <div>
                Tạm tính{" "}
                <strong>{selectedTotal.toLocaleString("vi-VN")}₫</strong>
              </div>
              <button className="checkout-btn" onClick={goCheckout}>
                Thanh toán ({selectedItems.length}) sản phẩm đã chọn
              </button>
            </aside>
          </div>
        )}
      </div>
    </div>
  );
}

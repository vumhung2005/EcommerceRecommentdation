import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { getOrders } from "../services/api";
import "./Orders.css";
export default function Orders() {
  const nav = useNavigate();
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  useEffect(() => {
    getOrders()
      .then((d) => setOrders(Array.isArray(d) ? d : d?.orders || []))
      .catch((e) => {
        alert(e.message);
        nav("/login");
      })
      .finally(() => setLoading(false));
  }, []);
  if (loading) return <div className="page-message">Đang tải đơn hàng...</div>;
  return (
    <div className="orders-page">
      <div className="orders-container">
        <h1>📦 Đơn hàng của tôi</h1>
        {!orders.length ? (
          <div className="empty-orders">Bạn chưa có đơn hàng nào.</div>
        ) : (
          orders.map((o) => (
            <div className="order-card" key={o.orderId}>
              <div className="order-header">
                <strong>Đơn #{o.orderId}</strong>
                <span>{o.status}</span>
              </div>
              <div className="order-body">
                {o.orderDetails?.map((d) => (
                  <div key={d.orderDetailId}>
                    {d.product?.name || `Sản phẩm #${d.productId}`} ×{" "}
                    {d.quantity}
                  </div>
                ))}
              </div>
              <div className="order-footer">
                <strong>
                  {Number(o.totalAmount || 0).toLocaleString("vi-VN")}₫
                </strong>
                <Link to={`/orders/${o.orderId}`}>Xem chi tiết</Link>
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
}

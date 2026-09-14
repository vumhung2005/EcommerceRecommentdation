import { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { getOrder, payOrder } from "../services/api";
import "./OrderDetail.css";
export default function OrderDetail() {
  const { id } = useParams();
  const nav = useNavigate();
  const [order, setOrder] = useState(null);
  const [paying, setPaying] = useState(false);
  useEffect(() => {
    getOrder(id)
      .then(setOrder)
      .catch((e) => {
        alert(e.message);
        nav("/orders");
      });
  }, [id]);
  if (!order) return <div className="page-message">Đang tải đơn hàng...</div>;
  return (
    <div className="order-detail-page">
      <div className="order-detail-card">
        <div className="detail-title">
          <h1>Đơn hàng #{order.orderId}</h1>
          <span>{order.status}</span>
        </div>
        <p>Ngày đặt: {new Date(order.orderDate).toLocaleString("vi-VN")}</p>
        <div className="shipping-info">
          <h3>Thông tin nhận hàng</h3>
          <p>
            <b>Người nhận:</b> {order.receiverName}
          </p>
          <p>
            <b>Số điện thoại:</b> {order.phone}
          </p>
          <p>
            <b>Địa chỉ:</b> {order.shippingAddress}
          </p>
          <p>
            <b>Thanh toán:</b> {order.paymentMethod} —{" "}
            {order.payment?.status || "Pending"}
          </p>
        </div>
        <div className="detail-items">
          {order.orderDetails?.map((d) => (
            <div key={d.orderDetailId}>
              <div>
                <strong>{d.product?.name || `Sản phẩm #${d.productId}`}</strong>
                <span> × {d.quantity}</span>
              </div>
              <strong>
                {Number((d.price || 0) * d.quantity).toLocaleString("vi-VN")}₫
              </strong>
            </div>
          ))}
        </div>
        <div className="detail-total">
          Tổng cộng{" "}
          <strong>
            {Number(order.totalAmount || 0).toLocaleString("vi-VN")}₫
          </strong>
        </div>
        <div className="order-detail-actions">
          {order.payment?.status === "Pending" &&
            order.paymentMethod === "BankTransfer" && (
              <button
                disabled={paying}
                onClick={async () => {
                  try {
                    setPaying(true);
                    await payOrder(order.orderId);
                    setOrder(await getOrder(id));
                  } catch (e) {
                    alert(e.message);
                  } finally {
                    setPaying(false);
                  }
                }}
              >
                {paying ? "Đang thanh toán..." : "💳 Thanh toán ngay"}
              </button>
            )}
          <Link to="/orders">← Quay lại đơn hàng</Link>
        </div>
      </div>
    </div>
  );
}

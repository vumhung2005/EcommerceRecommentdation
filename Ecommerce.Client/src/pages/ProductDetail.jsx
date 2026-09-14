import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import {
  getProduct,
  addToCart,
  getProductReviews,
  createReview,
  canReview,
  token,
} from "../services/api";
import { SERVER_URL } from "../services/api";
import "./ProductDetail.css";
export default function ProductDetail() {
  const { id } = useParams();
  const nav = useNavigate();
  const [p, setP] = useState(null);
  const [reviews, setReviews] = useState([]);
  const [reviewAllowed, setReviewAllowed] = useState(false);
  const [q, setQ] = useState(1);
  const [msg, setMsg] = useState("");
  const [rating, setRating] = useState(5);
  const [comment, setComment] = useState("");
  const load = async () => {
    try {
      const d = await getProduct(id);
      setP(d);
      const r = await getProductReviews(id);
      setReviews(Array.isArray(r) ? r : r?.reviews || []);
      setReviewAllowed(await canReview(id));
    } catch (e) {
      setMsg(e.message);
    }
  };
  useEffect(() => {
    load();
  }, [id]);
  const add = async (buy) => {
    if (!token()) {
      nav("/login");
      return;
    }
    try {
      await addToCart(p.productId, q);
      setMsg("Đã thêm vào giỏ hàng và ghi nhận hành vi AddToCart.");
      if (buy) nav("/cart");
    } catch (e) {
      setMsg(e.message);
    }
  };
  const review = async (e) => {
    e.preventDefault();
    try {
      await createReview(id, rating, comment);
      setComment("");
      setMsg("Đã gửi đánh giá.");
      const r = await getProductReviews(id);
      setReviews(Array.isArray(r) ? r : r?.reviews || []);
      setReviewAllowed(await canReview(id));
    } catch (e) {
      setMsg(e.message);
    }
  };
  if (!p)
    return <div className="page-message">{msg || "Đang tải sản phẩm..."}</div>;
  const image = p.image
    ? p.image.startsWith("http")
      ? p.image
      : `${SERVER_URL}${p.image}`
    : null;
  return (
    <div className="product-detail-page">
      <div className="product-detail">
        <div className="detail-image">
          {image ? <img src={image} alt={p.name} /> : "🛍️"}
        </div>
        <div className="detail-info">
          <h1>{p.name}</h1>
          <div className="detail-rating">⭐ 5.0</div>
          <div className="detail-meta">
            <span>
              Danh mục: <b>{p.category?.categoryName || "—"}</b>
            </span>
            <span>
              Thương hiệu: <b>{p.brand?.brandName || "—"}</b>
            </span>
          </div>
          <div className="detail-price">
            {Number(p.price || 0).toLocaleString("vi-VN")}₫
          </div>
          <p>{p.description || "Chưa có mô tả."}</p>
          <div className="detail-stock">Kho: {p.stock}</div>
          <div className="quantity-row">
            <span>Số lượng</span>
            <div className="quantity-control">
              <button onClick={() => setQ(Math.max(1, q - 1))}>−</button>
              <span>{q}</span>
              <button onClick={() => setQ(Math.min(p.stock || 1, q + 1))}>
                +
              </button>
            </div>
          </div>
          <div className="detail-actions">
            <button className="add-cart" onClick={() => add(false)}>
              🛒 Thêm vào giỏ
            </button>
            <button className="buy-now" onClick={() => add(true)}>
              Mua ngay
            </button>
          </div>
          {msg && <div className="detail-message">{msg}</div>}
        </div>
      </div>
      <section className="reviews">
        <h2>Đánh giá sản phẩm</h2>
        {reviews.length ? (
          reviews.map((r) => (
            <div className="review" key={r.reviewId}>
              <strong>⭐ {r.rating}/5</strong>
              <p>{r.comment || "Không có bình luận"}</p>
            </div>
          ))
        ) : (
          <p>Chưa có đánh giá.</p>
        )}
        {reviewAllowed && (
          <form onSubmit={review}>
            <select value={rating} onChange={(e) => setRating(e.target.value)}>
              {[5, 4, 3, 2, 1].map((x) => (
                <option key={x} value={x}>
                  {x} sao
                </option>
              ))}
            </select>
            <textarea
              value={comment}
              onChange={(e) => setComment(e.target.value)}
              placeholder="Viết đánh giá..."
            />
            <button>Gửi đánh giá</button>
          </form>
        )}
        {token() && !reviewAllowed && (
          <p className="review-note">
            Bạn chỉ có thể đánh giá sau khi đơn hàng đã hoàn thành và thanh
            toán.
          </p>
        )}
      </section>
    </div>
  );
}

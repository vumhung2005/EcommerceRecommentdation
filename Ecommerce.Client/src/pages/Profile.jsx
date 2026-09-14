import { useNavigate } from "react-router-dom";
import { token } from "../services/api";

import "./Profile.css";

function Profile() {
  const navigate = useNavigate();

  const userText = localStorage.getItem("user");

  let user = null;

  try {
    user = userText ? JSON.parse(userText) : null;
  } catch {
    user = null;
  }

  if (!token()) {
    navigate("/login");
    return null;
  }

  return (
    <div className="profile-page">
      <div className="profile-card">
        <div className="profile-avatar">👤</div>

        <h1>{user?.fullName || "Người dùng"}</h1>

        <p>{user?.email || "Chưa có email"}</p>

        <div className="profile-actions">
          <button onClick={() => navigate("/orders")}>
            📦 Đơn hàng của tôi
          </button>

          <button onClick={() => navigate("/cart")}>🛒 Giỏ hàng</button>
        </div>
      </div>
    </div>
  );
}

export default Profile;

import { Link, useNavigate } from "react-router-dom";
import { becomeSeller, getCurrentUser, isAdmin, logout } from "../services/api";
import "./Header.css";

export default function Header() {
  const navigate = useNavigate();
  const user = getCurrentUser();
  const admin = isAdmin();
  const isSeller = (user?.roles || []).some(r => String(r).toLowerCase() === "seller");

  const handleBecomeSeller = async () => {
    if (!user) { navigate("/login"); return; }
    if (isSeller || admin) { navigate("/admin"); return; }
    try {
      const result = await becomeSeller();
      if (result?.token) localStorage.setItem("token", result.token);
      const refreshed = result?.user || { ...user, roles: [...new Set([...(user.roles || []), "Seller"])], sellerStatus: "Approved" };
      localStorage.setItem("user", JSON.stringify(refreshed));
      alert("Đăng ký trở thành người bán thành công.");
      window.location.reload();
    } catch (e) { alert(e.message); }
  };

  return <header className="header">
    <div className="header-top"><div className="header-container top-row">
      <div><span>Kênh người bán</span><button className="top-link-button" onClick={handleBecomeSeller}>{isSeller || admin ? "Kênh bán hàng" : "Trở thành người bán"}</button><span>Trợ giúp</span></div>
      <div>{user ? <><Link to="/profile">Tài khoản</Link><button onClick={() => { logout(); navigate("/"); window.location.reload(); }}>Đăng xuất</button></> : <><Link to="/register">Đăng ký</Link><Link to="/login">Đăng nhập</Link></>}</div>
    </div></div>
    <div className="header-main"><div className="header-container main-content">
      <Link to="/" className="logo"><div className="logo-icon">E</div><div><div className="logo-name">Ecommerce</div><div className="logo-subtitle">Recommendation</div></div></Link>
      <form className="search-box" onSubmit={e => { e.preventDefault(); const q=e.currentTarget.search.value.trim(); navigate(q?`/?search=${encodeURIComponent(q)}`:"/"); }}><input name="search" placeholder="Tìm kiếm sản phẩm..."/><button type="submit">🔍</button></form>
      <div className="header-actions"><Link className="header-action" to="/cart">🛒<span>Giỏ hàng</span></Link>{(admin || isSeller) && <Link className="admin-link" to="/admin">⚙️ {admin ? "Admin" : "Quản lý bán hàng"}</Link>}{user?<Link className="header-action" to="/profile">👤<span>{user.fullName||"Tài khoản"}</span></Link>:<Link className="header-action" to="/login">👤<span>Đăng nhập</span></Link>}</div>
    </div></div>
  </header>;
}

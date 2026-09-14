import { useEffect, useMemo, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import {
  getProducts,
  getOrders,
  getCategories,
  getBrands,
  createProduct,
  updateProduct,
  deleteProduct,
  createCategory,
  updateCategory,
  deleteCategory,
  createBrand,
  updateBrand,
  deleteBrand,
  updateOrderStatus,
  generateRecommendations,
  getBehaviorSummary,
  getAdminDashboard,
  getCurrentUser,
  isAdmin,
  token,
  uploadProductImage,
} from "../services/api";
import { SERVER_URL } from "../services/api";
import "./AdminDashboard.css";

const emptyProduct = {
  name: "",
  description: "",
  price: "",
  stock: "",
  categoryId: "",
  brandId: "",
  image: "",
};
const emptyEntity = { name: "", description: "" };

const money = (n) => Number(n || 0).toLocaleString("vi-VN") + "₫";
const imageUrl = (value) =>
  value ? (value.startsWith("http") ? value : `${SERVER_URL}${value}`) : "";

export default function AdminDashboard() {
  const nav = useNavigate();
  const user = getCurrentUser();
  const admin = isAdmin();
  const seller = (user?.roles || []).some(
    (r) => String(r).toLowerCase() === "seller",
  );
  const [tab, setTab] = useState("dashboard");
  const [products, setProducts] = useState([]);
  const [orders, setOrders] = useState([]);
  const [cats, setCats] = useState([]);
  const [brands, setBrands] = useState([]);
  const [behaviors, setBehaviors] = useState([]);
  const [dashboard, setDashboard] = useState(null);
  const [form, setForm] = useState(emptyProduct);
  const [editId, setEditId] = useState(null);
  const [entity, setEntity] = useState(emptyEntity);
  const [entityEdit, setEntityEdit] = useState(null);
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);
  const [uploading, setUploading] = useState(false);

  useEffect(() => {
    if (!token() || (!admin && !seller)) {
      nav("/");
      return;
    }
    load();
  }, []);

  const load = async () => {
    setLoading(true);
    try {
      const [p, o, c, b, behavior] = await Promise.all([
        getProducts(),
        getOrders().catch(() => []),
        getCategories(),
        getBrands(),
        getBehaviorSummary().catch(() => []),
      ]);
      const allProducts = Array.isArray(p) ? p : p?.products || [];
      setProducts(
        admin ? allProducts : allProducts.filter((x) => x.ownerId === user?.id),
      );
      setOrders(Array.isArray(o) ? o : o?.orders || []);
      setCats(Array.isArray(c) ? c : c?.categories || []);
      setBrands(Array.isArray(b) ? b : b?.brands || []);
      setBehaviors(Array.isArray(behavior) ? behavior : []);
      if (admin) setDashboard(await getAdminDashboard());
    } catch (e) {
      alert(e.message);
    } finally {
      setLoading(false);
    }
  };

  const stats = useMemo(
    () => ({
      products: products.length,
      stock: products.reduce((s, p) => s + Number(p.stock || 0), 0),
      orders: orders.length,
      pending: orders.filter(
        (o) => String(o.status).toLowerCase() === "pending",
      ).length,
      revenue: dashboard?.revenue || 0,
    }),
    [products, orders, dashboard],
  );

  const saveProduct = async (e) => {
    e.preventDefault();
    try {
      setBusy(true);
      if (editId) await updateProduct(editId, form);
      else await createProduct(form);
      alert(editId ? "Đã cập nhật sản phẩm." : "Đã thêm sản phẩm.");
      setForm(emptyProduct);
      setEditId(null);
      setTab("products");
      await load();
    } catch (e) {
      alert(e.message);
    } finally {
      setBusy(false);
    }
  };

  const editProduct = (p) => {
    if (!admin && p.ownerId !== user?.id) return;
    setEditId(p.productId);
    setForm({
      name: p.name || "",
      description: p.description || "",
      price: p.price ?? "",
      stock: p.stock ?? "",
      categoryId: p.categoryId ?? "",
      brandId: p.brandId ?? "",
      image: p.image || "",
    });
    setTab("product-form");
  };

  const removeProduct = async (id) => {
    if (!confirm("Xóa sản phẩm này?")) return;
    try {
      await deleteProduct(id);
      await load();
    } catch (e) {
      alert(e.message);
    }
  };

  const uploadImage = async (e) => {
    const file = e.target.files?.[0];
    if (!file) return;
    try {
      setUploading(true);
      const result = await uploadProductImage(file);
      setForm((f) => ({ ...f, image: result.imageUrl }));
    } catch (e) {
      alert(e.message);
    } finally {
      setUploading(false);
    }
  };

  const saveEntity = async (e) => {
    e.preventDefault();
    if (!entity.name.trim()) return;
    try {
      setBusy(true);
      if (tab === "categories")
        entityEdit
          ? await updateCategory(entityEdit, entity)
          : await createCategory(entity);
      else
        entityEdit
          ? await updateBrand(entityEdit, entity)
          : await createBrand(entity);
      setEntity(emptyEntity);
      setEntityEdit(null);
      await load();
    } catch (e) {
      alert(e.message);
    } finally {
      setBusy(false);
    }
  };

  const removeEntity = async (id) => {
    if (!confirm("Xóa mục này?")) return;
    try {
      tab === "categories" ? await deleteCategory(id) : await deleteBrand(id);
      await load();
    } catch (e) {
      alert(e.message);
    }
  };

  const setStatus = async (id, status) => {
    try {
      await updateOrderStatus(id, status);
      await load();
    } catch (e) {
      alert(e.message);
    }
  };

  if (loading) return <div className="page-message">Đang tải quản trị...</div>;

  return (
    <div className="admin-layout">
      <aside className="admin-sidebar">
        <div className="admin-brand">
          <b>E</b>
          <div>
            <strong>Ecommerce</strong>
            <small>{admin ? "ADMIN" : "SELLER"}</small>
          </div>
        </div>
        {[
          "dashboard",
          "products",
          "product-form",
          ...(admin ? ["orders", "categories", "brands"] : []),
        ].map((key) => {
          const labels = {
            dashboard: "📊 Dashboard",
            products: "📦 Sản phẩm",
            "product-form": "➕ Thêm sản phẩm",
            orders: "🛒 Đơn hàng",
            categories: "🗂️ Danh mục",
            brands: "🏷️ Thương hiệu",
          };
          return (
            <button
              key={key}
              className={tab === key ? "active" : ""}
              onClick={() => {
                if (key === "product-form") {
                  setForm(emptyProduct);
                  setEditId(null);
                }
                setTab(key);
              }}
            >
              {labels[key]}
            </button>
          );
        })}
        <div className="admin-side-bottom">
          <Link to="/">🏠 Về cửa hàng</Link>
        </div>
      </aside>

      <main className="admin-main">
        <header className="admin-topbar">
          <div>
            <h1>
              {tab === "dashboard"
                ? "Dashboard"
                : tab === "products"
                  ? "Quản lý sản phẩm"
                  : tab === "product-form"
                    ? editId
                      ? "Chỉnh sửa sản phẩm"
                      : "Thêm sản phẩm"
                    : tab === "orders"
                      ? "Quản lý đơn hàng"
                      : tab === "categories"
                        ? "Quản lý danh mục"
                        : "Quản lý thương hiệu"}
            </h1>
            <p>Xin chào, {user?.fullName || user?.email}</p>
          </div>
          <button onClick={load}>↻ Làm mới</button>
        </header>

        {tab === "dashboard" && (
          <>
            <div className="stat-grid">
              <div>
                <span>Sản phẩm</span>
                <b>{stats.products}</b>
              </div>
              <div>
                <span>Đơn hàng</span>
                <b>{stats.orders}</b>
              </div>
              <div>
                <span>Tồn kho</span>
                <b>{stats.stock}</b>
              </div>
              <div>
                <span>Doanh thu hoàn thành</span>
                <b>{money(stats.revenue)}</b>
              </div>
            </div>
            {admin && (
              <div className="admin-panel">
                <div className="panel-head">
                  <div>
                    <h2>📈 Doanh thu 30 ngày gần nhất</h2>
                    <p>Chỉ tính các đơn hàng có trạng thái Completed.</p>
                  </div>
                  <strong>{money(stats.revenue)}</strong>
                </div>
                <RevenueChart data={dashboard?.chart || []} />
              </div>
            )}
            <div className="admin-panel">
              <h2>Hành vi người dùng</h2>
              {behaviors.length ? (
                <div className="behavior-grid">
                  {behaviors.map((x) => (
                    <div key={x.actionType}>
                      <b>{x.actionType}</b>
                      <span>{x.count} lần</span>
                      <small>Điểm: {x.totalWeight}</small>
                    </div>
                  ))}
                </div>
              ) : (
                <p>Chưa có dữ liệu hành vi.</p>
              )}
              {admin && (
                <button
                  className="primary"
                  onClick={async () => {
                    try {
                      const r = await generateRecommendations();
                      alert(r?.message || "Đã tạo recommendation");
                    } catch (e) {
                      alert(e.message);
                    }
                  }}
                >
                  🤖 Tạo recommendation
                </button>
              )}
            </div>
          </>
        )}

        {tab === "products" && (
          <div className="admin-panel">
            <div className="panel-head">
              <h2>{admin ? "Tất cả sản phẩm" : "Sản phẩm của tôi"}</h2>
              <button
                className="primary"
                onClick={() => {
                  setForm(emptyProduct);
                  setEditId(null);
                  setTab("product-form");
                }}
              >
                + Thêm
              </button>
            </div>
            <div className="table-wrap">
              <table>
                <thead>
                  <tr>
                    <th>Ảnh</th>
                    <th>Tên</th>
                    <th>Danh mục</th>
                    <th>Thương hiệu</th>
                    <th>Giá</th>
                    <th>Kho</th>
                    <th>Thao tác</th>
                  </tr>
                </thead>
                <tbody>
                  {products.map((p) => {
                    const catName =
                      p.category?.categoryName ||
                      cats.find((c) => c.categoryId === p.categoryId)
                        ?.categoryName ||
                      p.categoryId;
                    const brandName =
                      p.brand?.brandName ||
                      brands.find((b) => b.brandId === p.brandId)?.brandName ||
                      p.brandId;
                    return (
                      <tr key={p.productId}>
                        <td>
                          {p.image && (
                            <img
                              className="thumb"
                              src={imageUrl(p.image)}
                              alt=""
                            />
                          )}
                        </td>
                        <td>{p.name}</td>
                        <td>{catName}</td>
                        <td>{brandName}</td>
                        <td>{money(p.price)}</td>
                        <td>{p.stock}</td>
                        <td>
                          <button onClick={() => editProduct(p)}>Sửa</button>
                          <button
                            className="danger"
                            onClick={() => removeProduct(p.productId)}
                          >
                            Xóa
                          </button>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          </div>
        )}

        {tab === "product-form" && (
          <div className="admin-panel">
            <form className="admin-form" onSubmit={saveProduct}>
              <label>
                Tên sản phẩm
                <input
                  required
                  value={form.name}
                  onChange={(e) => setForm({ ...form, name: e.target.value })}
                />
              </label>
              <label>
                Danh mục
                <select
                  required
                  value={form.categoryId}
                  onChange={(e) =>
                    setForm({ ...form, categoryId: e.target.value })
                  }
                >
                  <option value="">Chọn</option>
                  {cats.map((c) => (
                    <option key={c.categoryId} value={c.categoryId}>
                      {c.categoryName}
                    </option>
                  ))}
                </select>
              </label>
              <label>
                Thương hiệu
                <select
                  required
                  value={form.brandId}
                  onChange={(e) =>
                    setForm({ ...form, brandId: e.target.value })
                  }
                >
                  <option value="">Chọn</option>
                  {brands.map((b) => (
                    <option key={b.brandId} value={b.brandId}>
                      {b.brandName}
                    </option>
                  ))}
                </select>
              </label>
              <label>
                Giá
                <input
                  required
                  type="number"
                  min="0"
                  value={form.price}
                  onChange={(e) => setForm({ ...form, price: e.target.value })}
                />
              </label>
              <label>
                Tồn kho
                <input
                  required
                  type="number"
                  min="0"
                  value={form.stock}
                  onChange={(e) => setForm({ ...form, stock: e.target.value })}
                />
              </label>
              <label className="full">
                Ảnh
                <input
                  type="file"
                  accept="image/png,image/jpeg,image/webp"
                  onChange={uploadImage}
                />
                {uploading && <small>Đang upload...</small>}
                <input
                  value={form.image}
                  placeholder="Hoặc nhập URL"
                  onChange={(e) => setForm({ ...form, image: e.target.value })}
                />
              </label>
              <label className="full">
                Mô tả
                <textarea
                  value={form.description}
                  onChange={(e) =>
                    setForm({ ...form, description: e.target.value })
                  }
                />
              </label>
              <div>
                <button
                  type="button"
                  onClick={() => {
                    setForm(emptyProduct);
                    setEditId(null);
                    setTab("products");
                  }}
                >
                  Hủy
                </button>
                <button className="primary" disabled={busy || uploading}>
                  {busy
                    ? "Đang lưu..."
                    : editId
                      ? "Lưu thay đổi"
                      : "Thêm sản phẩm"}
                </button>
              </div>
            </form>
          </div>
        )}

        {tab === "orders" && (
          <div className="admin-panel">
            {!orders.length ? (
              <div className="empty-state">
                <h3>Chưa có đơn hàng nào</h3>
                <p>Hiện tại chưa có dữ liệu đơn hàng để xử lý.</p>
              </div>
            ) : (
              <div className="table-wrap">
                <table>
                  <thead>
                    <tr>
                      <th>Mã</th>
                      <th>Người nhận</th>
                      <th>Ngày</th>
                      <th>Tổng</th>
                      <th>Trạng thái</th>
                      <th>Xử lý</th>
                    </tr>
                  </thead>
                  <tbody>
                    {orders.map((o) => (
                      <tr key={o.orderId}>
                        <td>#{o.orderId}</td>
                        <td>{o.receiverName || o.userId}</td>
                        <td>{new Date(o.orderDate).toLocaleString("vi-VN")}</td>
                        <td>{money(o.totalAmount)}</td>
                        <td>
                          <span
                            className={`status status-${String(o.status).toLowerCase()}`}
                          >
                            {o.status}
                          </span>
                        </td>
                        <td>
                          <div className="order-actions">
                            {o.status === "Pending" && (
                              <button
                                className="primary"
                                onClick={() => setStatus(o.orderId, "Confirmed")}
                              >
                                ✓ Xác nhận
                              </button>
                            )}
                            {o.status === "Pending" && (
                              <button
                                className="primary"
                                onClick={() => setStatus(o.orderId, "Completed")}
                              >
                                🛍️ Xác nhận đã mua
                              </button>
                            )}
                            {o.status === "Confirmed" && (
                              <button
                                onClick={() => setStatus(o.orderId, "Shipping")}
                              >
                                🚚 Giao hàng
                              </button>
                            )}
                            {o.status === "Confirmed" && (
                              <button
                                className="primary"
                                onClick={() => setStatus(o.orderId, "Completed")}
                              >
                                ✓ Hoàn thành
                              </button>
                            )}
                            {o.status === "Shipping" && (
                              <button
                                className="primary"
                                onClick={() => setStatus(o.orderId, "Completed")}
                              >
                                ✓ Hoàn thành
                              </button>
                            )}
                            {["Pending", "Confirmed", "Shipping"].includes(
                              o.status,
                            ) && (
                              <button
                                className="danger"
                                onClick={() => setStatus(o.orderId, "Cancelled")}
                              >
                                Hủy
                              </button>
                            )}
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        )}

        {(tab === "categories" || tab === "brands") && (
          <div className="admin-panel">
            <form className="inline-form" onSubmit={saveEntity}>
              <input
                placeholder="Tên"
                value={entity.name}
                onChange={(e) => setEntity({ ...entity, name: e.target.value })}
              />
              <button className="primary" disabled={busy}>
                {entityEdit ? "Lưu" : "Thêm"}
              </button>
            </form>
            <div className="simple-list">
              {(tab === "categories" ? cats : brands).map((x) => {
                const id = tab === "categories" ? x.categoryId : x.brandId;
                const name =
                  tab === "categories" ? x.categoryName : x.brandName;
                return (
                  <div key={id}>
                    <span>
                      <b>{name}</b>
                    </span>
                    <div>
                      <button
                        onClick={() => {
                          setEntity({ name, description: "" });
                          setEntityEdit(id);
                        }}
                      >
                        Sửa
                      </button>
                      <button
                        className="danger"
                        onClick={() => removeEntity(id)}
                      >
                        Xóa
                      </button>
                    </div>
                  </div>
                );
              })}
            </div>
          </div>
        )}
      </main>
    </div>
  );
}

function RevenueChart({ data }) {
  const max = Math.max(...data.map((x) => Number(x.revenue || 0)), 1);
  return (
    <div className="revenue-chart">
      {data.map((x) => (
        <div
          className="chart-col"
          key={x.date}
          title={`${x.date}: ${money(x.revenue)}`}
        >
          <div
            className="chart-bar"
            style={{
              height: `${Math.max(4, (Number(x.revenue || 0) / max) * 100)}%`,
            }}
          ></div>
          <small>{x.date.slice(8)}</small>
        </div>
      ))}
    </div>
  );
}

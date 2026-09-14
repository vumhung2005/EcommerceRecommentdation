import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import {
  createProduct,
  getCategories,
  getBrands,
  uploadProductImage,
} from "../services/api";
import "./AddProduct.css";
export default function AddProduct() {
  const nav = useNavigate();
  const [f, setF] = useState({
    name: "",
    description: "",
    price: "",
    stock: "",
    categoryId: "",
    brandId: "",
    image: "",
  });
  const [c, setC] = useState([]);
  const [b, setB] = useState([]);
  const [file, setFile] = useState(null);
  const [msg, setMsg] = useState("");
  const [busy, setBusy] = useState(false);
  useEffect(() => {
    Promise.all([getCategories(), getBrands()])
      .then(([x, y]) => {
        setC(x || []);
        setB(y || []);
      })
      .catch((e) => setMsg(e.message));
  }, []);
  const submit = async (e) => {
    e.preventDefault();
    try {
      setBusy(true);
      let image = f.image;
      if (file) {
        const r = await uploadProductImage(file);
        image = r.imageUrl || r.fileName;
      }
      await createProduct({ ...f, image });
      setMsg("Thêm sản phẩm thành công");
      setTimeout(() => nav("/admin"), 700);
    } catch (e) {
      setMsg(e.message);
    } finally {
      setBusy(false);
    }
  };
  return (
    <div className="add-product-page">
      <div className="add-product-card">
        <div className="page-head">
          <div>
            <h1>➕ Thêm sản phẩm</h1>
            <p>Nhập thông tin sản phẩm và hình ảnh.</p>
          </div>
          <Link to="/admin">← Dashboard</Link>
        </div>
        {msg && <div className="form-message">{msg}</div>}
        <form onSubmit={submit}>
          <label>
            Tên sản phẩm *
            <input
              required
              value={f.name}
              onChange={(e) => setF({ ...f, name: e.target.value })}
            />
          </label>
          <div className="two-col">
            <label>
              Danh mục *
              <select
                required
                value={f.categoryId}
                onChange={(e) => setF({ ...f, categoryId: e.target.value })}
              >
                <option value="">Chọn danh mục</option>
                {c.map((x) => (
                  <option key={x.categoryId} value={x.categoryId}>
                    {x.categoryName}
                  </option>
                ))}
              </select>
            </label>
            <label>
              Thương hiệu *
              <select
                required
                value={f.brandId}
                onChange={(e) => setF({ ...f, brandId: e.target.value })}
              >
                <option value="">Chọn thương hiệu</option>
                {b.map((x) => (
                  <option key={x.brandId} value={x.brandId}>
                    {x.brandName}
                  </option>
                ))}
              </select>
            </label>
            <label>
              Giá *
              <input
                required
                type="number"
                min="0"
                value={f.price}
                onChange={(e) => setF({ ...f, price: e.target.value })}
              />
            </label>
            <label>
              Tồn kho *
              <input
                required
                type="number"
                min="0"
                value={f.stock}
                onChange={(e) => setF({ ...f, stock: e.target.value })}
              />
            </label>
          </div>
          <label>
            Upload ảnh
            <input
              type="file"
              accept="image/png,image/jpeg,image/webp"
              onChange={(e) => setFile(e.target.files?.[0] || null)}
            />
          </label>
          <label>
            Hoặc nhập đường dẫn ảnh
            <input
              value={f.image}
              onChange={(e) => setF({ ...f, image: e.target.value })}
              placeholder="/images/products/iphone-17-pro.jpg"
            />
          </label>
          <label>
            Mô tả
            <textarea
              rows="6"
              value={f.description}
              onChange={(e) => setF({ ...f, description: e.target.value })}
            />
          </label>
          <div className="form-actions">
            <Link to="/admin">Hủy</Link>
            <button disabled={busy}>
              {busy ? "Đang lưu..." : "✓ Thêm sản phẩm"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

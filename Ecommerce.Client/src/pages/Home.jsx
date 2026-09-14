import { useEffect, useState } from "react";
import { useSearchParams } from "react-router-dom";
import ProductCard from "../components/ProductCard";
import RecommendationSection from "../components/RecommendationSection";
import {
  getProducts,
  getCategories,
  getSavedRecommendations,
  getRecommendations,
} from "../services/api";
import "./Home.css";
export default function Home() {
  const [params] = useSearchParams();
  const search = params.get("search") || "";
  const [products, setProducts] = useState([]);
  const [categories, setCategories] = useState([]);
  const [selectedCategory, setSelectedCategory] = useState("");
  const [recs, setRecs] = useState([]);
  const [loading, setLoading] = useState(true);
  const [recLoading, setRecLoading] = useState(true);
  const load = async () => {
    setLoading(true);
    try {
      const d = await getProducts();
      setProducts(Array.isArray(d) ? d : d?.products || []);
    } catch (e) {
      console.error(e);
    } finally {
      setLoading(false);
    }
  };
  const loadRec = async () => {
    setRecLoading(true);
    try {
      let d = await getSavedRecommendations(10);
      if (!d.length) d = await getRecommendations(5, 10);
      setRecs(d);
    } catch (e) {
      console.error(e);
      setRecs([]);
    } finally {
      setRecLoading(false);
    }
  };
  useEffect(() => {
    load();
  }, []);

  useEffect(() => {
    getCategories()
      .then((data) => setCategories(Array.isArray(data) ? data : []))
      .catch((e) => console.error(e));
  }, []);

  useEffect(() => {
    loadRec();
  }, []);

  useEffect(() => {
    const onRecommendationRefresh = () => loadRec();
    window.addEventListener("recommendations:refresh", onRecommendationRefresh);
    return () => window.removeEventListener("recommendations:refresh", onRecommendationRefresh);
  }, []);
  const recProducts = recs
    .map((r) => {
      if (r?.product)
        return { ...r.product, recommendationScore: r.score ?? r.Score };
      const id = r?.productId ?? r?.ProductId;
      return products.find(
        (p) => Number(p.productId ?? p.ProductId) === Number(id),
      );
    })
    .filter(Boolean);
  const filtered = products.filter((p) => {
    const matchesSearch = p.name?.toLowerCase().includes(search.toLowerCase());
    const productCategoryId =
      p.categoryId ?? p.CategoryId ?? p.category?.categoryId;
    const matchesCategory =
      !selectedCategory ||
      Number(productCategoryId) === Number(selectedCategory);

    return matchesSearch && matchesCategory;
  });
  return (
    <div className="home">
      <section className="home-banner">
        <div className="banner-content">
          <h1>Khám phá sản phẩm phù hợp với bạn</h1>
          <p>Hệ thống sử dụng AI để cá nhân hóa sản phẩm dành riêng cho bạn.</p>
          <button
            onClick={() =>
              document
                .getElementById("all-products")
                ?.scrollIntoView({ behavior: "smooth" })
            }
          >
            Khám phá ngay
          </button>
        </div>
      </section>
      <section className="category-filter-bar" aria-label="Lọc sản phẩm theo danh mục">
        <span className="category-filter-title">Danh mục:</span>
        <button
          type="button"
          className={!selectedCategory ? "active" : ""}
          onClick={() => setSelectedCategory("")}
        >
          Tất cả
        </button>
        {categories.map((category) => {
          const categoryId = category.categoryId ?? category.CategoryId;
          return (
            <button
              type="button"
              className={Number(selectedCategory) === Number(categoryId) ? "active" : ""}
              key={categoryId}
              onClick={() => setSelectedCategory(String(categoryId))}
            >
              {category.categoryName ?? category.CategoryName}
            </button>
          );
        })}
      </section>
      <RecommendationSection
        recommendations={recProducts}
        loading={recLoading || loading}
      />
      <section id="all-products" className="products-section">
        <div className="section-header">
          <div>
            <h2>
              {search ? `Kết quả tìm kiếm: "${search}"` : "Tất cả sản phẩm"}
            </h2>
            <p>{filtered.length} sản phẩm</p>
          </div>
        </div>
        {loading ? (
          <div className="loading">Đang tải sản phẩm...</div>
        ) : filtered.length ? (
          <div className="product-grid">
            {filtered.map((p) => (
              <ProductCard key={p.productId ?? p.ProductId} product={p} />
            ))}
          </div>
        ) : (
          <div className="empty-products">Không tìm thấy sản phẩm.</div>
        )}
      </section>
    </div>
  );
}

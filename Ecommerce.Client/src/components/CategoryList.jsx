import "./CategoryList.css";

const categories = [
  {
    icon: "👕",
    name: "Thời trang",
  },
  {
    icon: "📱",
    name: "Điện thoại",
  },
  {
    icon: "💻",
    name: "Laptop",
  },
  {
    icon: "🎧",
    name: "Phụ kiện",
  },
  {
    icon: "📚",
    name: "Sách",
  },
  {
    icon: "👟",
    name: "Giày dép",
  },
  {
    icon: "🏠",
    name: "Gia dụng",
  },
  {
    icon: "🎮",
    name: "Gaming",
  },
];

function CategoryList() {
  return (
    <section className="category-section">

      <div className="section-title">
        Danh mục
      </div>

      <div className="category-grid">

        {categories.map((category) => (
          <div
            className="category-item"
            key={category.name}
          >
            <div className="category-icon">
              {category.icon}
            </div>

            <div className="category-name">
              {category.name}
            </div>
          </div>
        ))}

      </div>

    </section>
  );
}

export default CategoryList;
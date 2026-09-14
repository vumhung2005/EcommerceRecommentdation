import ProductCard from "./ProductCard";
import "./FlashSale.css";

const flashProducts = [
  {
    id: 1,
    name: "Áo thun nam thời trang",
    price: 99000,
  },
  {
    id: 2,
    name: "Tai nghe Bluetooth",
    price: 199000,
  },
  {
    id: 3,
    name: "Chuột Gaming RGB",
    price: 249000,
  },
  {
    id: 4,
    name: "Balo laptop thời trang",
    price: 299000,
  },
  {
    id: 5,
    name: "Giày sneaker nam",
    price: 399000,
  },
];

function FlashSale() {
  return (
    <section className="flash-section">

      <div className="flash-header">

        <div>
           FLASH SALE
        </div>

        <span>
          Kết thúc trong 02:15:30
        </span>

      </div>

      <div className="product-grid">

        {flashProducts.map((product) => (
          <ProductCard
            key={product.id}
            product={product}
          />
        ))}

      </div>

    </section>
  );
}

export default FlashSale;
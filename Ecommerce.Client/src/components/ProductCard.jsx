import { useNavigate } from "react-router-dom";
import { clickProduct, SERVER_URL } from "../services/api";
import "./ProductCard.css";
export default function ProductCard({product,recommendation=false})
{const navigate=useNavigate();
    if(!product)return null;
    const id=product.productId??product.ProductId;
    const raw=product.image??product.Image;
    const image=raw?(raw.startsWith("http")?raw:`${SERVER_URL}${raw}`):null;
    return <article className="product-card" onClick={async()=>{await clickProduct(id);
        navigate(`/products/${id}`)}}><div className="product-image">{image?<img src={image} 
        alt={product.name} onError={e=>e.currentTarget.parentElement.classList.add("image-error")}/>:<div className="image-placeholder"></div>}{recommendation&&<span className="recommend-badge">Đề xuất</span>}</div><div className="product-info"><div className="product-name">{product.name}</div><div className="product-price">{Number(product.price||0).toLocaleString("vi-VN")}₫</div><div className="product-bottom"><span>⭐ {product.rating||"5.0"}
        </span>
        <span>Kho {product.stock??0}</span></div></div>
        </article>}

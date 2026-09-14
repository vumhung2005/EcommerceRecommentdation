import ProductCard from "./ProductCard";
import "./RecommendationSection.css";

export default function RecommendationSection({
	recommendations = [],
	loading = false,
}) {
	return (
		<section className="recommendation-section">
			<div className="recommendation-header">
				<div>
					<h2>Gợi ý dành cho bạn</h2>
					<p>
						{recommendations.length
							? "Đề xuất dựa trên hành vi và Collaborative Filtering"
							: "Hãy xem hoặc thêm sản phẩm vào giỏ để AI hiểu sở thích của bạn."}
					</p>
				</div>
				<span className="ai-label">AI Recommendation</span>
			</div>

			{loading ? (
				<div className="recommendation-loading">Đang phân tích sở thích...</div>
			) : recommendations.length ? (
				<div className="product-grid recommendation-grid">
					{recommendations.map((product) => (
						<ProductCard
							key={`r-${product.productId ?? product.ProductId}`}
							product={product}
							recommendation
						/>
					))}
				</div>
			) : (
				<div className="recommendation-empty">
					<div></div>
					<h3>Chưa có đủ dữ liệu để gợi ý</h3>
					<p>Hãy xem, click hoặc thêm sản phẩm vào giỏ hàng.</p>
				</div>
			)}
		</section>
	);
}

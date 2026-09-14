const API_URL = "http://localhost:5116/api";
export const SERVER_URL = "http://localhost:5116";

function decodeTokenPayload(tokenValue) {
  try {
    const payload = tokenValue.split(".")[1];
    if (!payload) return null;
    const base64 = payload.replace(/-/g, "+").replace(/_/g, "/");
    const padded = base64.padEnd(Math.ceil(base64.length / 4) * 4, "=");
    const binary = atob(padded);
    const json = decodeURIComponent(
      Array.from(binary, (char) => `%${(`00${char.charCodeAt(0).toString(16)}`).slice(-2)}`).join(""),
    );
    return JSON.parse(json);
  } catch {
    return null;
  }
}

function isTokenExpired(tokenValue) {
  if (!tokenValue) return true;
  const payload = decodeTokenPayload(tokenValue);
  if (!payload || !payload.exp) return false;
  return Date.now() >= Number(payload.exp) * 1000;
}

export function token() {
  const storedToken = localStorage.getItem("token");
  if (!storedToken) return null;
  if (isTokenExpired(storedToken)) {
    logout();
    return null;
  }
  return storedToken;
}

export function isAuthenticated() {
  return !!token();
}
function authHeaders() {
  const t = token();
  if (!t) throw new Error("Vui lòng đăng nhập.");
  return { Authorization: `Bearer ${t}` };
}

async function request(url, options = {}) {
  const headers = { ...(options.body instanceof FormData ? {} : { "Content-Type": "application/json" }), ...(options.headers || {}) };
  const res = await fetch(`${API_URL}${url}`, { ...options, headers });
  const text = await res.text();
  let data = null;
  try { data = text ? JSON.parse(text) : null; } catch { data = text; }
  if (!res.ok) {
    if (res.status === 401) {
      console.warn("401 Unauthorized", url);
      logout();
    }
    const validationErrors = data?.errors && !Array.isArray(data.errors)
      ? Object.values(data.errors).flat().join(", ")
      : Array.isArray(data?.errors)
        ? data.errors.join(", ")
        : null;
    const msg = typeof data === "string" ? data : data?.message || validationErrors || data?.title || data?.error || `HTTP ${res.status}`;
    throw new Error(msg);
  }
  return data;
}

export async function login(email, password) {
  const data = await request("/auth/login", { method: "POST", body: JSON.stringify({ email, password }) });
  if (data?.token) localStorage.setItem("token", data.token);
  if (data?.user) localStorage.setItem("user", JSON.stringify(data.user));
  return data;
}
export async function register(fullName, email, password) { return request("/auth/register", { method: "POST", body: JSON.stringify({ fullName, email, password }) }); }
export function logout() { localStorage.removeItem("token"); localStorage.removeItem("user"); }
export function getCurrentUser() {
  if (!token()) {
    localStorage.removeItem("user");
    return null;
  }
  try {
    return JSON.parse(localStorage.getItem("user") || "null");
  } catch {
    localStorage.removeItem("user");
    return null;
  }
}
export function isLoggedIn() { return !!token(); }
export function isAdmin() { return (getCurrentUser()?.roles || []).some(r => String(r).toLowerCase() === "admin"); }

export async function getProducts() { return request("/products"); }
export async function getProduct(id) { return request(`/products/${id}`, { headers: token() ? { Authorization: `Bearer ${token()}` } : {} }); }
export async function createProduct(product) { return request("/products", { method: "POST", headers: authHeaders(), body: JSON.stringify({ categoryId:+product.categoryId, brandId:+product.brandId, name:product.name, description:product.description||"", price:+product.price, stock:+product.stock, image:product.image||"" }) }); }
export async function updateProduct(id, product) { return request(`/products/${id}`, { method:"PUT", headers:authHeaders(), body:JSON.stringify({ productId:+id, categoryId:+product.categoryId, brandId:+product.brandId, name:product.name, description:product.description||"", price:+product.price, stock:+product.stock, image:product.image||"" }) }); }
export async function deleteProduct(id) { return request(`/products/${id}`, { method:"DELETE", headers:authHeaders() }); }
export async function uploadProductImage(file) { const fd = new FormData(); fd.append("file", file); return request("/products/upload-image", { method:"POST", headers:authHeaders(), body:fd }); }

export async function getCategories() { return request("/categories"); }
export async function createCategory(category) { return request("/categories", { method:"POST", headers:authHeaders(), body:JSON.stringify({ categoryName:category.name }) }); }
export async function updateCategory(id, category) { return request(`/categories/${id}`, { method:"PUT", headers:authHeaders(), body:JSON.stringify({ categoryId:+id, categoryName:category.name }) }); }
export async function deleteCategory(id) { return request(`/categories/${id}`, { method:"DELETE", headers:authHeaders() }); }

export async function getBrands() { return request("/brands"); }
export async function createBrand(brand) { return request("/brands", { method:"POST", headers:authHeaders(), body:JSON.stringify({ brandName:brand.name }) }); }
export async function updateBrand(id, brand) { return request(`/brands/${id}`, { method:"PUT", headers:authHeaders(), body:JSON.stringify({ brandId:+id, brandName:brand.name }) }); }
export async function deleteBrand(id) { return request(`/brands/${id}`, { method:"DELETE", headers:authHeaders() }); }

export async function clickProduct(productId) { if (!token()) return null; try { return await request(`/products/${productId}/click`, { method:"POST", headers:{Authorization:`Bearer ${token()}`} }); } catch(e) { console.warn("Click:",e.message); return null; } }
export async function getCart() { return request("/cart", { headers:authHeaders() }); }
export async function addToCart(productId, quantity=1) {
  const result = await request("/cart/items", { method:"POST", headers:authHeaders(), body:JSON.stringify({ productId:+productId, quantity:+quantity }) });
  window.dispatchEvent(new Event("recommendations:refresh"));
  return result;
}
export async function updateCartItem(cartItemId, quantity) { return request(`/cart/items/${cartItemId}`, { method:"PUT", headers:authHeaders(), body:JSON.stringify({quantity:+quantity}) }); }
export async function removeCartItem(cartItemId) { return request(`/cart/items/${cartItemId}`, { method:"DELETE", headers:authHeaders() }); }
export async function clearCart() { return request("/cart", { method:"DELETE", headers:authHeaders() }); }

export async function createOrder(order) {
  const result = await request("/orders", { method:"POST", headers:authHeaders(), body:JSON.stringify({ receiverName:order.receiverName, phone:order.phone, shippingAddress:order.shippingAddress, paymentMethod:order.paymentMethod, cartItemIds:order.cartItemIds||null }) });
  window.dispatchEvent(new Event("recommendations:refresh"));
  return result;
}
export async function getOrders() { return request("/orders", { headers:authHeaders() }); }
export async function getOrder(id) { return request(`/orders/${id}`, { headers:authHeaders() }); }
export async function updateOrderStatus(id,status) { return request(`/orders/${id}/status`, { method:"PUT", headers:authHeaders(), body:JSON.stringify({ status, Status: status }) }); }

function recList(data) { return Array.isArray(data) ? data : Array.isArray(data?.recommendations) ? data.recommendations : []; }
export async function getRecommendations(topUsers=5,topProducts=10) { if(!token()) return []; try { return recList(await request(`/recommendations/collaborative?topUsers=${topUsers}&topProducts=${topProducts}`,{headers:authHeaders()})); } catch { return []; } }
export async function getSavedRecommendations(topProducts=10) { if(!token()) return []; try { return recList(await request(`/recommendations/collaborative/saved?topProducts=${topProducts}`,{headers:authHeaders()})); } catch { return []; } }
export async function generateRecommendations(topUsers=5,topProducts=10) { return request(`/recommendations/collaborative/generate?topUsers=${topUsers}&topProducts=${topProducts}`,{method:"POST",headers:authHeaders()}); }
export async function getSimilarUsers(topUsers=5) { return request(`/recommendations/collaborative/similar-users?topUsers=${topUsers}`,{headers:authHeaders()}); }
export async function getPythonRecommendations() { return request("/recommendations/python",{headers:authHeaders()}); }

export async function getProductReviews(productId) { return request(`/reviews/product/${productId}`); }
export async function canReview(productId) { if (!token()) return false; try { const d=await request(`/reviews/can-review/${productId}`,{headers:authHeaders()}); return !!d?.canReview; } catch { return false; } }
export async function getMyReviews() { return request("/reviews/my",{headers:authHeaders()}); }
export async function createReview(productId,rating,comment) { return request("/reviews",{method:"POST",headers:authHeaders(),body:JSON.stringify({productId:+productId,rating:+rating,comment:comment||""})}); }
export async function updateReview(id,rating,comment) { return request(`/reviews/${id}`,{method:"PUT",headers:authHeaders(),body:JSON.stringify({rating:+rating,comment:comment||""})}); }
export async function deleteReview(id) { return request(`/reviews/${id}`,{method:"DELETE",headers:authHeaders()}); }

export async function getMyBehaviors() { return request("/user-behaviors/my",{headers:authHeaders()}); }
export async function getBehaviorSummary() { return request("/user-behaviors/summary",{headers:authHeaders()}); }
export async function getProductScores() { return request("/user-behaviors/product-score",{headers:authHeaders()}); }
export async function createBehavior(productId,actionType) { return request("/user-behaviors",{method:"POST",headers:authHeaders(),body:JSON.stringify({productId:+productId,actionType})}); }
export async function makeAdmin() { return request("/auth/make-admin",{method:"POST",headers:authHeaders()}); }
export async function becomeSeller() { const data = await request("/auth/become-seller",{method:"POST",headers:authHeaders()}); if (data?.token) localStorage.setItem("token", data.token); if (data?.user) localStorage.setItem("user", JSON.stringify(data.user)); return data; }

export async function getAdminDashboard() { return request("/admin/dashboard", { headers:authHeaders() }); }

export async function payOrder(orderId) { return request(`/payments/${orderId}/pay`, { method:"POST", headers:authHeaders() }); }

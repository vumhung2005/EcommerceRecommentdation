from fastapi import FastAPI
from pydantic import BaseModel
from typing import List
import numpy as np
from sklearn.metrics.pairwise import cosine_similarity



app = FastAPI(
    title="Ecommerce Recommendation AI",
    description="AI Recommendation Service",
    version="1.0.0"
)



class UserProductData(BaseModel):
    user_id: str
    product_id: int
    score: float


class RecommendationRequest(BaseModel):
    user_id: str
    interactions: List[UserProductData]



@app.get("/")
def root():
    return {
        "message": "Recommendation AI is running",
        "status": "success"
    }




@app.post("/recommend")
def recommend(request: RecommendationRequest):

    data = request.interactions


    if not data:
        return {
            "user_id": request.user_id,
            "similar_users": [],
            "recommendations": []
        }

    

    users = sorted(
        set(item.user_id for item in data)
    )


    products = sorted(
        set(item.product_id for item in data)
    )


    matrix = np.zeros(
        (len(users), len(products))
    )

    user_index = {
        user_id: index
        for index, user_id in enumerate(users)
    }

    product_index = {
        product_id: index
        for index, product_id in enumerate(products)
    }

    for item in data:

        user_position = user_index[item.user_id]

        product_position = product_index[item.product_id]

        matrix[user_position][product_position] = item.score

   

    similarity_matrix = cosine_similarity(matrix)

    current_user_index = user_index[request.user_id]

    similarities = similarity_matrix[current_user_index]

    

    similar_users = []

    for index, similarity in enumerate(similarities):

       
        if users[index] == request.user_id:
            continue

        similar_users.append({
            "user_id": users[index],
            "similarity": round(
                float(similarity),
                4
            )
        })

   
    similar_users.sort(
        key=lambda x: x["similarity"],
        reverse=True
    )

    

    current_user_products = {
        item.product_id
        for item in data
        if item.user_id == request.user_id
    }

    

    recommendation_scores = {}

    for similar_user in similar_users:

        similar_user_id = similar_user["user_id"]

        similarity = similar_user["similarity"]

       
        if similarity <= 0:
            continue

        
        for item in data:

            if item.user_id != similar_user_id:
                continue

            product_id = item.product_id

            
            if product_id in current_user_products:
                continue

            score = similarity * item.score

            if product_id not in recommendation_scores:
                recommendation_scores[product_id] = 0

            recommendation_scores[product_id] += score

   

    recommendations = []

    for product_id, score in recommendation_scores.items():

        recommendations.append({
            "product_id": product_id,
            "score": round(
                float(score),
                4
            )
        })

    
    recommendations.sort(
        key=lambda x: x["score"],
        reverse=True
    )

   
    return {
        "user_id": request.user_id,

        "similar_users": similar_users[:5],

        "recommendations": recommendations[:10]
    }
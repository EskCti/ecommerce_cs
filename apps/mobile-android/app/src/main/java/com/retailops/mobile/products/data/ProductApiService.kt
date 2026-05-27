package com.retailops.mobile.products.data

import retrofit2.Response
import retrofit2.http.GET
import retrofit2.http.Path
import retrofit2.http.Query

interface ProductApiService {
    @GET("api/catalog/products")
    suspend fun listProducts(
        @Query("page") page: Int = 1,
        @Query("pageSize") pageSize: Int = 20,
    ): Response<PaginatedProductsDto>

    @GET("api/catalog/products/{id}")
    suspend fun getProduct(@Path("id") id: String): Response<ProductOutputDto>

    @GET("api/catalog/products/by-barcode/{code}")
    suspend fun getByBarcode(@Path("code") code: String): Response<ProductOutputDto>
}

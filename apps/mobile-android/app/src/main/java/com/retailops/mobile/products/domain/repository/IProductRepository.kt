package com.retailops.mobile.products.domain.repository

import com.retailops.mobile.products.domain.Product
import com.retailops.mobile.products.domain.ProductPage
import com.retailops.mobile.products.domain.Result

interface IProductRepository {
    suspend fun listProducts(page: Int = 1, pageSize: Int = 20): Result<ProductPage>

    suspend fun findById(id: String): Result<Product>

    suspend fun findByBarcode(code: String): Result<Product>
}

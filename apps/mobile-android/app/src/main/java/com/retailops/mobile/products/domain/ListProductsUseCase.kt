package com.retailops.mobile.products.domain

import com.retailops.mobile.products.domain.repository.IProductRepository
import javax.inject.Inject

class ListProductsUseCase @Inject constructor(
    private val repository: IProductRepository,
) {
    suspend operator fun invoke(page: Int = 1, pageSize: Int = 20): Result<ProductPage> =
        repository.listProducts(page, pageSize)
}

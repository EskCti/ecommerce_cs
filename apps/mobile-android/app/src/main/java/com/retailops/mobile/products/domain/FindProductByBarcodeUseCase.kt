package com.retailops.mobile.products.domain

import com.retailops.mobile.products.domain.repository.IProductRepository
import javax.inject.Inject

class FindProductByBarcodeUseCase @Inject constructor(
    private val repository: IProductRepository,
) {
    suspend operator fun invoke(code: String): Result<Product> =
        repository.findByBarcode(code.trim())
}

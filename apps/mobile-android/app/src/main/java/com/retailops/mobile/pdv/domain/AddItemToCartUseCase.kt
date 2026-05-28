package com.retailops.mobile.pdv.domain

import com.retailops.mobile.pdv.domain.repository.ISalesRepository
import javax.inject.Inject

class AddItemToCartUseCase @Inject constructor(
    private val repository: ISalesRepository,
) {
    suspend operator fun invoke(scannedValue: String, unitPriceOverride: Double? = null): Result<CashSession> =
        repository.addItemToCart(scannedValue.trim(), unitPriceOverride)
}

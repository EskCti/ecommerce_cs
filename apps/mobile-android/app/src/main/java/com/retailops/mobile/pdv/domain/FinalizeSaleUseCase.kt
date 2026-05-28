package com.retailops.mobile.pdv.domain

import com.retailops.mobile.pdv.domain.repository.ISalesRepository
import javax.inject.Inject

class FinalizeSaleUseCase @Inject constructor(
    private val repository: ISalesRepository,
) {
    suspend operator fun invoke(
        paymentMethodId: String,
        paymentTerms: String,
        amountPaid: Double,
        discountAmount: Double = 0.0,
        customerId: String? = null,
        sellerCommissionPercent: Double? = null,
    ): Result<Sale> =
        repository.finalizeSale(
            paymentMethodId = paymentMethodId.trim(),
            paymentTerms = paymentTerms.trim(),
            amountPaid = amountPaid,
            discountAmount = discountAmount,
            customerId = customerId?.trim()?.ifBlank { null },
            sellerCommissionPercent = sellerCommissionPercent,
        )
}

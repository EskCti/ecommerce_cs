package com.retailops.mobile.pdv.domain.repository

import com.retailops.mobile.pdv.domain.CashSession
import com.retailops.mobile.pdv.domain.Result
import com.retailops.mobile.pdv.domain.Sale

interface ISalesRepository {
    suspend fun openCashSession(
        terminalId: String,
        managerUserId: String,
        managerPin: String,
        openingFloat: Double,
    ): Result<CashSession>

    suspend fun getCurrentSession(): Result<CashSession?>

    suspend fun addItemToCart(scannedValue: String, unitPriceOverride: Double? = null): Result<CashSession>

    suspend fun removeCartLine(lineId: String): Result<CashSession>

    suspend fun finalizeSale(
        paymentMethodId: String,
        paymentTerms: String,
        amountPaid: Double,
        discountAmount: Double = 0.0,
        customerId: String? = null,
        sellerCommissionPercent: Double? = null,
    ): Result<Sale>
}

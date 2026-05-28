package com.retailops.mobile.pdv.data

data class OpenCashSessionInputDto(
    val terminalId: String,
    val managerUserId: String,
    val managerPin: String,
    val openingFloat: Double,
)

data class AddItemToCartInputDto(
    val scannedValue: String,
    val unitPriceOverride: Double? = null,
)

data class FinalizeSaleInputDto(
    val paymentMethodId: String,
    val paymentTerms: String,
    val customerId: String? = null,
    val amountPaid: Double,
    val discountAmount: Double = 0.0,
    val sellerCommissionPercent: Double? = null,
)

data class CartLineOutputDto(
    val id: String,
    val productId: String,
    val barcode: String,
    val quantity: Int,
    val unitPrice: Double,
    val lineTotal: Double,
    val status: String,
    val requiresGrade: Boolean,
    val gradeOptionIds: List<String> = emptyList(),
)

data class CashSessionOutputDto(
    val id: String,
    val tenantId: Int,
    val terminalId: String,
    val operatorUserId: String,
    val status: String,
    val openingFloat: Double,
    val totalSold: Double,
    val totalWithdrawals: Double,
    val countedCash: Double? = null,
    val breakage: Double? = null,
    val openedAt: String,
    val closedAt: String? = null,
    val lines: List<CartLineOutputDto> = emptyList(),
)

data class SaleOutputDto(
    val id: String,
    val tenantId: Int,
    val cashSessionId: String,
    val operatorUserId: String,
    val paymentTerms: String,
    val customerId: String? = null,
    val paymentMethodId: String,
    val subtotal: Double,
    val discount: Double,
    val total: Double,
    val change: Double,
    val commissionAmount: Double,
    val isCancelled: Boolean,
    val completedAt: String,
    val cancelledAt: String? = null,
    val lines: List<CartLineOutputDto> = emptyList(),
)

data class ApiErrorDto(
    val error: String? = null,
)

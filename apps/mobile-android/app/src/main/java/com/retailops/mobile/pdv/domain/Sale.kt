package com.retailops.mobile.pdv.domain

sealed class Result<out T> {
    data class Success<T>(val value: T) : Result<T>()
    data class Failure(val message: String) : Result<Nothing>()

    companion object {
        const val UNAUTHORIZED = "Unauthorized"
        const val NO_SESSION = "NoSession"
    }
}

data class Sale(
    val id: String,
    val tenantId: Int,
    val cashSessionId: String,
    val operatorUserId: String,
    val paymentTerms: String,
    val customerId: String?,
    val paymentMethodId: String,
    val subtotal: Double,
    val discount: Double,
    val total: Double,
    val change: Double,
    val commissionAmount: Double,
    val isCancelled: Boolean,
    val completedAt: String,
    val cancelledAt: String?,
    val lines: List<CartLine>,
)

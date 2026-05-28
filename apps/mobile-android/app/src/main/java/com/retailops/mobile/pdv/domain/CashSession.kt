package com.retailops.mobile.pdv.domain

data class CashSession(
    val id: String,
    val tenantId: Int,
    val terminalId: String,
    val operatorUserId: String,
    val status: String,
    val openingFloat: Double,
    val totalSold: Double,
    val totalWithdrawals: Double,
    val countedCash: Double?,
    val breakage: Double?,
    val openedAt: String,
    val closedAt: String?,
    val lines: List<CartLine>,
) {
    val cartSubtotal: Double
        get() = lines.sumOf { it.lineTotal }
}

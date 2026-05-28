package com.retailops.mobile.pdv.domain

data class CartLine(
    val id: String,
    val productId: String,
    val barcode: String,
    val quantity: Int,
    val unitPrice: Double,
    val lineTotal: Double,
    val status: String,
    val requiresGrade: Boolean,
    val gradeOptionIds: List<String>,
)

package com.retailops.mobile.products.domain

data class Product(
    val id: String,
    val barcode: String,
    val name: String,
    val salePrice: Double,
    val stock: Int,
    val categoryId: String,
    val isActive: Boolean = true,
    val isLowStock: Boolean = false,
    val description: String? = null,
    val costPrice: Double? = null,
    val profitMargin: Double? = null,
    val stockAlertLevel: Int? = null,
    val supplierId: String? = null,
    val photoPath: String? = null,
    val isOpenPrice: Boolean = false,
)

data class ProductPage(
    val items: List<Product>,
    val page: Int,
    val pageSize: Int,
    val totalCount: Int,
)

sealed class Result<out T> {
    data class Success<T>(val value: T) : Result<T>()
    data class Failure(val message: String) : Result<Nothing>()

    companion object {
        const val UNAUTHORIZED = "Unauthorized"
    }
}

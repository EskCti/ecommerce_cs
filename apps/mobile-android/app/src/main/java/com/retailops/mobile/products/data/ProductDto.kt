package com.retailops.mobile.products.data

data class ProductListItemDto(
    val id: String,
    val barcode: String,
    val name: String,
    val salePrice: Double,
    val stock: Int,
    val categoryId: String,
    val isActive: Boolean = true,
    val isLowStock: Boolean = false,
)

data class PaginatedProductsDto(
    val items: List<ProductListItemDto>,
    val page: Int,
    val pageSize: Int,
    val totalCount: Int,
)

data class ProductOutputDto(
    val id: String,
    val tenantId: Int,
    val barcode: String,
    val name: String,
    val description: String?,
    val salePrice: Double,
    val costPrice: Double,
    val stock: Int,
    val profitMargin: Double,
    val stockAlertLevel: Int,
    val categoryId: String,
    val supplierId: String?,
    val photoPath: String?,
    val isActive: Boolean,
    val isOpenPrice: Boolean,
    val isLowStock: Boolean,
)

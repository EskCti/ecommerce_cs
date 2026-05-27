package com.retailops.mobile.products.data

import com.retailops.mobile.products.domain.Product
import com.retailops.mobile.products.domain.ProductPage
import com.retailops.mobile.products.domain.Result
import com.retailops.mobile.products.domain.repository.IProductRepository
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class ProductRepositoryImpl @Inject constructor(
    private val api: ProductApiService,
) : IProductRepository {
    override suspend fun listProducts(page: Int, pageSize: Int): Result<ProductPage> {
        val response = api.listProducts(page = page, pageSize = pageSize)
        if (response.code() == 401) return Result.Failure(Result.UNAUTHORIZED)
        if (!response.isSuccessful) return Result.Failure("Falha ao carregar produtos")
        val body = response.body() ?: return Result.Failure("Resposta vazia")
        return Result.Success(
            ProductPage(
                items = body.items.map { it.toDomain() },
                page = body.page,
                pageSize = body.pageSize,
                totalCount = body.totalCount,
            ),
        )
    }

    override suspend fun findById(id: String): Result<Product> {
        val response = api.getProduct(id.trim())
        if (response.code() == 401) return Result.Failure(Result.UNAUTHORIZED)
        if (response.code() == 404) return Result.Failure("Produto não encontrado")
        if (!response.isSuccessful) return Result.Failure("Falha ao carregar produto")
        val dto = response.body() ?: return Result.Failure("Resposta vazia")
        return Result.Success(dto.toDomain())
    }

    override suspend fun findByBarcode(code: String): Result<Product> {
        val response = api.getByBarcode(code.trim())
        if (response.code() == 401) return Result.Failure(Result.UNAUTHORIZED)
        if (response.code() == 404) return Result.Failure("Produto não encontrado")
        if (!response.isSuccessful) return Result.Failure("Falha ao buscar produto")
        val dto = response.body() ?: return Result.Failure("Resposta vazia")
        return Result.Success(dto.toDomain())
    }

    private fun ProductListItemDto.toDomain(): Product =
        Product(
            id = id,
            barcode = barcode,
            name = name,
            salePrice = salePrice,
            stock = stock,
            categoryId = categoryId,
            isActive = isActive,
            isLowStock = isLowStock,
        )

    private fun ProductOutputDto.toDomain(): Product =
        Product(
            id = id,
            barcode = barcode,
            name = name,
            salePrice = salePrice,
            stock = stock,
            categoryId = categoryId,
            isActive = isActive,
            isLowStock = isLowStock,
            description = description,
            costPrice = costPrice,
            profitMargin = profitMargin,
            stockAlertLevel = stockAlertLevel,
            supplierId = supplierId,
            photoPath = photoPath,
            isOpenPrice = isOpenPrice,
        )
}

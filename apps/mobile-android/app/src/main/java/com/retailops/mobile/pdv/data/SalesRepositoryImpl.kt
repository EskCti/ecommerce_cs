package com.retailops.mobile.pdv.data

import com.retailops.mobile.pdv.domain.CartLine
import com.retailops.mobile.pdv.domain.CashSession
import com.retailops.mobile.pdv.domain.Result
import com.retailops.mobile.pdv.domain.Sale
import com.retailops.mobile.pdv.domain.repository.ISalesRepository
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class SalesRepositoryImpl @Inject constructor(
    private val api: SalesApiService,
) : ISalesRepository {
    override suspend fun openCashSession(
        terminalId: String,
        managerUserId: String,
        managerPin: String,
        openingFloat: Double,
    ): Result<CashSession> {
        val response = api.openCashSession(
            OpenCashSessionInputDto(
                terminalId = terminalId,
                managerUserId = managerUserId,
                managerPin = managerPin,
                openingFloat = openingFloat,
            ),
        )
        if (response.code() == 401) return Result.Failure(Result.UNAUTHORIZED)
        if (response.code() == 403) return Result.Failure("PIN do gerente inválido")
        if (!response.isSuccessful) return Result.Failure(extractError(response, "Falha ao abrir caixa"))
        val dto = response.body() ?: return Result.Failure("Resposta vazia")
        return Result.Success(dto.toDomain())
    }

    override suspend fun getCurrentSession(): Result<CashSession?> {
        val response = api.getCurrentSession()
        if (response.code() == 401) return Result.Failure(Result.UNAUTHORIZED)
        if (response.code() == 404) return Result.Success(null)
        if (!response.isSuccessful) return Result.Failure(extractError(response, "Falha ao carregar sessão"))
        val dto = response.body() ?: return Result.Success(null)
        return Result.Success(dto.toDomain())
    }

    override suspend fun addItemToCart(scannedValue: String, unitPriceOverride: Double?): Result<CashSession> {
        val response = api.addItemToCart(
            AddItemToCartInputDto(
                scannedValue = scannedValue,
                unitPriceOverride = unitPriceOverride,
            ),
        )
        if (response.code() == 401) return Result.Failure(Result.UNAUTHORIZED)
        if (response.code() == 409) return Result.Failure("Nenhuma sessão de caixa aberta")
        if (!response.isSuccessful) return Result.Failure(extractError(response, "Falha ao adicionar item"))
        val dto = response.body() ?: return Result.Failure("Resposta vazia")
        return Result.Success(dto.toDomain())
    }

    override suspend fun removeCartLine(lineId: String): Result<CashSession> {
        val response = api.removeCartLine(lineId)
        if (response.code() == 401) return Result.Failure(Result.UNAUTHORIZED)
        if (!response.isSuccessful) return Result.Failure(extractError(response, "Falha ao remover item"))
        val dto = response.body() ?: return Result.Failure("Resposta vazia")
        return Result.Success(dto.toDomain())
    }

    override suspend fun finalizeSale(
        paymentMethodId: String,
        paymentTerms: String,
        amountPaid: Double,
        discountAmount: Double,
        customerId: String?,
        sellerCommissionPercent: Double?,
    ): Result<Sale> {
        val response = api.finalizeSale(
            FinalizeSaleInputDto(
                paymentMethodId = paymentMethodId,
                paymentTerms = paymentTerms,
                customerId = customerId,
                amountPaid = amountPaid,
                discountAmount = discountAmount,
                sellerCommissionPercent = sellerCommissionPercent,
            ),
        )
        if (response.code() == 401) return Result.Failure(Result.UNAUTHORIZED)
        if (!response.isSuccessful) return Result.Failure(extractError(response, "Falha ao finalizar venda"))
        val dto = response.body() ?: return Result.Failure("Resposta vazia")
        return Result.Success(dto.toDomain())
    }

    private fun extractError(response: retrofit2.Response<*>, fallback: String): String {
        val errorBody = response.errorBody()?.string().orEmpty()
        if (errorBody.contains("\"error\"")) {
            val match = Regex(""""error"\s*:\s*"([^"]+)"""").find(errorBody)
            if (match != null) return match.groupValues[1]
        }
        return fallback
    }

    private fun CashSessionOutputDto.toDomain(): CashSession =
        CashSession(
            id = id,
            tenantId = tenantId,
            terminalId = terminalId,
            operatorUserId = operatorUserId,
            status = status,
            openingFloat = openingFloat,
            totalSold = totalSold,
            totalWithdrawals = totalWithdrawals,
            countedCash = countedCash,
            breakage = breakage,
            openedAt = openedAt,
            closedAt = closedAt,
            lines = lines.map { it.toDomain() },
        )

    private fun SaleOutputDto.toDomain(): Sale =
        Sale(
            id = id,
            tenantId = tenantId,
            cashSessionId = cashSessionId,
            operatorUserId = operatorUserId,
            paymentTerms = paymentTerms,
            customerId = customerId,
            paymentMethodId = paymentMethodId,
            subtotal = subtotal,
            discount = discount,
            total = total,
            change = change,
            commissionAmount = commissionAmount,
            isCancelled = isCancelled,
            completedAt = completedAt,
            cancelledAt = cancelledAt,
            lines = lines.map { it.toDomain() },
        )

    private fun CartLineOutputDto.toDomain(): CartLine =
        CartLine(
            id = id,
            productId = productId,
            barcode = barcode,
            quantity = quantity,
            unitPrice = unitPrice,
            lineTotal = lineTotal,
            status = status,
            requiresGrade = requiresGrade,
            gradeOptionIds = gradeOptionIds,
        )
}

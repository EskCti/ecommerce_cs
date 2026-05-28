package com.retailops.mobile.pdv.data

import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.DELETE
import retrofit2.http.GET
import retrofit2.http.POST
import retrofit2.http.Path

interface SalesApiService {
    @POST("api/sales/cash-session/open")
    suspend fun openCashSession(@Body request: OpenCashSessionInputDto): Response<CashSessionOutputDto>

    @GET("api/sales/cash-session/current")
    suspend fun getCurrentSession(): Response<CashSessionOutputDto>

    @POST("api/sales/cash-session/current/cart/items")
    suspend fun addItemToCart(@Body request: AddItemToCartInputDto): Response<CashSessionOutputDto>

    @DELETE("api/sales/cash-session/current/cart/items/{lineId}")
    suspend fun removeCartLine(@Path("lineId") lineId: String): Response<CashSessionOutputDto>

    @POST("api/sales/cash-session/finalize")
    suspend fun finalizeSale(@Body request: FinalizeSaleInputDto): Response<SaleOutputDto>
}

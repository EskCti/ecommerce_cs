package com.retailops.mobile.customers.data

import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.POST

data class CustomerDto(
    val id: String,
    val name: String,
    val cpf: String,
    val phone: String?,
    val email: String?,
    val address: String?,
    val isActive: Boolean = true,
)

data class FindOrCreateByCpfRequestDto(
    val name: String,
    val cpf: String,
    val phone: String? = null,
    val email: String? = null,
    val address: String? = null,
)

interface CustomerApiService {
    @GET("api/crm/customers")
    suspend fun listCustomers(): Response<List<CustomerDto>>

    @POST("api/crm/customers")
    suspend fun createCustomer(@Body request: FindOrCreateByCpfRequestDto): Response<CustomerDto>

    @POST("api/crm/customers/find-or-create-by-cpf")
    suspend fun findOrCreateByCpf(@Body request: FindOrCreateByCpfRequestDto): Response<CustomerDto>
}

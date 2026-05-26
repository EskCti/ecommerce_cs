package com.retailops.mobile.customers.data

import com.retailops.mobile.customers.domain.Customer
import com.retailops.mobile.customers.domain.Result
import com.retailops.mobile.customers.domain.repository.ICustomerRepository
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class CustomerRepositoryImpl @Inject constructor(
    private val api: CustomerApiService,
) : ICustomerRepository {
    override suspend fun listCustomers(): Result<List<Customer>> {
        val response = api.listCustomers()
        if (response.code() == 401) return Result.Failure(Result.UNAUTHORIZED)
        if (!response.isSuccessful) return Result.Failure("Falha ao carregar clientes")
        val customers = response.body()?.map { it.toDomain() } ?: return Result.Failure("Resposta vazia")
        return Result.Success(customers)
    }

    override suspend fun findOrCreateByCpf(
        name: String,
        cpf: String,
        phone: String?,
        email: String?,
        address: String?,
    ): Result<Customer> {
        val request = FindOrCreateByCpfRequestDto(
            name = name.trim(),
            cpf = cpf.trim(),
            phone = phone?.trim()?.ifBlank { null },
            email = email?.trim()?.ifBlank { null },
            address = address?.trim()?.ifBlank { null },
        )
        val response = api.findOrCreateByCpf(request)
        if (response.code() == 401) return Result.Failure(Result.UNAUTHORIZED)
        if (!response.isSuccessful) return Result.Failure("Falha ao salvar cliente")
        val dto = response.body() ?: return Result.Failure("Resposta vazia")
        return Result.Success(dto.toDomain())
    }

    private fun CustomerDto.toDomain(): Customer =
        Customer(
            id = id,
            name = name,
            cpf = cpf,
            phone = phone,
            email = email,
            address = address,
            isActive = isActive,
        )
}

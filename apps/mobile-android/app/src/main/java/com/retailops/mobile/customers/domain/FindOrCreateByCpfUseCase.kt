package com.retailops.mobile.customers.domain

import com.retailops.mobile.customers.domain.repository.ICustomerRepository
import javax.inject.Inject

class FindOrCreateByCpfUseCase @Inject constructor(
    private val repository: ICustomerRepository,
) {
    suspend operator fun invoke(
        name: String,
        cpf: String,
        phone: String?,
        email: String?,
        address: String?,
    ): Result<Customer> = repository.findOrCreateByCpf(name, cpf, phone, email, address)
}

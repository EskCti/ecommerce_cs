package com.retailops.mobile.customers.domain.repository

import com.retailops.mobile.customers.domain.Customer
import com.retailops.mobile.customers.domain.Result

interface ICustomerRepository {
    suspend fun listCustomers(): Result<List<Customer>>

    suspend fun findOrCreateByCpf(
        name: String,
        cpf: String,
        phone: String?,
        email: String?,
        address: String?,
    ): Result<Customer>
}

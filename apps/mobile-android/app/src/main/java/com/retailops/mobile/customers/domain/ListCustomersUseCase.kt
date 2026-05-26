package com.retailops.mobile.customers.domain

import com.retailops.mobile.customers.domain.repository.ICustomerRepository
import javax.inject.Inject

class ListCustomersUseCase @Inject constructor(
    private val repository: ICustomerRepository,
) {
    suspend operator fun invoke(): Result<List<Customer>> = repository.listCustomers()
}

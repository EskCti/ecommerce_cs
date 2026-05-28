package com.retailops.mobile.pdv.domain

import com.retailops.mobile.pdv.domain.repository.ISalesRepository
import javax.inject.Inject

class GetCurrentSessionUseCase @Inject constructor(
    private val repository: ISalesRepository,
) {
    suspend operator fun invoke(): Result<CashSession?> =
        repository.getCurrentSession()
}

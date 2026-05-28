package com.retailops.mobile.pdv.domain

import com.retailops.mobile.pdv.domain.repository.ISalesRepository
import javax.inject.Inject

class OpenCashSessionUseCase @Inject constructor(
    private val repository: ISalesRepository,
) {
    suspend operator fun invoke(
        terminalId: String,
        managerUserId: String,
        managerPin: String,
        openingFloat: Double,
    ): Result<CashSession> =
        repository.openCashSession(
            terminalId = terminalId.trim(),
            managerUserId = managerUserId.trim(),
            managerPin = managerPin.trim(),
            openingFloat = openingFloat,
        )
}

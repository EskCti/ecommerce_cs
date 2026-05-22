package com.retailops.mobile.profile.domain

import com.retailops.mobile.profile.domain.repository.IAuthRepository
import javax.inject.Inject

class GetCurrentUserUseCase @Inject constructor(
    private val repository: IAuthRepository,
) {
    suspend operator fun invoke(): Result<User> = repository.getCurrentUser()
}

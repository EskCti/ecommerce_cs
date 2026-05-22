package com.retailops.mobile.profile.domain.repository

import com.retailops.mobile.profile.domain.Result
import com.retailops.mobile.profile.domain.User

interface IAuthRepository {
    suspend fun getCurrentUser(): Result<User>
}

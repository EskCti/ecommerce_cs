package com.retailops.mobile.profile.data

import com.retailops.mobile.profile.domain.Result
import com.retailops.mobile.profile.domain.User
import com.retailops.mobile.profile.domain.repository.IAuthRepository
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class AuthRepositoryImpl @Inject constructor(
    private val api: AuthApiService,
) : IAuthRepository {
    override suspend fun getCurrentUser(): Result<User> {
        val response = api.me()
        if (response.code() == 401) return Result.Failure(Result.UNAUTHORIZED)
        if (!response.isSuccessful) return Result.Failure("Failed to load profile")
        val dto = response.body() ?: return Result.Failure("Empty response")
        return Result.Success(
            User(
                id = dto.id,
                legacyUserId = dto.legacyUserId,
                tenantId = dto.tenantId,
                name = dto.name,
                email = dto.email,
                userLevel = dto.userLevel,
                permissionKeys = dto.permissionKeys,
            ),
        )
    }
}

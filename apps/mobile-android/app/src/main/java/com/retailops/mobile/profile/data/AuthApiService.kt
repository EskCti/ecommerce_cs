package com.retailops.mobile.profile.data

import retrofit2.Response
import retrofit2.http.GET

data class MeResponseDto(
    val id: String,
    val legacyUserId: Int,
    val tenantId: Int,
    val name: String,
    val email: String?,
    val userLevel: String,
    val permissionKeys: List<String>,
)

interface AuthApiService {
    @GET("api/auth/me")
    suspend fun me(): Response<MeResponseDto>
}

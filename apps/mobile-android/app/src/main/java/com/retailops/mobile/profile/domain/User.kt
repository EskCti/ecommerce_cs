package com.retailops.mobile.profile.domain

data class User(
    val id: String,
    val legacyUserId: Int,
    val tenantId: Int,
    val name: String,
    val email: String?,
    val userLevel: String,
    val permissionKeys: List<String>,
)

sealed class Result<out T> {
    data class Success<T>(val value: T) : Result<T>()
    data class Failure(val message: String) : Result<Nothing>()

    companion object {
        const val UNAUTHORIZED = "Unauthorized"
    }
}

package com.retailops.mobile.customers.domain

data class Customer(
    val id: String,
    val name: String,
    val cpf: String,
    val phone: String?,
    val email: String?,
    val address: String?,
    val isActive: Boolean = true,
)

sealed class Result<out T> {
    data class Success<T>(val value: T) : Result<T>()
    data class Failure(val message: String) : Result<Nothing>()

    companion object {
        const val UNAUTHORIZED = "Unauthorized"
    }
}

package com.retailops.mobile.core.network

import retrofit2.http.GET

data class HealthResponse(val status: String, val service: String)

interface HealthApiService {
    @GET("health")
    suspend fun getHealth(): HealthResponse
}

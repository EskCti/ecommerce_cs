package com.retailops.mobile.pdv.di

import com.retailops.mobile.pdv.data.SalesApiService
import com.retailops.mobile.pdv.data.SalesRepositoryImpl
import com.retailops.mobile.pdv.domain.repository.ISalesRepository
import dagger.Binds
import dagger.Module
import dagger.Provides
import dagger.hilt.InstallIn
import dagger.hilt.components.SingletonComponent
import retrofit2.Retrofit
import javax.inject.Singleton

@Module
@InstallIn(SingletonComponent::class)
abstract class PdvModule {
    @Binds
    @Singleton
    abstract fun bindSalesRepository(impl: SalesRepositoryImpl): ISalesRepository

    companion object {
        @Provides
        @Singleton
        fun provideSalesApiService(retrofit: Retrofit): SalesApiService =
            retrofit.create(SalesApiService::class.java)
    }
}

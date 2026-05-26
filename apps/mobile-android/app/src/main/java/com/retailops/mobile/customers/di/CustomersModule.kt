package com.retailops.mobile.customers.di

import com.retailops.mobile.customers.data.CustomerApiService
import com.retailops.mobile.customers.data.CustomerRepositoryImpl
import com.retailops.mobile.customers.domain.repository.ICustomerRepository
import dagger.Binds
import dagger.Module
import dagger.Provides
import dagger.hilt.InstallIn
import dagger.hilt.components.SingletonComponent
import retrofit2.Retrofit
import javax.inject.Singleton

@Module
@InstallIn(SingletonComponent::class)
abstract class CustomersModule {
    @Binds
    @Singleton
    abstract fun bindCustomerRepository(impl: CustomerRepositoryImpl): ICustomerRepository

    companion object {
        @Provides
        @Singleton
        fun provideCustomerApiService(retrofit: Retrofit): CustomerApiService =
            retrofit.create(CustomerApiService::class.java)
    }
}

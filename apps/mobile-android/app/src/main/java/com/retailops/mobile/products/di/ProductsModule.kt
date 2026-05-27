package com.retailops.mobile.products.di

import com.retailops.mobile.products.data.ProductApiService
import com.retailops.mobile.products.data.ProductRepositoryImpl
import com.retailops.mobile.products.domain.repository.IProductRepository
import dagger.Binds
import dagger.Module
import dagger.Provides
import dagger.hilt.InstallIn
import dagger.hilt.components.SingletonComponent
import retrofit2.Retrofit
import javax.inject.Singleton

@Module
@InstallIn(SingletonComponent::class)
abstract class ProductsModule {
    @Binds
    @Singleton
    abstract fun bindProductRepository(impl: ProductRepositoryImpl): IProductRepository

    companion object {
        @Provides
        @Singleton
        fun provideProductApiService(retrofit: Retrofit): ProductApiService =
            retrofit.create(ProductApiService::class.java)
    }
}

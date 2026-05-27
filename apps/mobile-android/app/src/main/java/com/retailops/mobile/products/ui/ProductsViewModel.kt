package com.retailops.mobile.products.ui

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.retailops.mobile.products.domain.FindProductByBarcodeUseCase
import com.retailops.mobile.products.domain.ListProductsUseCase
import com.retailops.mobile.products.domain.Product
import com.retailops.mobile.products.domain.Result
import com.retailops.mobile.products.domain.repository.IProductRepository
import com.retailops.mobile.profile.data.AuthTokenStore
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import javax.inject.Inject

data class ProductsUiState(
    val listLoading: Boolean = true,
    val refreshing: Boolean = false,
    val products: List<Product> = emptyList(),
    val totalCount: Int = 0,
    val listError: String? = null,
    val sessionExpired: Boolean = false,
    val detailLoading: Boolean = false,
    val detailProduct: Product? = null,
    val detailError: String? = null,
)

@HiltViewModel
class ProductsViewModel @Inject constructor(
    private val listProducts: ListProductsUseCase,
    private val findProductByBarcode: FindProductByBarcodeUseCase,
    private val repository: IProductRepository,
    private val tokenStore: AuthTokenStore,
) : ViewModel() {
    private val _state = MutableStateFlow(ProductsUiState())
    val state: StateFlow<ProductsUiState> = _state.asStateFlow()

    init {
        load()
    }

    fun load() {
        viewModelScope.launch {
            _state.value = _state.value.copy(listLoading = true, listError = null, sessionExpired = false)
            when (val result = listProducts()) {
                is Result.Success ->
                    _state.value = _state.value.copy(
                        listLoading = false,
                        refreshing = false,
                        products = result.value.items,
                        totalCount = result.value.totalCount,
                    )
                is Result.Failure -> handleListFailure(result.message)
            }
        }
    }

    fun refresh() {
        viewModelScope.launch {
            _state.value = _state.value.copy(refreshing = true, listError = null, sessionExpired = false)
            when (val result = listProducts()) {
                is Result.Success ->
                    _state.value = _state.value.copy(
                        listLoading = false,
                        refreshing = false,
                        products = result.value.items,
                        totalCount = result.value.totalCount,
                    )
                is Result.Failure -> {
                    _state.value = _state.value.copy(refreshing = false)
                    handleListFailure(result.message)
                }
            }
        }
    }

    fun loadProduct(id: String) {
        viewModelScope.launch {
            _state.value = _state.value.copy(
                detailLoading = true,
                detailProduct = null,
                detailError = null,
                sessionExpired = false,
            )
            when (val result = repository.findById(id)) {
                is Result.Success ->
                    _state.value = _state.value.copy(
                        detailLoading = false,
                        detailProduct = result.value,
                    )
                is Result.Failure -> handleDetailFailure(result.message)
            }
        }
    }

    fun findByBarcode(code: String) {
        viewModelScope.launch {
            _state.value = _state.value.copy(
                detailLoading = true,
                detailProduct = null,
                detailError = null,
                sessionExpired = false,
            )
            when (val result = findProductByBarcode(code)) {
                is Result.Success ->
                    _state.value = _state.value.copy(
                        detailLoading = false,
                        detailProduct = result.value,
                    )
                is Result.Failure -> handleDetailFailure(result.message)
            }
        }
    }

    fun clearDetail() {
        _state.value = _state.value.copy(
            detailLoading = false,
            detailProduct = null,
            detailError = null,
        )
    }

    private fun handleListFailure(message: String) {
        if (message == Result.UNAUTHORIZED) {
            viewModelScope.launch { tokenStore.clear() }
            _state.value = ProductsUiState(
                listLoading = false,
                sessionExpired = true,
                listError = "Sessão expirada. Faça login novamente.",
            )
        } else {
            _state.value = _state.value.copy(listLoading = false, listError = message)
        }
    }

    private fun handleDetailFailure(message: String) {
        if (message == Result.UNAUTHORIZED) {
            viewModelScope.launch { tokenStore.clear() }
            _state.value = _state.value.copy(
                detailLoading = false,
                sessionExpired = true,
                detailError = "Sessão expirada. Faça login novamente.",
            )
        } else {
            _state.value = _state.value.copy(detailLoading = false, detailError = message)
        }
    }
}

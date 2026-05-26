package com.retailops.mobile.customers.ui

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.retailops.mobile.customers.domain.Customer
import com.retailops.mobile.customers.domain.FindOrCreateByCpfUseCase
import com.retailops.mobile.customers.domain.ListCustomersUseCase
import com.retailops.mobile.customers.domain.Result
import com.retailops.mobile.profile.data.AuthTokenStore
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import javax.inject.Inject

data class CustomersUiState(
    val listLoading: Boolean = true,
    val refreshing: Boolean = false,
    val customers: List<Customer> = emptyList(),
    val listError: String? = null,
    val sessionExpired: Boolean = false,
    val formSubmitting: Boolean = false,
    val formError: String? = null,
    val formSuccess: Boolean = false,
)

@HiltViewModel
class CustomersViewModel @Inject constructor(
    private val listCustomers: ListCustomersUseCase,
    private val findOrCreateByCpf: FindOrCreateByCpfUseCase,
    private val tokenStore: AuthTokenStore,
) : ViewModel() {
    private val _state = MutableStateFlow(CustomersUiState())
    val state: StateFlow<CustomersUiState> = _state.asStateFlow()

    init {
        load()
    }

    fun load() {
        viewModelScope.launch {
            _state.value = _state.value.copy(listLoading = true, listError = null, sessionExpired = false)
            when (val result = listCustomers()) {
                is Result.Success ->
                    _state.value = _state.value.copy(
                        listLoading = false,
                        refreshing = false,
                        customers = result.value,
                    )
                is Result.Failure -> handleListFailure(result.message)
            }
        }
    }

    fun refresh() {
        viewModelScope.launch {
            _state.value = _state.value.copy(refreshing = true, listError = null, sessionExpired = false)
            when (val result = listCustomers()) {
                is Result.Success ->
                    _state.value = _state.value.copy(
                        listLoading = false,
                        refreshing = false,
                        customers = result.value,
                    )
                is Result.Failure -> {
                    _state.value = _state.value.copy(refreshing = false)
                    handleListFailure(result.message)
                }
            }
        }
    }

    fun submitCustomer(
        name: String,
        cpf: String,
        phone: String,
        email: String,
        address: String,
    ) {
        viewModelScope.launch {
            _state.value = _state.value.copy(
                formSubmitting = true,
                formError = null,
                formSuccess = false,
            )
            when (
                val result = findOrCreateByCpf(
                    name = name,
                    cpf = cpf,
                    phone = phone.ifBlank { null },
                    email = email.ifBlank { null },
                    address = address.ifBlank { null },
                )
            ) {
                is Result.Success -> {
                    _state.value = _state.value.copy(
                        formSubmitting = false,
                        formSuccess = true,
                        customers = listOf(result.value) + _state.value.customers.filter { it.id != result.value.id },
                    )
                }
                is Result.Failure -> {
                    if (result.message == Result.UNAUTHORIZED) {
                        tokenStore.clear()
                        _state.value = _state.value.copy(
                            formSubmitting = false,
                            sessionExpired = true,
                            formError = "Sessão expirada. Faça login novamente.",
                        )
                    } else {
                        _state.value = _state.value.copy(
                            formSubmitting = false,
                            formError = result.message,
                        )
                    }
                }
            }
        }
    }

    fun clearFormSuccess() {
        _state.value = _state.value.copy(formSuccess = false, formError = null)
    }

    private fun handleListFailure(message: String) {
        if (message == Result.UNAUTHORIZED) {
            viewModelScope.launch { tokenStore.clear() }
            _state.value = CustomersUiState(
                listLoading = false,
                sessionExpired = true,
                listError = "Sessão expirada. Faça login novamente.",
            )
        } else {
            _state.value = _state.value.copy(listLoading = false, listError = message)
        }
    }
}

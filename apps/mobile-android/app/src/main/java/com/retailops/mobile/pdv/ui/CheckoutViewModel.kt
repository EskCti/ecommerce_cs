package com.retailops.mobile.pdv.ui

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.retailops.mobile.pdv.domain.FinalizeSaleUseCase
import com.retailops.mobile.pdv.domain.GetCurrentSessionUseCase
import com.retailops.mobile.pdv.domain.Result
import com.retailops.mobile.pdv.domain.Sale
import com.retailops.mobile.profile.data.AuthTokenStore
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import javax.inject.Inject

data class CheckoutUiState(
    val loading: Boolean = true,
    val submitting: Boolean = false,
    val cartSubtotal: Double = 0.0,
    val paymentMethodId: String = "",
    val paymentTerms: String = "Cash",
    val amountPaid: String = "",
    val discountAmount: String = "0",
    val customerId: String = "",
    val error: String? = null,
    val sessionExpired: Boolean = false,
    val finalizeSuccess: Sale? = null,
)

@HiltViewModel
class CheckoutViewModel @Inject constructor(
    private val getCurrentSession: GetCurrentSessionUseCase,
    private val finalizeSale: FinalizeSaleUseCase,
    private val tokenStore: AuthTokenStore,
) : ViewModel() {
    private val _state = MutableStateFlow(CheckoutUiState())
    val state: StateFlow<CheckoutUiState> = _state.asStateFlow()

    init {
        loadCart()
    }

    fun loadCart() {
        viewModelScope.launch {
            _state.value = _state.value.copy(loading = true, error = null, sessionExpired = false)
            when (val result = getCurrentSession()) {
                is Result.Success -> {
                    val subtotal = result.value?.cartSubtotal ?: 0.0
                    _state.value = _state.value.copy(
                        loading = false,
                        cartSubtotal = subtotal,
                        amountPaid = if (_state.value.amountPaid.isBlank()) {
                            "%.2f".format(subtotal)
                        } else {
                            _state.value.amountPaid
                        },
                    )
                }
                is Result.Failure -> handleFailure(result.message)
            }
        }
    }

    fun updatePaymentMethodId(value: String) {
        _state.value = _state.value.copy(paymentMethodId = value)
    }

    fun updatePaymentTerms(value: String) {
        _state.value = _state.value.copy(paymentTerms = value)
    }

    fun updateAmountPaid(value: String) {
        _state.value = _state.value.copy(amountPaid = value)
    }

    fun updateDiscountAmount(value: String) {
        _state.value = _state.value.copy(discountAmount = value)
    }

    fun updateCustomerId(value: String) {
        _state.value = _state.value.copy(customerId = value)
    }

    fun submit() {
        val paymentMethodId = _state.value.paymentMethodId.trim()
        if (paymentMethodId.isBlank()) {
            _state.value = _state.value.copy(error = "Informe o ID da forma de pagamento")
            return
        }

        val amountPaid = _state.value.amountPaid.replace(',', '.').toDoubleOrNull()
        val discountAmount = _state.value.discountAmount.replace(',', '.').toDoubleOrNull() ?: 0.0
        if (amountPaid == null || amountPaid < 0) {
            _state.value = _state.value.copy(error = "Valor pago inválido")
            return
        }
        if (_state.value.cartSubtotal <= 0) {
            _state.value = _state.value.copy(error = "Carrinho vazio")
            return
        }

        viewModelScope.launch {
            _state.value = _state.value.copy(submitting = true, error = null, finalizeSuccess = null)
            when (
                val result = finalizeSale(
                    paymentMethodId = paymentMethodId,
                    paymentTerms = _state.value.paymentTerms,
                    amountPaid = amountPaid,
                    discountAmount = discountAmount,
                    customerId = _state.value.customerId.ifBlank { null },
                )
            ) {
                is Result.Success ->
                    _state.value = _state.value.copy(
                        submitting = false,
                        finalizeSuccess = result.value,
                    )
                is Result.Failure -> {
                    _state.value = _state.value.copy(submitting = false)
                    handleFailure(result.message)
                }
            }
        }
    }

    private fun handleFailure(message: String) {
        if (message == Result.UNAUTHORIZED) {
            viewModelScope.launch { tokenStore.clear() }
            _state.value = CheckoutUiState(
                loading = false,
                sessionExpired = true,
                error = "Sessão expirada. Faça login novamente.",
            )
        } else {
            _state.value = _state.value.copy(loading = false, error = message)
        }
    }
}

package com.retailops.mobile.pdv.ui

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.retailops.mobile.pdv.domain.AddItemToCartUseCase
import com.retailops.mobile.pdv.domain.CashSession
import com.retailops.mobile.pdv.domain.GetCurrentSessionUseCase
import com.retailops.mobile.pdv.domain.Result
import com.retailops.mobile.pdv.domain.repository.ISalesRepository
import com.retailops.mobile.profile.data.AuthTokenStore
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import javax.inject.Inject

data class PdvUiState(
    val loading: Boolean = true,
    val session: CashSession? = null,
    val scanInput: String = "",
    val scanning: Boolean = false,
    val removingLineId: String? = null,
    val error: String? = null,
    val sessionExpired: Boolean = false,
    val noSession: Boolean = false,
)

@HiltViewModel
class PdvViewModel @Inject constructor(
    private val getCurrentSession: GetCurrentSessionUseCase,
    private val addItemToCart: AddItemToCartUseCase,
    private val repository: ISalesRepository,
    private val tokenStore: AuthTokenStore,
) : ViewModel() {
    private val _state = MutableStateFlow(PdvUiState())
    val state: StateFlow<PdvUiState> = _state.asStateFlow()

    init {
        loadSession()
    }

    fun loadSession() {
        viewModelScope.launch {
            _state.value = _state.value.copy(loading = true, error = null, sessionExpired = false, noSession = false)
            when (val result = getCurrentSession()) {
                is Result.Success -> {
                    val session = result.value
                    _state.value = _state.value.copy(
                        loading = false,
                        session = session,
                        noSession = session == null,
                    )
                }
                is Result.Failure -> handleFailure(result.message)
            }
        }
    }

    fun updateScanInput(value: String) {
        _state.value = _state.value.copy(scanInput = value)
    }

    fun scanBarcode() {
        val code = _state.value.scanInput.trim()
        if (code.isBlank()) {
            _state.value = _state.value.copy(error = "Informe o código de barras")
            return
        }

        viewModelScope.launch {
            _state.value = _state.value.copy(scanning = true, error = null)
            when (val result = addItemToCart(code)) {
                is Result.Success ->
                    _state.value = _state.value.copy(
                        scanning = false,
                        session = result.value,
                        scanInput = "",
                    )
                is Result.Failure -> {
                    _state.value = _state.value.copy(scanning = false)
                    handleFailure(result.message)
                }
            }
        }
    }

    fun removeLine(lineId: String) {
        viewModelScope.launch {
            _state.value = _state.value.copy(removingLineId = lineId, error = null)
            when (val result = repository.removeCartLine(lineId)) {
                is Result.Success ->
                    _state.value = _state.value.copy(
                        removingLineId = null,
                        session = result.value,
                    )
                is Result.Failure -> {
                    _state.value = _state.value.copy(removingLineId = null)
                    handleFailure(result.message)
                }
            }
        }
    }

    fun clearError() {
        _state.value = _state.value.copy(error = null)
    }

    private fun handleFailure(message: String) {
        if (message == Result.UNAUTHORIZED) {
            viewModelScope.launch { tokenStore.clear() }
            _state.value = PdvUiState(
                loading = false,
                sessionExpired = true,
                error = "Sessão expirada. Faça login novamente.",
            )
        } else {
            _state.value = _state.value.copy(loading = false, error = message)
        }
    }
}

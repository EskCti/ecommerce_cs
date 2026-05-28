package com.retailops.mobile.pdv.ui

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.retailops.mobile.pdv.domain.GetCurrentSessionUseCase
import com.retailops.mobile.pdv.domain.OpenCashSessionUseCase
import com.retailops.mobile.pdv.domain.Result
import com.retailops.mobile.profile.data.AuthTokenStore
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import javax.inject.Inject

data class OpenCashSessionUiState(
    val loading: Boolean = false,
    val checkingSession: Boolean = true,
    val hasOpenSession: Boolean = false,
    val error: String? = null,
    val sessionExpired: Boolean = false,
    val openSuccess: Boolean = false,
)

@HiltViewModel
class OpenCashSessionViewModel @Inject constructor(
    private val openCashSession: OpenCashSessionUseCase,
    private val getCurrentSession: GetCurrentSessionUseCase,
    private val tokenStore: AuthTokenStore,
) : ViewModel() {
    private val _state = MutableStateFlow(OpenCashSessionUiState())
    val state: StateFlow<OpenCashSessionUiState> = _state.asStateFlow()

    init {
        checkExistingSession()
    }

    fun checkExistingSession() {
        viewModelScope.launch {
            _state.value = _state.value.copy(checkingSession = true, error = null, sessionExpired = false)
            when (val result = getCurrentSession()) {
                is Result.Success ->
                    _state.value = _state.value.copy(
                        checkingSession = false,
                        hasOpenSession = result.value != null,
                    )
                is Result.Failure -> handleFailure(result.message, resetChecking = true)
            }
        }
    }

    fun submit(
        terminalId: String,
        managerUserId: String,
        managerPin: String,
        openingFloatText: String,
    ) {
        val openingFloat = openingFloatText.replace(',', '.').toDoubleOrNull()
        if (terminalId.isBlank() || managerUserId.isBlank() || managerPin.isBlank()) {
            _state.value = _state.value.copy(error = "Preencha terminal, gerente e PIN")
            return
        }
        if (openingFloat == null || openingFloat < 0) {
            _state.value = _state.value.copy(error = "Fundo inicial inválido")
            return
        }

        viewModelScope.launch {
            _state.value = _state.value.copy(loading = true, error = null, openSuccess = false)
            when (
                val result = openCashSession(
                    terminalId = terminalId,
                    managerUserId = managerUserId,
                    managerPin = managerPin,
                    openingFloat = openingFloat,
                )
            ) {
                is Result.Success ->
                    _state.value = _state.value.copy(
                        loading = false,
                        openSuccess = true,
                        hasOpenSession = true,
                    )
                is Result.Failure -> handleFailure(result.message)
            }
        }
    }

    fun clearOpenSuccess() {
        _state.value = _state.value.copy(openSuccess = false)
    }

    private fun handleFailure(message: String, resetChecking: Boolean = false) {
        if (message == Result.UNAUTHORIZED) {
            viewModelScope.launch { tokenStore.clear() }
            _state.value = OpenCashSessionUiState(
                checkingSession = false,
                sessionExpired = true,
                error = "Sessão expirada. Faça login novamente.",
            )
        } else {
            _state.value = _state.value.copy(
                loading = false,
                checkingSession = if (resetChecking) false else _state.value.checkingSession,
                error = message,
            )
        }
    }
}

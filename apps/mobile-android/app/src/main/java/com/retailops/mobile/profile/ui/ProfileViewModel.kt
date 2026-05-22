package com.retailops.mobile.profile.ui

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.retailops.mobile.profile.data.AuthTokenStore
import com.retailops.mobile.profile.domain.GetCurrentUserUseCase
import com.retailops.mobile.profile.domain.Result
import com.retailops.mobile.profile.domain.User
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import javax.inject.Inject

data class ProfileUiState(
    val loading: Boolean = true,
    val user: User? = null,
    val error: String? = null,
    val sessionExpired: Boolean = false,
)

@HiltViewModel
class ProfileViewModel @Inject constructor(
    private val getCurrentUser: GetCurrentUserUseCase,
    private val tokenStore: AuthTokenStore,
) : ViewModel() {
    private val _state = MutableStateFlow(ProfileUiState())
    val state: StateFlow<ProfileUiState> = _state.asStateFlow()

    init {
        load()
    }

    fun load() {
        viewModelScope.launch {
            _state.value = ProfileUiState(loading = true)
            when (val result = getCurrentUser()) {
                is Result.Success ->
                    _state.value = ProfileUiState(loading = false, user = result.value)
                is Result.Failure -> {
                    if (result.message == Result.UNAUTHORIZED) {
                        tokenStore.clear()
                        _state.value = ProfileUiState(
                            loading = false,
                            sessionExpired = true,
                            error = "Sessão expirada. Faça login novamente.",
                        )
                    } else {
                        _state.value = ProfileUiState(loading = false, error = result.message)
                    }
                }
            }
        }
    }
}

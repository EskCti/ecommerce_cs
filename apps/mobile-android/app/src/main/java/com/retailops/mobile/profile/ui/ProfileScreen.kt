package com.retailops.mobile.profile.ui

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel

@Composable
fun ProfileScreen(viewModel: ProfileViewModel = hiltViewModel()) {
    val state by viewModel.state.collectAsState()
    Column(modifier = Modifier.fillMaxSize().padding(24.dp)) {
        Text("Perfil", style = MaterialTheme.typography.headlineMedium)
        when {
            state.loading -> CircularProgressIndicator()
            state.sessionExpired -> Text(
                state.error ?: "Sessão expirada",
                color = MaterialTheme.colorScheme.error,
            )
            state.error != null -> Text("Erro: ${state.error}")
            state.user != null -> {
                val user = state.user!!
                Text(user.name, style = MaterialTheme.typography.titleLarge)
                Text("Nível: ${user.userLevel}")
                Text("Tenant: ${user.tenantId}")
                user.email?.let { Text("E-mail: $it") }
                Text("Permissões: ${user.permissionKeys.size}")
            }
        }
    }
}

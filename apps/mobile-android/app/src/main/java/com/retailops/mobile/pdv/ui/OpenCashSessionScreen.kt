package com.retailops.mobile.pdv.ui

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.Button
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun OpenCashSessionScreen(
    onSessionReady: () -> Unit,
    viewModel: OpenCashSessionViewModel = hiltViewModel(),
) {
    val state by viewModel.state.collectAsState()
    var terminalId by remember { mutableStateOf("") }
    var managerUserId by remember { mutableStateOf("") }
    var managerPin by remember { mutableStateOf("") }
    var openingFloat by remember { mutableStateOf("") }

    LaunchedEffect(state.hasOpenSession, state.openSuccess) {
        if (state.hasOpenSession && (state.openSuccess || !state.checkingSession)) {
            onSessionReady()
        }
    }

    Scaffold(
        topBar = {
            TopAppBar(title = { Text("Abrir caixa") })
        },
    ) { padding ->
        when {
            state.checkingSession -> {
                CircularProgressIndicator(
                    modifier = Modifier
                        .fillMaxSize()
                        .padding(padding),
                )
            }
            state.sessionExpired -> {
                Text(
                    text = state.error ?: "Sessão expirada",
                    color = MaterialTheme.colorScheme.error,
                    modifier = Modifier
                        .fillMaxSize()
                        .padding(padding)
                        .padding(24.dp),
                )
            }
            else -> {
                Column(
                    modifier = Modifier
                        .fillMaxSize()
                        .padding(padding)
                        .padding(24.dp)
                        .verticalScroll(rememberScrollState()),
                ) {
                    Text(
                        text = "Informe o terminal, fundo inicial e PIN do gerente.",
                        style = MaterialTheme.typography.bodyMedium,
                        modifier = Modifier.padding(bottom = 16.dp),
                    )
                    OutlinedTextField(
                        value = terminalId,
                        onValueChange = { terminalId = it },
                        label = { Text("ID do terminal") },
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(bottom = 12.dp),
                        singleLine = true,
                    )
                    OutlinedTextField(
                        value = managerUserId,
                        onValueChange = { managerUserId = it },
                        label = { Text("ID do gerente") },
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(bottom = 12.dp),
                        singleLine = true,
                    )
                    OutlinedTextField(
                        value = managerPin,
                        onValueChange = { managerPin = it },
                        label = { Text("PIN do gerente") },
                        visualTransformation = PasswordVisualTransformation(),
                        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.NumberPassword),
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(bottom = 12.dp),
                        singleLine = true,
                    )
                    OutlinedTextField(
                        value = openingFloat,
                        onValueChange = { openingFloat = it },
                        label = { Text("Fundo inicial (R$)") },
                        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Decimal),
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(bottom = 12.dp),
                        singleLine = true,
                    )
                    state.error?.let {
                        Text(
                            text = it,
                            color = MaterialTheme.colorScheme.error,
                            modifier = Modifier.padding(bottom = 12.dp),
                        )
                    }
                    Button(
                        onClick = {
                            viewModel.submit(
                                terminalId = terminalId,
                                managerUserId = managerUserId,
                                managerPin = managerPin,
                                openingFloatText = openingFloat,
                            )
                        },
                        enabled = !state.loading,
                        modifier = Modifier.fillMaxWidth(),
                    ) {
                        if (state.loading) {
                            CircularProgressIndicator(
                                modifier = Modifier.padding(end = 8.dp),
                                strokeWidth = 2.dp,
                            )
                        }
                        Text("Abrir caixa")
                    }
                }
            }
        }
    }
}

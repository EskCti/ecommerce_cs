package com.retailops.mobile.pdv.ui

import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.text.KeyboardActions
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material3.Button
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.IconButton
import androidx.compose.material3.ListItem
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.input.ImeAction
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.navigation.NavBackStackEntry

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun PdvScanScreen(
    onNavigateToOpenSession: () -> Unit,
    onNavigateToCheckout: () -> Unit,
    parentBackStackEntry: NavBackStackEntry? = null,
) {
    val viewModel: PdvViewModel = if (parentBackStackEntry != null) {
        hiltViewModel(parentBackStackEntry)
    } else {
        hiltViewModel()
    }
    val state by viewModel.state.collectAsState()

    LaunchedEffect(state.noSession) {
        if (state.noSession && !state.loading) {
            onNavigateToOpenSession()
        }
    }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("PDV") },
                actions = {
                    TextButton(onClick = { viewModel.loadSession() }) {
                        Text("Atualizar")
                    }
                },
            )
        },
    ) { padding ->
        Box(
            modifier = Modifier
                .fillMaxSize()
                .padding(padding),
        ) {
            when {
                state.loading -> {
                    CircularProgressIndicator(modifier = Modifier.align(Alignment.Center))
                }
                state.sessionExpired -> {
                    Text(
                        text = state.error ?: "Sessão expirada",
                        color = MaterialTheme.colorScheme.error,
                        modifier = Modifier
                            .align(Alignment.Center)
                            .padding(24.dp),
                    )
                }
                else -> {
                    Column(modifier = Modifier.fillMaxSize()) {
                        Column(modifier = Modifier.padding(16.dp)) {
                            Text(
                                text = "Código de barras",
                                style = MaterialTheme.typography.labelLarge,
                            )
                            Text(
                                text = "Digite o código ou use leitor USB (câmera em breve).",
                                style = MaterialTheme.typography.bodySmall,
                                color = MaterialTheme.colorScheme.onSurfaceVariant,
                                modifier = Modifier.padding(bottom = 8.dp),
                            )
                            OutlinedTextField(
                                value = state.scanInput,
                                onValueChange = { viewModel.updateScanInput(it) },
                                modifier = Modifier.fillMaxWidth(),
                                placeholder = { Text("7891234567890 ou 2*789...") },
                                singleLine = true,
                                keyboardOptions = KeyboardOptions(
                                    keyboardType = KeyboardType.Text,
                                    imeAction = ImeAction.Done,
                                ),
                                keyboardActions = KeyboardActions(
                                    onDone = { viewModel.scanBarcode() },
                                ),
                                trailingIcon = {
                                    if (state.scanning) {
                                        CircularProgressIndicator(strokeWidth = 2.dp)
                                    }
                                },
                            )
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(top = 8.dp),
                            ) {
                                Button(
                                    onClick = { viewModel.scanBarcode() },
                                    enabled = !state.scanning,
                                    modifier = Modifier.weight(1f),
                                ) {
                                    Text("Adicionar")
                                }
                            }
                            state.error?.let {
                                Text(
                                    text = it,
                                    color = MaterialTheme.colorScheme.error,
                                    modifier = Modifier.padding(top = 8.dp),
                                )
                            }
                        }

                        HorizontalDivider()

                        val session = state.session
                        if (session != null) {
                            Text(
                                text = "Subtotal: R$ ${"%.2f".format(session.cartSubtotal)}",
                                style = MaterialTheme.typography.titleMedium,
                                modifier = Modifier.padding(horizontal = 16.dp, vertical = 8.dp),
                            )
                        }

                        LazyColumn(modifier = Modifier.weight(1f)) {
                            val lines = state.session?.lines.orEmpty()
                            if (lines.isEmpty()) {
                                item {
                                    Text(
                                        text = "Carrinho vazio. Escaneie um produto.",
                                        modifier = Modifier.padding(24.dp),
                                        style = MaterialTheme.typography.bodyLarge,
                                    )
                                }
                            } else {
                                items(lines, key = { it.id }) { line ->
                                    ListItem(
                                        headlineContent = { Text(line.barcode) },
                                        supportingContent = {
                                            Column {
                                                Text("Qtd: ${line.quantity} × R$ ${"%.2f".format(line.unitPrice)}")
                                                Text(
                                                    "Total: R$ ${"%.2f".format(line.lineTotal)}",
                                                    fontFamily = FontFamily.Monospace,
                                                )
                                                if (line.requiresGrade) {
                                                    Text(
                                                        "Grade pendente",
                                                        color = MaterialTheme.colorScheme.error,
                                                        style = MaterialTheme.typography.labelMedium,
                                                    )
                                                }
                                            }
                                        },
                                        trailingContent = {
                                            IconButton(
                                                onClick = { viewModel.removeLine(line.id) },
                                                enabled = state.removingLineId != line.id,
                                            ) {
                                                if (state.removingLineId == line.id) {
                                                    CircularProgressIndicator(strokeWidth = 2.dp)
                                                } else {
                                                    Text("✕")
                                                }
                                            }
                                        },
                                    )
                                    HorizontalDivider(modifier = Modifier.fillMaxWidth())
                                }
                            }
                        }

                        Button(
                            onClick = onNavigateToCheckout,
                            enabled = (state.session?.lines?.isNotEmpty() == true),
                            modifier = Modifier
                                .fillMaxWidth()
                                .padding(16.dp),
                        ) {
                            Text("Finalizar venda")
                        }
                    }
                }
            }
        }
    }
}

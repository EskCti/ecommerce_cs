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
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.navigation.NavBackStackEntry

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun CheckoutScreen(
    onBack: () -> Unit,
    onSaleCompleted: () -> Unit,
    parentBackStackEntry: NavBackStackEntry? = null,
) {
    val viewModel: CheckoutViewModel = hiltViewModel()
    val pdvViewModel: PdvViewModel = if (parentBackStackEntry != null) {
        hiltViewModel(parentBackStackEntry)
    } else {
        hiltViewModel()
    }
    val state by viewModel.state.collectAsState()

    if (state.finalizeSuccess != null) {
        val sale = state.finalizeSuccess!!
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(24.dp),
        ) {
            Text(
                text = "Venda finalizada",
                style = MaterialTheme.typography.headlineSmall,
            )
            Text(
                text = "Total: R$ ${"%.2f".format(sale.total)}",
                style = MaterialTheme.typography.titleLarge,
                modifier = Modifier.padding(top = 8.dp),
            )
            Text(
                text = "Troco: R$ ${"%.2f".format(sale.change)}",
                modifier = Modifier.padding(top = 4.dp),
            )
            Button(
                onClick = {
                    pdvViewModel.loadSession()
                    onSaleCompleted()
                },
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(top = 24.dp),
            ) {
                Text("Nova venda")
            }
        }
        return
    }

    Scaffold(
        topBar = {
            TopAppBar(title = { Text("Checkout") })
        },
    ) { padding ->
        when {
            state.loading -> {
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
                        text = "Subtotal: R$ ${"%.2f".format(state.cartSubtotal)}",
                        style = MaterialTheme.typography.titleMedium,
                        modifier = Modifier.padding(bottom = 16.dp),
                    )
                    OutlinedTextField(
                        value = state.paymentMethodId,
                        onValueChange = { viewModel.updatePaymentMethodId(it) },
                        label = { Text("ID forma de pagamento") },
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(bottom = 12.dp),
                        singleLine = true,
                    )
                    OutlinedTextField(
                        value = state.paymentTerms,
                        onValueChange = { viewModel.updatePaymentTerms(it) },
                        label = { Text("Condição (Cash ou Credit)") },
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(bottom = 12.dp),
                        singleLine = true,
                    )
                    OutlinedTextField(
                        value = state.amountPaid,
                        onValueChange = { viewModel.updateAmountPaid(it) },
                        label = { Text("Valor pago (R$)") },
                        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Decimal),
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(bottom = 12.dp),
                        singleLine = true,
                    )
                    OutlinedTextField(
                        value = state.discountAmount,
                        onValueChange = { viewModel.updateDiscountAmount(it) },
                        label = { Text("Desconto (R$)") },
                        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Decimal),
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(bottom = 12.dp),
                        singleLine = true,
                    )
                    OutlinedTextField(
                        value = state.customerId,
                        onValueChange = { viewModel.updateCustomerId(it) },
                        label = { Text("ID cliente (fiado)") },
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
                        onClick = { viewModel.submit() },
                        enabled = !state.submitting,
                        modifier = Modifier.fillMaxWidth(),
                    ) {
                        if (state.submitting) {
                            CircularProgressIndicator(
                                modifier = Modifier.padding(end = 8.dp),
                                strokeWidth = 2.dp,
                            )
                        }
                        Text("Confirmar venda")
                    }
                    Button(
                        onClick = onBack,
                        enabled = !state.submitting,
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(top = 8.dp),
                    ) {
                        Text("Voltar")
                    }
                }
            }
        }
    }
}

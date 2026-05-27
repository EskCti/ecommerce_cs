package com.retailops.mobile.products.ui

import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.Button
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.navigation.NavBackStackEntry

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ProductDetailScreen(
    productId: String,
    onBack: () -> Unit,
    parentBackStackEntry: NavBackStackEntry? = null,
) {
    val viewModel: ProductsViewModel = if (parentBackStackEntry != null) {
        hiltViewModel(parentBackStackEntry)
    } else {
        hiltViewModel()
    }
    val state by viewModel.state.collectAsState()

    LaunchedEffect(productId) {
        viewModel.loadProduct(productId)
    }

    Scaffold(
        topBar = {
            TopAppBar(title = { Text("Detalhe do produto") })
        },
    ) { padding ->
        Box(
            modifier = Modifier
                .fillMaxSize()
                .padding(padding),
        ) {
            when {
                state.detailLoading -> {
                    CircularProgressIndicator(modifier = Modifier.align(Alignment.Center))
                }
                state.sessionExpired -> {
                    Column(
                        modifier = Modifier
                            .align(Alignment.Center)
                            .padding(24.dp),
                    ) {
                        Text(
                            state.detailError ?: "Sessão expirada",
                            color = MaterialTheme.colorScheme.error,
                        )
                    }
                }
                state.detailError != null -> {
                    Column(
                        modifier = Modifier
                            .align(Alignment.Center)
                            .padding(24.dp),
                        horizontalAlignment = Alignment.CenterHorizontally,
                    ) {
                        Text("Erro: ${state.detailError}", color = MaterialTheme.colorScheme.error)
                        Button(
                            onClick = { viewModel.loadProduct(productId) },
                            modifier = Modifier.padding(top = 12.dp),
                        ) {
                            Text("Tentar novamente")
                        }
                    }
                }
                state.detailProduct != null -> {
                    val product = state.detailProduct!!
                    Column(
                        modifier = Modifier
                            .fillMaxSize()
                            .verticalScroll(rememberScrollState())
                            .padding(24.dp),
                    ) {
                        Text(
                            text = product.barcode,
                            style = MaterialTheme.typography.headlineMedium,
                            fontFamily = FontFamily.Monospace,
                        )
                        Text(
                            text = "Código de barras",
                            style = MaterialTheme.typography.labelMedium,
                            color = MaterialTheme.colorScheme.onSurfaceVariant,
                            modifier = Modifier.padding(top = 4.dp, bottom = 16.dp),
                        )
                        HorizontalDivider(modifier = Modifier.fillMaxWidth())
                        Text(
                            text = product.name,
                            style = MaterialTheme.typography.titleLarge,
                            modifier = Modifier.padding(top = 16.dp),
                        )
                        product.description?.let {
                            Text(
                                text = it,
                                style = MaterialTheme.typography.bodyMedium,
                                modifier = Modifier.padding(top = 8.dp),
                            )
                        }
                        Text(
                            text = "Preço: R$ ${"%.2f".format(product.salePrice)}",
                            style = MaterialTheme.typography.bodyLarge,
                            modifier = Modifier.padding(top = 16.dp),
                        )
                        product.costPrice?.let {
                            Text(
                                text = "Custo: R$ ${"%.2f".format(it)}",
                                style = MaterialTheme.typography.bodyMedium,
                            )
                        }
                        Text(
                            text = "Estoque: ${product.stock}",
                            style = MaterialTheme.typography.bodyMedium,
                            modifier = Modifier.padding(top = 8.dp),
                        )
                        if (product.isLowStock) {
                            Text(
                                text = "Estoque baixo",
                                color = MaterialTheme.colorScheme.error,
                                style = MaterialTheme.typography.labelLarge,
                                modifier = Modifier.padding(top = 8.dp),
                            )
                        }
                        Text(
                            text = if (product.isActive) "Ativo" else "Inativo",
                            style = MaterialTheme.typography.labelLarge,
                            modifier = Modifier.padding(top = 8.dp),
                        )
                        Button(
                            onClick = onBack,
                            modifier = Modifier
                                .fillMaxWidth()
                                .padding(top = 24.dp),
                        ) {
                            Text("Voltar")
                        }
                    }
                }
            }
        }
    }
}

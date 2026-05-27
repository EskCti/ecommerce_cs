package com.retailops.mobile.products.ui

import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material3.Button
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.ListItem
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.material3.pulltorefresh.PullToRefreshBox
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ProductListScreen(
    onNavigateToDetail: (String) -> Unit,
    viewModel: ProductsViewModel = hiltViewModel(),
) {
    val state by viewModel.state.collectAsState()

    Scaffold(
        topBar = {
            TopAppBar(title = { Text("Produtos") })
        },
    ) { padding ->
        Box(
            modifier = Modifier
                .fillMaxSize()
                .padding(padding),
        ) {
            when {
                state.listLoading && state.products.isEmpty() -> {
                    CircularProgressIndicator(modifier = Modifier.align(Alignment.Center))
                }
                state.sessionExpired -> {
                    Column(
                        modifier = Modifier
                            .align(Alignment.Center)
                            .padding(24.dp),
                    ) {
                        Text(
                            state.listError ?: "Sessão expirada",
                            color = MaterialTheme.colorScheme.error,
                        )
                    }
                }
                state.listError != null && state.products.isEmpty() -> {
                    Column(
                        modifier = Modifier
                            .align(Alignment.Center)
                            .padding(24.dp),
                        horizontalAlignment = Alignment.CenterHorizontally,
                    ) {
                        Text("Erro: ${state.listError}", color = MaterialTheme.colorScheme.error)
                        Button(
                            onClick = { viewModel.load() },
                            modifier = Modifier.padding(top = 12.dp),
                        ) {
                            Text("Tentar novamente")
                        }
                    }
                }
                else -> {
                    PullToRefreshBox(
                        isRefreshing = state.refreshing,
                        onRefresh = { viewModel.refresh() },
                        modifier = Modifier.fillMaxSize(),
                    ) {
                        LazyColumn(modifier = Modifier.fillMaxSize()) {
                            if (state.products.isEmpty()) {
                                item {
                                    Text(
                                        "Nenhum produto cadastrado.",
                                        modifier = Modifier.padding(24.dp),
                                        style = MaterialTheme.typography.bodyLarge,
                                    )
                                }
                            } else {
                                items(state.products, key = { it.id }) { product ->
                                    ListItem(
                                        modifier = Modifier.clickable {
                                            onNavigateToDetail(product.id)
                                        },
                                        headlineContent = { Text(product.name) },
                                        supportingContent = {
                                            Column {
                                                Text("Código: ${product.barcode}")
                                                Text("Preço: R$ ${"%.2f".format(product.salePrice)}")
                                                Text("Estoque: ${product.stock}")
                                            }
                                        },
                                    )
                                    HorizontalDivider(modifier = Modifier.fillMaxWidth())
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

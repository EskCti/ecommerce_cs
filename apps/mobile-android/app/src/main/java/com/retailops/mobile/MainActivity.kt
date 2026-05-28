package com.retailops.mobile

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.NavigationBar
import androidx.compose.material3.NavigationBarItem
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.navigation.NavType
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.currentBackStackEntryAsState
import androidx.navigation.compose.rememberNavController
import androidx.navigation.navArgument
import com.retailops.mobile.customers.ui.CustomerFormScreen
import com.retailops.mobile.customers.ui.CustomerListScreen
import com.retailops.mobile.pdv.ui.CheckoutScreen
import com.retailops.mobile.pdv.ui.OpenCashSessionScreen
import com.retailops.mobile.pdv.ui.PdvScanScreen
import com.retailops.mobile.products.ui.ProductDetailScreen
import com.retailops.mobile.products.ui.ProductListScreen
import com.retailops.mobile.ui.theme.RetailOpsTheme
import dagger.hilt.android.AndroidEntryPoint

private object Routes {
    const val CUSTOMERS = "customers"
    const val CUSTOMER_FORM = "customers/new"
    const val PRODUCTS = "products"
    const val PRODUCT_DETAIL = "products/{productId}"
    const val PDV = "pdv"
    const val PDV_OPEN = "pdv/open"
    const val PDV_CHECKOUT = "pdv/checkout"

    fun productDetail(productId: String) = "products/$productId"
}

@AndroidEntryPoint
class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContent {
            RetailOpsTheme {
                Surface(modifier = Modifier.fillMaxSize()) {
                    RetailOpsNavHost()
                }
            }
        }
    }
}

@Composable
private fun RetailOpsNavHost() {
    val navController = rememberNavController()
    val navBackStackEntry by navController.currentBackStackEntryAsState()
    val currentRoute = navBackStackEntry?.destination?.route

    val bottomRoutes = setOf(Routes.CUSTOMERS, Routes.PRODUCTS, Routes.PDV)
    val showBottomBar = currentRoute in bottomRoutes ||
        currentRoute == Routes.PDV_OPEN ||
        currentRoute == Routes.PDV_CHECKOUT

    Scaffold(
        bottomBar = {
            if (showBottomBar) {
                NavigationBar {
                    NavigationBarItem(
                        selected = currentRoute == Routes.CUSTOMERS,
                        onClick = {
                            navController.navigate(Routes.CUSTOMERS) {
                                popUpTo(navController.graph.startDestinationId) {
                                    saveState = true
                                }
                                launchSingleTop = true
                                restoreState = true
                            }
                        },
                        icon = { Text("C") },
                        label = { Text("Clientes") },
                    )
                    NavigationBarItem(
                        selected = currentRoute == Routes.PRODUCTS,
                        onClick = {
                            navController.navigate(Routes.PRODUCTS) {
                                popUpTo(navController.graph.startDestinationId) {
                                    saveState = true
                                }
                                launchSingleTop = true
                                restoreState = true
                            }
                        },
                        icon = { Text("P") },
                        label = { Text("Produtos") },
                    )
                    NavigationBarItem(
                        selected = currentRoute == Routes.PDV ||
                            currentRoute == Routes.PDV_OPEN ||
                            currentRoute == Routes.PDV_CHECKOUT,
                        onClick = {
                            navController.navigate(Routes.PDV) {
                                popUpTo(navController.graph.startDestinationId) {
                                    saveState = true
                                }
                                launchSingleTop = true
                                restoreState = true
                            }
                        },
                        icon = { Text("$") },
                        label = { Text("PDV") },
                    )
                }
            }
        },
    ) { padding ->
        NavHost(
            navController = navController,
            startDestination = Routes.CUSTOMERS,
            modifier = Modifier.padding(padding),
        ) {
            composable(Routes.CUSTOMERS) {
                CustomerListScreen(
                    onNavigateToForm = { navController.navigate(Routes.CUSTOMER_FORM) },
                )
            }
            composable(Routes.CUSTOMER_FORM) {
                val parentEntry = navController.getBackStackEntry(Routes.CUSTOMERS)
                CustomerFormScreen(
                    onBack = { navController.popBackStack() },
                    parentBackStackEntry = parentEntry,
                )
            }
            composable(Routes.PRODUCTS) {
                ProductListScreen(
                    onNavigateToDetail = { productId ->
                        navController.navigate(Routes.productDetail(productId))
                    },
                )
            }
            composable(
                route = Routes.PRODUCT_DETAIL,
                arguments = listOf(navArgument("productId") { type = NavType.StringType }),
            ) { backStackEntry ->
                val productId = backStackEntry.arguments?.getString("productId").orEmpty()
                val parentEntry = navController.getBackStackEntry(Routes.PRODUCTS)
                ProductDetailScreen(
                    productId = productId,
                    onBack = { navController.popBackStack() },
                    parentBackStackEntry = parentEntry,
                )
            }
            composable(Routes.PDV) {
                val parentEntry = navController.getBackStackEntry(Routes.PDV)
                PdvScanScreen(
                    onNavigateToOpenSession = {
                        navController.navigate(Routes.PDV_OPEN)
                    },
                    onNavigateToCheckout = {
                        navController.navigate(Routes.PDV_CHECKOUT)
                    },
                    parentBackStackEntry = parentEntry,
                )
            }
            composable(Routes.PDV_OPEN) {
                OpenCashSessionScreen(
                    onSessionReady = {
                        navController.navigate(Routes.PDV) {
                            popUpTo(Routes.PDV) { inclusive = true }
                            launchSingleTop = true
                        }
                    },
                )
            }
            composable(Routes.PDV_CHECKOUT) {
                val parentEntry = navController.getBackStackEntry(Routes.PDV)
                CheckoutScreen(
                    onBack = { navController.popBackStack() },
                    onSaleCompleted = {
                        navController.popBackStack(Routes.PDV, inclusive = false)
                    },
                    parentBackStackEntry = parentEntry,
                )
            }
        }
    }
}

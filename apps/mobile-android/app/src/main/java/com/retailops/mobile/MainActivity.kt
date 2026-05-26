package com.retailops.mobile

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController
import com.retailops.mobile.customers.ui.CustomerFormScreen
import com.retailops.mobile.customers.ui.CustomerListScreen
import com.retailops.mobile.ui.theme.RetailOpsTheme
import dagger.hilt.android.AndroidEntryPoint

private object Routes {
    const val CUSTOMERS = "customers"
    const val CUSTOMER_FORM = "customers/new"
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

    NavHost(
        navController = navController,
        startDestination = Routes.CUSTOMERS,
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
    }
}

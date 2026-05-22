package com.retailops.mobile

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.retailops.mobile.ui.theme.RetailOpsTheme
import dagger.hilt.android.AndroidEntryPoint

@AndroidEntryPoint
class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContent {
            RetailOpsTheme {
                Surface(modifier = Modifier.fillMaxSize()) {
                    Text(
                        text = "RetailOps Mobile — API ${BuildConfig.API_BASE_URL}",
                        modifier = Modifier.padding(24.dp),
                    )
                }
            }
        }
    }
}

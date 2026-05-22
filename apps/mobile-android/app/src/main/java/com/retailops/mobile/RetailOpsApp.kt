package com.retailops.mobile

import android.app.Application
import com.retailops.mobile.profile.data.AuthTokenStore
import dagger.hilt.android.HiltAndroidApp
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.SupervisorJob
import kotlinx.coroutines.launch
import javax.inject.Inject

@HiltAndroidApp
class RetailOpsApp : Application() {
    @Inject
    lateinit var authTokenStore: AuthTokenStore

    private val appScope = CoroutineScope(SupervisorJob() + Dispatchers.IO)

    override fun onCreate() {
        super.onCreate()
        appScope.launch {
            authTokenStore.load()
        }
    }
}

package com.retailops.mobile.profile.data

import android.content.Context
import androidx.datastore.core.DataStore
import androidx.datastore.preferences.core.Preferences
import androidx.datastore.preferences.core.edit
import androidx.datastore.preferences.core.stringPreferencesKey
import androidx.datastore.preferences.preferencesDataStore
import dagger.hilt.android.qualifiers.ApplicationContext
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.runBlocking
import javax.inject.Inject
import javax.inject.Singleton

private val Context.authDataStore: DataStore<Preferences> by preferencesDataStore(name = "auth")

@Singleton
class AuthTokenStore @Inject constructor(
    @ApplicationContext private val context: Context,
) {
    private val tokenKey = stringPreferencesKey("jwt")

    @Volatile
    private var cached: String? = runBlocking { readFromStore() }

    fun bearerToken(): String? = cached

    suspend fun load() {
        cached = readFromStore()
    }

    suspend fun setToken(token: String?) {
        cached = token
        context.authDataStore.edit { prefs ->
            if (token == null) prefs.remove(tokenKey)
            else prefs[tokenKey] = token
        }
    }

    suspend fun clear() = setToken(null)

    private suspend fun readFromStore(): String? {
        val prefs = context.authDataStore.data.first()
        return prefs[tokenKey]
    }
}

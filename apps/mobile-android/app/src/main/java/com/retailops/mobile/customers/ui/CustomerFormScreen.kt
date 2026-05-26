package com.retailops.mobile.customers.ui

import androidx.compose.foundation.layout.Arrangement
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
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.navigation.NavBackStackEntry

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun CustomerFormScreen(
    onBack: () -> Unit,
    parentBackStackEntry: NavBackStackEntry? = null,
) {
    val viewModel: CustomersViewModel = if (parentBackStackEntry != null) {
        hiltViewModel(parentBackStackEntry)
    } else {
        hiltViewModel()
    }
    val state by viewModel.state.collectAsState()

    var name by remember { mutableStateOf("") }
    var cpf by remember { mutableStateOf("") }
    var phone by remember { mutableStateOf("") }
    var email by remember { mutableStateOf("") }
    var address by remember { mutableStateOf("") }
    var nameError by remember { mutableStateOf<String?>(null) }
    var cpfError by remember { mutableStateOf<String?>(null) }

    LaunchedEffect(state.formSuccess) {
        if (state.formSuccess) {
            viewModel.clearFormSuccess()
            onBack()
        }
    }

    fun validate(): Boolean {
        nameError = when {
            name.trim().length < 2 -> "Mínimo 2 caracteres"
            else -> null
        }
        cpfError = when {
            cpf.replace(Regex("\\D"), "").length != 11 -> "CPF deve ter 11 dígitos"
            else -> null
        }
        return nameError == null && cpfError == null
    }

    Scaffold(
        topBar = {
            TopAppBar(title = { Text("Cadastro de Cliente") })
        },
    ) { padding ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(padding)
                .padding(16.dp)
                .verticalScroll(rememberScrollState()),
            verticalArrangement = Arrangement.spacedBy(12.dp),
        ) {
            OutlinedTextField(
                value = name,
                onValueChange = {
                    name = it
                    nameError = null
                },
                label = { Text("Nome *") },
                isError = nameError != null,
                supportingText = nameError?.let { { Text(it) } },
                modifier = Modifier.fillMaxWidth(),
            )
            OutlinedTextField(
                value = cpf,
                onValueChange = {
                    if (it.length <= 14) {
                        cpf = it
                        cpfError = null
                    }
                },
                label = { Text("CPF *") },
                keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                isError = cpfError != null,
                supportingText = cpfError?.let { { Text(it) } },
                modifier = Modifier.fillMaxWidth(),
            )
            OutlinedTextField(
                value = phone,
                onValueChange = { phone = it },
                label = { Text("Telefone") },
                keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Phone),
                modifier = Modifier.fillMaxWidth(),
            )
            OutlinedTextField(
                value = email,
                onValueChange = { email = it },
                label = { Text("E-mail") },
                keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Email),
                modifier = Modifier.fillMaxWidth(),
            )
            OutlinedTextField(
                value = address,
                onValueChange = { address = it },
                label = { Text("Endereço") },
                modifier = Modifier.fillMaxWidth(),
            )

            state.formError?.let { error ->
                Text(error, color = MaterialTheme.colorScheme.error)
            }

            Button(
                onClick = {
                    if (validate()) {
                        viewModel.submitCustomer(name, cpf, phone, email, address)
                    }
                },
                enabled = !state.formSubmitting,
                modifier = Modifier.fillMaxWidth(),
            ) {
                if (state.formSubmitting) {
                    CircularProgressIndicator(modifier = Modifier.padding(end = 8.dp))
                }
                Text("Salvar")
            }
        }
    }
}

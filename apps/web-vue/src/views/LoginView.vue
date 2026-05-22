<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import InputText from 'primevue/inputtext'
import Password from 'primevue/password'
import Button from 'primevue/button'
import Message from 'primevue/message'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const auth = useAuthStore()

const login = ref('')
const password = ref('')
const error = ref('')
const loading = ref(false)

async function onSubmit() {
  error.value = ''
  loading.value = true
  try {
    const session = await auth.login(login.value, password.value)
    const target = session.user.isSas ? '/platform' : '/tenant'
    await router.push(target)
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Falha no login'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="flex min-h-screen items-center justify-center bg-slate-100 p-6">
    <form class="w-full max-w-md space-y-4 rounded-xl bg-white p-8 shadow" @submit.prevent="onSubmit">
      <h1 class="text-2xl font-semibold text-slate-900">RetailOps</h1>
      <p class="text-sm text-slate-600">Entre com e-mail ou CPF</p>
      <Message v-if="error" severity="error" :closable="false">{{ error }}</Message>
      <div class="flex flex-col gap-1">
        <label class="text-sm font-medium">Login</label>
        <InputText v-model="login" autocomplete="username" class="w-full" />
      </div>
      <div class="flex flex-col gap-1">
        <label class="text-sm font-medium">Senha</label>
        <Password v-model="password" :feedback="false" toggle-mask input-class="w-full" class="w-full" />
      </div>
      <Button type="submit" label="Entrar" class="w-full" :loading="loading" />
    </form>
  </div>
</template>

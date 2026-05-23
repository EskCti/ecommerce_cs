<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import InputText from 'primevue/inputtext'
import Password from 'primevue/password'
import Button from 'primevue/button'
import Message from 'primevue/message'
import AuthPageLayout from '@/components/AuthPageLayout.vue'
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
  <AuthPageLayout>
    <form class="space-y-4" @submit.prevent="onSubmit">
      <div>
        <h1 class="text-2xl font-semibold text-foreground">RetailOps</h1>
        <p class="text-sm text-muted-foreground">Entre com e-mail ou CPF</p>
      </div>
      <Message v-if="error" severity="error" :closable="false">{{ error }}</Message>
      <div class="flex flex-col gap-1">
        <label class="text-sm font-medium text-foreground">Login</label>
        <InputText v-model="login" autocomplete="username" class="w-full" />
      </div>
      <div class="flex flex-col gap-1">
        <label class="text-sm font-medium text-foreground">Senha</label>
        <Password v-model="password" :feedback="false" toggle-mask input-class="w-full" class="w-full" />
      </div>
      <Button type="submit" label="Entrar" class="w-full" :loading="loading" />
      <RouterLink to="/register-trial" class="block text-center text-sm text-primary hover:text-primary/80">
        Criar conta trial
      </RouterLink>
    </form>
  </AuthPageLayout>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import InputText from 'primevue/inputtext'
import Password from 'primevue/password'
import Button from 'primevue/button'
import Message from 'primevue/message'
import AuthPageLayout from '@/components/AuthPageLayout.vue'
import { createPlatformModule } from '@/modules/platform/composition'

const router = useRouter()
const platform = createPlatformModule(() => null)

const companyName = ref('')
const adminName = ref('')
const adminEmail = ref('')
const adminPassword = ref('')
const phone = ref('')
const error = ref('')
const success = ref('')
const loading = ref(false)

async function onSubmit() {
  error.value = ''
  success.value = ''
  loading.value = true
  try {
    const result = await platform.registerTrialUseCase.execute({
      companyName: companyName.value,
      adminName: adminName.value,
      adminEmail: adminEmail.value,
      adminPassword: adminPassword.value,
      phone: phone.value || undefined,
    })
    if (!result.ok) {
      error.value = result.error
      return
    }
    success.value = `Trial criado! Tenant #${result.data.tenantId}. Faça login com seu e-mail.`
    setTimeout(() => router.push('/login'), 1500)
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <AuthPageLayout wide>
    <form class="space-y-4" @submit.prevent="onSubmit">
      <div>
        <h1 class="text-2xl font-semibold text-foreground">Cadastro trial</h1>
        <p class="text-sm text-muted-foreground">Crie sua empresa e usuário administrador.</p>
      </div>
      <Message v-if="error" severity="error" :closable="false">{{ error }}</Message>
      <Message v-if="success" severity="success" :closable="false">{{ success }}</Message>
      <div class="grid gap-3 md:grid-cols-2">
        <div class="flex flex-col gap-1 md:col-span-2">
          <label class="text-sm font-medium text-foreground">Empresa</label>
          <InputText v-model="companyName" class="w-full" />
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-sm font-medium text-foreground">Seu nome</label>
          <InputText v-model="adminName" class="w-full" />
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-sm font-medium text-foreground">Telefone</label>
          <InputText v-model="phone" class="w-full" />
        </div>
        <div class="flex flex-col gap-1 md:col-span-2">
          <label class="text-sm font-medium text-foreground">E-mail admin</label>
          <InputText v-model="adminEmail" type="email" class="w-full" />
        </div>
        <div class="flex flex-col gap-1 md:col-span-2">
          <label class="text-sm font-medium text-foreground">Senha</label>
          <Password v-model="adminPassword" :feedback="false" toggle-mask input-class="w-full" class="w-full" />
        </div>
      </div>
      <Button type="submit" label="Criar trial" class="w-full" :loading="loading" />
      <RouterLink to="/login" class="block text-center text-sm text-primary hover:text-primary/80">
        Já tenho conta
      </RouterLink>
    </form>
  </AuthPageLayout>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import InputText from 'primevue/inputtext'
import Button from 'primevue/button'
import Message from 'primevue/message'
import { useAuthStore } from '@/stores/auth'
import { createPlatformModule } from '@/modules/platform/composition'
import type { CompanyEntity } from '@/modules/platform/domain/company.entity'

const auth = useAuthStore()
const platform = createPlatformModule(() => auth.token)

const items = ref<CompanyEntity[]>([])
const total = ref(0)
const nameFilter = ref('')
const error = ref('')
const loading = ref(false)

async function load() {
  error.value = ''
  loading.value = true
  try {
    const result = await platform.listCompaniesUseCase.execute({
      name: nameFilter.value || undefined,
      page: 1,
      pageSize: 50,
    })
    if (!result.ok) {
      error.value = result.error
      return
    }
    items.value = result.data.items
    total.value = result.data.total
  } finally {
    loading.value = false
  }
}

onMounted(load)
</script>

<template>
  <div class="space-y-4 p-6">
    <h1 class="text-2xl font-semibold">Empresas SAS</h1>
    <Message v-if="error" severity="error" :closable="false">{{ error }}</Message>
    <div class="flex gap-2">
      <InputText v-model="nameFilter" placeholder="Filtrar por nome" class="w-64" />
      <Button label="Buscar" @click="load" :loading="loading" />
    </div>
    <DataTable :value="items" :loading="loading" striped-rows>
      <Column field="id" header="ID" />
      <Column field="name" header="Nome" />
      <Column field="email" header="E-mail" />
      <Column header="Trial">
        <template #body="{ data }">{{ data.trial ? 'Sim' : 'Não' }}</template>
      </Column>
      <Column header="Ativo">
        <template #body="{ data }">{{ data.active ? 'Sim' : 'Não' }}</template>
      </Column>
      <Column field="nextBillingDate" header="Vencimento" />
    </DataTable>
    <p class="text-sm text-muted-foreground">Total: {{ total }}</p>
  </div>
</template>

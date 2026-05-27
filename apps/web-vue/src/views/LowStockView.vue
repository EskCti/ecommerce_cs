<script setup lang="ts">
import { onMounted, ref } from 'vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Message from 'primevue/message'
import { useAuthStore } from '@/stores/auth'
import { createCatalogModule } from '@/modules/catalog/composition'
import type { LowStockProduct } from '@/modules/catalog/application/stock.repository'

const auth = useAuthStore()
const module = createCatalogModule(() => auth.token)

const items = ref<LowStockProduct[]>([])
const error = ref('')
const loading = ref(false)

async function load() {
  error.value = ''
  loading.value = true
  try {
    const result = await module.listLowStockUseCase.execute()
    if (!result.ok) {
      error.value = result.error
      return
    }
    items.value = result.data
  } finally {
    loading.value = false
  }
}

onMounted(load)
</script>

<template>
  <div class="space-y-4 p-6">
    <div class="flex items-center justify-between">
      <h1 class="text-xl font-semibold">Estoque baixo</h1>
      <span class="text-sm text-gray-500">{{ items.length }} produto(s) abaixo do nível mínimo</span>
    </div>
    <Message v-if="error" severity="error" :closable="false">{{ error }}</Message>
    <DataTable :value="items" data-key="id" :loading="loading">
      <Column field="barcode" header="Código" />
      <Column field="name" header="Produto" />
      <Column field="stock" header="Estoque atual" />
      <Column field="stockAlertLevel" header="Nível mínimo" />
      <Column header="Déficit">
        <template #body="{ data }">
          {{ (data as LowStockProduct).stockAlertLevel - (data as LowStockProduct).stock }}
        </template>
      </Column>
    </DataTable>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import Message from 'primevue/message'
import { useAuthStore } from '@/stores/auth'
import { createSalesModule } from '@/modules/sales/composition'
import type { SaleListItemEntity } from '@/modules/sales/domain/sale.entity'

const auth = useAuthStore()
const sales = createSalesModule(() => auth.token)

const items = ref<SaleListItemEntity[]>([])
const total = ref(0)
const page = ref(1)
const pageSize = ref(20)
const error = ref('')
const loading = ref(false)
const cancellingId = ref<string | null>(null)

function formatMoney(value: number): string {
  return value.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
}

function formatDate(value: string): string {
  return new Date(value).toLocaleString('pt-BR')
}

function paymentTermsLabel(terms: string): string {
  return terms === 'Credit' ? 'Fiado' : 'À vista'
}

async function load() {
  error.value = ''
  loading.value = true
  try {
    const result = await sales.listSalesUseCase.execute({
      page: page.value,
      pageSize: pageSize.value,
    })
    if (!result.ok) {
      error.value = result.error
      return
    }
    items.value = [...result.data.items]
    total.value = result.data.totalCount
  } finally {
    loading.value = false
  }
}

function onPage(event: { page: number; rows: number }) {
  page.value = event.page + 1
  pageSize.value = event.rows
  load()
}

async function cancelSale(row: SaleListItemEntity) {
  if (row.isCancelled) return
  if (!confirm('Cancelar esta venda?')) return
  error.value = ''
  cancellingId.value = row.id
  try {
    const result = await sales.cancelSaleUseCase.execute(row.id)
    if (!result.ok) {
      error.value = result.error
      return
    }
    await load()
  } finally {
    cancellingId.value = null
  }
}

onMounted(load)
</script>

<template>
  <div class="space-y-4 p-6">
    <div class="flex items-center justify-between">
      <h1 class="text-xl font-semibold text-foreground">Vendas</h1>
      <Button label="Atualizar" icon="pi pi-refresh" :loading="loading" @click="load" />
    </div>
    <Message v-if="error" severity="error" :closable="false">{{ error }}</Message>
    <DataTable
      :value="items"
      data-key="id"
      :loading="loading"
      lazy
      paginator
      :rows="pageSize"
      :total-records="total"
      :first="(page - 1) * pageSize"
      @page="onPage"
    >
      <Column header="Data">
        <template #body="{ data }">
          {{ formatDate((data as SaleListItemEntity).completedAt) }}
        </template>
      </Column>
      <Column header="Total">
        <template #body="{ data }">
          {{ formatMoney((data as SaleListItemEntity).total) }}
        </template>
      </Column>
      <Column header="Pagamento">
        <template #body="{ data }">
          {{ paymentTermsLabel((data as SaleListItemEntity).paymentTerms) }}
        </template>
      </Column>
      <Column header="Status">
        <template #body="{ data }">
          {{ (data as SaleListItemEntity).isCancelled ? 'Cancelada' : 'Concluída' }}
        </template>
      </Column>
      <Column header="Ações">
        <template #body="{ data }">
          <Button
            v-if="!(data as SaleListItemEntity).isCancelled"
            icon="pi pi-times"
            text
            severity="danger"
            :loading="cancellingId === (data as SaleListItemEntity).id"
            @click="cancelSale(data as SaleListItemEntity)"
          />
        </template>
      </Column>
    </DataTable>
  </div>
</template>

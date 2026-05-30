<script setup lang="ts">
import { onMounted, ref } from 'vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Message from 'primevue/message'
import { useAuthStore } from '@/stores/auth'
import { createFinanceModule } from '@/modules/finance/composition'
import type { PayableEntity } from '@/modules/finance/domain/finance.entity'
import { formatDate, formatMoney, formatStatus } from '@/modules/finance/ui/finance-format'

const auth = useAuthStore()
const module = createFinanceModule(() => auth.token)

const items = ref<PayableEntity[]>([])
const total = ref(0)
const page = ref(1)
const pageSize = ref(20)
const error = ref('')
const loading = ref(false)

async function load() {
  loading.value = true
  error.value = ''
  try {
    const result = await module.listPurchasesUseCase.execute({ page: page.value, pageSize: pageSize.value })
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

function onPage(event: { page: number; rows: number }) {
  page.value = event.page + 1
  pageSize.value = event.rows
  load()
}

onMounted(load)
</script>

<template>
  <div class="space-y-4">
    <div>
      <h1 class="text-xl font-semibold">Compras</h1>
      <p class="mt-1 text-sm text-muted-foreground">
        Contas a pagar geradas automaticamente ao registrar compras de estoque.
      </p>
    </div>

    <Message v-if="error" severity="error" :closable="false" class="whitespace-pre-wrap">{{ error }}</Message>

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
      <Column field="description" header="Descrição" />
      <Column field="amount" header="Valor">
        <template #body="{ data }">{{ formatMoney((data as PayableEntity).amount) }}</template>
      </Column>
      <Column field="dueDate" header="Vencimento">
        <template #body="{ data }">{{ formatDate((data as PayableEntity).dueDate) }}</template>
      </Column>
      <Column field="status" header="Status">
        <template #body="{ data }">{{ formatStatus((data as PayableEntity).status) }}</template>
      </Column>
      <template #empty>
        <div class="py-10 text-center text-sm text-muted-foreground">
          Nenhuma compra registrada. Compras aparecem aqui após movimentações de estoque do tipo compra.
        </div>
      </template>
    </DataTable>
  </div>
</template>

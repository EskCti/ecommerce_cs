<script setup lang="ts">
import { onMounted, ref } from 'vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import Message from 'primevue/message'
import { useAuthStore } from '@/stores/auth'
import { createFinanceModule } from '@/modules/finance/composition'
import type { CashFlowData, CommissionEntity } from '@/modules/finance/domain/finance.entity'

const auth = useAuthStore()
const module = createFinanceModule(() => auth.token)

const commissions = ref<CommissionEntity[]>([])
const selected = ref<CommissionEntity[]>([])
const cashFlow = ref<CashFlowData | null>(null)
const error = ref('')
const from = ref(new Date(new Date().getFullYear(), new Date().getMonth(), 1).toISOString().slice(0, 10))
const to = ref(new Date().toISOString().slice(0, 10))

async function loadCommissions() {
  const result = await module.listCommissionsUseCase.execute({ isPaid: false, page: 1, pageSize: 100 })
  if (!result.ok) {
    error.value = result.error
    return
  }
  commissions.value = result.data.items
}

async function loadCashFlow() {
  const result = await module.getCashFlowUseCase.execute(
    new Date(from.value).toISOString(),
    new Date(to.value).toISOString(),
  )
  if (!result.ok) {
    error.value = result.error
    return
  }
  cashFlow.value = result.data
}

async function paySelected() {
  if (selected.value.length === 0) return
  const result = await module.payCommissionsBatchUseCase.execute(
    selected.value.map((c) => c.id),
    new Date().toISOString(),
  )
  if (!result.ok) {
    error.value = result.error
    return
  }
  selected.value = []
  await loadCommissions()
  await loadCashFlow()
}

onMounted(async () => {
  await loadCommissions()
  await loadCashFlow()
})
</script>

<template>
  <div class="p-6 space-y-8">
    <section>
      <h1 class="mb-4 text-2xl font-semibold">Comissões</h1>
      <Message v-if="error" severity="error" class="mb-4">{{ error }}</Message>
      <Button label="Pagar selecionadas" class="mb-4" :disabled="selected.length === 0" @click="paySelected" />
      <DataTable v-model:selection="selected" :value="commissions" data-key="id">
        <Column selection-mode="multiple" header-style="width: 3rem" />
        <Column field="sellerLegacyId" header="Vendedor" />
        <Column field="amount" header="Valor">
          <template #body="{ data }">R$ {{ data.amount.toFixed(2) }}</template>
        </Column>
        <Column field="isPaid" header="Pago">
          <template #body="{ data }">{{ data.isPaid ? 'Sim' : 'Não' }}</template>
        </Column>
      </DataTable>
    </section>

    <section>
      <h2 class="mb-4 text-xl font-semibold">Fluxo de caixa</h2>
      <div class="mb-4 flex gap-2">
        <input v-model="from" type="date" class="rounded border px-2 py-1" />
        <input v-model="to" type="date" class="rounded border px-2 py-1" />
        <Button label="Atualizar" @click="loadCashFlow" />
      </div>
      <div v-if="cashFlow" class="grid grid-cols-3 gap-4">
        <div class="rounded border p-4">
          <p class="text-sm text-gray-500">Entradas</p>
          <p class="text-xl font-semibold text-green-700">R$ {{ cashFlow.totalInflow.toFixed(2) }}</p>
        </div>
        <div class="rounded border p-4">
          <p class="text-sm text-gray-500">Saídas</p>
          <p class="text-xl font-semibold text-red-700">R$ {{ cashFlow.totalOutflow.toFixed(2) }}</p>
        </div>
        <div class="rounded border p-4">
          <p class="text-sm text-gray-500">Saldo</p>
          <p class="text-xl font-semibold">R$ {{ cashFlow.balance.toFixed(2) }}</p>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'
import InputText from 'primevue/inputtext'
import InputNumber from 'primevue/inputnumber'
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
const dialogVisible = ref(false)
const form = ref({ description: '', amount: 0, dueDate: new Date().toISOString().slice(0, 10), recurrenceDays: 0 })

async function load() {
  loading.value = true
  error.value = ''
  try {
    const result = await module.listPayablesUseCase.execute({ page: page.value, pageSize: pageSize.value })
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

function openCreate() {
  form.value = { description: '', amount: 0, dueDate: new Date().toISOString().slice(0, 10), recurrenceDays: 0 }
  dialogVisible.value = true
}

async function save() {
  error.value = ''
  if (!form.value.description.trim()) {
    error.value = 'Descrição é obrigatória'
    return
  }
  if (form.value.amount <= 0) {
    error.value = 'Valor deve ser maior que zero'
    return
  }
  const result = await module.createPayableUseCase.execute({
    description: form.value.description.trim(),
    amount: form.value.amount,
    dueDate: new Date(form.value.dueDate).toISOString(),
    recurrenceDays: form.value.recurrenceDays || undefined,
  })
  if (!result.ok) {
    error.value = result.error
    return
  }
  dialogVisible.value = false
  page.value = 1
  await load()
}

async function settle(row: PayableEntity) {
  error.value = ''
  const result = await module.settlePayableUseCase.execute(row.id, new Date().toISOString())
  if (!result.ok) {
    error.value = result.error
    return
  }
  await load()
}

onMounted(load)
</script>

<template>
  <div class="space-y-4">
    <div class="flex items-center justify-between">
      <h1 class="text-xl font-semibold">Despesas</h1>
      <Button label="Nova despesa" icon="pi pi-plus" @click="openCreate" />
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
      <Column header="Ações">
        <template #body="{ data }">
          <Button
            v-if="(data as PayableEntity).status === 'Open'"
            label="Baixar"
            size="small"
            @click="settle(data as PayableEntity)"
          />
        </template>
      </Column>
      <template #empty>
        <div class="py-10 text-center text-sm text-muted-foreground">
          Nenhuma despesa cadastrada. Clique em <strong>Nova despesa</strong> para adicionar.
        </div>
      </template>
    </DataTable>

    <Dialog v-model:visible="dialogVisible" header="Nova despesa" modal class="w-full max-w-lg">
      <div class="space-y-3">
        <div>
          <label class="mb-1 block text-sm">Descrição</label>
          <InputText v-model="form.description" class="w-full" placeholder="Ex.: Aluguel, energia..." />
        </div>
        <div>
          <label class="mb-1 block text-sm">Valor</label>
          <InputNumber v-model="form.amount" mode="currency" currency="BRL" locale="pt-BR" class="w-full" />
        </div>
        <div>
          <label class="mb-1 block text-sm">Vencimento</label>
          <InputText v-model="form.dueDate" type="date" class="w-full" />
        </div>
        <div>
          <label class="mb-1 block text-sm">Recorrência (dias)</label>
          <InputNumber v-model="form.recurrenceDays" class="w-full" placeholder="Opcional" />
        </div>
      </div>
      <template #footer>
        <Button label="Cancelar" text @click="dialogVisible = false" />
        <Button label="Salvar" @click="save" />
      </template>
    </Dialog>
  </div>
</template>

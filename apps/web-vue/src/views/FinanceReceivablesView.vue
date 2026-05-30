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
import type { ReceivableEntity } from '@/modules/finance/domain/finance.entity'
import { formatDate, formatMoney, formatStatus } from '@/modules/finance/ui/finance-format'

const auth = useAuthStore()
const module = createFinanceModule(() => auth.token)

const items = ref<ReceivableEntity[]>([])
const total = ref(0)
const page = ref(1)
const pageSize = ref(20)
const error = ref('')
const loading = ref(false)
const dialogVisible = ref(false)
const attachmentDialog = ref(false)
const selectedId = ref('')
const form = ref({ description: '', amount: 0, dueDate: new Date().toISOString().slice(0, 10) })
const attachmentForm = ref({ name: '', path: '' })

async function load() {
  error.value = ''
  loading.value = true
  try {
    const result = await module.listReceivablesUseCase.execute({ page: page.value, pageSize: pageSize.value })
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
  form.value = { description: '', amount: 0, dueDate: new Date().toISOString().slice(0, 10) }
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
  const result = await module.createReceivableUseCase.execute({
    description: form.value.description.trim(),
    amount: form.value.amount,
    dueDate: new Date(form.value.dueDate).toISOString(),
  })
  if (!result.ok) {
    error.value = result.error
    return
  }
  dialogVisible.value = false
  page.value = 1
  await load()
}

async function settle(row: ReceivableEntity) {
  error.value = ''
  const result = await module.settleReceivableUseCase.execute(row.id, new Date().toISOString())
  if (!result.ok) {
    error.value = result.error
    return
  }
  await load()
}

function openAttachment(row: ReceivableEntity) {
  selectedId.value = row.id
  attachmentForm.value = { name: '', path: '' }
  attachmentDialog.value = true
}

async function saveAttachment() {
  error.value = ''
  const result = await module.repository.addReceivableAttachment(
    selectedId.value,
    attachmentForm.value.name,
    attachmentForm.value.path,
  )
  if (!result.ok) {
    error.value = result.error
    return
  }
  attachmentDialog.value = false
  await load()
}

onMounted(load)
</script>

<template>
  <div class="space-y-4">
    <div class="flex items-center justify-between">
      <h1 class="text-xl font-semibold">Contas a receber</h1>
      <Button label="Nova conta" icon="pi pi-plus" @click="openCreate" />
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
        <template #body="{ data }">{{ formatMoney((data as ReceivableEntity).amount) }}</template>
      </Column>
      <Column field="dueDate" header="Vencimento">
        <template #body="{ data }">{{ formatDate((data as ReceivableEntity).dueDate) }}</template>
      </Column>
      <Column field="status" header="Status">
        <template #body="{ data }">{{ formatStatus((data as ReceivableEntity).status) }}</template>
      </Column>
      <Column header="Ações">
        <template #body="{ data }">
          <div class="flex gap-2">
            <Button
              v-if="(data as ReceivableEntity).status === 'Open'"
              label="Baixar"
              size="small"
              @click="settle(data as ReceivableEntity)"
            />
            <Button label="Anexo" size="small" severity="secondary" @click="openAttachment(data as ReceivableEntity)" />
          </div>
        </template>
      </Column>
      <template #empty>
        <div class="py-10 text-center text-sm text-muted-foreground">
          Nenhuma conta a receber cadastrada. Clique em <strong>Nova conta</strong> para adicionar.
        </div>
      </template>
    </DataTable>

    <Dialog v-model:visible="dialogVisible" header="Nova conta a receber" modal class="w-full max-w-lg">
      <div class="space-y-3">
        <div>
          <label class="mb-1 block text-sm">Descrição</label>
          <InputText v-model="form.description" class="w-full" placeholder="Ex.: Mensalidade, serviço..." />
        </div>
        <div>
          <label class="mb-1 block text-sm">Valor</label>
          <InputNumber v-model="form.amount" mode="currency" currency="BRL" locale="pt-BR" class="w-full" />
        </div>
        <div>
          <label class="mb-1 block text-sm">Vencimento</label>
          <InputText v-model="form.dueDate" type="date" class="w-full" />
        </div>
      </div>
      <template #footer>
        <Button label="Cancelar" text @click="dialogVisible = false" />
        <Button label="Salvar" @click="save" />
      </template>
    </Dialog>

    <Dialog v-model:visible="attachmentDialog" header="Anexo" modal class="w-full max-w-lg">
      <div class="space-y-3">
        <InputText v-model="attachmentForm.name" class="w-full" placeholder="Nome" />
        <InputText v-model="attachmentForm.path" class="w-full" placeholder="Caminho / URL" />
      </div>
      <template #footer>
        <Button label="Cancelar" text @click="attachmentDialog = false" />
        <Button label="Salvar anexo" @click="saveAttachment" />
      </template>
    </Dialog>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'
import InputText from 'primevue/inputtext'
import Dropdown from 'primevue/dropdown'
import Message from 'primevue/message'
import { useAuthStore } from '@/stores/auth'
import { createStoreSettingsModule } from '@/modules/store-settings/composition'
import { createUsersModule } from '@/modules/users/composition'
import type { CashRegisterTerminalEntity } from '@/modules/store-settings/domain/cash-register-terminal.entity'
import type { TenantUserEntity } from '@/modules/users/domain/tenant-user.entity'

const auth = useAuthStore()
const module = createStoreSettingsModule(() => auth.token)
const usersModule = createUsersModule(() => auth.token)

const items = ref<CashRegisterTerminalEntity[]>([])
const operators = ref<TenantUserEntity[]>([])
const error = ref('')
const dialogVisible = ref(false)
const editing = ref<CashRegisterTerminalEntity | null>(null)
const form = ref<{ name: string; status: string; assignedOperatorId: string | null }>({
  name: '',
  status: 'Closed',
  assignedOperatorId: null,
})

const statusOptions = [
  { label: 'Fechado', value: 'Closed' },
  { label: 'Aberto', value: 'Open' },
]

async function load() {
  error.value = ''
  const [listResult, usersResult] = await Promise.all([
    module.listCashRegistersUseCase.execute(),
    usersModule.listUsersUseCase.execute(),
  ])
  if (!listResult.ok) {
    error.value = listResult.error
    return
  }
  if (usersResult.ok) operators.value = usersResult.data
  items.value = listResult.data
}

function openCreate() {
  editing.value = null
  form.value = { name: '', status: 'Closed', assignedOperatorId: null }
  dialogVisible.value = true
}

function openEdit(row: CashRegisterTerminalEntity) {
  editing.value = row
  form.value = {
    name: row.name,
    status: row.status,
    assignedOperatorId: row.assignedOperatorId ?? null,
  }
  dialogVisible.value = true
}

async function save() {
  error.value = ''
  const payload = {
    name: form.value.name,
    status: form.value.status,
    assignedOperatorId: form.value.assignedOperatorId,
  }
  const result = editing.value
    ? await module.updateCashRegisterUseCase.execute(editing.value.id, payload)
    : await module.createCashRegisterUseCase.execute(payload)
  if (!result.ok) {
    error.value = result.error
    return
  }
  dialogVisible.value = false
  await load()
}

async function remove(row: CashRegisterTerminalEntity) {
  error.value = ''
  const result = await module.deleteCashRegisterUseCase.execute(row.id)
  if (!result.ok) {
    error.value = result.error
    return
  }
  await load()
}

onMounted(load)
</script>

<template>
  <div class="space-y-4 p-6">
    <div class="flex items-center justify-between">
      <h1 class="text-xl font-semibold">Caixas físicos</h1>
      <Button label="Novo caixa" icon="pi pi-plus" @click="openCreate" />
    </div>
    <Message v-if="error" severity="error" :closable="false">{{ error }}</Message>
    <DataTable :value="items" data-key="id">
      <Column field="name" header="Nome" />
      <Column field="status" header="Status" />
      <Column field="assignedOperatorName" header="Operador">
        <template #body="{ data }">
          {{ (data as CashRegisterTerminalEntity).assignedOperatorName || '—' }}
        </template>
      </Column>
      <Column header="Ações">
        <template #body="{ data }">
          <div class="flex gap-2">
            <Button icon="pi pi-pencil" text @click="openEdit(data as CashRegisterTerminalEntity)" />
            <Button icon="pi pi-trash" text severity="danger" @click="remove(data as CashRegisterTerminalEntity)" />
          </div>
        </template>
      </Column>
    </DataTable>

    <Dialog
      v-model:visible="dialogVisible"
      :header="editing ? 'Editar caixa' : 'Novo caixa'"
      modal
      class="w-full max-w-md"
    >
      <div class="space-y-3">
        <div>
          <label class="mb-1 block text-sm">Nome</label>
          <InputText v-model="form.name" class="w-full" />
        </div>
        <div>
          <label class="mb-1 block text-sm">Status</label>
          <Dropdown v-model="form.status" :options="statusOptions" option-label="label" option-value="value" class="w-full" />
        </div>
        <div>
          <label class="mb-1 block text-sm">Operador</label>
          <Dropdown
            v-model="form.assignedOperatorId"
            :options="operators"
            option-label="name"
            option-value="id"
            placeholder="Opcional"
            show-clear
            class="w-full"
          />
        </div>
      </div>
      <template #footer>
        <Button label="Cancelar" text @click="dialogVisible = false" />
        <Button label="Salvar" @click="save" />
      </template>
    </Dialog>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'
import InputText from 'primevue/inputtext'
import InputNumber from 'primevue/inputnumber'
import Checkbox from 'primevue/checkbox'
import Message from 'primevue/message'
import { useAuthStore } from '@/stores/auth'
import { createStoreSettingsModule } from '@/modules/store-settings/composition'
import type { PaymentMethodEntity } from '@/modules/store-settings/domain/payment-method.entity'

const auth = useAuthStore()
const module = createStoreSettingsModule(() => auth.token)

const items = ref<PaymentMethodEntity[]>([])
const error = ref('')
const dialogVisible = ref(false)
const editing = ref<PaymentMethodEntity | null>(null)
const form = ref({ name: '', surchargePercent: 0, isActive: true })

async function load() {
  error.value = ''
  const result = await module.listPaymentMethodsUseCase.execute()
  if (!result.ok) {
    error.value = result.error
    return
  }
  items.value = result.data
}

function openCreate() {
  editing.value = null
  form.value = { name: '', surchargePercent: 0, isActive: true }
  dialogVisible.value = true
}

function openEdit(row: PaymentMethodEntity) {
  editing.value = row
  form.value = {
    name: row.name,
    surchargePercent: row.surchargePercent,
    isActive: row.isActive,
  }
  dialogVisible.value = true
}

async function save() {
  error.value = ''
  const payload = { ...form.value }
  const result = editing.value
    ? await module.updatePaymentMethodUseCase.execute(editing.value.id, payload)
    : await module.createPaymentMethodUseCase.execute(payload)
  if (!result.ok) {
    error.value = result.error
    return
  }
  dialogVisible.value = false
  await load()
}

async function remove(row: PaymentMethodEntity) {
  error.value = ''
  const result = await module.deletePaymentMethodUseCase.execute(row.id)
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
      <h1 class="text-xl font-semibold">Formas de pagamento</h1>
      <Button label="Nova forma" icon="pi pi-plus" @click="openCreate" />
    </div>
    <Message v-if="error" severity="error" :closable="false">{{ error }}</Message>
    <DataTable :value="items" data-key="id">
      <Column field="name" header="Nome" />
      <Column field="surchargePercent" header="Acréscimo (%)">
        <template #body="{ data }">{{ (data as PaymentMethodEntity).surchargePercent }}%</template>
      </Column>
      <Column field="isActive" header="Ativo">
        <template #body="{ data }">{{ (data as PaymentMethodEntity).isActive ? 'Sim' : 'Não' }}</template>
      </Column>
      <Column header="Ações">
        <template #body="{ data }">
          <div class="flex gap-2">
            <Button icon="pi pi-pencil" text @click="openEdit(data as PaymentMethodEntity)" />
            <Button icon="pi pi-trash" text severity="danger" @click="remove(data as PaymentMethodEntity)" />
          </div>
        </template>
      </Column>
    </DataTable>

    <Dialog
      v-model:visible="dialogVisible"
      :header="editing ? 'Editar forma de pagamento' : 'Nova forma de pagamento'"
      modal
      class="w-full max-w-md"
    >
      <div class="space-y-3">
        <div>
          <label class="mb-1 block text-sm">Nome</label>
          <InputText v-model="form.name" class="w-full" />
        </div>
        <div>
          <label class="mb-1 block text-sm">Acréscimo (%)</label>
          <InputNumber v-model="form.surchargePercent" class="w-full" :min="0" :max="100" />
        </div>
        <div class="flex items-center gap-2">
          <Checkbox v-model="form.isActive" binary input-id="isActive" />
          <label for="isActive">Ativo</label>
        </div>
      </div>
      <template #footer>
        <Button label="Cancelar" text @click="dialogVisible = false" />
        <Button label="Salvar" @click="save" />
      </template>
    </Dialog>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'
import InputText from 'primevue/inputtext'
import Message from 'primevue/message'
import { useAuthStore } from '@/stores/auth'
import { createCrmModule } from '@/modules/crm/composition'
import type { CustomerEntity } from '@/modules/crm/domain/customer.entity'

const auth = useAuthStore()
const module = createCrmModule(() => auth.token)

const items = ref<CustomerEntity[]>([])
const total = ref(0)
const page = ref(1)
const pageSize = ref(20)
const nameFilter = ref('')
const cpfFilter = ref('')
const error = ref('')
const loading = ref(false)
const dialogVisible = ref(false)
const editing = ref<CustomerEntity | null>(null)
const form = ref({
  name: '',
  cpf: '',
  phone: '',
  email: '',
  address: '',
})

function formatCpf(value: string): string {
  const digits = value.replace(/\D/g, '').slice(0, 11)
  if (digits.length <= 3) return digits
  if (digits.length <= 6) return `${digits.slice(0, 3)}.${digits.slice(3)}`
  if (digits.length <= 9) return `${digits.slice(0, 3)}.${digits.slice(3, 6)}.${digits.slice(6)}`
  return `${digits.slice(0, 3)}.${digits.slice(3, 6)}.${digits.slice(6, 9)}-${digits.slice(9)}`
}

async function load() {
  error.value = ''
  loading.value = true
  try {
    const result = await module.listCustomersUseCase.execute({
      name: nameFilter.value || undefined,
      cpf: cpfFilter.value || undefined,
      page: page.value,
      pageSize: pageSize.value,
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

function onPage(event: { page: number; rows: number }) {
  page.value = event.page + 1
  pageSize.value = event.rows
  load()
}

function search() {
  page.value = 1
  load()
}

function openCreate() {
  editing.value = null
  form.value = { name: '', cpf: '', phone: '', email: '', address: '' }
  dialogVisible.value = true
}

function openEdit(row: CustomerEntity) {
  editing.value = row
  form.value = {
    name: row.name,
    cpf: formatCpf(row.cpf),
    phone: row.phone ?? '',
    email: row.email ?? '',
    address: row.address ?? '',
  }
  dialogVisible.value = true
}

async function save() {
  error.value = ''
  const payload = {
    name: form.value.name.trim(),
    cpf: form.value.cpf,
    phone: form.value.phone.trim() || undefined,
    email: form.value.email.trim() || undefined,
    address: form.value.address.trim() || undefined,
  }
  const result = editing.value
    ? await module.updateCustomerUseCase.execute(editing.value.id, payload)
    : await module.createCustomerUseCase.execute(payload)
  if (!result.ok) {
    error.value = result.error
    return
  }
  dialogVisible.value = false
  await load()
}

async function deactivate(row: CustomerEntity) {
  error.value = ''
  const result = await module.deactivateCustomerUseCase.execute(row.id)
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
      <h1 class="text-xl font-semibold">Clientes</h1>
      <Button label="Novo cliente" icon="pi pi-plus" @click="openCreate" />
    </div>
    <Message v-if="error" severity="error" :closable="false">{{ error }}</Message>
    <div class="flex flex-wrap gap-2">
      <InputText v-model="nameFilter" placeholder="Filtrar por nome" class="w-64" @keyup.enter="search" />
      <InputText v-model="cpfFilter" placeholder="Filtrar por CPF" class="w-48" @keyup.enter="search" />
      <Button label="Buscar" icon="pi pi-search" :loading="loading" @click="search" />
    </div>
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
      <Column field="name" header="Nome" />
      <Column field="cpf" header="CPF">
        <template #body="{ data }">{{ formatCpf((data as CustomerEntity).cpf) }}</template>
      </Column>
      <Column field="phone" header="Telefone">
        <template #body="{ data }">{{ (data as CustomerEntity).phone || '—' }}</template>
      </Column>
      <Column field="email" header="E-mail">
        <template #body="{ data }">{{ (data as CustomerEntity).email || '—' }}</template>
      </Column>
      <Column field="isActive" header="Ativo">
        <template #body="{ data }">{{ (data as CustomerEntity).isActive ? 'Sim' : 'Não' }}</template>
      </Column>
      <Column header="Ações">
        <template #body="{ data }">
          <div class="flex gap-2">
            <Button icon="pi pi-pencil" text @click="openEdit(data as CustomerEntity)" />
            <Button
              v-if="(data as CustomerEntity).isActive"
              icon="pi pi-ban"
              text
              severity="danger"
              @click="deactivate(data as CustomerEntity)"
            />
          </div>
        </template>
      </Column>
    </DataTable>

    <Dialog
      v-model:visible="dialogVisible"
      :header="editing ? 'Editar cliente' : 'Novo cliente'"
      modal
      class="w-full max-w-lg"
    >
      <div class="space-y-3">
        <div>
          <label class="mb-1 block text-sm">Nome</label>
          <InputText v-model="form.name" class="w-full" />
        </div>
        <div>
          <label class="mb-1 block text-sm">CPF</label>
          <InputText
            v-model="form.cpf"
            class="w-full"
            placeholder="000.000.000-00"
            :disabled="!!editing"
            @input="form.cpf = formatCpf(form.cpf)"
          />
        </div>
        <div>
          <label class="mb-1 block text-sm">Telefone</label>
          <InputText v-model="form.phone" class="w-full" />
        </div>
        <div>
          <label class="mb-1 block text-sm">E-mail</label>
          <InputText v-model="form.email" class="w-full" type="email" />
        </div>
        <div>
          <label class="mb-1 block text-sm">Endereço</label>
          <InputText v-model="form.address" class="w-full" />
        </div>
      </div>
      <template #footer>
        <Button label="Cancelar" text @click="dialogVisible = false" />
        <Button label="Salvar" @click="save" />
      </template>
    </Dialog>
  </div>
</template>

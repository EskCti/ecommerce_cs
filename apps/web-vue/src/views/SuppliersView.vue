<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'
import InputText from 'primevue/inputtext'
import Dropdown from 'primevue/dropdown'
import Message from 'primevue/message'
import { useAuthStore } from '@/stores/auth'
import { createCrmModule } from '@/modules/crm/composition'
import { isValidCpf } from '@/modules/crm/domain/customer.entity'
import { isValidCnpj, type PersonType, type SupplierEntity } from '@/modules/crm/domain/supplier.entity'

const auth = useAuthStore()
const module = createCrmModule(() => auth.token)

const items = ref<SupplierEntity[]>([])
const total = ref(0)
const page = ref(1)
const pageSize = ref(20)
const nameFilter = ref('')
const personTypeFilter = ref<PersonType | null>(null)
const error = ref('')
const loading = ref(false)
const dialogVisible = ref(false)
const editing = ref<SupplierEntity | null>(null)
const form = ref({
  name: '',
  personType: 'Individual' as PersonType,
  taxDocument: '',
  phone: '',
  email: '',
  address: '',
})

const personTypeOptions = [
  { label: 'Pessoa física (F)', value: 'Individual' as PersonType },
  { label: 'Pessoa jurídica (J)', value: 'Company' as PersonType },
]

const filterPersonTypeOptions = [{ label: 'Todos', value: null }, ...personTypeOptions]

const taxDocumentLabel = computed(() =>
  form.value.personType === 'Company' ? 'CNPJ' : 'CPF',
)

function onlyDigits(value: string): string {
  return value.replace(/\D/g, '')
}

function formatCpf(value: string): string {
  const digits = onlyDigits(value).slice(0, 11)
  if (digits.length <= 3) return digits
  if (digits.length <= 6) return `${digits.slice(0, 3)}.${digits.slice(3)}`
  if (digits.length <= 9) return `${digits.slice(0, 3)}.${digits.slice(3, 6)}.${digits.slice(6)}`
  return `${digits.slice(0, 3)}.${digits.slice(3, 6)}.${digits.slice(6, 9)}-${digits.slice(9)}`
}

function formatCnpj(value: string): string {
  const digits = onlyDigits(value).slice(0, 14)
  if (digits.length <= 2) return digits
  if (digits.length <= 5) return `${digits.slice(0, 2)}.${digits.slice(2)}`
  if (digits.length <= 8) return `${digits.slice(0, 2)}.${digits.slice(2, 5)}.${digits.slice(5)}`
  if (digits.length <= 12) {
    return `${digits.slice(0, 2)}.${digits.slice(2, 5)}.${digits.slice(5, 8)}/${digits.slice(8)}`
  }
  return `${digits.slice(0, 2)}.${digits.slice(2, 5)}.${digits.slice(5, 8)}/${digits.slice(8, 12)}-${digits.slice(12)}`
}

function formatTaxDocument(value: string, personType: PersonType): string {
  return personType === 'Company' ? formatCnpj(value) : formatCpf(value)
}

function personTypeLabel(type: PersonType): string {
  return type === 'Company' ? 'Jurídica' : 'Física'
}

async function load() {
  error.value = ''
  loading.value = true
  try {
    const result = await module.listSuppliersUseCase.execute({
      name: nameFilter.value || undefined,
      personType: personTypeFilter.value ?? undefined,
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

function onPersonTypeChange() {
  form.value.taxDocument = ''
}

function openCreate() {
  editing.value = null
  form.value = {
    name: '',
    personType: 'Individual',
    taxDocument: '',
    phone: '',
    email: '',
    address: '',
  }
  dialogVisible.value = true
}

function openEdit(row: SupplierEntity) {
  editing.value = row
  form.value = {
    name: row.name,
    personType: row.personType,
    taxDocument: formatTaxDocument(row.taxDocument, row.personType),
    phone: row.phone ?? '',
    email: row.email ?? '',
    address: row.address ?? '',
  }
  dialogVisible.value = true
}

function validateForm(): string[] {
  const errors: string[] = []
  if (!form.value.name.trim()) errors.push('Nome do fornecedor é obrigatório')
  if (!form.value.taxDocument.trim()) {
    errors.push(form.value.personType === 'Company' ? 'CNPJ é obrigatório' : 'CPF é obrigatório')
  } else if (form.value.personType === 'Company') {
    if (!isValidCnpj(form.value.taxDocument)) errors.push('CNPJ inválido')
  } else if (!isValidCpf(form.value.taxDocument)) {
    errors.push('CPF inválido')
  }
  if (form.value.email.trim() && !form.value.email.includes('@')) errors.push('E-mail inválido')
  return errors
}

async function save() {
  error.value = ''
  const validationErrors = validateForm()
  if (validationErrors.length > 0) {
    error.value = validationErrors.join(' • ')
    return
  }
  const payload = {
    name: form.value.name.trim(),
    personType: form.value.personType,
    taxDocument: form.value.taxDocument,
    phone: form.value.phone.trim() || undefined,
    email: form.value.email.trim() || undefined,
    address: form.value.address.trim() || undefined,
  }
  const result = editing.value
    ? await module.updateSupplierUseCase.execute(editing.value.id, payload)
    : await module.createSupplierUseCase.execute(payload)
  if (!result.ok) {
    error.value = result.error
    return
  }
  dialogVisible.value = false
  await load()
}

async function deactivate(row: SupplierEntity) {
  error.value = ''
  const result = await module.deactivateSupplierUseCase.execute(row.id)
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
      <h1 class="text-xl font-semibold">Fornecedores</h1>
      <Button label="Novo fornecedor" icon="pi pi-plus" @click="openCreate" />
    </div>
    <Message v-if="error" severity="error" :closable="false">{{ error }}</Message>
    <div class="flex flex-wrap gap-2">
      <InputText v-model="nameFilter" placeholder="Filtrar por nome" class="w-64" @keyup.enter="search" />
      <Dropdown
        v-model="personTypeFilter"
        :options="filterPersonTypeOptions"
        option-label="label"
        option-value="value"
        placeholder="Tipo de pessoa"
        class="w-52"
      />
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
      <Column field="personType" header="Tipo">
        <template #body="{ data }">{{ personTypeLabel((data as SupplierEntity).personType) }}</template>
      </Column>
      <Column field="taxDocument" header="CPF/CNPJ">
        <template #body="{ data }">
          {{
            formatTaxDocument(
              (data as SupplierEntity).taxDocument,
              (data as SupplierEntity).personType,
            )
          }}
        </template>
      </Column>
      <Column field="phone" header="Telefone">
        <template #body="{ data }">{{ (data as SupplierEntity).phone || '—' }}</template>
      </Column>
      <Column field="email" header="E-mail">
        <template #body="{ data }">{{ (data as SupplierEntity).email || '—' }}</template>
      </Column>
      <Column field="isActive" header="Ativo">
        <template #body="{ data }">{{ (data as SupplierEntity).isActive ? 'Sim' : 'Não' }}</template>
      </Column>
      <Column header="Ações">
        <template #body="{ data }">
          <div class="flex gap-2">
            <Button icon="pi pi-pencil" text @click="openEdit(data as SupplierEntity)" />
            <Button
              v-if="(data as SupplierEntity).isActive"
              icon="pi pi-ban"
              text
              severity="danger"
              @click="deactivate(data as SupplierEntity)"
            />
          </div>
        </template>
      </Column>
    </DataTable>

    <Dialog
      v-model:visible="dialogVisible"
      :header="editing ? 'Editar fornecedor' : 'Novo fornecedor'"
      modal
      class="w-full max-w-lg"
    >
      <div class="space-y-3">
        <div>
          <label class="mb-1 block text-sm">Nome</label>
          <InputText v-model="form.name" class="w-full" />
        </div>
        <div>
          <label class="mb-1 block text-sm">Tipo de pessoa</label>
          <Dropdown
            v-model="form.personType"
            :options="personTypeOptions"
            option-label="label"
            option-value="value"
            class="w-full"
            @change="onPersonTypeChange"
          />
        </div>
        <div>
          <label class="mb-1 block text-sm">{{ taxDocumentLabel }}</label>
          <InputText
            v-model="form.taxDocument"
            class="w-full"
            :placeholder="form.personType === 'Company' ? '00.000.000/0000-00' : '000.000.000-00'"
            @input="form.taxDocument = formatTaxDocument(form.taxDocument, form.personType)"
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

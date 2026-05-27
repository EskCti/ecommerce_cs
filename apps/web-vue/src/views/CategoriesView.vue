<script setup lang="ts">
import { onMounted, ref } from 'vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'
import InputText from 'primevue/inputtext'
import Message from 'primevue/message'
import { useAuthStore } from '@/stores/auth'
import { createCatalogModule } from '@/modules/catalog/composition'
import type { CategoryEntity } from '@/modules/catalog/domain/category.entity'

const auth = useAuthStore()
const module = createCatalogModule(() => auth.token)

const items = ref<CategoryEntity[]>([])
const nameFilter = ref('')
const error = ref('')
const loading = ref(false)
const dialogVisible = ref(false)
const editing = ref<CategoryEntity | null>(null)
const form = ref({ name: '' })

async function load() {
  error.value = ''
  loading.value = true
  try {
    const result = await module.listCategoriesUseCase.execute({
      name: nameFilter.value || undefined,
    })
    if (!result.ok) {
      error.value = result.error
      return
    }
    items.value = result.data
  } finally {
    loading.value = false
  }
}

function search() {
  load()
}

function openCreate() {
  editing.value = null
  form.value = { name: '' }
  dialogVisible.value = true
}

function openEdit(row: CategoryEntity) {
  editing.value = row
  form.value = { name: row.name }
  dialogVisible.value = true
}

async function save() {
  error.value = ''
  const name = form.value.name.trim()
  const result = editing.value
    ? await module.updateCategoryUseCase.execute(editing.value.id, { name })
    : await module.createCategoryUseCase.execute({ name })
  if (!result.ok) {
    error.value = result.error
    return
  }
  dialogVisible.value = false
  await load()
}

async function deactivate(row: CategoryEntity) {
  error.value = ''
  const result = await module.deactivateCategoryUseCase.execute(row.id)
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
      <h1 class="text-xl font-semibold">Categorias</h1>
      <Button label="Nova categoria" icon="pi pi-plus" @click="openCreate" />
    </div>
    <Message v-if="error" severity="error" :closable="false">{{ error }}</Message>
    <div class="flex flex-wrap gap-2">
      <InputText v-model="nameFilter" placeholder="Filtrar por nome" class="w-64" @keyup.enter="search" />
      <Button label="Buscar" icon="pi pi-search" :loading="loading" @click="search" />
    </div>
    <DataTable :value="items" data-key="id" :loading="loading">
      <Column field="name" header="Nome" />
      <Column field="isActive" header="Ativo">
        <template #body="{ data }">{{ (data as CategoryEntity).isActive ? 'Sim' : 'Não' }}</template>
      </Column>
      <Column header="Ações">
        <template #body="{ data }">
          <div class="flex gap-2">
            <Button icon="pi pi-pencil" text @click="openEdit(data as CategoryEntity)" />
            <Button
              v-if="(data as CategoryEntity).isActive"
              icon="pi pi-ban"
              text
              severity="danger"
              @click="deactivate(data as CategoryEntity)"
            />
          </div>
        </template>
      </Column>
    </DataTable>

    <Dialog
      v-model:visible="dialogVisible"
      :header="editing ? 'Editar categoria' : 'Nova categoria'"
      modal
      class="w-full max-w-lg"
    >
      <div class="space-y-3">
        <div>
          <label class="mb-1 block text-sm">Nome</label>
          <InputText v-model="form.name" class="w-full" />
        </div>
      </div>
      <template #footer>
        <Button label="Cancelar" text @click="dialogVisible = false" />
        <Button label="Salvar" @click="save" />
      </template>
    </Dialog>
  </div>
</template>

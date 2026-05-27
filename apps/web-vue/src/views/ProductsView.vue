<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'
import InputText from 'primevue/inputtext'
import InputNumber from 'primevue/inputnumber'
import Dropdown from 'primevue/dropdown'
import Checkbox from 'primevue/checkbox'
import Message from 'primevue/message'
import { useAuthStore } from '@/stores/auth'
import { createCatalogModule } from '@/modules/catalog/composition'
import type { ProductEntity } from '@/modules/catalog/domain/product.entity'
import type { CategoryEntity } from '@/modules/catalog/domain/category.entity'

const auth = useAuthStore()
const module = createCatalogModule(() => auth.token)

const items = ref<ProductEntity[]>([])
const categories = ref<CategoryEntity[]>([])
const total = ref(0)
const page = ref(1)
const pageSize = ref(20)
const nameFilter = ref('')
const barcodeFilter = ref('')
const error = ref('')
const loading = ref(false)
const dialogVisible = ref(false)
const gradesDialogVisible = ref(false)
const gradesLoading = ref(false)
const gradeProduct = ref<ProductEntity | null>(null)
const grades = ref<{ id: string; name: string; options: { id: string; label: string; stock: number }[] }[]>([])
const gradeForm = ref({ dimensionName: '', optionLabel: '', stock: 0, selectedDimensionId: null as string | null })
const editing = ref<ProductEntity | null>(null)
const form = ref({
  barcode: '',
  name: '',
  description: '',
  salePrice: 0,
  costPrice: 0,
  initialStock: 0,
  stockAlertLevel: 0,
  categoryId: '' as string | null,
  photoPath: '',
  openPrice: false,
})

const categoryOptions = computed(() =>
  categories.value
    .filter((c) => c.isActive)
    .map((c) => ({ label: c.name, value: c.id })),
)

watch(
  () => form.value.openPrice,
  (openPrice) => {
    if (openPrice) form.value.salePrice = 0
  },
)

watch(
  () => form.value.salePrice,
  (salePrice) => {
    if (salePrice > 0 && form.value.openPrice) form.value.openPrice = false
  },
)

async function loadCategories() {
  const result = await module.listCategoriesUseCase.execute({ isActive: true, pageSize: 200 })
  if (result.ok) categories.value = result.data
}

async function load() {
  error.value = ''
  loading.value = true
  try {
    const result = await module.listProductsUseCase.execute({
      name: nameFilter.value || undefined,
      barcode: barcodeFilter.value || undefined,
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
  form.value = {
    barcode: '',
    name: '',
    description: '',
    salePrice: 0,
    costPrice: 0,
    initialStock: 0,
    stockAlertLevel: 0,
    categoryId: null,
    photoPath: '',
    openPrice: false,
  }
  dialogVisible.value = true
}

function openEdit(row: ProductEntity) {
  editing.value = row
  form.value = {
    barcode: row.barcode,
    name: row.name,
    description: row.description ?? '',
    salePrice: row.salePrice,
    costPrice: row.costPrice,
    initialStock: row.stock,
    stockAlertLevel: row.stockAlertLevel,
    categoryId: row.categoryId,
    photoPath: row.photoPath ?? '',
    openPrice: row.isOpenPrice,
  }
  dialogVisible.value = true
}

async function generateBarcode() {
  error.value = ''
  const result = await module.generateBarcodeUseCase.execute()
  if (!result.ok) {
    error.value = result.error
    return
  }
  form.value.barcode = result.data.barcode
}

function categoryName(categoryId: string): string {
  return categories.value.find((c) => c.id === categoryId)?.name ?? '—'
}

function formatMoney(value: number): string {
  if (value === 0) return 'Preço aberto'
  return value.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
}

async function save() {
  error.value = ''
  if (!form.value.categoryId) {
    error.value = 'Selecione uma categoria'
    return
  }

  const salePrice = form.value.openPrice ? 0 : Number(form.value.salePrice)

  if (editing.value) {
    const result = await module.updateProductUseCase.execute(editing.value.id, {
      name: form.value.name.trim(),
      description: form.value.description.trim() || undefined,
      salePrice,
      costPrice: Number(form.value.costPrice),
      stockAlertLevel: Number(form.value.stockAlertLevel),
      categoryId: form.value.categoryId,
      photoPath: form.value.photoPath.trim() || undefined,
    })
    if (!result.ok) {
      error.value = result.error
      return
    }
  } else {
    const result = await module.createProductUseCase.execute({
      barcode: form.value.barcode.trim(),
      name: form.value.name.trim(),
      description: form.value.description.trim() || undefined,
      salePrice,
      costPrice: Number(form.value.costPrice),
      initialStock: Number(form.value.initialStock),
      stockAlertLevel: Number(form.value.stockAlertLevel),
      categoryId: form.value.categoryId,
      photoPath: form.value.photoPath.trim() || undefined,
    })
    if (!result.ok) {
      error.value = result.error
      return
    }
  }

  dialogVisible.value = false
  await load()
}

async function openGrades(row: ProductEntity) {
  gradeProduct.value = row
  gradeForm.value = { dimensionName: '', optionLabel: '', stock: 0, selectedDimensionId: null }
  error.value = ''
  gradesLoading.value = true
  gradesDialogVisible.value = true
  try {
    const result = await module.listGradesUseCase.execute(row.id)
    if (!result.ok) {
      error.value = result.error
      grades.value = []
      return
    }
    grades.value = result.data
    if (result.data.length > 0) gradeForm.value.selectedDimensionId = result.data[0].id
  } finally {
    gradesLoading.value = false
  }
}

async function addDimension() {
  if (!gradeProduct.value || !gradeForm.value.dimensionName.trim()) return
  gradesLoading.value = true
  try {
    const result = await module.configureGradeUseCase.execute(gradeProduct.value.id, {
      action: 'AddDimension',
      dimensionName: gradeForm.value.dimensionName.trim(),
    })
    if (!result.ok) {
      error.value = result.error
      return
    }
    grades.value = result.data
    gradeForm.value.dimensionName = ''
    if (result.data.length > 0) gradeForm.value.selectedDimensionId = result.data[result.data.length - 1].id
  } finally {
    gradesLoading.value = false
  }
}

async function addOption() {
  if (!gradeProduct.value || !gradeForm.value.selectedDimensionId || !gradeForm.value.optionLabel.trim()) return
  gradesLoading.value = true
  try {
    const result = await module.configureGradeUseCase.execute(gradeProduct.value.id, {
      action: 'AddOption',
      dimensionId: gradeForm.value.selectedDimensionId,
      optionLabel: gradeForm.value.optionLabel.trim(),
      stock: Number(gradeForm.value.stock),
    })
    if (!result.ok) {
      error.value = result.error
      return
    }
    grades.value = result.data
    gradeForm.value.optionLabel = ''
    gradeForm.value.stock = 0
  } finally {
    gradesLoading.value = false
  }
}

async function deactivate(row: ProductEntity) {
  error.value = ''
  const result = await module.deactivateProductUseCase.execute(row.id)
  if (!result.ok) {
    error.value = result.error
    return
  }
  await load()
}

onMounted(async () => {
  await loadCategories()
  await load()
})
</script>

<template>
  <div class="space-y-4 p-6">
    <div class="flex items-center justify-between">
      <h1 class="text-xl font-semibold">Produtos</h1>
      <Button label="Novo produto" icon="pi pi-plus" @click="openCreate" />
    </div>
    <Message v-if="error" severity="error" :closable="false">{{ error }}</Message>
    <div class="flex flex-wrap gap-2">
      <InputText v-model="nameFilter" placeholder="Filtrar por nome" class="w-64" @keyup.enter="search" />
      <InputText v-model="barcodeFilter" placeholder="Filtrar por código" class="w-48" @keyup.enter="search" />
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
      <Column field="barcode" header="Código" />
      <Column field="name" header="Nome" />
      <Column header="Preço venda">
        <template #body="{ data }">{{ formatMoney((data as ProductEntity).salePrice) }}</template>
      </Column>
      <Column field="stock" header="Estoque" />
      <Column header="Categoria">
        <template #body="{ data }">{{ categoryName((data as ProductEntity).categoryId) }}</template>
      </Column>
      <Column field="isLowStock" header="Estoque baixo">
        <template #body="{ data }">{{ (data as ProductEntity).isLowStock ? 'Sim' : 'Não' }}</template>
      </Column>
      <Column field="isActive" header="Ativo">
        <template #body="{ data }">{{ (data as ProductEntity).isActive ? 'Sim' : 'Não' }}</template>
      </Column>
      <Column header="Ações">
        <template #body="{ data }">
          <div class="flex gap-2">
            <Button icon="pi pi-pencil" text @click="openEdit(data as ProductEntity)" />
            <Button icon="pi pi-th-large" text title="Grades" @click="openGrades(data as ProductEntity)" />
            <Button
              v-if="(data as ProductEntity).isActive"
              icon="pi pi-ban"
              text
              severity="danger"
              @click="deactivate(data as ProductEntity)"
            />
          </div>
        </template>
      </Column>
    </DataTable>

    <Dialog
      v-model:visible="dialogVisible"
      :header="editing ? 'Editar produto' : 'Novo produto'"
      modal
      class="w-full max-w-2xl"
    >
      <div class="grid gap-3 sm:grid-cols-2">
        <div class="sm:col-span-2">
          <label class="mb-1 block text-sm">Código de barras</label>
          <div class="flex gap-2">
            <InputText
              v-model="form.barcode"
              class="w-full"
              :disabled="!!editing"
            />
            <Button
              v-if="!editing"
              label="Gerar"
              icon="pi pi-barcode"
              outlined
              @click="generateBarcode"
            />
          </div>
        </div>
        <div class="sm:col-span-2">
          <label class="mb-1 block text-sm">Nome</label>
          <InputText v-model="form.name" class="w-full" />
        </div>
        <div class="sm:col-span-2">
          <label class="mb-1 block text-sm">Descrição</label>
          <InputText v-model="form.description" class="w-full" />
        </div>
        <div class="flex items-center gap-2 sm:col-span-2">
          <Checkbox v-model="form.openPrice" binary input-id="openPrice" />
          <label for="openPrice" class="text-sm">Preço aberto (venda no PDV)</label>
        </div>
        <div>
          <label class="mb-1 block text-sm">Preço de venda</label>
          <InputNumber
            v-model="form.salePrice"
            class="w-full"
            mode="currency"
            currency="BRL"
            locale="pt-BR"
            :min="0"
            :disabled="form.openPrice"
          />
        </div>
        <div>
          <label class="mb-1 block text-sm">Preço de custo</label>
          <InputNumber
            v-model="form.costPrice"
            class="w-full"
            mode="currency"
            currency="BRL"
            locale="pt-BR"
            :min="0"
          />
        </div>
        <div v-if="!editing">
          <label class="mb-1 block text-sm">Estoque inicial</label>
          <InputNumber v-model="form.initialStock" class="w-full" :min="0" />
        </div>
        <div v-else>
          <label class="mb-1 block text-sm">Estoque atual</label>
          <InputNumber :model-value="editing.stock" class="w-full" disabled />
        </div>
        <div>
          <label class="mb-1 block text-sm">Nível mínimo (alerta)</label>
          <InputNumber v-model="form.stockAlertLevel" class="w-full" :min="0" />
        </div>
        <div class="sm:col-span-2">
          <label class="mb-1 block text-sm">Categoria</label>
          <Dropdown
            v-model="form.categoryId"
            :options="categoryOptions"
            option-label="label"
            option-value="value"
            placeholder="Selecione"
            class="w-full"
          />
        </div>
        <div class="sm:col-span-2">
          <label class="mb-1 block text-sm">Caminho da foto</label>
          <InputText v-model="form.photoPath" class="w-full" placeholder="/uploads/produto.jpg" />
        </div>
      </div>
      <template #footer>
        <Button label="Cancelar" text @click="dialogVisible = false" />
        <Button label="Salvar" @click="save" />
      </template>
    </Dialog>

    <Dialog
      v-model:visible="gradesDialogVisible"
      :header="gradeProduct ? `Grades — ${gradeProduct.name}` : 'Grades'"
      modal
      class="w-full max-w-2xl"
    >
      <div v-if="gradesLoading" class="text-sm text-gray-500">Carregando...</div>
      <div v-else class="space-y-4">
        <div v-for="dim in grades" :key="dim.id" class="rounded border p-3">
          <p class="font-medium">{{ dim.name }}</p>
          <ul class="mt-2 space-y-1 text-sm">
            <li v-for="opt in dim.options" :key="opt.id">
              {{ opt.label }} — estoque: {{ opt.stock }}
            </li>
            <li v-if="dim.options.length === 0" class="text-gray-500">Sem opções</li>
          </ul>
        </div>
        <div class="grid gap-2 sm:grid-cols-2">
          <div>
            <label class="mb-1 block text-sm">Nova dimensão (Cor, Tamanho)</label>
            <InputText v-model="gradeForm.dimensionName" class="w-full" />
          </div>
          <div class="flex items-end">
            <Button label="Adicionar dimensão" size="small" @click="addDimension" />
          </div>
        </div>
        <div v-if="grades.length > 0" class="grid gap-2 sm:grid-cols-3">
          <div>
            <label class="mb-1 block text-sm">Dimensão</label>
            <Dropdown
              v-model="gradeForm.selectedDimensionId"
              :options="grades.map((g) => ({ label: g.name, value: g.id }))"
              option-label="label"
              option-value="value"
              class="w-full"
            />
          </div>
          <div>
            <label class="mb-1 block text-sm">Opção</label>
            <InputText v-model="gradeForm.optionLabel" class="w-full" placeholder="P, M, Azul..." />
          </div>
          <div>
            <label class="mb-1 block text-sm">Estoque</label>
            <InputNumber v-model="gradeForm.stock" class="w-full" :min="0" />
          </div>
          <div class="sm:col-span-3">
            <Button label="Adicionar opção" size="small" @click="addOption" />
          </div>
        </div>
      </div>
    </Dialog>
  </div>
</template>

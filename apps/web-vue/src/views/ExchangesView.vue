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
import { createReturnsModule } from '@/modules/returns/composition'
import { createCatalogModule } from '@/modules/catalog/composition'
import type { ExchangeListItemData } from '@/modules/returns/domain/exchange.entity'
import type { ProductEntity } from '@/modules/catalog/domain/product.entity'

const auth = useAuthStore()
const returnsModule = createReturnsModule(() => auth.token)
const catalogModule = createCatalogModule(() => auth.token)

const items = ref<ExchangeListItemData[]>([])
const products = ref<ProductEntity[]>([])
const productLabels = ref<Record<string, string>>({})
const total = ref(0)
const page = ref(1)
const pageSize = ref(20)
const error = ref('')
const success = ref('')
const loading = ref(false)
const deletingId = ref<string | null>(null)
const dialogVisible = ref(false)
const submitting = ref(false)

const form = ref({
  cpf: '',
  customerName: '',
  productInId: '',
  productOutId: '',
})

const productOptions = computed(() =>
  products.value.map((p) => ({
    label: `${p.name} (est: ${p.stock})`,
    value: p.id,
  })),
)

function formatDate(value: string): string {
  return new Date(value).toLocaleString('pt-BR')
}

function productLabel(id: string): string {
  return productLabels.value[id] ?? id.slice(0, 8)
}

async function loadProducts() {
  const result = await catalogModule.listProductsUseCase.execute({ page: 1, pageSize: 200 })
  if (!result.ok) return
  products.value = result.data.items
  productLabels.value = Object.fromEntries(result.data.items.map((p) => [p.id, p.name]))
}

async function load() {
  error.value = ''
  loading.value = true
  try {
    const result = await returnsModule.listExchangesUseCase.execute({
      page: page.value,
      pageSize: pageSize.value,
    })
    if (!result.ok) {
      error.value = result.error
      return
    }
    items.value = [...result.data.items]
    total.value = result.data.totalCount
  } finally {
    loading.value = false
  }
}

function onPage(event: { page: number; rows: number }) {
  page.value = event.page + 1
  pageSize.value = event.rows
  load()
}

function openDialog() {
  success.value = ''
  error.value = ''
  form.value = { cpf: '', customerName: '', productInId: '', productOutId: '' }
  dialogVisible.value = true
}

async function submitExchange() {
  success.value = ''
  error.value = ''
  submitting.value = true
  try {
    const result = await returnsModule.registerExchangeUseCase.execute({
      cpf: form.value.cpf || undefined,
      customerName: form.value.customerName || undefined,
      productInId: form.value.productInId,
      productOutId: form.value.productOutId,
    })
    if (!result.ok) {
      error.value = result.error
      return
    }
    success.value = 'Troca registrada com sucesso.'
    dialogVisible.value = false
    await load()
  } finally {
    submitting.value = false
  }
}

async function deleteExchange(row: ExchangeListItemData) {
  if (!confirm('Excluir esta troca? O estoque será revertido.')) return
  error.value = ''
  deletingId.value = row.id
  try {
    const result = await returnsModule.deleteExchangeUseCase.execute(row.id)
    if (!result.ok) {
      error.value = result.error
      return
    }
    await load()
  } finally {
    deletingId.value = null
  }
}

onMounted(async () => {
  await loadProducts()
  await load()
})
</script>

<template>
  <div class="space-y-4 p-6">
    <div class="flex flex-wrap items-center justify-between gap-3">
      <div>
        <h1 class="text-xl font-semibold">Trocas</h1>
        <p class="mt-1 text-sm text-muted-foreground">
          Registro 1:1 de troca de produtos com ajuste automático de estoque (RF-060 / RF-061).
        </p>
      </div>
      <Button label="Nova troca" icon="pi pi-plus" @click="openDialog" />
    </div>

    <Message v-if="error" severity="error" :closable="false" class="whitespace-pre-wrap">{{ error }}</Message>
    <Message v-if="success" severity="success" :closable="false">{{ success }}</Message>

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
      <Column header="Entrada">
        <template #body="{ data }">{{ productLabel((data as ExchangeListItemData).productInId) }}</template>
      </Column>
      <Column header="Saída">
        <template #body="{ data }">{{ productLabel((data as ExchangeListItemData).productOutId) }}</template>
      </Column>
      <Column header="Data">
        <template #body="{ data }">{{ formatDate((data as ExchangeListItemData).registeredAt) }}</template>
      </Column>
      <Column header="Ações" style="width: 8rem">
        <template #body="{ data }">
          <Button
            icon="pi pi-trash"
            severity="danger"
            text
            :loading="deletingId === (data as ExchangeListItemData).id"
            @click="deleteExchange(data as ExchangeListItemData)"
          />
        </template>
      </Column>
      <template #empty>
        <div class="py-10 text-center text-sm text-muted-foreground">Nenhuma troca registrada.</div>
      </template>
    </DataTable>

    <Dialog v-model:visible="dialogVisible" header="Registrar troca" modal :style="{ width: '32rem' }">
      <div class="space-y-4">
        <div>
          <label class="mb-1 block text-sm font-medium">CPF do cliente</label>
          <InputText v-model="form.cpf" class="w-full" placeholder="000.000.000-00" />
        </div>
        <div>
          <label class="mb-1 block text-sm font-medium">Nome (se novo cliente)</label>
          <InputText v-model="form.customerName" class="w-full" />
        </div>
        <div>
          <label class="mb-1 block text-sm font-medium">Produto entrada</label>
          <Dropdown
            v-model="form.productInId"
            :options="productOptions"
            option-label="label"
            option-value="value"
            placeholder="Selecione"
            class="w-full"
          />
        </div>
        <div>
          <label class="mb-1 block text-sm font-medium">Produto saída</label>
          <Dropdown
            v-model="form.productOutId"
            :options="productOptions"
            option-label="label"
            option-value="value"
            placeholder="Selecione"
            class="w-full"
          />
        </div>
        <p class="text-xs text-muted-foreground">Quantidade fixa: 1 unidade de cada lado (RN-050).</p>
      </div>
      <template #footer>
        <Button label="Cancelar" severity="secondary" text @click="dialogVisible = false" />
        <Button label="Registrar" :loading="submitting" @click="submitExchange" />
      </template>
    </Dialog>
  </div>
</template>

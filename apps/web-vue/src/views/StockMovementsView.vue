<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import Button from 'primevue/button'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Dialog from 'primevue/dialog'
import Dropdown from 'primevue/dropdown'
import InputText from 'primevue/inputtext'
import InputNumber from 'primevue/inputnumber'
import Message from 'primevue/message'
import TabView from 'primevue/tabview'
import TabPanel from 'primevue/tabpanel'
import { useAuthStore } from '@/stores/auth'
import { createCatalogModule } from '@/modules/catalog/composition'
import type { ProductEntity } from '@/modules/catalog/domain/product.entity'
import type { StockMovement } from '@/modules/catalog/application/stock.repository'

const auth = useAuthStore()
const module = createCatalogModule(() => auth.token)

const products = ref<ProductEntity[]>([])
const movements = ref<StockMovement[]>([])
const total = ref(0)
const page = ref(1)
const pageSize = ref(20)
const filterProductId = ref<string | null>(null)
const error = ref('')
const success = ref('')
const loading = ref(false)
const dialogVisible = ref(false)
const movementType = ref<'entry' | 'exit' | 'purchase'>('entry')

const form = ref({
  productId: null as string | null,
  quantity: 1,
  reason: '',
  unitCost: 0,
})

const productOptions = computed(() =>
  products.value.map((p) => ({ label: `${p.barcode} — ${p.name}`, value: p.id })),
)

const productNameById = computed(() =>
  Object.fromEntries(products.value.map((p) => [p.id, `${p.barcode} — ${p.name}`])),
)

const userId = computed(() => auth.user?.legacyUserId ?? 1)

function movementTypeLabel(type: string): string {
  if (type === 'Entry') return 'Entrada'
  if (type === 'Exit') return 'Saída'
  if (type === 'Purchase') return 'Compra'
  return type
}

function formatDate(value: string): string {
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return '—'
  return date.toLocaleString('pt-BR')
}

async function loadProducts() {
  const result = await module.listProductsUseCase.execute({ pageSize: 200, isActive: true })
  if (result.ok) products.value = result.data.items
}

async function loadMovements() {
  error.value = ''
  loading.value = true
  try {
    const result = await module.listStockMovementsUseCase.execute({
      productId: filterProductId.value ?? undefined,
      page: page.value,
      pageSize: pageSize.value,
    })
    if (!result.ok) {
      error.value = result.error
      return
    }
    movements.value = result.data.items
    total.value = result.data.total
  } finally {
    loading.value = false
  }
}

function onPage(event: { page: number; rows: number }) {
  page.value = event.page + 1
  pageSize.value = event.rows
  loadMovements()
}

function onFilterChange() {
  page.value = 1
  loadMovements()
}

function openMovement(type: 'entry' | 'exit' | 'purchase') {
  movementType.value = type
  form.value = { productId: filterProductId.value, quantity: 1, reason: '', unitCost: 0 }
  error.value = ''
  dialogVisible.value = true
}

async function submit() {
  error.value = ''
  success.value = ''
  if (!form.value.productId) {
    error.value = 'Selecione um produto'
    return
  }
  if (!form.value.reason.trim()) {
    error.value = 'Informe o motivo'
    return
  }
  if (movementType.value === 'purchase' && form.value.unitCost <= 0) {
    error.value = 'Informe o custo unitário'
    return
  }

  loading.value = true
  try {
    const base = {
      productId: form.value.productId,
      quantity: Number(form.value.quantity),
      reason: form.value.reason.trim(),
      userId: userId.value,
    }

    let result
    if (movementType.value === 'entry') {
      result = await module.recordStockEntryUseCase.execute(base)
    } else if (movementType.value === 'exit') {
      result = await module.recordStockExitUseCase.execute(base)
    } else {
      result = await module.purchaseStockUseCase.execute({
        ...base,
        unitCost: Number(form.value.unitCost),
      })
    }

    if (!result.ok) {
      error.value = result.error
      return
    }

    success.value = 'Movimentação registrada com sucesso'
    dialogVisible.value = false
    page.value = 1
    await Promise.all([loadProducts(), loadMovements()])
  } finally {
    loading.value = false
  }
}

onMounted(async () => {
  await loadProducts()
  await loadMovements()
})
</script>

<template>
  <div class="space-y-4">
    <div class="flex flex-wrap items-center justify-between gap-3">
      <h1 class="text-xl font-semibold">Movimentação de estoque</h1>
      <Dropdown
        v-model="filterProductId"
        :options="[{ label: 'Todos os produtos', value: null }, ...productOptions]"
        option-label="label"
        option-value="value"
        placeholder="Filtrar por produto"
        class="w-72"
        filter
        show-clear
        @change="onFilterChange"
      />
    </div>

    <Message v-if="success" severity="success" :closable="false">{{ success }}</Message>
    <Message v-if="error && !dialogVisible" severity="error" :closable="false" class="whitespace-pre-wrap">{{ error }}</Message>

    <TabView>
      <TabPanel value="entry" header="Entrada manual">
        <p class="mb-4 text-sm text-muted-foreground">Registre entrada de estoque (RF-040).</p>
        <Button label="Nova entrada" icon="pi pi-plus" @click="openMovement('entry')" />
      </TabPanel>
      <TabPanel value="exit" header="Saída manual">
        <p class="mb-4 text-sm text-muted-foreground">Registre saída de estoque (RF-041).</p>
        <Button label="Nova saída" icon="pi pi-minus" @click="openMovement('exit')" />
      </TabPanel>
      <TabPanel value="purchase" header="Compra">
        <p class="mb-4 text-sm text-muted-foreground">Compra de mercadoria com geração de conta a pagar (RF-042).</p>
        <Button label="Registrar compra" icon="pi pi-shopping-cart" @click="openMovement('purchase')" />
      </TabPanel>
    </TabView>

    <DataTable
      :value="movements"
      data-key="id"
      :loading="loading"
      lazy
      paginator
      :rows="pageSize"
      :total-records="total"
      :first="(page - 1) * pageSize"
      @page="onPage"
    >
      <Column header="Data">
        <template #body="{ data }">{{ formatDate((data as StockMovement).createdAt) }}</template>
      </Column>
      <Column header="Tipo">
        <template #body="{ data }">{{ movementTypeLabel((data as StockMovement).type) }}</template>
      </Column>
      <Column header="Produto">
        <template #body="{ data }">
          {{ productNameById[(data as StockMovement).productId] ?? (data as StockMovement).productId }}
        </template>
      </Column>
      <Column field="quantity" header="Quantidade" />
      <Column field="reason" header="Motivo" />
      <template #empty>
        <div class="py-10 text-center text-sm text-muted-foreground">
          Nenhuma movimentação registrada ainda. Use os botões acima para lançar entrada, saída ou compra.
        </div>
      </template>
    </DataTable>

    <Dialog
      v-model:visible="dialogVisible"
      :header="
        movementType === 'entry'
          ? 'Entrada de estoque'
          : movementType === 'exit'
            ? 'Saída de estoque'
            : 'Compra de estoque'
      "
      modal
      class="w-full max-w-lg"
    >
      <Message v-if="error" severity="error" :closable="false" class="mb-3 whitespace-pre-wrap">{{ error }}</Message>
      <div class="grid gap-3">
        <div>
          <label class="mb-1 block text-sm">Produto</label>
          <Dropdown
            v-model="form.productId"
            :options="productOptions"
            option-label="label"
            option-value="value"
            placeholder="Selecione"
            class="w-full"
            filter
          />
        </div>
        <div>
          <label class="mb-1 block text-sm">Quantidade</label>
          <InputNumber v-model="form.quantity" class="w-full" :min="1" />
        </div>
        <div v-if="movementType === 'purchase'">
          <label class="mb-1 block text-sm">Custo unitário</label>
          <InputNumber
            v-model="form.unitCost"
            class="w-full"
            mode="currency"
            currency="BRL"
            locale="pt-BR"
            :min="0"
          />
        </div>
        <div>
          <label class="mb-1 block text-sm">Motivo</label>
          <InputText v-model="form.reason" class="w-full" />
        </div>
      </div>
      <template #footer>
        <Button label="Cancelar" text @click="dialogVisible = false" />
        <Button label="Confirmar" :loading="loading" @click="submit" />
      </template>
    </Dialog>
  </div>
</template>

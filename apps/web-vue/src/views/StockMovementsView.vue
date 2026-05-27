<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import Button from 'primevue/button'
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

const auth = useAuthStore()
const module = createCatalogModule(() => auth.token)

const products = ref<ProductEntity[]>([])
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

const userId = computed(() => auth.user?.legacyUserId ?? 1)

async function loadProducts() {
  const result = await module.listProductsUseCase.execute({ pageSize: 200, isActive: true })
  if (result.ok) products.value = result.data.items
}

function openMovement(type: 'entry' | 'exit' | 'purchase') {
  movementType.value = type
  form.value = { productId: null, quantity: 1, reason: '', unitCost: 0 }
  error.value = ''
  success.value = ''
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
    await loadProducts()
  } finally {
    loading.value = false
  }
}

onMounted(loadProducts)
</script>

<template>
  <div class="space-y-4 p-6">
    <h1 class="text-xl font-semibold">Movimentação de estoque</h1>
    <Message v-if="success" severity="success" :closable="false">{{ success }}</Message>
    <Message v-if="error && !dialogVisible" severity="error" :closable="false">{{ error }}</Message>

    <TabView>
      <TabPanel value="entry" header="Entrada manual">
        <p class="mb-4 text-sm text-gray-600">Registre entrada de estoque (RF-040).</p>
        <Button label="Nova entrada" icon="pi pi-plus" @click="openMovement('entry')" />
      </TabPanel>
      <TabPanel value="exit" header="Saída manual">
        <p class="mb-4 text-sm text-gray-600">Registre saída de estoque (RF-041).</p>
        <Button label="Nova saída" icon="pi pi-minus" @click="openMovement('exit')" />
      </TabPanel>
      <TabPanel value="purchase" header="Compra">
        <p class="mb-4 text-sm text-gray-600">Compra de mercadoria com evento ProductPurchased (RF-042).</p>
        <Button label="Registrar compra" icon="pi pi-shopping-cart" @click="openMovement('purchase')" />
      </TabPanel>
    </TabView>

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
      <Message v-if="error" severity="error" :closable="false" class="mb-3">{{ error }}</Message>
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

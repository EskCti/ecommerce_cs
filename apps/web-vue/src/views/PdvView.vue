<script setup lang="ts">
import { computed, nextTick, onMounted, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'
import Dropdown from 'primevue/dropdown'
import InputText from 'primevue/inputtext'
import InputNumber from 'primevue/inputnumber'
import Password from 'primevue/password'
import Message from 'primevue/message'
import { useAuthStore } from '@/stores/auth'
import { createSalesModule } from '@/modules/sales/composition'
import { createStoreSettingsModule } from '@/modules/store-settings/composition'
import { createUsersModule } from '@/modules/users/composition'
import { createCrmModule } from '@/modules/crm/composition'
import { createCatalogModule } from '@/modules/catalog/composition'
import type { CashSessionEntity, CartLineEntity } from '@/modules/sales/domain/cash-session.entity'
import type { CashRegisterTerminalEntity } from '@/modules/store-settings/domain/cash-register-terminal.entity'
import type { PaymentMethodEntity } from '@/modules/store-settings/domain/payment-method.entity'
import type { TenantUserEntity } from '@/modules/users/domain/tenant-user.entity'
import type { CustomerEntity } from '@/modules/crm/domain/customer.entity'
import type {
  GradeConfiguration,
  GradeDimension,
  GradeVariant,
} from '@/modules/catalog/application/grade.repository'

const router = useRouter()
const auth = useAuthStore()
const sales = createSalesModule(() => auth.token)
const storeSettings = createStoreSettingsModule(() => auth.token)
const usersModule = createUsersModule(() => auth.token)
const crm = createCrmModule(() => auth.token)
const catalog = createCatalogModule(() => auth.token)

const session = ref<CashSessionEntity | null>(null)
const terminals = ref<CashRegisterTerminalEntity[]>([])
const paymentMethods = ref<PaymentMethodEntity[]>([])
const managers = ref<TenantUserEntity[]>([])
const customers = ref<CustomerEntity[]>([])
const error = ref('')
const loading = ref(false)
const scanValue = ref('')
const SCAN_INPUT_ID = 'pdv-scan-input'

const openDialogVisible = ref(false)
const checkoutDialogVisible = ref(false)
const withdrawalDialogVisible = ref(false)
const closeDialogVisible = ref(false)
const gradeDialogVisible = ref(false)

const openForm = ref({
  terminalId: null as string | null,
  openingFloat: 100,
  managerUserId: null as string | null,
  managerPin: '',
})

const checkoutForm = ref({
  paymentMethodId: null as string | null,
  paymentTerms: 'Cash' as 'Cash' | 'Credit',
  customerId: null as string | null,
  amountPaid: 0,
  discountAmount: 0,
})

const withdrawalForm = ref({ amount: 0 })

const closeForm = ref({
  managerUserId: null as string | null,
  managerPin: '',
  countedCash: 0,
})

const gradeLine = ref<CartLineEntity | null>(null)
const gradeConfig = ref<GradeConfiguration>({ dimensions: [], variants: [] })
const gradeDimensions = computed(() => gradeConfig.value.dimensions)
const gradeVariants = computed(() => gradeConfig.value.variants)
const selectedGradeOptions = ref<Record<string, string | null>>({})
const selectedGradeVariantId = ref<string | null>(null)
const selectedVariantStock = ref<number | null>(null)
const gradesLoading = ref(false)

const usesCombinationStock = computed(() => gradeVariants.value.length > 0)

const paymentTermsOptions = [
  { label: 'À vista', value: 'Cash' },
  { label: 'Fiado', value: 'Credit' },
]

const openTerminalOptions = computed(() =>
  terminals.value
    .filter((t) => t.status === 'Open')
    .map((t) => ({ label: t.name, value: t.id })),
)

const managerOptions = computed(() =>
  managers.value.map((u) => ({ label: u.name, value: u.id })),
)

const paymentMethodOptions = computed(() =>
  paymentMethods.value
    .filter((m) => m.isActive)
    .map((m) => ({ label: m.name, value: m.id })),
)

const customerOptions = computed(() =>
  customers.value.map((c) => ({ label: `${c.name} (${c.cpf})`, value: c.id })),
)

const cartSubtotal = computed(() => session.value?.cartSubtotal ?? 0)
const checkoutTotal = computed(() =>
  Math.max(0, cartSubtotal.value - (checkoutForm.value.discountAmount || 0)),
)
const expectedChange = computed(() =>
  Math.max(0, (checkoutForm.value.amountPaid || 0) - checkoutTotal.value),
)

function formatMoney(value: number): string {
  return value.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
}

async function focusScanner() {
  await nextTick()
  const root = document.getElementById(SCAN_INPUT_ID)
  const input = root?.querySelector('input') as HTMLInputElement | null
  input?.focus()
}

async function loadSession() {
  const result = await sales.getCurrentCashSessionUseCase.execute()
  if (!result.ok) {
    error.value = result.error
    return
  }
  session.value = result.data
  if (result.data?.pendingGradeLine) {
    await openGradeDialog(result.data.pendingGradeLine)
  }
}

async function loadSupportData() {
  const [terminalsResult, paymentsResult, usersResult, customersResult] = await Promise.all([
    storeSettings.listCashRegistersUseCase.execute(),
    storeSettings.listPaymentMethodsUseCase.execute(),
    usersModule.listUsersUseCase.execute(),
    crm.listCustomersUseCase.execute({ page: 1, pageSize: 200 }),
  ])
  if (terminalsResult.ok) terminals.value = terminalsResult.data
  if (paymentsResult.ok) paymentMethods.value = paymentsResult.data
  if (usersResult.ok) managers.value = usersResult.data
  if (customersResult.ok) customers.value = [...customersResult.data.items]
}

async function init() {
  error.value = ''
  loading.value = true
  try {
    await loadSupportData()
    await loadSession()
    if (!session.value) openDialogVisible.value = true
    await focusScanner()
  } finally {
    loading.value = false
  }
}

async function onScan() {
  if (!session.value) {
    openDialogVisible.value = true
    return
  }
  const value = scanValue.value.trim()
  if (!value) return
  error.value = ''
  loading.value = true
  scanValue.value = ''
  try {
    const result = await sales.addCartItemUseCase.execute({ scannedValue: value })
    if (!result.ok) {
      error.value = result.error
      return
    }
    session.value = result.data
    if (result.data.pendingGradeLine) {
      await openGradeDialog(result.data.pendingGradeLine)
    }
  } finally {
    loading.value = false
    await focusScanner()
  }
}

async function removeLine(line: CartLineEntity) {
  error.value = ''
  loading.value = true
  try {
    const result = await sales.removeCartLineUseCase.execute(line.id)
    if (!result.ok) {
      error.value = result.error
      return
    }
    session.value = result.data
  } finally {
    loading.value = false
    await focusScanner()
  }
}

async function openSession() {
  error.value = ''
  if (!openForm.value.terminalId || !openForm.value.managerUserId || !openForm.value.managerPin) {
    error.value = 'Terminal, gerente e PIN são obrigatórios'
    return
  }
  loading.value = true
  try {
    const result = await sales.openCashSessionUseCase.execute({
      terminalId: openForm.value.terminalId,
      managerUserId: openForm.value.managerUserId,
      managerPin: openForm.value.managerPin,
      openingFloat: Number(openForm.value.openingFloat) || 0,
    })
    if (!result.ok) {
      error.value = result.error
      return
    }
    session.value = result.data
    openDialogVisible.value = false
    openForm.value.managerPin = ''
    await focusScanner()
  } finally {
    loading.value = false
  }
}

function openCheckout() {
  if (!session.value || session.value.lines.length === 0) {
    error.value = 'Carrinho vazio'
    return
  }
  if (session.value.pendingGradeLine) {
    error.value = 'Confirme a grade pendente antes de finalizar'
    return
  }
  checkoutForm.value = {
    paymentMethodId: paymentMethodOptions.value[0]?.value ?? null,
    paymentTerms: 'Cash',
    customerId: null,
    amountPaid: checkoutTotal.value,
    discountAmount: 0,
  }
  checkoutDialogVisible.value = true
}

watch(checkoutTotal, (total) => {
  if (checkoutForm.value.paymentTerms === 'Cash' && checkoutDialogVisible.value) {
    checkoutForm.value.amountPaid = total
  }
})

async function finalizeSale() {
  error.value = ''
  if (!checkoutForm.value.paymentMethodId) {
    error.value = 'Selecione a forma de pagamento'
    return
  }
  if (checkoutForm.value.paymentTerms === 'Credit' && !checkoutForm.value.customerId) {
    error.value = 'Cliente obrigatório para venda fiado'
    return
  }
  loading.value = true
  try {
    const result = await sales.finalizeSaleUseCase.execute({
      paymentMethodId: checkoutForm.value.paymentMethodId,
      paymentTerms: checkoutForm.value.paymentTerms,
      customerId: checkoutForm.value.customerId ?? undefined,
      amountPaid: Number(checkoutForm.value.amountPaid) || 0,
      discountAmount: Number(checkoutForm.value.discountAmount) || 0,
    })
    if (!result.ok) {
      error.value = result.error
      return
    }
    checkoutDialogVisible.value = false
    await loadSession()
    if (!session.value) openDialogVisible.value = true
  } finally {
    loading.value = false
    await focusScanner()
  }
}

async function submitWithdrawal() {
  error.value = ''
  loading.value = true
  try {
    const result = await sales.registerCashWithdrawalUseCase.execute({
      amount: Number(withdrawalForm.value.amount) || 0,
    })
    if (!result.ok) {
      error.value = result.error
      return
    }
    session.value = result.data
    withdrawalDialogVisible.value = false
    withdrawalForm.value.amount = 0
  } finally {
    loading.value = false
  }
}

async function submitCloseSession() {
  error.value = ''
  if (!closeForm.value.managerUserId || !closeForm.value.managerPin) {
    error.value = 'Gerente e PIN são obrigatórios'
    return
  }
  loading.value = true
  try {
    const result = await sales.closeCashSessionUseCase.execute({
      managerUserId: closeForm.value.managerUserId,
      managerPin: closeForm.value.managerPin,
      countedCash: Number(closeForm.value.countedCash) || 0,
    })
    if (!result.ok) {
      error.value = result.error
      return
    }
    session.value = null
    closeDialogVisible.value = false
    closeForm.value.managerPin = ''
    openDialogVisible.value = true
  } finally {
    loading.value = false
  }
}

async function openGradeDialog(line: CartLineEntity) {
  gradeLine.value = line
  selectedGradeOptions.value = {}
  gradesLoading.value = true
  gradeDialogVisible.value = true
  try {
    const result = await catalog.listGradesUseCase.execute(line.productId)
    if (!result.ok) {
      error.value = result.error
      gradeConfig.value = { dimensions: [], variants: [] }
      return
    }
    gradeConfig.value = result.data
    selectedGradeVariantId.value = null
    for (const dim of result.data.dimensions) {
      selectedGradeOptions.value[dim.id] = dim.options[0]?.id ?? null
    }
    syncSelectedVariant()
  } finally {
    gradesLoading.value = false
  }
}

function syncSelectedVariant() {
  const optionIds = Object.values(selectedGradeOptions.value).filter(
    (id): id is string => !!id,
  )
  const match = gradeVariants.value.find((v) => {
    const sorted = [...v.optionIds].sort().join(',')
    const selected = [...optionIds].sort().join(',')
    return sorted === selected && sorted.length > 0
  })
  selectedGradeVariantId.value = match?.id ?? null
  selectedVariantStock.value = match?.stock ?? null
}

watch(selectedGradeOptions, syncSelectedVariant, { deep: true })

async function confirmGrade() {
  if (!gradeLine.value) return
  const gradeOptionIds = Object.values(selectedGradeOptions.value).filter(
    (id): id is string => !!id,
  )
  if (gradeOptionIds.length !== gradeDimensions.value.length) {
    error.value = 'Selecione uma opção em cada dimensão'
    return
  }
  syncSelectedVariant()
  if (usesCombinationStock.value && !selectedGradeVariantId.value) {
    error.value =
      'Combinação não cadastrada. Cadastre a combinação com estoque em Variações (grade) no produto.'
    return
  }
  const qty = gradeLine.value.quantity
  if (selectedVariantStock.value !== null && selectedVariantStock.value < qty) {
    error.value = `Estoque insuficiente para esta combinação. Disponível: ${selectedVariantStock.value}.`
    return
  }
  loading.value = true
  try {
    const result = await sales.confirmGradeForItemUseCase.execute({
      lineId: gradeLine.value.id,
      gradeOptionIds,
      gradeVariantId: selectedGradeVariantId.value ?? undefined,
    })
    if (!result.ok) {
      error.value = result.error
      return
    }
    session.value = result.data
    gradeDialogVisible.value = false
    gradeLine.value = null
    if (result.data.pendingGradeLine) {
      await openGradeDialog(result.data.pendingGradeLine)
    }
  } finally {
    loading.value = false
    await focusScanner()
  }
}

function goToSalesList() {
  router.push('/tenant/sales')
}

function logout() {
  auth.logout()
  router.push('/login')
}

onMounted(init)
</script>

<template>
  <div class="flex min-h-screen flex-col bg-background text-foreground">
    <header class="flex items-center justify-between border-b border-border bg-card px-4 py-3">
      <div>
        <h1 class="text-lg font-semibold">PDV</h1>
        <p v-if="session" class="text-sm text-muted-foreground">
          Caixa aberto · {{ formatMoney(session.expectedCashInDrawer) }} esperado no gaveta
        </p>
        <p v-else class="text-sm text-muted-foreground">Nenhuma sessão aberta</p>
      </div>
      <div class="flex flex-wrap gap-2">
        <Button label="Vendas" icon="pi pi-list" text @click="goToSalesList" />
        <Button
          v-if="session"
          label="Sangria"
          icon="pi pi-wallet"
          severity="secondary"
          @click="withdrawalDialogVisible = true"
        />
        <Button
          v-if="session"
          label="Fechar caixa"
          icon="pi pi-lock"
          severity="warning"
          @click="
            closeForm.countedCash = session?.expectedCashInDrawer ?? 0;
            closeDialogVisible = true
          "
        />
        <Button v-if="!session" label="Abrir caixa" icon="pi pi-unlock" @click="openDialogVisible = true" />
        <Button label="Sair" icon="pi pi-sign-out" text severity="danger" @click="logout" />
      </div>
    </header>

    <main class="flex flex-1 flex-col gap-4 p-4 lg:flex-row">
      <section class="flex flex-1 flex-col gap-3">
        <Message v-if="error" severity="error" :closable="false" class="whitespace-pre-wrap">{{ error }}</Message>
        <div class="rounded-lg border border-border bg-card p-4">
          <label class="mb-2 block text-sm font-medium">Leitura de código de barras</label>
          <InputText
            :id="SCAN_INPUT_ID"
            v-model="scanValue"
            class="w-full"
            placeholder="Passe o código ou digite e pressione Enter"
            :disabled="!session || loading"
            autofocus
            @keyup.enter="onScan"
          />
        </div>
        <DataTable
          :value="session?.lines ?? []"
          data-key="id"
          :loading="loading"
          class="rounded-lg border border-border bg-card"
          empty-message="Carrinho vazio"
        >
          <Column field="barcode" header="Código" />
          <Column field="quantity" header="Qtd" />
          <Column header="Unitário">
            <template #body="{ data }">
              {{ formatMoney((data as CartLineEntity).unitPrice) }}
            </template>
          </Column>
          <Column header="Total">
            <template #body="{ data }">
              {{ formatMoney((data as CartLineEntity).lineTotal) }}
            </template>
          </Column>
          <Column header="Status">
            <template #body="{ data }">
              <span
                :class="
                  (data as CartLineEntity).isPendingGrade ? 'text-amber-400' : 'text-muted-foreground'
                "
              >
                {{ (data as CartLineEntity).status }}
              </span>
            </template>
          </Column>
          <Column header="">
            <template #body="{ data }">
              <div class="flex gap-1">
                <Button
                  v-if="(data as CartLineEntity).isPendingGrade"
                  icon="pi pi-check"
                  text
                  @click="openGradeDialog(data as CartLineEntity)"
                />
                <Button
                  icon="pi pi-trash"
                  text
                  severity="danger"
                  @click="removeLine(data as CartLineEntity)"
                />
              </div>
            </template>
          </Column>
        </DataTable>
      </section>

      <aside class="w-full shrink-0 space-y-3 rounded-lg border border-border bg-card p-4 lg:w-80">
        <h2 class="text-sm font-semibold uppercase tracking-wide text-muted-foreground">Totais</h2>
        <div class="space-y-2 text-sm">
          <div class="flex justify-between">
            <span>Subtotal carrinho</span>
            <span class="font-medium">{{ formatMoney(cartSubtotal) }}</span>
          </div>
          <div v-if="session" class="flex justify-between">
            <span>Vendido na sessão</span>
            <span>{{ formatMoney(session.totalSold) }}</span>
          </div>
          <div v-if="session" class="flex justify-between">
            <span>Sangrias</span>
            <span>{{ formatMoney(session.totalWithdrawals) }}</span>
          </div>
          <div v-if="session" class="flex justify-between border-t border-border pt-2">
            <span>Fundo + vendas − sangrias</span>
            <span class="font-semibold text-primary">
              {{ formatMoney(session.expectedCashInDrawer) }}
            </span>
          </div>
        </div>
        <Button
          label="Finalizar venda"
          icon="pi pi-shopping-cart"
          class="w-full"
          :disabled="!session || !session.lines.length || loading"
          @click="openCheckout"
        />
      </aside>
    </main>

    <Dialog v-model:visible="openDialogVisible" header="Abrir caixa" modal :closable="!!session" class="w-full max-w-md">
      <div class="space-y-3">
        <div>
          <label class="mb-1 block text-sm">Terminal (status aberto)</label>
          <Dropdown
            v-model="openForm.terminalId"
            :options="openTerminalOptions"
            option-label="label"
            option-value="value"
            placeholder="Selecione o caixa"
            class="w-full"
          />
        </div>
        <div>
          <label class="mb-1 block text-sm">Fundo de troco</label>
          <InputNumber v-model="openForm.openingFloat" mode="currency" currency="BRL" locale="pt-BR" class="w-full" />
        </div>
        <div>
          <label class="mb-1 block text-sm">Gerente</label>
          <Dropdown
            v-model="openForm.managerUserId"
            :options="managerOptions"
            option-label="label"
            option-value="value"
            placeholder="Selecione o gerente"
            class="w-full"
          />
        </div>
        <div>
          <label class="mb-1 block text-sm">PIN do gerente</label>
          <Password v-model="openForm.managerPin" :feedback="false" toggle-mask input-class="w-full" class="w-full" />
        </div>
      </div>
      <template #footer>
        <Button v-if="session" label="Cancelar" text @click="openDialogVisible = false" />
        <Button label="Abrir" :loading="loading" @click="openSession" />
      </template>
    </Dialog>

    <Dialog v-model:visible="checkoutDialogVisible" header="Finalizar venda" modal class="w-full max-w-lg">
      <div class="space-y-3">
        <div>
          <label class="mb-1 block text-sm">Forma de pagamento</label>
          <Dropdown
            v-model="checkoutForm.paymentMethodId"
            :options="paymentMethodOptions"
            option-label="label"
            option-value="value"
            class="w-full"
          />
        </div>
        <div>
          <label class="mb-1 block text-sm">Condição</label>
          <Dropdown
            v-model="checkoutForm.paymentTerms"
            :options="paymentTermsOptions"
            option-label="label"
            option-value="value"
            class="w-full"
          />
        </div>
        <div v-if="checkoutForm.paymentTerms === 'Credit'">
          <label class="mb-1 block text-sm">Cliente (fiado)</label>
          <Dropdown
            v-model="checkoutForm.customerId"
            :options="customerOptions"
            option-label="label"
            option-value="value"
            filter
            class="w-full"
          />
        </div>
        <div>
          <label class="mb-1 block text-sm">Desconto</label>
          <InputNumber
            v-model="checkoutForm.discountAmount"
            mode="currency"
            currency="BRL"
            locale="pt-BR"
            class="w-full"
          />
        </div>
        <div class="rounded-md bg-muted p-3 text-sm">
          <div class="flex justify-between">
            <span>Total</span>
            <span class="font-semibold">{{ formatMoney(checkoutTotal) }}</span>
          </div>
          <div v-if="checkoutForm.paymentTerms === 'Cash'" class="mt-2 flex justify-between">
            <span>Troco</span>
            <span>{{ formatMoney(expectedChange) }}</span>
          </div>
        </div>
        <div v-if="checkoutForm.paymentTerms === 'Cash'">
          <label class="mb-1 block text-sm">Valor pago</label>
          <InputNumber
            v-model="checkoutForm.amountPaid"
            mode="currency"
            currency="BRL"
            locale="pt-BR"
            class="w-full"
          />
        </div>
      </div>
      <template #footer>
        <Button label="Cancelar" text @click="checkoutDialogVisible = false" />
        <Button label="Confirmar" :loading="loading" @click="finalizeSale" />
      </template>
    </Dialog>

    <Dialog v-model:visible="withdrawalDialogVisible" header="Sangria" modal class="w-full max-w-sm">
      <div>
        <label class="mb-1 block text-sm">Valor</label>
        <InputNumber v-model="withdrawalForm.amount" mode="currency" currency="BRL" locale="pt-BR" class="w-full" />
      </div>
      <template #footer>
        <Button label="Cancelar" text @click="withdrawalDialogVisible = false" />
        <Button label="Registrar" :loading="loading" @click="submitWithdrawal" />
      </template>
    </Dialog>

    <Dialog v-model:visible="closeDialogVisible" header="Fechar caixa" modal class="w-full max-w-md">
      <div class="space-y-3">
        <p class="text-sm text-muted-foreground">
          Esperado: {{ formatMoney(session?.expectedCashInDrawer ?? 0) }}
        </p>
        <div>
          <label class="mb-1 block text-sm">Dinheiro contado</label>
          <InputNumber v-model="closeForm.countedCash" mode="currency" currency="BRL" locale="pt-BR" class="w-full" />
        </div>
        <div>
          <label class="mb-1 block text-sm">Gerente</label>
          <Dropdown
            v-model="closeForm.managerUserId"
            :options="managerOptions"
            option-label="label"
            option-value="value"
            class="w-full"
          />
        </div>
        <div>
          <label class="mb-1 block text-sm">PIN do gerente</label>
          <Password v-model="closeForm.managerPin" :feedback="false" toggle-mask input-class="w-full" class="w-full" />
        </div>
      </div>
      <template #footer>
        <Button label="Cancelar" text @click="closeDialogVisible = false" />
        <Button label="Fechar caixa" severity="warning" :loading="loading" @click="submitCloseSession" />
      </template>
    </Dialog>

    <Dialog
      v-model:visible="gradeDialogVisible"
      header="Confirmar grade"
      modal
      :closable="false"
      class="w-full max-w-lg"
    >
      <div v-if="gradesLoading" class="text-sm text-muted-foreground">Carregando opções…</div>
      <div v-else class="space-y-4">
        <p class="text-sm">Produto: {{ gradeLine?.barcode }}</p>
        <div v-for="dim in gradeDimensions" :key="dim.id" class="space-y-1">
          <label class="block text-sm font-medium">{{ dim.name }}</label>
          <Dropdown
            v-model="selectedGradeOptions[dim.id]"
            :options="dim.options.map((o) => ({ label: o.label, value: o.id }))"
            option-label="label"
            option-value="value"
            class="w-full"
          />
        </div>
        <p
          v-if="usesCombinationStock && selectedGradeVariantId"
          class="text-sm text-muted-foreground"
        >
          Estoque da combinação selecionada:
          <span class="font-medium text-foreground">{{ selectedVariantStock ?? 0 }}</span>
        </p>
        <p
          v-else-if="usesCombinationStock && !gradesLoading"
          class="text-sm text-amber-600"
        >
          Selecione uma combinação cadastrada no produto (Variações / grade).
        </p>
      </div>
      <template #footer>
        <Button label="Confirmar" :loading="loading || gradesLoading" @click="confirmGrade" />
      </template>
    </Dialog>
  </div>
</template>

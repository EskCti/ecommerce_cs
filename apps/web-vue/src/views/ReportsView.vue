<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'
import Calendar from 'primevue/calendar'
import Dropdown from 'primevue/dropdown'
import Message from 'primevue/message'
import { useAuthStore } from '@/stores/auth'
import { createReportingModule } from '@/modules/reporting/composition'
import { createCrmModule } from '@/modules/crm/composition'
import { createUsersModule } from '@/modules/users/composition'
import {
  REPORT_CARDS,
  type ReportCardData,
  type ReportFilterInput,
  type ReportKind,
} from '@/modules/reporting/domain/report.entity'
import { triggerBlobDownload } from '@/modules/reporting/application/reporting.usecase'
import type { CustomerEntity } from '@/modules/crm/domain/customer.entity'
import type { TenantUserEntity } from '@/modules/users/domain/tenant-user.entity'

const auth = useAuthStore()
const reporting = createReportingModule(() => auth.token)
const crm = createCrmModule(() => auth.token)
const usersModule = createUsersModule(() => auth.token)

const error = ref('')
const success = ref('')
const loading = ref(false)
const filterDialogVisible = ref(false)
const activeReport = ref<ReportCardData | null>(null)
const customers = ref<CustomerEntity[]>([])
const sellers = ref<TenantUserEntity[]>([])

const filterForm = ref({
  from: null as Date | null,
  to: null as Date | null,
  customerId: null as string | null,
  sellerId: null as string | null,
  status: null as string | null,
})

const statusOptions = [
  { label: 'Todos', value: null },
  { label: 'Pago', value: 'Pago' },
  { label: 'Aberto', value: 'Aberto' },
]

const customerOptions = computed(() =>
  customers.value.map((c) => ({ label: `${c.name} (${c.cpf})`, value: c.id })),
)

const sellerOptions = computed(() =>
  sellers.value.map((u) => ({ label: u.name, value: u.id })),
)

function toIsoDate(value: Date | null): string | undefined {
  if (!value) return undefined
  return value.toISOString().slice(0, 10)
}

function buildFilter(): ReportFilterInput {
  return {
    from: toIsoDate(filterForm.value.from),
    to: toIsoDate(filterForm.value.to),
    customerId: filterForm.value.customerId ?? undefined,
    sellerId: filterForm.value.sellerId ?? undefined,
    status: filterForm.value.status ?? undefined,
  }
}

function openFilters(card: ReportCardData) {
  activeReport.value = card
  const today = new Date()
  const monthStart = new Date(today.getFullYear(), today.getMonth(), 1)
  filterForm.value = {
    from: card.requiresDateRange ? monthStart : null,
    to: card.requiresDateRange ? today : null,
    customerId: null,
    sellerId: null,
    status: card.id === 'sales' ? null : null,
  }
  filterDialogVisible.value = true
}

async function exportReport(kind: ReportKind, filter: ReportFilterInput, fileName: string) {
  error.value = ''
  success.value = ''
  loading.value = true
  try {
    const result = await reporting.downloadReportUseCase.execute(kind, filter)
    if (!result.ok) {
      error.value = result.error
      return
    }
    triggerBlobDownload(result.data, `${fileName}.pdf`)
    success.value = 'Download iniciado'
    filterDialogVisible.value = false
  } finally {
    loading.value = false
  }
}

async function confirmExport() {
  if (!activeReport.value) return
  const filter = buildFilter()
  if (activeReport.value.requiresDateRange && (!filter.from || !filter.to)) {
    error.value = 'Informe o período (de/até)'
    return
  }
  await exportReport(activeReport.value.id, filter, activeReport.value.id)
}

async function quickExport(card: ReportCardData) {
  if (card.requiresDateRange) {
    openFilters(card)
    return
  }
  await exportReport(card.id, {}, card.id)
}

async function loadSupportData() {
  const [customersResult, usersResult] = await Promise.all([
    crm.listCustomersUseCase.execute({ page: 1, pageSize: 200 }),
    usersModule.listUsersUseCase.execute(),
  ])
  if (customersResult.ok) customers.value = [...customersResult.data.items]
  if (usersResult.ok) sellers.value = usersResult.data
}

onMounted(loadSupportData)
</script>

<template>
  <div class="space-y-4">
    <div>
      <h1 class="text-2xl font-semibold">Relatórios</h1>
      <p class="text-sm text-muted-foreground">Exportação PDF operacional e financeira (RF-080)</p>
    </div>

    <Message v-if="error" severity="error" :closable="false">{{ error }}</Message>
    <Message v-if="success" severity="success" :closable="false">{{ success }}</Message>

    <div class="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
      <article
        v-for="card in REPORT_CARDS"
        :key="card.id"
        class="flex flex-col rounded-lg border border-border bg-card p-4 shadow-sm"
      >
        <h2 class="text-lg font-medium">{{ card.title }}</h2>
        <p class="mt-2 flex-1 text-sm text-muted-foreground">{{ card.description }}</p>
        <div class="mt-4 flex gap-2">
          <Button
            label="Exportar PDF"
            icon="pi pi-file-pdf"
            :loading="loading"
            @click="quickExport(card)"
          />
          <Button
            v-if="card.requiresDateRange"
            label="Filtros"
            icon="pi pi-filter"
            text
            @click="openFilters(card)"
          />
        </div>
      </article>
    </div>

    <Dialog
      v-model:visible="filterDialogVisible"
      :header="activeReport ? `Filtros — ${activeReport.title}` : 'Filtros'"
      modal
      class="w-full max-w-lg"
    >
      <div class="space-y-3">
        <div class="grid grid-cols-2 gap-3">
          <div>
            <label class="mb-1 block text-sm">De</label>
            <Calendar v-model="filterForm.from" date-format="dd/mm/yy" class="w-full" />
          </div>
          <div>
            <label class="mb-1 block text-sm">Até</label>
            <Calendar v-model="filterForm.to" date-format="dd/mm/yy" class="w-full" />
          </div>
        </div>
        <div v-if="activeReport?.id === 'sales'">
          <label class="mb-1 block text-sm">Cliente</label>
          <Dropdown
            v-model="filterForm.customerId"
            :options="customerOptions"
            option-label="label"
            option-value="value"
            placeholder="Todos"
            show-clear
            filter
            class="w-full"
          />
        </div>
        <div v-if="activeReport?.id === 'sales'">
          <label class="mb-1 block text-sm">Vendedor</label>
          <Dropdown
            v-model="filterForm.sellerId"
            :options="sellerOptions"
            option-label="label"
            option-value="value"
            placeholder="Todos"
            show-clear
            filter
            class="w-full"
          />
        </div>
        <div v-if="activeReport?.id === 'sales'">
          <label class="mb-1 block text-sm">Status pagamento</label>
          <Dropdown
            v-model="filterForm.status"
            :options="statusOptions"
            option-label="label"
            option-value="value"
            class="w-full"
          />
        </div>
      </div>
      <template #footer>
        <Button label="Cancelar" text @click="filterDialogVisible = false" />
        <Button label="Exportar PDF" icon="pi pi-download" :loading="loading" @click="confirmExport" />
      </template>
    </Dialog>
  </div>
</template>

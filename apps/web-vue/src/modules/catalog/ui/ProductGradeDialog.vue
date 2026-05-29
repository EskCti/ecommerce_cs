<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import InputNumber from 'primevue/inputnumber'
import Dropdown from 'primevue/dropdown'
import Message from 'primevue/message'
import { createCatalogModule } from '@/modules/catalog/composition'
import type { ProductEntity } from '@/modules/catalog/domain/product.entity'
import type {
  GradeConfiguration,
  GradeVariant,
} from '@/modules/catalog/application/grade.repository'

const props = defineProps<{
  visible: boolean
  product: ProductEntity | null
  getToken: () => string | null
}>()

const emit = defineEmits<{
  'update:visible': [value: boolean]
}>()

const module = createCatalogModule(() => props.getToken())

const gradesLoading = ref(false)
const error = ref('')
const gradeConfig = ref<GradeConfiguration>({ dimensions: [], variants: [] })
const grades = computed(() => gradeConfig.value.dimensions)
const gradeVariants = computed(() => gradeConfig.value.variants)
const isTwoDimensionalGrade = computed(() => grades.value.length >= 2)
const canManageCombinations = computed(() =>
  grades.value.length > 0 && grades.value.every((d) => d.options.length > 0),
)

const gradeForm = ref({
  dimensionName: '',
  optionLabel: '',
  selectedDimensionId: null as string | null,
})

const combinationStock = ref(0)
const combinationSelection = ref<Record<string, string | null>>({})

const dialogVisible = computed({
  get: () => props.visible,
  set: (value: boolean) => emit('update:visible', value),
})

function resetCombinationSelection() {
  const next: Record<string, string | null> = {}
  for (const dim of grades.value) {
    next[dim.id] = combinationSelection.value[dim.id] ?? null
  }
  combinationSelection.value = next
}

function optionIdsKey(ids: readonly string[]): string {
  return [...ids].sort().join(',')
}

function isDuplicateCombination(optionIds: string[]): boolean {
  const key = optionIdsKey(optionIds)
  return gradeVariants.value.some((v) => optionIdsKey(v.optionIds) === key)
}

function optionUsedInCombination(optionId: string): boolean {
  return gradeVariants.value.some((v) => v.optionIds.includes(optionId))
}

watch(
  () => [props.visible, props.product?.id] as const,
  async ([visible, productId]) => {
    if (!visible || !productId || !props.product) return
    await loadGrades()
  },
)

watch(grades, () => resetCombinationSelection())

async function loadGrades() {
  if (!props.product) return
  error.value = ''
  gradesLoading.value = true
  gradeForm.value = { dimensionName: '', optionLabel: '', selectedDimensionId: null }
  combinationStock.value = 0
  try {
    const result = await module.listGradesUseCase.execute(props.product.id)
    if (!result.ok) {
      error.value = result.error
      gradeConfig.value = { dimensions: [], variants: [] }
      return
    }
    gradeConfig.value = result.data
    if (result.data.dimensions.length > 0) {
      gradeForm.value.selectedDimensionId = result.data.dimensions[0].id
    }
    resetCombinationSelection()
  } finally {
    gradesLoading.value = false
  }
}

async function runConfigure(
  input: Parameters<typeof module.configureGradeUseCase.execute>[1],
) {
  if (!props.product) return
  gradesLoading.value = true
  try {
    const result = await module.configureGradeUseCase.execute(props.product.id, input)
    if (!result.ok) {
      error.value = result.error
      return
    }
    gradeConfig.value = result.data
    error.value = ''
  } finally {
    gradesLoading.value = false
  }
}

async function addDimension() {
  if (!gradeForm.value.dimensionName.trim()) return
  await runConfigure({
    action: 'AddDimension',
    dimensionName: gradeForm.value.dimensionName.trim(),
  })
  gradeForm.value.dimensionName = ''
  const dims = gradeConfig.value.dimensions
  if (dims.length > 0) gradeForm.value.selectedDimensionId = dims[dims.length - 1].id
}

async function addOption() {
  if (!gradeForm.value.selectedDimensionId || !gradeForm.value.optionLabel.trim()) return
  await runConfigure({
    action: 'AddOption',
    dimensionId: gradeForm.value.selectedDimensionId,
    optionLabel: gradeForm.value.optionLabel.trim(),
    stock: 0,
  })
  gradeForm.value.optionLabel = ''
}

async function addCombination() {
  const optionIds = grades.value
    .map((d) => combinationSelection.value[d.id])
    .filter((id): id is string => !!id)

  if (optionIds.length !== grades.value.length) {
    error.value = 'Selecione uma opção em cada dimensão'
    return
  }

  if (isDuplicateCombination(optionIds)) {
    error.value = 'Esta combinação já está cadastrada'
    return
  }

  await runConfigure({
    action: 'AddVariant',
    optionIds,
    stock: Number(combinationStock.value) || 0,
  })

  for (const dim of grades.value) {
    combinationSelection.value[dim.id] = null
  }
  combinationStock.value = 0
}

async function updateVariantStock(variant: GradeVariant, stock: number) {
  await runConfigure({
    action: 'AdjustVariantStock',
    variantId: variant.id,
    stock: Number(stock) || 0,
  })
}

async function removeVariant(variant: GradeVariant) {
  await runConfigure({
    action: 'RemoveVariant',
    variantId: variant.id,
  })
}

async function removeOption(optionId: string) {
  if (optionUsedInCombination(optionId)) {
    error.value = 'Exclua as combinações que usam esta opção antes de removê-la.'
    return
  }

  await runConfigure({
    action: 'RemoveGrade',
    optionId,
  })
}

function dimensionOptionChoices(dimensionId: string) {
  const dim = grades.value.find((d) => d.id === dimensionId)
  return (dim?.options ?? []).map((o) => ({ label: o.label, value: o.id }))
}
</script>

<template>
  <Dialog
    v-model:visible="dialogVisible"
    :header="product ? `Variações (grade) — ${product.name}` : 'Variações (grade)'"
    modal
    class="w-full max-w-3xl"
  >
    <Message v-if="error" severity="error" :closable="false" class="mb-3">{{ error }}</Message>
    <p v-if="product" class="mb-4 text-sm text-muted-foreground">
      Cadastre dimensões e opções (Cor, Tamanho…). Depois inclua cada combinação com estoque na lista abaixo.
    </p>
    <div v-if="gradesLoading" class="text-sm text-gray-500">Carregando...</div>
    <div v-else class="space-y-5">
      <section class="space-y-3">
        <h3 class="text-sm font-semibold">Dimensões e opções</h3>
        <div v-for="dim in grades" :key="dim.id" class="rounded border p-3">
          <p class="font-medium">{{ dim.name }}</p>
          <ul class="mt-2 space-y-1 text-sm">
            <li
              v-for="opt in dim.options"
              :key="opt.id"
              class="flex items-center justify-between gap-2 rounded px-1 py-0.5 hover:bg-gray-50"
            >
              <span>{{ opt.label }}</span>
              <Button
                icon="pi pi-trash"
                text
                severity="danger"
                size="small"
                :disabled="optionUsedInCombination(opt.id)"
                :title="
                  optionUsedInCombination(opt.id)
                    ? 'Opção usada em combinação — exclua a combinação antes'
                    : 'Excluir opção'
                "
                @click="removeOption(opt.id)"
              />
            </li>
            <li v-if="dim.options.length === 0" class="text-gray-500">Nenhuma opção</li>
          </ul>
        </div>
        <div class="grid gap-2 sm:grid-cols-2">
          <div>
            <label class="mb-1 block text-sm">Nova dimensão</label>
            <InputText v-model="gradeForm.dimensionName" class="w-full" placeholder="Cor, Tamanho…" />
          </div>
          <div class="flex items-end">
            <Button label="Adicionar dimensão" size="small" :disabled="grades.length >= 2" @click="addDimension" />
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
          <div class="sm:col-span-2">
            <label class="mb-1 block text-sm">Nova opção</label>
            <div class="flex gap-2">
              <InputText v-model="gradeForm.optionLabel" class="w-full" placeholder="Azul, P, Cx…" />
              <Button label="Adicionar opção" size="small" @click="addOption" />
            </div>
          </div>
        </div>
      </section>

      <section v-if="canManageCombinations" class="space-y-3 rounded border border-dashed p-4">
        <h3 class="text-sm font-semibold">Nova combinação</h3>
        <div class="grid gap-3" :class="isTwoDimensionalGrade ? 'sm:grid-cols-3' : 'sm:grid-cols-2'">
          <div v-for="dim in grades" :key="`pick-${dim.id}`">
            <label class="mb-1 block text-sm">{{ dim.name }}</label>
            <Dropdown
              v-model="combinationSelection[dim.id]"
              :options="dimensionOptionChoices(dim.id)"
              option-label="label"
              option-value="value"
              placeholder="Selecione"
              class="w-full"
            />
          </div>
          <div>
            <label class="mb-1 block text-sm">Estoque</label>
            <InputNumber v-model="combinationStock" class="w-full" :min="0" />
          </div>
          <div class="flex items-end">
            <Button label="Adicionar combinação" icon="pi pi-plus" size="small" @click="addCombination" />
          </div>
        </div>
      </section>

      <section class="space-y-2">
        <h3 class="text-sm font-semibold">Combinações cadastradas</h3>
        <p v-if="!canManageCombinations" class="text-sm text-muted-foreground">
          Adicione ao menos uma opção em cada dimensão para cadastrar combinações.
        </p>
        <div v-else-if="gradeVariants.length === 0" class="rounded border border-dashed p-4 text-sm text-muted-foreground">
          Nenhuma combinação. Use os campos acima para adicionar (ex.: Azul + P com estoque 10).
        </div>
        <div v-else class="overflow-auto rounded border">
          <table class="w-full text-sm">
            <thead>
              <tr class="border-b bg-gray-50 text-left">
                <th class="p-2">Combinação</th>
                <th class="p-2 w-28">Estoque</th>
                <th class="p-2 w-16 text-center">Ações</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="variant in gradeVariants" :key="variant.id" class="border-b">
                <td class="p-2">{{ variant.label }}</td>
                <td class="p-2">
                  <InputNumber
                    :model-value="variant.stock"
                    class="w-full"
                    :min="0"
                    @update:model-value="(v) => updateVariantStock(variant, Number(v) || 0)"
                  />
                </td>
                <td class="p-2 text-center">
                  <Button
                    icon="pi pi-trash"
                    text
                    severity="danger"
                    size="small"
                    title="Excluir combinação"
                    @click="removeVariant(variant)"
                  />
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </div>
    <template #footer>
      <Button label="Fechar" @click="dialogVisible = false" />
    </template>
  </Dialog>
</template>

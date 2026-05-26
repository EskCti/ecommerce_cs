<template>
  <div class="store-config-form">
    <div v-if="error" class="p-message p-message-error mb-4">
      <span class="p-message-icon pi pi-times-circle"></span>
      <span class="p-message-text">{{ error }}</span>
    </div>
    
    <div v-if="success" class="p-message p-message-success mb-4">
      <span class="p-message-icon pi pi-check-circle"></span>
      <span class="p-message-text">Configuração da loja atualizada com sucesso!</span>
    </div>
    
    <form @submit.prevent="handleSubmit">
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <!-- Nome da Loja -->
        <div class="field col-span-2">
          <label for="name" class="block text-sm font-medium mb-1">Nome da Loja *</label>
          <InputText
            id="name"
            v-model="form.name"
            :class="{ 'p-invalid': validationErrors.includes('Nome da loja é obrigatório') || validationErrors.includes('Nome muito longo') }"
            placeholder="Digite o nome da sua loja"
            class="w-full"
          />
          <small v-if="validationErrors.includes('Nome da loja é obrigatório')" class="p-error">
            Nome da loja é obrigatório
          </small>
          <small v-if="validationErrors.includes('Nome muito longo')" class="p-error">
            Nome muito longo (máximo 100 caracteres)
          </small>
        </div>
        
        <!-- Contatos -->
        <div class="field col-span-2">
          <label for="contacts" class="block text-sm font-medium mb-1">Contatos</label>
          <InputText id="contacts" v-model="form.contacts" class="w-full" placeholder="Telefone, e-mail, WhatsApp" />
        </div>

        <!-- Endereço -->
        <div class="field col-span-2">
          <label for="address" class="block text-sm font-medium mb-1">Endereço</label>
          <InputText id="address" v-model="form.address" class="w-full" placeholder="Endereço completo da loja" />
        </div>

        <!-- CNPJ -->
        <div class="field">
          <label for="cnpj" class="block text-sm font-medium mb-1">CNPJ</label>
          <InputText
            id="cnpj"
            v-model="form.cnpj"
            :class="{ 'p-invalid': validationErrors.includes('CNPJ deve estar no formato 00.000.000/0000-00') }"
            placeholder="00.000.000/0000-00"
            class="w-full"
          />
          <small v-if="validationErrors.includes('CNPJ deve estar no formato 00.000.000/0000-00')" class="p-error">
            CNPJ deve estar no formato 00.000.000/0000-00
          </small>
        </div>
        
        <!-- Tipo de Desconto -->
        <div class="field">
          <label for="discountType" class="block text-sm font-medium mb-1">Tipo de Desconto *</label>
          <Dropdown
            id="discountType"
            v-model="form.discountType"
            :options="discountTypeOptions"
            option-label="label"
            option-value="value"
            placeholder="Selecione o tipo de desconto"
            class="w-full"
          />
        </div>
        
        <!-- Valor do Desconto -->
        <div class="field">
          <label for="discountValue" class="block text-sm font-medium mb-1">Valor do Desconto *</label>
          <InputNumber
            id="discountValue"
            v-model="form.discountValue"
            :class="{ 'p-invalid': validationErrors.includes('Valor do desconto não pode ser negativo') }"
            mode="decimal"
            :min="0"
            :max="form.discountType === 'Percentage' ? 100 : undefined"
            :suffix="form.discountType === 'Percentage' ? '%' : ''"
            class="w-full"
          />
          <small v-if="validationErrors.includes('Valor do desconto não pode ser negativo')" class="p-error">
            Valor do desconto não pode ser negativo
          </small>
        </div>
        
        <!-- Taxa de Comissão -->
        <div class="field">
          <label for="commissionRate" class="block text-sm font-medium mb-1">Taxa de Comissão (%) *</label>
          <InputNumber
            id="commissionRate"
            v-model="form.commissionRate"
            :class="{ 'p-invalid': validationErrors.includes('Taxa de comissão não pode ser negativa') || validationErrors.includes('Taxa de comissão não pode exceder 100%') }"
            mode="decimal"
            :min="0"
            :max="100"
            suffix="%"
            class="w-full"
          />
          <small v-if="validationErrors.includes('Taxa de comissão não pode ser negativa')" class="p-error">
            Taxa de comissão não pode ser negativa
          </small>
          <small v-if="validationErrors.includes('Taxa de comissão não pode exceder 100%')" class="p-error">
            Taxa de comissão não pode exceder 100%
          </small>
        </div>
        
        <!-- Formato do Relatório -->
        <div class="field">
          <label for="reportFormat" class="block text-sm font-medium mb-1">Formato do Relatório *</label>
          <Dropdown
            id="reportFormat"
            v-model="form.reportFormat"
            :options="reportFormatOptions"
            option-label="label"
            option-value="value"
            placeholder="Selecione o formato do relatório"
            class="w-full"
          />
        </div>
        
        <!-- Token da API -->
        <div class="field col-span-2">
          <label for="apiToken" class="block text-sm font-medium mb-1">Token da API</label>
          <InputText
            id="apiToken"
            v-model="form.apiToken"
            placeholder="Digite o token da API de integração"
            class="w-full"
          />
          <small class="p-text-secondary">Token para integração com sistemas externos (ex: WhatsApp API)</small>
        </div>
        
        <!-- Caminho do Logo -->
        <div class="field col-span-2">
          <label for="logoPath" class="block text-sm font-medium mb-1">Caminho do Logo</label>
          <InputText
            id="logoPath"
            v-model="form.logoPath"
            placeholder="/caminho/para/logo.png"
            class="w-full"
          />
          <small class="p-text-secondary">Caminho ou URL para o logo da loja nos relatórios</small>
        </div>
      </div>
      
      <div class="flex justify-end gap-2 mt-6">
        <Button
          type="button"
          label="Cancelar"
          severity="secondary"
          @click="clearForm"
          :disabled="isLoading"
        />
        <Button
          type="submit"
          label="Salvar Configuração"
          :loading="isLoading"
          :disabled="isLoading"
        />
      </div>
    </form>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import InputText from 'primevue/inputtext'
import InputNumber from 'primevue/inputnumber'
import Dropdown from 'primevue/dropdown'
import Button from 'primevue/button'
import { useStoreConfigForm, type StoreConfigFormValues } from '../composables/use-store-config-form'

// Props
interface Props {
  initialData?: StoreConfigFormValues
  onSubmit?: (values: StoreConfigFormValues) => Promise<void>
}

const props = defineProps<Props>()

// Emits
const emit = defineEmits<{
  submit: [values: StoreConfigFormValues]
  cancel: []
}>()

// Composable
const {
  isLoading,
  error,
  success,
  form,
  validateForm,
  clearForm: clearFormComposable,
  discountTypeOptions,
  reportFormatOptions
} = useStoreConfigForm()

// Computed
const validationErrors = computed(() => validateForm())

// Methods
const handleSubmit = async () => {
  const errors = validateForm()
  if (errors.length > 0) {
    return
  }
  
  isLoading.value = true
  
  try {
    if (props.onSubmit) {
      await props.onSubmit(form)
    } else {
      emit('submit', form)
    }
  } catch (err: unknown) {
    error.value = err instanceof Error ? err.message : 'Erro ao salvar configuração'
  } finally {
    isLoading.value = false
  }
}

const clearForm = () => {
  clearFormComposable()
  emit('cancel')
}

// Initialize form with initial data if provided
if (props.initialData) {
  form.name = props.initialData.name
  form.cnpj = props.initialData.cnpj
  form.contacts = props.initialData.contacts
  form.address = props.initialData.address
  form.discountType = props.initialData.discountType
  form.discountValue = props.initialData.discountValue
  form.commissionRate = props.initialData.commissionRate
  form.reportFormat = props.initialData.reportFormat
  form.apiToken = props.initialData.apiToken
  form.logoPath = props.initialData.logoPath
}
</script>

<style scoped>
.store-config-form {
  max-width: 800px;
  margin: 0 auto;
}

.field {
  margin-bottom: 1rem;
}

.p-message {
  display: flex;
  align-items: center;
  padding: 0.75rem 1rem;
  border-radius: 6px;
  margin-bottom: 1rem;
}

.p-message-error {
  background-color: #fef2f2;
  border: 1px solid #fecaca;
  color: #dc2626;
}

.p-message-success {
  background-color: #f0fdf4;
  border: 1px solid #bbf7d0;
  color: #16a34a;
}

.p-message-icon {
  margin-right: 0.5rem;
}

.p-error {
  color: #dc2626;
  font-size: 0.875rem;
  margin-top: 0.25rem;
  display: block;
}

.p-text-secondary {
  color: #6b7280;
  font-size: 0.875rem;
  margin-top: 0.25rem;
  display: block;
}
</style>
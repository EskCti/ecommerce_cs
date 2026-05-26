<template>
  <div class="store-settings-page">
    <div class="page-header mb-6">
      <h1 class="text-2xl font-bold text-gray-900">Configurações da Loja</h1>
      <p class="text-gray-600 mt-1">Gerencie as configurações operacionais da sua loja</p>
    </div>
    
    <!-- Error Message -->
    <div v-if="error" class="p-message p-message-error mb-4">
      <span class="p-message-icon pi pi-times-circle"></span>
      <span class="p-message-text">{{ error }}</span>
      <Button
        icon="pi pi-times"
        class="p-message-close p-button-text p-button-rounded p-button-plain"
        @click="clearError"
      />
    </div>
    
    <!-- Success Message -->
    <div v-if="success" class="p-message p-message-success mb-4">
      <span class="p-message-icon pi pi-check-circle"></span>
      <span class="p-message-text">Configuração atualizada com sucesso!</span>
      <Button
        icon="pi pi-times"
        class="p-message-close p-button-text p-button-rounded p-button-plain"
        @click="clearSuccess"
      />
    </div>
    
    <!-- Loading State -->
    <div v-if="isLoading && !storeConfig" class="flex justify-center items-center h-64">
      <ProgressSpinner />
    </div>
    
    <!-- Store Config Form -->
    <div v-else-if="storeConfig" class="store-config-section">
      <Card>
        <template #title>
          <div class="flex items-center gap-2">
            <i class="pi pi-cog text-primary"></i>
            <span>Configuração Geral da Loja</span>
          </div>
        </template>
        
        <template #content>
          <StoreConfigForm
            :initial-data="{
              name: storeConfig.name,
              cnpj: storeConfig.cnpj || '',
              contacts: storeConfig.contacts || '',
              address: storeConfig.address || '',
              discountType: storeConfig.discountType,
              discountValue: storeConfig.discountValue,
              commissionRate: storeConfig.commissionRate,
              reportFormat: storeConfig.reportFormat,
              apiToken: storeConfig.apiToken || '',
              logoPath: storeConfig.logoPath || ''
            }"
            :on-submit="handleUpdateStoreConfig"
            @cancel="handleCancel"
          />
        </template>
        
        <template #footer>
          <div class="text-sm text-gray-500">
            <p>Última atualização: {{ formatDate(storeConfig.updatedAt) }}</p>
            <p>Criado em: {{ formatDate(storeConfig.createdAt) }}</p>
          </div>
        </template>
      </Card>
    </div>
    
    <!-- Empty State -->
    <div v-else class="empty-state">
      <Card>
        <template #content>
          <div class="text-center py-8">
            <i class="pi pi-exclamation-circle text-4xl text-gray-400 mb-4"></i>
            <h3 class="text-lg font-medium text-gray-900 mb-2">Configuração não encontrada</h3>
            <p class="text-gray-600 mb-4">Não foi possível carregar as configurações da loja.</p>
            <Button
              label="Tentar novamente"
              icon="pi pi-refresh"
              @click="loadStoreConfig"
              :loading="isLoading"
            />
          </div>
        </template>
      </Card>
    </div>
  </div>
</template>

<script setup lang="ts">
import Card from 'primevue/card'
import Button from 'primevue/button'
import ProgressSpinner from 'primevue/progressspinner'
import StoreConfigForm from '@/modules/store-settings/presentation/components/StoreConfigForm.vue'
import { useStoreConfigPage } from '@/modules/store-settings/presentation/composables/use-store-config-page'
import type { StoreConfigFormValues } from '@/modules/store-settings/presentation/composables/use-store-config-form'

// Composable
const {
  isLoading,
  storeConfig,
  error,
  success,
  loadStoreConfig,
  updateStoreConfig,
  clearError,
  clearSuccess
} = useStoreConfigPage()

// Methods
const handleUpdateStoreConfig = async (formValues: StoreConfigFormValues) => {
  if (!storeConfig.value) return
  
  // Create updated entity
  const updatedConfig = storeConfig.value.update({
    name: formValues.name,
    cnpj: formValues.cnpj || undefined,
    contacts: formValues.contacts || undefined,
    address: formValues.address || undefined,
    discountType: formValues.discountType,
    discountValue: formValues.discountValue,
    commissionRate: formValues.commissionRate,
    reportFormat: formValues.reportFormat,
    apiToken: formValues.apiToken || undefined,
    logoPath: formValues.logoPath || undefined,
  })
  
  if (updatedConfig.ok === false) {
    error.value = updatedConfig.error
    return
  }
  
  await updateStoreConfig(updatedConfig.data)
}

const handleCancel = () => {
  // Reload original data
  loadStoreConfig()
}

const formatDate = (dateString: string) => {
  const date = new Date(dateString)
  return date.toLocaleDateString('pt-BR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}
</script>

<style scoped>
.store-settings-page {
  max-width: 1000px;
  margin: 0 auto;
  padding: 1rem;
}

.page-header {
  border-bottom: 1px solid #e5e7eb;
  padding-bottom: 1rem;
}

.store-config-section {
  margin-top: 2rem;
}

.empty-state {
  max-width: 400px;
  margin: 4rem auto;
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

.p-message-close {
  margin-left: auto;
  margin-right: 0;
}
</style>
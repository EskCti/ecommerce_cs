import { ref, reactive } from 'vue'
import type { StoreConfigEntity } from '../../domain/store-config.entity'

export type StoreConfigFormValues = {
  name: string
  cnpj: string
  contacts: string
  address: string
  discountType: string
  discountValue: number
  commissionRate: number
  reportFormat: string
  apiToken: string
  logoPath: string
}

export function useStoreConfigForm() {
  const isLoading = ref(false)
  const error = ref<string | null>(null)
  const success = ref(false)

  const form = reactive<StoreConfigFormValues>({
    name: '',
    cnpj: '',
    contacts: '',
    address: '',
    discountType: 'None',
    discountValue: 0,
    commissionRate: 0,
    reportFormat: 'PDF',
    apiToken: '',
    logoPath: '',
  })

  const validateForm = (): string[] => {
    const errors: string[] = []
    if (!form.name.trim()) errors.push('Nome da loja é obrigatório')
    if (form.discountValue < 0) errors.push('Valor do desconto não pode ser negativo')
    if (form.commissionRate < 0) errors.push('Taxa de comissão não pode ser negativa')
    if (form.commissionRate > 100) errors.push('Taxa de comissão não pode exceder 100%')
    return errors
  }

  const populateForm = (entity: StoreConfigEntity) => {
    form.name = entity.name
    form.cnpj = entity.cnpj || ''
    form.contacts = entity.contacts || ''
    form.address = entity.address || ''
    form.discountType = entity.discountType
    form.discountValue = entity.discountValue
    form.commissionRate = entity.commissionRate
    form.reportFormat = entity.reportFormat
    form.apiToken = entity.apiToken || ''
    form.logoPath = entity.logoPath || ''
  }

  const clearForm = () => {
    form.name = ''
    form.cnpj = ''
    form.contacts = ''
    form.address = ''
    form.discountType = 'None'
    form.discountValue = 0
    form.commissionRate = 0
    form.reportFormat = 'PDF'
    form.apiToken = ''
    form.logoPath = ''
    error.value = null
    success.value = false
  }

  const discountTypeOptions = [
    { label: 'Nenhum', value: 'None' },
    { label: 'Percentual', value: 'Percentage' },
    { label: 'Valor fixo', value: 'FixedAmount' },
  ]

  const reportFormatOptions = [
    { label: 'PDF', value: 'PDF' },
    { label: 'HTML', value: 'HTML' },
    { label: 'Excel', value: 'EXCEL' },
    { label: 'CSV', value: 'CSV' },
  ]

  return {
    isLoading,
    error,
    success,
    form,
    validateForm,
    populateForm,
    clearForm,
    discountTypeOptions,
    reportFormatOptions,
  }
}

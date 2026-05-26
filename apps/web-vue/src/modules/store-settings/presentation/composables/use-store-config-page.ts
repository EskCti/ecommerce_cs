import { ref, onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { createStoreSettingsModule } from '../../composition'
import { StoreConfigEntity } from '../../domain/store-config.entity'

export function useStoreConfigPage() {
  const auth = useAuthStore()
  const module = createStoreSettingsModule(() => auth.token)

  const isLoading = ref(false)
  const storeConfig = ref<StoreConfigEntity | null>(null)
  const error = ref<string | null>(null)
  const success = ref(false)

  const loadStoreConfig = async () => {
    isLoading.value = true
    error.value = null
    const result = await module.getStoreConfigUseCase.execute()
    if (!result.ok) {
      error.value = result.error
    } else {
      storeConfig.value = result.data
    }
    isLoading.value = false
  }

  const updateStoreConfig = async (config: StoreConfigEntity) => {
    isLoading.value = true
    error.value = null
    success.value = false
    const result = await module.updateStoreConfigUseCase.execute(config)
    if (!result.ok) {
      error.value = result.error
    } else {
      storeConfig.value = result.data
      success.value = true
      setTimeout(() => { success.value = false }, 3000)
    }
    isLoading.value = false
  }

  onMounted(loadStoreConfig)

  return {
    isLoading,
    storeConfig,
    error,
    success,
    loadStoreConfig,
    updateStoreConfig,
    clearError: () => { error.value = null },
    clearSuccess: () => { success.value = false },
  }
}

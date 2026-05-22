import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import type { AuthUserEntity } from '@/modules/auth/domain/auth-user.entity'
import { createAuthModule } from '@/modules/auth/composition'

const TOKEN_KEY = 'retailops_token'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem(TOKEN_KEY))
  const user = ref<AuthUserEntity | null>(null)

  const authModule = createAuthModule(
    () => token.value,
    (value) => {
      token.value = value
      if (value) localStorage.setItem(TOKEN_KEY, value)
      else localStorage.removeItem(TOKEN_KEY)
    },
  )

  const isAuthenticated = computed(() => !!token.value && !!user.value)
  const isSas = computed(() => user.value?.isSas ?? false)
  const isPrivileged = computed(() => user.value?.isPrivileged ?? false)

  function applyUser(profile: AuthUserEntity) {
    user.value = profile
  }

  function clearSession() {
    token.value = null
    user.value = null
    localStorage.removeItem(TOKEN_KEY)
  }

  function hasPermission(key: string) {
    return user.value?.hasPermission(key) ?? false
  }

  async function login(loginValue: string, password: string) {
    const result = await authModule.loginUseCase.execute({ login: loginValue, password })
    if (!result.ok) throw new Error(result.error)
    applyUser(result.data.user)
    return result.data
  }

  async function restoreSession() {
    if (!token.value) return
    const result = await authModule.restoreSessionUseCase.execute()
    if (!result.ok) {
      clearSession()
      return
    }
    applyUser(result.data)
  }

  function logout() {
    clearSession()
  }

  return {
    token,
    user,
    isAuthenticated,
    isSas,
    isPrivileged,
    clearSession,
    hasPermission,
    login,
    restoreSession,
    logout,
    authModule,
  }
})

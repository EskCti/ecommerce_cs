import { createRouter, createWebHistory } from 'vue-router'
import { shellRoutes } from './shell.routes'
import { useAuthStore } from '@/stores/auth'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: () => import('@/views/LoginView.vue'),
      meta: { public: true },
    },
    {
      path: '/register-trial',
      name: 'register-trial',
      component: () => import('@/views/TrialRegisterView.vue'),
      meta: { public: true },
    },
    ...shellRoutes,
    {
      path: '/platform',
      component: () => import('@/layouts/AdminShell.vue'),
      children: [
        {
          path: '',
          name: 'platform',
          component: () => import('@/views/DashboardView.vue'),
        },
        {
          path: 'companies',
          name: 'platform-companies',
          component: () => import('@/views/PlatformCompaniesView.vue'),
          meta: { sasOnly: true },
        },
      ],
    },
    {
      path: '/tenant',
      component: () => import('@/layouts/AdminShell.vue'),
      children: [
        {
          path: '',
          name: 'tenant',
          component: () => import('@/views/DashboardView.vue'),
        },
        {
          path: 'users',
          name: 'tenant-users',
          component: () => import('@/views/UsersPermissionsView.vue'),
          meta: { permission: 'usuarios' },
        },
        {
          path: 'settings/store',
          name: 'tenant-settings-store',
          component: () => import('@/views/StoreSettingsView.vue'),
          meta: { permission: 'configuracoes' },
        },
        {
          path: 'settings/payment-methods',
          name: 'tenant-settings-payment-methods',
          component: () => import('@/views/PaymentMethodsView.vue'),
          meta: { permission: 'configuracoes' },
        },
        {
          path: 'settings/cash-registers',
          name: 'tenant-settings-cash-registers',
          component: () => import('@/views/CashRegistersView.vue'),
          meta: { permission: 'configuracoes' },
        },
        {
          path: 'crm/customers',
          name: 'tenant-crm-customers',
          component: () => import('@/views/CustomersView.vue'),
          meta: { permission: 'clientes' },
        },
        {
          path: 'crm/suppliers',
          name: 'tenant-crm-suppliers',
          component: () => import('@/views/SuppliersView.vue'),
          meta: { permission: 'fornecedores' },
        },
        {
          path: 'catalog/products',
          name: 'tenant-catalog-products',
          component: () => import('@/views/ProductsView.vue'),
          meta: { permission: 'produtos' },
        },
        {
          path: 'catalog/categories',
          name: 'tenant-catalog-categories',
          component: () => import('@/views/CategoriesView.vue'),
          meta: { permission: 'categorias' },
        },
        {
          path: 'catalog/inventory/low-stock',
          name: 'tenant-catalog-low-stock',
          component: () => import('@/views/LowStockView.vue'),
          meta: { permission: 'produtos' },
        },
        {
          path: 'catalog/stock/movements',
          name: 'tenant-catalog-stock-movements',
          component: () => import('@/views/StockMovementsView.vue'),
          meta: { permission: 'produtos' },
        },
      ],
    },
  ],
})

router.beforeEach(async (to) => {
  const auth = useAuthStore()
  if (!auth.user && auth.token) await auth.restoreSession()

  if (to.meta.public) {
    if (auth.isAuthenticated && to.name === 'login') {
      return auth.isSas ? '/platform' : '/tenant'
    }
    return true
  }

  if (!auth.isAuthenticated) return { name: 'login', query: { redirect: to.fullPath } }

  if (to.meta.sasOnly && !auth.isSas) return '/tenant'

  const permission = to.meta.permission as string | undefined
  if (permission && !auth.hasPermission(permission)) return '/tenant'

  return true
})

export default router

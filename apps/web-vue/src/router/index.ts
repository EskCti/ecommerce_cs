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
    {
      path: '/tenant/pdv',
      name: 'tenant-pdv',
      component: () => import('@/views/PdvView.vue'),
      meta: { permission: 'vendas' },
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
        {
          path: 'sales',
          name: 'tenant-sales',
          component: () => import('@/views/SalesListView.vue'),
          meta: { permissions: ['vendas', 'produtos'] },
        },
        {
          path: 'returns/exchanges',
          name: 'tenant-returns-exchanges',
          component: () => import('@/views/ExchangesView.vue'),
          meta: { permission: 'devolucoes' },
        },
        {
          path: 'reports',
          name: 'tenant-reports',
          component: () => import('@/views/ReportsView.vue'),
          meta: { permissions: ['rel_vendas', 'rel_estoque', 'rel_financeiro', 'rel_caixa'] },
        },
        {
          path: 'finance/receivables',
          name: 'tenant-finance-receivables',
          component: () => import('@/views/FinanceReceivablesView.vue'),
          meta: { permission: 'receber' },
        },
        {
          path: 'finance/payables',
          name: 'tenant-finance-payables',
          component: () => import('@/views/FinancePayablesView.vue'),
          meta: { permission: 'pagar' },
        },
        {
          path: 'finance/purchases',
          name: 'tenant-finance-purchases',
          component: () => import('@/views/FinancePurchasesView.vue'),
          meta: { permission: 'pagar' },
        },
        {
          path: 'finance/commissions',
          name: 'tenant-finance-commissions',
          component: () => import('@/views/FinanceCommissionsView.vue'),
          meta: { permissions: ['comissoes', 'caixa'] },
        },
        {
          path: 'finance/cash-flow',
          name: 'tenant-finance-cash-flow',
          component: () => import('@/views/FinanceCommissionsView.vue'),
          meta: { permission: 'caixa' },
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
  if (permission && !auth.hasPermission(permission)) {
    return { path: '/tenant', query: { denied: permission } }
  }

  const permissions = to.meta.permissions as string[] | undefined
  if (permissions?.length && !permissions.some((key) => auth.hasPermission(key))) {
    return { path: '/tenant', query: { denied: permissions.join(',') } }
  }

  return true
})

export default router

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

  const permission = to.meta.permission as string | undefined
  if (permission && !auth.hasPermission(permission)) return '/tenant'

  return true
})

export default router

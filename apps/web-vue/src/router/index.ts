import { createRouter, createWebHistory } from 'vue-router'
import { shellRoutes } from './shell.routes'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
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
      ],
    },
  ],
})

export default router

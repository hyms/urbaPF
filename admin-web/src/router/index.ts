import { createRouter, createWebHistory } from 'vue-router'
import AdminLayout from '@/layouts/AdminLayout.vue'
import HealthPage from '@/pages/HealthPage.vue'
import LoginPage from '@/pages/LoginPage.vue'

export const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: LoginPage,
    },
    {
      path: '/',
      component: AdminLayout,
      children: [
        {
          path: '',
          redirect: '/health',
        },
        {
          path: 'health',
          name: 'health',
          component: HealthPage,
        },
      ],
    },
  ],
})
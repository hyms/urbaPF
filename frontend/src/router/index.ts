import { createRouter, createWebHistory, Router, RouteRecordRaw, NavigationGuardNext } from 'vue-router'
import { Loading } from 'quasar'
import { useAuthStore } from '../stores/auth'

const routes: RouteRecordRaw[] = [
  {
    path: '/login',
    component: () => import('../pages/LoginPage.vue'),
    meta: { requiresAuth: false }
  },
  {
    path: '/register',
    component: () => import('../pages/RegisterPage.vue'),
    meta: { requiresAuth: false }
  },
  {
    path: '/',
    component: () => import('../layouts/MainLayout.vue'),
    meta: { requiresAuth: true },
    children: [
      {
        path: '',
        name: 'dashboard',
        component: () => import('../pages/DashboardPage.vue')
      },
      {
        path: 'condominiums',
        name: 'condominiums',
        component: () => import('../pages/CondominiumsPage.vue')
      },
      {
        path: 'posts',
        name: 'posts',
        component: () => import('../pages/PostsPage.vue')
      },
      {
        path: 'polls',
        name: 'polls',
        component: () => import('../pages/PollsPage.vue')
      },
      {
        path: 'incidents',
        name: 'incidents',
        component: () => import('../pages/IncidentsPage.vue')
      },
      {
        path: 'security',
        name: 'security',
        component: () => import('../pages/SecurityPage.vue'),
        meta: { requiresManager: true }
      },
      {
        path: 'alerts',
        name: 'alerts',
        component: () => import('../pages/AlertsPage.vue'),
        meta: { requiresManager: true }
      },
      {
        path: 'users',
        name: 'users',
        component: () => import('../pages/UsersPage.vue'),
        meta: { requiresAdmin: true }
      },
      {
        path: 'settings',
        name: 'settings',
        component: () => import('../pages/SettingsPage.vue')
      },
      {
        path: 'directory',
        name: 'directory',
        component: () => import('../pages/DirectoryPage.vue')
      },
      {
        path: 'expenses',
        name: 'expenses',
        component: () => import('../pages/ExpensesPage.vue')
      }
    ]
  }
]

const router: Router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach(async (to, from) => {
  const authStore = useAuthStore()
  const isAuthenticated = await authStore.check()
  
  const publicPaths = ['/login', '/register']
  const isPublicPath = publicPaths.includes(to.path)

  if (!isAuthenticated && !isPublicPath) {
    return '/login'
  } else if (isAuthenticated && isPublicPath) {
    return '/'
  } else {
    return true
  }
})

router.afterEach(() => {
  // Loading.hide()
})

export default router
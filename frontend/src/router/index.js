import { createRouter, createWebHistory } from 'vue-router'

const routes = [
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
        path: 'users',
        name: 'users',
        component: () => import('../pages/UsersPage.vue'),
        meta: { requiresAdmin: true }
      },
      {
        path: 'settings',
        name: 'settings',
        component: () => import('../pages/SettingsPage.vue')
      }
    ]
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to, from, next) => {
  const token = localStorage.getItem('token')
  const isAuthenticated = !!token
  
  const publicPaths = ['/login', '/register']
  const isPublicPath = publicPaths.includes(to.path)

  if (!isAuthenticated && !isPublicPath) {
    next('/login')
  } else if (isAuthenticated && isPublicPath) {
    next('/')
  } else {
    next()
  }
})

export default router

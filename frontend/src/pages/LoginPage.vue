<template>
  <q-layout view="lHh Lpr lFf">
    <q-page-container>
      <q-page class="flex flex-center">
        <div class="login-container">
          <q-card class="login-card" bordered>
            <q-card-section class="login-header text-center q-pb-none">
              <div class="logo-container q-mb-md">
                <q-avatar size="80px" color="primary" text-color="white" icon="home_work" />
              </div>
              <div class="text-h4 text-primary text-weight-bold">UrbaPF</div>
              <div class="text-subtitle1 text-grey-7 q-mt-sm">Plataforma de Coordinación Vecinal</div>
            </q-card-section>

            <q-card-section class="q-pt-md">
              <q-form @submit="onSubmit" class="q-gutter-md">
                <q-input
                  filled
                  v-model="email"
                  label="Correo electrónico"
                  type="email"
                  lazy-rules
                  :rules="[val => !!val || 'El correo es requerido', val => /.+@.+\..+/.test(val) || 'Ingrese un correo válido']"
                  aria-label="Correo electrónico"
                >
                  <template v-slot:prepend>
                    <q-icon name="email" color="grey-6" />
                  </template>
                </q-input>

                <q-input
                  filled
                  v-model="password"
                  label="Contraseña"
                  :type="showPassword ? 'text' : 'password'"
                  lazy-rules
                  :rules="[val => !!val || 'La contraseña es requerida']"
                  aria-label="Contraseña"
                >
                  <template v-slot:prepend>
                    <q-icon name="lock" color="grey-6" />
                  </template>
                  <template v-slot:append>
                    <q-icon
                      :name="showPassword ? 'visibility' : 'visibility_off'"
                      class="cursor-pointer"
                      @click="showPassword = !showPassword"
                      :aria-label="showPassword ? 'Ocultar contraseña' : 'Mostrar contraseña'"
                    />
                  </template>
                </q-input>

                <div class="row items-center justify-end q-mt-xs">
                  <q-btn flat no-caps color="primary" label="¿Olvidaste tu contraseña?" class="text-body2" />
                </div>

                <q-banner v-if="authStore.error" class="bg-red-1 text-negative q-mb-md" rounded>
                  <template v-slot:avatar>
                    <q-icon name="error" color="negative" />
                  </template>
                  {{ authStore.error }}
                </q-banner>

                <q-btn
                  type="submit"
                  color="primary"
                  class="full-width q-mt-md"
                  size="lg"
                  :loading="authStore.loading"
                  label="Iniciar Sesión"
                  no-caps
                />
              </q-form>
            </q-card-section>

            <q-card-section class="text-center q-pt-none">
              <div class="text-grey-7">
                ¿No tienes cuenta?
                <router-link to="/register" class="text-primary text-weight-medium text-decoration-none">
                  Regístrate
                </router-link>
              </div>
            </q-card-section>
          </q-card>
          
          <div class="text-center q-mt-lg text-grey-6 text-caption">
            © 2026 UrbaPF. Todos los derechos reservados.
          </div>
        </div>
      </q-page>
    </q-page-container>
  </q-layout>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useQuasar } from 'quasar'
import { useAuthStore } from '../stores/auth'

const router = useRouter()
const $q = useQuasar()
const authStore = useAuthStore()

const email = ref('')
const password = ref('')
const showPassword = ref(false)

async function onSubmit() {
  const success = await authStore.login(email.value, password.value)
  if (success) {
    $q.notify({
      type: 'positive',
      message: 'Bienvenido a UrbaPF',
      position: 'top',
      actions: [
        { icon: 'close', color: 'white', round: true, dense: true }
      ]
    })
    router.push('/')
  }
}
</script>

<style scoped>
.login-container {
  width: 100%;
  max-width: 440px;
  padding: 20px;
}

.login-card {
  border-radius: 16px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.08);
  transition: box-shadow 0.3s ease;
}

.login-card:hover {
  box-shadow: 0 12px 40px rgba(0, 0, 0, 0.12);
}

.logo-container {
  animation: float 3s ease-in-out infinite;
}

@keyframes float {
  0%, 100% {
    transform: translateY(0);
  }
  50% {
    transform: translateY(-8px);
  }
}

.text-decoration-none {
  text-decoration: none;
}

.text-weight-medium {
  font-weight: 500;
}

@media (max-width: 599px) {
  .login-container {
    padding: 16px;
  }
  
  .login-card {
    border-radius: 12px;
  }
}
</style>

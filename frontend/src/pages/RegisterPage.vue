<template>
  <q-layout view="lHh Lpr lFf">
    <q-page-container>
      <q-page class="flex flex-center bg-grey-2">
        <q-card class="q-pa-md shadow-2 my_card" bordered style="width: 450px; max-width: 90vw;">
          <q-card-section class="text-center">
            <div class="text-h4 text-primary text-weight-bold q-mb-sm">Registrarse</div>
            <div class="text-subtitle1 text-grey-7">Crea tu cuenta en UrbaPF</div>
          </q-card-section>

          <q-card-section>
            <q-form @submit="onSubmit" class="q-gutter-md">
              <q-input
                filled
                v-model="fullName"
                label="Nombre completo"
                lazy-rules
                :rules="[val => !!val || 'El nombre es requerido']"
              >
                <template v-slot:prepend>
                  <q-icon name="person" />
                </template>
              </q-input>

              <q-input
                filled
                v-model="email"
                label="Correo electrónico"
                type="email"
                lazy-rules
                :rules="[val => !!val || 'El correo es requerido', val => /.+@.+\..+/.test(val) || 'Ingrese un correo válido']"
              >
                <template v-slot:prepend>
                  <q-icon name="email" />
                </template>
              </q-input>

              <q-input
                filled
                v-model="phone"
                label="Teléfono (opcional)"
                type="tel"
              >
                <template v-slot:prepend>
                  <q-icon name="phone" />
                </template>
              </q-input>

              <q-input
                filled
                v-model="password"
                label="Contraseña"
                :type="showPassword ? 'text' : 'password'"
                lazy-rules
                :rules="[val => !!val || 'La contraseña es requerida', val => val.length >= 6 || 'Mínimo 6 caracteres']"
              >
                <template v-slot:prepend>
                  <q-icon name="lock" />
                </template>
                <template v-slot:append>
                  <q-icon
                    :name="showPassword ? 'visibility' : 'visibility_off'"
                    class="cursor-pointer"
                    @click="showPassword = !showPassword"
                  />
                </template>
              </q-input>

              <q-input
                filled
                v-model="confirmPassword"
                label="Confirmar contraseña"
                :type="showPassword ? 'text' : 'password'"
                lazy-rules
                :rules="[val => !!val || 'Confirme su contraseña', val => val === password || 'Las contraseñas no coinciden']"
              >
                <template v-slot:prepend>
                  <q-icon name="lock_outline" />
                </template>
              </q-input>

              <div v-if="authStore.error" class="text-negative q-mb-md">
                {{ authStore.error }}
              </div>

              <q-btn
                type="submit"
                color="primary"
                class="full-width q-mt-md"
                size="lg"
                :loading="authStore.loading"
                label="Registrarse"
              />
            </q-form>
          </q-card-section>

          <q-card-section class="text-center q-pt-none">
            <router-link to="/login" class="text-primary text-decoration-none">
              ¿Ya tienes cuenta? Inicia sesión
            </router-link>
          </q-card-section>
        </q-card>
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

const fullName = ref('')
const email = ref('')
const phone = ref('')
const password = ref('')
const confirmPassword = ref('')
const showPassword = ref(false)

async function onSubmit() {
  const success = await authStore.register(email.value, password.value, fullName.value, phone.value)
  if (success) {
    $q.notify({
      type: 'positive',
      message: 'Cuenta creada exitosamente. Por favor inicia sesión.',
      position: 'top'
    })
    router.push('/login')
  }
}
</script>

<style scoped>
.my_card {
  border-radius: 12px;
}
.text-decoration-none {
  text-decoration: none;
}
</style>

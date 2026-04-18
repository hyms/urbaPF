<template>
  <q-card :style="$q.screen.lt.sm ? 'width: 100%; max-width: 100%;' : 'min-width: 500px; max-width: 90vw'">
    <q-card-section>
      <div class="text-h6">{{ isEditing ? t('incidents.editIncident') : t('incidents.report') }}</div>
    </q-card-section>

    <q-card-section class="q-pt-none">
      <q-form class="q-gutter-md" @submit.prevent="submit">
        <q-input
          v-model="form.title"
          :label="t('incidents.title')"
          outlined
          :rules="[v => !!v || t('common.required')]"
        />

        <q-input
          v-model="form.description"
          :label="t('incidents.description')"
          type="textarea"
          outlined
          rows="3"
        />

        <q-select
          v-model="form.type"
          :options="typeOptions"
          :label="t('incidents.type')"
          outlined
          emit-value
          map-options
        />

        <div>
          <div class="text-subtitle2 q-mb-sm">{{ t('incidents.location') }}</div>
          <div class="row q-col-gutter-sm">
            <div class="col-6">
              <q-input
                v-model.number="form.latitude"
                :label="t('incidents.latitude')"
                type="number"
                step="any"
                outlined
                dense
              />
            </div>
            <div class="col-6">
              <q-input
                v-model.number="form.longitude"
                :label="t('incidents.longitude')"
                type="number"
                step="any"
                outlined
                dense
              />
            </div>
          </div>
          <q-btn
            flat
            color="primary"
            icon="my_location"
            :label="t('incidents.getCurrentLocation')"
            @click="getCurrentLocation"
            class="q-mt-sm"
            :loading="loadingLocation"
          />
        </div>

        <q-input
          v-model="form.addressReference"
          :label="t('incidents.addressReference')"
          outlined
          :hint="t('incidents.addressReferenceHint')"
        />

        <div>
          <div class="text-subtitle2 q-mb-sm">
            {{ t('incidents.photos') }} ({{ form.media.length }}/3)
          </div>
          <div class="row q-col-gutter-sm">
            <div v-for="(file, idx) in form.media" :key="idx" class="col-4">
              <q-img :src="file.preview" :ratio="1" class="rounded-borders">
                <q-btn
                  round
                  dense
                  color="negative"
                  icon="close"
                  size="sm"
                  class="absolute-top-right q-ma-xs"
                  @click="removePhoto(idx)"
                />
              </q-img>
            </div>
            <div v-if="form.media.length < 3" class="col-4">
              <q-card
                flat
                bordered
                class="cursor-pointer column items-center justify-center"
                style="height: 100px"
                @click="triggerFileInput"
              >
                <q-icon name="add_a_photo" size="md" color="grey" />
              </q-card>
            </div>
          </div>
          <input
            ref="fileInput"
            type="file"
            accept="image/*"
            multiple
            style="display: none"
            @change="handleFileSelect"
          />
        </div>
      </q-form>
    </q-card-section>

    <q-card-actions align="right">
      <q-btn flat :label="t('common.cancel')" @click="$emit('cancel')" />
      <q-btn
        color="primary"
        :label="t('common.save')"
        @click="submit"
        :loading="loading"
      />
    </q-card-actions>
  </q-card>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useI18n } from '@/composables/useI18n'

interface MediaFile {
  file?: File
  preview?: string
  type?: string
  path?: string
}

interface IncidentFormData {
  title: string
  description: string
  type: number
  latitude?: number
  longitude?: number
  addressReference: string
  media: MediaFile[]
  location?: string
}

const props = defineProps<{
  incident?: IncidentFormData | null
  loading?: boolean
}>()

const emit = defineEmits<{
  (e: 'submit', data: IncidentFormData): void
  (e: 'cancel'): void
}>()

const { t } = useI18n()

const fileInput = ref<HTMLInputElement | null>(null)
const loadingLocation = ref(false)

const defaultForm = (): IncidentFormData => ({
  title: '',
  description: '',
  type: 1,
  latitude: undefined,
  longitude: undefined,
  addressReference: '',
  media: []
})

const form = ref<IncidentFormData>(defaultForm())

const isEditing = computed(() => !!props.incident)

const typeOptions = [
  { label: 'Mantenimiento', value: 1 },
  { label: 'Seguridad', value: 2 },
  { label: 'Limpieza', value: 3 },
  { label: 'Infraestructura', value: 4 },
  { label: 'Otro', value: 5 }
]

watch(() => props.incident, (newIncident) => {
  if (newIncident) {
    form.value = { ...newIncident }
  } else {
    form.value = defaultForm()
  }
}, { immediate: true })

function getCurrentLocation() {
  loadingLocation.value = true
  if (navigator.geolocation) {
    navigator.geolocation.getCurrentPosition(
      (position) => {
        form.value.latitude = position.coords.latitude
        form.value.longitude = position.coords.longitude
        loadingLocation.value = false
      },
      (error) => {
        console.error('Error getting location:', error)
        loadingLocation.value = false
      }
    )
  } else {
    loadingLocation.value = false
  }
}

function triggerFileInput() {
  fileInput.value?.click()
}

function handleFileSelect(event: Event) {
  const input = event.target as HTMLInputElement
  if (!input.files) return

  const remaining = 3 - form.value.media.length
  const filesToAdd = Array.from(input.files).slice(0, remaining)

  filesToAdd.forEach((file) => {
    const reader = new FileReader()
    reader.onload = (e) => {
      form.value.media.push({
        file,
        preview: e.target?.result as string
      })
    }
    reader.readAsDataURL(file)
  })

  input.value = ''
}

function removePhoto(index: number) {
  form.value.media.splice(index, 1)
}

function submit() {
  if (!form.value.title) return

  const location = form.value.latitude && form.value.longitude
    ? `${form.value.latitude} ${form.value.longitude}`
    : undefined

  emit('submit', {
    ...form.value,
    location,
    media: form.value.media.map(m => ({
      type: 'image',
      path: m.file?.name || ''
    }))
  })
}
</script>

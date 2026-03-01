<template>
  <q-page class="q-pa-md">
    <div class="text-h4 q-mb-md">{{ t('dashboard.title') }}</div>

    <div class="row q-col-gutter-md q-mb-md">
      <div class="col-12 col-sm-6 col-md-3">
        <q-card class="bg-primary text-white">
          <q-card-section>
            <div class="text-h6">
              <q-icon name="home_work" class="q-mr-sm" />
              {{ t('common.condominiums') }}
            </div>
            <div class="text-h3">{{ stats.condominiums }}</div>
          </q-card-section>
        </q-card>
      </div>

      <div class="col-12 col-sm-6 col-md-3">
        <q-card class="bg-orange text-white">
          <q-card-section>
            <div class="text-h6">
              <q-icon name="warning" class="q-mr-sm" />
              {{ t('common.incidents') }}
            </div>
            <div class="text-h3">{{ stats.incidents }}</div>
          </q-card-section>
        </q-card>
      </div>

      <div class="col-12 col-sm-6 col-md-3">
        <q-card class="bg-purple text-white">
          <q-card-section>
            <div class="text-h6">
              <q-icon name="poll" class="q-mr-sm" />
              {{ t('common.polls') }}
            </div>
            <div class="text-h3">{{ stats.polls }}</div>
          </q-card-section>
        </q-card>
      </div>

      <div class="col-12 col-sm-6 col-md-3">
        <q-card class="bg-red text-white">
          <q-card-section>
            <div class="text-h6">
              <q-icon name="notifications_active" class="q-mr-sm" />
              {{ t('common.alerts') }}
            </div>
            <div class="text-h3">{{ stats.alerts }}</div>
          </q-card-section>
        </q-card>
      </div>
    </div>

    <div class="row q-col-gutter-md">
      <div class="col-12 col-md-6">
        <q-card>
          <q-card-section>
            <div class="text-h6">{{ t('dashboard.recentPosts') }}</div>
          </q-card-section>
          <q-separator />
          <q-list separator>
            <PostItem v-for="post in recentPosts" :key="post.id" :post="post" />
            <q-item v-if="recentPosts.length === 0">
              <q-item-section class="text-grey">
                {{ t('dashboard.noPosts') }}
              </q-item-section>
            </q-item>
          </q-list>
        </q-card>
      </div>

      <div class="col-12 col-md-6">
        <q-card>
          <q-card-section>
            <div class="text-h6">{{ t('dashboard.recentIncidents') }}</div>
          </q-card-section>
          <q-separator />
          <q-list separator>
            <IncidentItem v-for="incident in recentIncidents" :key="incident.id" :incident="incident" />
            <q-item v-if="recentIncidents.length === 0">
              <q-item-section class="text-grey">
                {{ t('dashboard.noIncidents') }}
              </q-item-section>
            </q-item>
          </q-list>
        </q-card>
      </div>
    </div>
  </q-page>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useCondominiumStore } from '../stores/condominium'
import { usePostStore } from '../stores/post'
import { useIncidentStore } from '../stores/incident'
import { usePollStore } from '../stores/poll'
import { useAlertStore } from '../stores/alert'
import { useI18n } from '../composables/useI18n'
import PostItem from '../components/PostItem.vue'
import IncidentItem from '../components/IncidentItem.vue'

const condoStore = useCondominiumStore()
const postStore = usePostStore()
const incidentStore = useIncidentStore()
const pollStore = usePollStore()
const alertStore = useAlertStore()
const { t } = useI18n()

const stats = ref({
  condominiums: 0,
  incidents: 0,
  polls: 0,
  alerts: 0
})

const recentPosts = ref([])
const recentIncidents = ref([])

onMounted(async () => {
  const condos = await condoStore.fetchAll()
  stats.value.condominiums = condos.length

  if (condos.length > 0) {
    const condoId = condos[0].id
    recentPosts.value = await postStore.fetchByCondominium(condoId)
    recentIncidents.value = await incidentStore.fetchByCondominium(condoId)
    const polls = await pollStore.fetchByCondominium(condoId)
    const alerts = await alertStore.fetchByCondominium(condoId)
    stats.value.polls = polls.length
    stats.value.alerts = alerts.length
    stats.value.incidents = recentIncidents.value.length
  }
})
</script>

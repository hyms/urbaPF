<template>
  <q-card class="condo-card cursor-pointer" @click="$emit('click', condo)">
    <q-card-section>
      <div class="row items-center">
        <div class="col">
          <div class="text-h6">{{ condo.name }}</div>
          <div class="text-caption text-grey">{{ condo.address }}</div>
        </div>
        <q-btn flat round dense icon="more_vert" v-if="showAdminControls" @click.stop>
          <q-menu>
            <q-list dense style="min-width: 150px">
              <q-item clickable v-close-popup @click="$emit('edit', condo)">
                <q-item-section>{{ t('common.update') }}</q-item-section>
              </q-item>
              <q-item clickable v-close-popup @click="$emit('delete', condo)" class="text-negative">
                <q-item-section>{{ t('common.delete') }}</q-item-section>
              </q-item>
            </q-list>
          </q-menu>
        </q-btn>
      </div>
    </q-card-section>

    <q-separator />

    <q-card-section>
      <div class="row q-col-gutter-sm">
        <div class="col-6">
          <div class="text-caption text-grey">{{ t('condominiums.monthlyFee') }}</div>
          <div class="text-body2">{{ condo.currency }} {{ condo.monthlyFee }}</div>
        </div>
        <div class="col-6" v-if="condo.description">
          <div class="text-caption text-grey">{{ t('condominiums.description') }}</div>
          <div class="text-body2 text-truncate">{{ condo.description }}</div>
        </div>
      </div>
    </q-card-section>
  </q-card>
</template>

<script setup>
import { useI18n } from '../composables/useI18n'

const props = defineProps({
  condo: {
    type: Object,
    required: true
  },
  showAdminControls: {
    type: Boolean,
    default: false
  }
})

defineEmits(['click', 'edit', 'delete'])

const { t } = useI18n()
</script>

<style scoped>
.condo-card:hover {
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
}
</style>

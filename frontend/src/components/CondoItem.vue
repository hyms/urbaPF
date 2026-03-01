<template>
  <q-card class="cursor-pointer" @click="$emit('click', condo)">
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
              <q-item clickable v-close-popup @click="$emit('delete', condo)">
                <q-item-section class="text-negative">{{ t('common.delete') }}</q-item-section>
              </q-item>
            </q-list>
          </q-menu>
        </q-btn>
      </div>
    </q-card-section>
    <q-separator />
    <q-card-section>
      <div class="row">
        <div class="col-6">
          <div class="text-caption text-grey">{{ t('condominiums.monthlyFee') }}</div>
          <div class="text-body1">{{ condo.monthlyFee }} {{ condo.currency }}</div>
        </div>
        <div class="col-6">
          <div class="text-caption text-grey">{{ t('condominiums.status') }}</div>
          <q-chip :color="condo.isActive ? 'green' : 'red'" text-color="white" size="sm">
            {{ condo.isActive ? t('condominiums.active') : t('condominiums.inactive') }}
          </q-chip>
        </div>
      </div>
    </q-card-section>
  </q-card>
</template>

<script setup>
import { useI18n } from '../composables/useI18n'

const props = defineProps({
  condo: { type: Object, required: true },
  showAdminControls: { type: Boolean, default: false }
})

const emit = defineEmits(['click', 'edit', 'delete'])

const { t } = useI18n()
</script>

<template>
  <q-card>
    <q-card-section>
      <div class="text-h6">{{ title }}</div>
    </q-card-section>
    <q-separator />
    <q-list separator>
      <slot name="items" />
      <q-item v-if="isEmpty">
        <q-item-section class="text-grey">
          {{ emptyMessage }}
        </q-item-section>
      </q-item>
    </q-list>
  </q-card>
</template>

<script setup lang="ts">
defineProps<{
  title: string
  isEmpty: boolean
  emptyMessage: string
}>()
</script>

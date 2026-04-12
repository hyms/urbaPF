<template>
  <q-card :class="['text-white', bgColor]">
    <q-card-section>
      <div class="text-h6">
        <q-icon :name="icon" class="q-mr-sm" />
        {{ title }}
      </div>
      <div class="text-h3">{{ value }}</div>
    </q-card-section>
  </q-card>
</template>

<script setup lang="ts">
defineProps<{
  title: string
  value: number | string
  icon: string
  bgColor: string
}>()
</script>
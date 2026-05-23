<script setup lang="ts">
withDefaults(
  defineProps<{
    wide?: boolean
  }>(),
  {
    wide: false,
  },
)
</script>

<template>
  <div class="flex min-h-screen items-center justify-center bg-background p-6">
    <div
      class="w-full space-y-4 rounded-xl border border-border bg-card p-8 text-card-foreground shadow-sm"
      :class="wide ? 'max-w-lg' : 'max-w-md'"
    >
      <slot />
    </div>
  </div>
</template>

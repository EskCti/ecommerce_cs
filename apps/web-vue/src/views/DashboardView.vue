<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import Message from 'primevue/message'

const route = useRoute()

const deniedMessage = computed(() => {
  const denied = route.query.denied
  if (!denied || typeof denied !== 'string') return ''
  return `Você não tem permissão para acessar este módulo (${denied}). Peça ao administrador para liberar o acesso.`
})
</script>

<template>
  <section class="mx-auto flex max-w-3xl flex-col items-center justify-center gap-4 py-16 text-center">
    <Message v-if="deniedMessage" severity="warn" :closable="false" class="w-full text-left">{{ deniedMessage }}</Message>
    <div class="rounded-full border border-border bg-muted/40 px-4 py-1 text-xs font-medium text-muted-foreground">
      Dashboard Vazio
    </div>
    <h1 class="text-3xl font-semibold tracking-tight text-foreground">Bem-vindo ao painel</h1>
    <p class="max-w-xl text-sm text-muted-foreground">
      O shell administrativo está pronto. Implemente módulos por Bounded Context usando os skills
      <code class="rounded bg-muted px-1 py-0.5">frontend-*-vue</code>.
    </p>
  </section>
</template>

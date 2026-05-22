<script setup lang="ts">
import { onMounted, ref } from 'vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import MultiSelect from 'primevue/multiselect'
import Button from 'primevue/button'
import Message from 'primevue/message'
import { useAuthStore } from '@/stores/auth'
import { createUsersModule } from '@/modules/users/composition'
import type { TenantUserEntity } from '@/modules/users/domain/tenant-user.entity'
import type { PermissionCatalogItem } from '@/modules/users/domain/permission-catalog-item'

const auth = useAuthStore()
const usersModule = createUsersModule(() => auth.token)

const users = ref<TenantUserEntity[]>([])
const catalog = ref<PermissionCatalogItem[]>([])
const selectedUser = ref<TenantUserEntity | null>(null)
const selectedKeys = ref<string[]>([])
const message = ref('')
const error = ref('')

onMounted(async () => {
  const [usersResult, catalogResult] = await Promise.all([
    usersModule.listUsersUseCase.execute(),
    usersModule.listPermissionCatalogUseCase.execute(),
  ])
  if (!usersResult.ok) {
    error.value = usersResult.error
    return
  }
  if (!catalogResult.ok) {
    error.value = catalogResult.error
    return
  }
  users.value = usersResult.data
  catalog.value = catalogResult.data
})

function editUser(row: TenantUserEntity) {
  selectedUser.value = row
  selectedKeys.value = [...row.permissionKeys]
  message.value = ''
  error.value = ''
}

async function savePermissions() {
  if (!selectedUser.value) return
  error.value = ''
  const result = await usersModule.assignPermissionsUseCase.execute(
    selectedUser.value.id,
    selectedKeys.value,
  )
  if (!result.ok) {
    error.value = result.error
    return
  }
  message.value = 'Permissões atualizadas'
  const refreshed = await usersModule.listUsersUseCase.execute()
  if (refreshed.ok) users.value = refreshed.data
}
</script>

<template>
  <div class="space-y-4 p-6">
    <h1 class="text-xl font-semibold">Usuários e permissões</h1>
    <Message v-if="error" severity="error" :closable="false">{{ error }}</Message>
    <p v-if="message" class="text-sm text-green-700">{{ message }}</p>
    <DataTable :value="users" data-key="id" @row-click="(e) => editUser(e.data as TenantUserEntity)">
      <Column field="name" header="Nome" />
      <Column field="email" header="E-mail" />
      <Column field="userLevel" header="Nível" />
      <Column header="Permissões">
        <template #body="{ data }">
          {{ (data as TenantUserEntity).permissionKeys.length }}
        </template>
      </Column>
    </DataTable>
    <div v-if="selectedUser" class="rounded border bg-white p-4">
      <h2 class="font-medium">{{ selectedUser.name }}</h2>
      <MultiSelect
        v-model="selectedKeys"
        :options="catalog"
        option-label="name"
        option-value="key"
        display="chip"
        class="mt-2 w-full"
      />
      <Button label="Salvar" class="mt-3" @click="savePermissions" />
    </div>
  </div>
</template>

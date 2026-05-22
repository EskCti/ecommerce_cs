export type SidebarMenuItem = {
  id: string
  label: string
  to: string
  match?: 'exact' | 'prefix'
}

export type SidebarMenuSection = {
  id: string
  label?: string
  items: SidebarMenuItem[]
}

export const DEFAULT_SHELL_MAIN_ITEM: SidebarMenuItem = {
  id: 'dashboard',
  label: 'Dashboard',
  to: '/dashboard',
  match: 'exact',
}

export const DEFAULT_SHELL_SECTIONS: SidebarMenuSection[] = [
  {
    id: 'scope',
    label: 'Escopo',
    items: [
      { id: 'platform', label: 'Plataforma (SAS)', to: '/platform', match: 'prefix' },
      { id: 'tenant', label: 'Loja (Tenant)', to: '/tenant', match: 'prefix' },
    ],
  },
  {
    id: 'modules',
    label: 'Módulos',
    items: [
      {
        id: 'example',
        label: 'Examples',
        to: '/example',
        match: 'prefix',
      },
    ],
  },
]

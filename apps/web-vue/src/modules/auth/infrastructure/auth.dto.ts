export type LoginResponseDto = {
  accessToken: string
  expiresAt: string
  userId: string
  tenantId: number
  userLevel: string
  permissionKeys: string[]
}

export type MeResponseDto = {
  id: string
  legacyUserId: number
  tenantId: number
  name: string
  email?: string
  userLevel: string
  permissionKeys: string[]
}

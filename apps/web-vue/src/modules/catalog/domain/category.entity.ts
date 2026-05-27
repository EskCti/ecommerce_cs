import { err, ok, type Result } from '@/shared/result'

export type CategoryData = {
  readonly id: string
  readonly name: string
  readonly isActive: boolean
  readonly createdAt?: string
  readonly updatedAt?: string
}

export class CategoryEntity implements CategoryData {
  readonly id: string
  readonly name: string
  readonly isActive: boolean
  readonly createdAt?: string
  readonly updatedAt?: string

  private constructor(data: CategoryData) {
    this.id = data.id
    this.name = data.name
    this.isActive = data.isActive
    this.createdAt = data.createdAt
    this.updatedAt = data.updatedAt
  }

  static create(data: CategoryData): Result<CategoryEntity> {
    if (!data.id?.trim()) return err('Id da categoria é obrigatório')
    if (!data.name?.trim()) return err('Nome da categoria é obrigatório')
    return ok(new CategoryEntity({ ...data, name: data.name.trim() }))
  }

  static fromApi(data: CategoryData): Result<CategoryEntity> {
    return CategoryEntity.create(data)
  }
}

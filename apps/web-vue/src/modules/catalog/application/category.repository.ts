import type { CategoryEntity } from '../domain/category.entity'
import type { Result } from '@/shared/result'

export type CategoryInput = {
  name: string
  isActive?: boolean
}

export type CategoryUpdateInput = {
  name?: string
  isActive?: boolean
}

export type CategoryListFilter = {
  name?: string
  isActive?: boolean
  page?: number
  pageSize?: number
}

export interface ICategoryRepository {
  list(filter: CategoryListFilter): Promise<Result<CategoryEntity[]>>
  create(input: CategoryInput): Promise<Result<CategoryEntity>>
  update(id: string, input: CategoryUpdateInput): Promise<Result<CategoryEntity>>
  deactivate(id: string): Promise<Result<void>>
}

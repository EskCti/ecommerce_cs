import { ok, type Result } from '@/shared/result'
import type {
  CategoryInput,
  CategoryListFilter,
  CategoryUpdateInput,
  ICategoryRepository,
} from './category.repository'
import type { CategoryEntity } from '../domain/category.entity'

export class ListCategoriesUseCase {
  constructor(private readonly repository: ICategoryRepository) {}
  execute(filter: CategoryListFilter): Promise<Result<CategoryEntity[]>> {
    return this.repository.list(filter)
  }
}

export class CreateCategoryUseCase {
  constructor(private readonly repository: ICategoryRepository) {}
  execute(input: CategoryInput): Promise<Result<CategoryEntity>> {
    return this.repository.create(input)
  }
}

export class UpdateCategoryUseCase {
  constructor(private readonly repository: ICategoryRepository) {}
  execute(id: string, input: CategoryUpdateInput): Promise<Result<CategoryEntity>> {
    return this.repository.update(id, input)
  }
}

export class DeactivateCategoryUseCase {
  constructor(private readonly repository: ICategoryRepository) {}
  async execute(id: string): Promise<Result<void>> {
    const result = await this.repository.deactivate(id)
    if (!result.ok) return result
    return ok(undefined)
  }
}

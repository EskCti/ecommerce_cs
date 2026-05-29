import type { Result } from '@/shared/result'
import type {
  ConfigureGradeInput,
  GradeConfiguration,
  IGradeRepository,
} from './grade.repository'

export class ListGradesUseCase {
  constructor(private readonly repository: IGradeRepository) {}
  execute(productId: string): Promise<Result<GradeConfiguration>> {
    return this.repository.listGrades(productId)
  }
}

export class ConfigureGradeUseCase {
  constructor(private readonly repository: IGradeRepository) {}
  execute(productId: string, input: ConfigureGradeInput): Promise<Result<GradeConfiguration>> {
    return this.repository.configureGrade(productId, input)
  }
}

export class AdjustVariantStockUseCase {
  constructor(private readonly repository: IGradeRepository) {}
  execute(productId: string, variantId: string, stock: number): Promise<Result<GradeConfiguration>> {
    return this.repository.configureGrade(productId, {
      action: 'AdjustVariantStock',
      variantId,
      stock,
    })
  }
}

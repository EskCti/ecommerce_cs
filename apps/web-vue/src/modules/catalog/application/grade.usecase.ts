import type { Result } from '@/shared/result'
import type {
  ConfigureGradeInput,
  GradeDimension,
  IGradeRepository,
} from './grade.repository'

export class ListGradesUseCase {
  constructor(private readonly repository: IGradeRepository) {}
  execute(productId: string): Promise<Result<GradeDimension[]>> {
    return this.repository.listGrades(productId)
  }
}

export class ConfigureGradeUseCase {
  constructor(private readonly repository: IGradeRepository) {}
  execute(productId: string, input: ConfigureGradeInput): Promise<Result<GradeDimension[]>> {
    return this.repository.configureGrade(productId, input)
  }
}

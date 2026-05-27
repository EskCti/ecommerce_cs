import type { Result } from '@/shared/result'

export type GradeOption = {
  id: string
  dimensionId: string
  label: string
  stock: number
}

export type GradeDimension = {
  id: string
  productId: string
  name: string
  options: GradeOption[]
}

export type ConfigureGradeInput = {
  action: 'AddDimension' | 'AddOption' | 'RemoveGrade' | 'AdjustGradeStock'
  dimensionName?: string
  dimensionId?: string
  optionId?: string
  optionLabel?: string
  stock?: number
}

export interface IGradeRepository {
  listGrades(productId: string): Promise<Result<GradeDimension[]>>
  configureGrade(productId: string, input: ConfigureGradeInput): Promise<Result<GradeDimension[]>>
}

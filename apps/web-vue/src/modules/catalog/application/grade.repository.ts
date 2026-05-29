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

export type GradeVariant = {
  id: string
  productId: string
  optionIds: string[]
  label: string
  stock: number
}

export type GradeConfiguration = {
  dimensions: GradeDimension[]
  variants: GradeVariant[]
}

export type ConfigureGradeInput = {
  action:
    | 'AddDimension'
    | 'AddOption'
    | 'RemoveGrade'
    | 'AdjustGradeStock'
    | 'AddVariant'
    | 'AdjustVariantStock'
    | 'RemoveVariant'
  dimensionName?: string
  dimensionId?: string
  optionId?: string
  optionLabel?: string
  stock?: number
  variantId?: string
  optionIds?: string[]
}

export interface IGradeRepository {
  listGrades(productId: string): Promise<Result<GradeConfiguration>>
  configureGrade(productId: string, input: ConfigureGradeInput): Promise<Result<GradeConfiguration>>
}

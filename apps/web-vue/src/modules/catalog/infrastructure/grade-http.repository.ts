import { parseApiError } from '@/shared/parse-api-error'
import { err, ok, type Result } from '@/shared/result'
import type {
  ConfigureGradeInput,
  GradeConfiguration,
  GradeDimension,
  GradeVariant,
  IGradeRepository,
} from '../application/grade.repository'

type TokenProvider = () => string | null

function mapConfiguration(body: unknown): GradeConfiguration {
  if (!body || typeof body !== 'object') {
    return { dimensions: [], variants: [] }
  }
  const record = body as Record<string, unknown>
  if (Array.isArray(record)) {
    return { dimensions: record as GradeDimension[], variants: [] }
  }
  return {
    dimensions: (record.dimensions ?? record.gradeDimensions ?? []) as GradeDimension[],
    variants: (record.variants ?? record.gradeVariants ?? []) as GradeVariant[],
  }
}

export class GradeHttpRepository implements IGradeRepository {
  constructor(private readonly getToken: TokenProvider) {}

  private headers() {
    const token = this.getToken()
    return {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    }
  }

  async listGrades(productId: string): Promise<Result<GradeConfiguration>> {
    try {
      const res = await fetch(`/api/catalog/products/${productId}/grades`, {
        headers: this.headers(),
      })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao listar grades'))
      return ok(mapConfiguration(await res.json()))
    } catch {
      return err('Erro de rede')
    }
  }

  async configureGrade(
    productId: string,
    input: ConfigureGradeInput,
  ): Promise<Result<GradeConfiguration>> {
    try {
      const res = await fetch(`/api/catalog/products/${productId}/grades`, {
        method: 'POST',
        headers: this.headers(),
        body: JSON.stringify(input),
      })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao configurar grade'))
      return ok(mapConfiguration(await res.json()))
    } catch {
      return err('Erro de rede')
    }
  }
}

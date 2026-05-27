import { parseApiError } from '@/shared/parse-api-error'
import { err, ok, type Result } from '@/shared/result'
import type {
  ConfigureGradeInput,
  GradeDimension,
  IGradeRepository,
} from '../application/grade.repository'

type TokenProvider = () => string | null

export class GradeHttpRepository implements IGradeRepository {
  constructor(private readonly getToken: TokenProvider) {}

  private headers() {
    const token = this.getToken()
    return {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    }
  }

  async listGrades(productId: string): Promise<Result<GradeDimension[]>> {
    try {
      const res = await fetch(`/api/catalog/products/${productId}/grades`, {
        headers: this.headers(),
      })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao listar grades'))
      return ok((await res.json()) as GradeDimension[])
    } catch {
      return err('Erro de rede')
    }
  }

  async configureGrade(
    productId: string,
    input: ConfigureGradeInput,
  ): Promise<Result<GradeDimension[]>> {
    try {
      const res = await fetch(`/api/catalog/products/${productId}/grades`, {
        method: 'POST',
        headers: this.headers(),
        body: JSON.stringify(input),
      })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao configurar grade'))
      const body = await res.json()
      if (Array.isArray(body)) return ok(body as GradeDimension[])
      if (body.gradeDimensions) return ok(body.gradeDimensions as GradeDimension[])
      return ok([])
    } catch {
      return err('Erro de rede')
    }
  }
}

import type { Result } from '@/shared/result'
import type { ExchangeEntity, RegisterExchangeFormInput } from '../domain/exchange.entity'
import { validateRegisterExchangeForm } from '../domain/exchange.entity'
import type { ExchangeListFilter, IReturnsRepository } from './returns.repository'

export class RegisterExchangeUseCase {
  constructor(private readonly repository: IReturnsRepository) {}

  execute(input: RegisterExchangeFormInput): Promise<Result<ExchangeEntity>> {
    const validated = validateRegisterExchangeForm(input)
    if (!validated.ok) return Promise.resolve(validated)
    return this.repository.registerExchange(validated.data)
  }
}

export class ListExchangesUseCase {
  constructor(private readonly repository: IReturnsRepository) {}

  execute(filter: ExchangeListFilter = {}) {
    return this.repository.listExchanges(filter)
  }
}

export class DeleteExchangeUseCase {
  constructor(private readonly repository: IReturnsRepository) {}

  execute(id: string): Promise<Result<ExchangeEntity>> {
    if (!id?.trim()) return Promise.resolve({ ok: false, error: 'Id da troca é obrigatório' })
    return this.repository.deleteExchange(id)
  }
}

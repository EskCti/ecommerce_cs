import type { Result } from '@/shared/result'
import { CommissionEntity, PayableEntity, ReceivableEntity } from '../domain/finance.entity'
import type { IFinanceRepository, PagedResult } from './finance.repository'

export class ListReceivablesUseCase {
  constructor(private readonly repo: IFinanceRepository) {}

  async execute(params: Parameters<IFinanceRepository['listReceivables']>[0]) {
    const result = await this.repo.listReceivables(params)
    if (!result.ok) return result
    const items: ReceivableEntity[] = []
    for (const row of result.data.items) {
      const mapped = ReceivableEntity.fromApi(row)
      if (mapped.ok) items.push(mapped.data)
    }
    return { ok: true as const, data: { ...result.data, items } satisfies PagedResult<ReceivableEntity> }
  }
}

export class CreateReceivableUseCase {
  constructor(private readonly repo: IFinanceRepository) {}
  execute(input: Parameters<IFinanceRepository['createReceivable']>[0]) {
    return this.repo.createReceivable(input)
  }
}

export class SettleReceivableUseCase {
  constructor(private readonly repo: IFinanceRepository) {}
  execute(id: string, settlementDate: string) {
    return this.repo.settleReceivable(id, settlementDate)
  }
}

export class ListPayablesUseCase {
  constructor(private readonly repo: IFinanceRepository) {}

  async execute(params: Parameters<IFinanceRepository['listPayables']>[0]) {
    const result = await this.repo.listPayables(params)
    if (!result.ok) return result
    const items: PayableEntity[] = []
    for (const row of result.data.items) {
      const mapped = PayableEntity.fromApi(row)
      if (mapped.ok) items.push(mapped.data)
    }
    return { ok: true as const, data: { ...result.data, items } satisfies PagedResult<PayableEntity> }
  }
}

export class CreatePayableUseCase {
  constructor(private readonly repo: IFinanceRepository) {}
  execute(input: Parameters<IFinanceRepository['createPayable']>[0]) {
    return this.repo.createPayable(input)
  }
}

export class SettlePayableUseCase {
  constructor(private readonly repo: IFinanceRepository) {}
  execute(id: string, settlementDate: string) {
    return this.repo.settlePayable(id, settlementDate)
  }
}

export class ListPurchasesUseCase {
  constructor(private readonly repo: IFinanceRepository) {}

  async execute(params: Parameters<IFinanceRepository['listPurchases']>[0]) {
    const result = await this.repo.listPurchases(params)
    if (!result.ok) return result
    const items: PayableEntity[] = []
    for (const row of result.data.items) {
      const mapped = PayableEntity.fromApi(row)
      if (mapped.ok) items.push(mapped.data)
    }
    return { ok: true as const, data: { ...result.data, items } satisfies PagedResult<PayableEntity> }
  }
}

export class ListCommissionsUseCase {
  constructor(private readonly repo: IFinanceRepository) {}

  async execute(params: Parameters<IFinanceRepository['listCommissions']>[0]) {
    const result = await this.repo.listCommissions(params)
    if (!result.ok) return result
    const items: CommissionEntity[] = []
    for (const row of result.data.items) {
      const mapped = CommissionEntity.fromApi(row)
      if (mapped.ok) items.push(mapped.data)
    }
    return { ok: true as const, data: { ...result.data, items } satisfies PagedResult<CommissionEntity> }
  }
}

export class PayCommissionUseCase {
  constructor(private readonly repo: IFinanceRepository) {}
  execute(id: string, settlementDate: string) {
    return this.repo.payCommission(id, settlementDate)
  }
}

export class PayCommissionsBatchUseCase {
  constructor(private readonly repo: IFinanceRepository) {}
  execute(commissionIds: string[], paymentDate: string): Promise<Result<CommissionEntity[]>> {
    return this.repo.payCommissionsBatch(commissionIds, paymentDate) as Promise<Result<CommissionEntity[]>>
  }
}

export class GetCashFlowUseCase {
  constructor(private readonly repo: IFinanceRepository) {}
  execute(from: string, to: string) {
    return this.repo.getCashFlow(from, to)
  }
}

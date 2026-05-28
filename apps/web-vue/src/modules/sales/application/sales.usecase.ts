import type { Result } from '@/shared/result'
import type { CashSessionEntity } from '../domain/cash-session.entity'
import type { SaleEntity, SaleListPageEntity } from '../domain/sale.entity'
import type {
  AddCartItemInput,
  CashWithdrawalInput,
  CloseCashSessionInput,
  ConfirmGradeInput,
  FinalizeSaleInput,
  ISalesRepository,
  OpenCashSessionInput,
  SaleListFilter,
} from './sales.repository'

export class OpenCashSessionUseCase {
  constructor(private readonly repository: ISalesRepository) {}
  execute(input: OpenCashSessionInput): Promise<Result<CashSessionEntity>> {
    return this.repository.openSession(input)
  }
}

export class GetCurrentCashSessionUseCase {
  constructor(private readonly repository: ISalesRepository) {}
  execute(): Promise<Result<CashSessionEntity | null>> {
    return this.repository.getCurrentSession()
  }
}

export class AddCartItemUseCase {
  constructor(private readonly repository: ISalesRepository) {}
  execute(input: AddCartItemInput): Promise<Result<CashSessionEntity>> {
    return this.repository.addItem(input)
  }
}

export class ConfirmGradeForItemUseCase {
  constructor(private readonly repository: ISalesRepository) {}
  execute(input: ConfirmGradeInput): Promise<Result<CashSessionEntity>> {
    return this.repository.confirmGrade(input)
  }
}

export class RemoveCartLineUseCase {
  constructor(private readonly repository: ISalesRepository) {}
  execute(lineId: string): Promise<Result<CashSessionEntity>> {
    return this.repository.removeItem(lineId)
  }
}

export class RegisterCashWithdrawalUseCase {
  constructor(private readonly repository: ISalesRepository) {}
  execute(input: CashWithdrawalInput): Promise<Result<CashSessionEntity>> {
    return this.repository.withdrawal(input)
  }
}

export class CloseCashSessionUseCase {
  constructor(private readonly repository: ISalesRepository) {}
  execute(input: CloseCashSessionInput): Promise<Result<CashSessionEntity>> {
    return this.repository.closeSession(input)
  }
}

export class FinalizeSaleUseCase {
  constructor(private readonly repository: ISalesRepository) {}
  execute(input: FinalizeSaleInput): Promise<Result<SaleEntity>> {
    return this.repository.finalizeSale(input)
  }
}

export class ListSalesUseCase {
  constructor(private readonly repository: ISalesRepository) {}
  execute(filter: SaleListFilter): Promise<Result<SaleListPageEntity>> {
    return this.repository.listSales(filter)
  }
}

export class CancelSaleUseCase {
  constructor(private readonly repository: ISalesRepository) {}
  execute(saleId: string): Promise<Result<SaleEntity>> {
    return this.repository.cancelSale(saleId)
  }
}

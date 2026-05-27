import { parseApiError } from '@/shared/parse-api-error'
import { err, ok, type Result } from '@/shared/result'
import { StoreConfigEntity } from '../domain/store-config.entity'
import { PaymentMethodEntity } from '../domain/payment-method.entity'
import { CashRegisterTerminalEntity } from '../domain/cash-register-terminal.entity'
import type {
  CashRegisterInput,
  IStoreSettingsRepository,
  PaymentMethodInput,
} from '../application/store-settings.repository'

type TokenProvider = () => string | null

export class StoreSettingsHttpRepository implements IStoreSettingsRepository {
  constructor(private readonly getToken: TokenProvider) {}

  private headers() {
    const token = this.getToken()
    return {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    }
  }

  async getStoreConfig(): Promise<Result<StoreConfigEntity>> {
    try {
      const res = await fetch('/api/settings/store-config', { headers: this.headers() })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao carregar configuração da loja'))
      return StoreConfigEntity.fromApiResponse(await res.json())
    } catch {
      return err('Erro de rede')
    }
  }

  async updateStoreConfig(config: StoreConfigEntity): Promise<Result<StoreConfigEntity>> {
    try {
      const res = await fetch('/api/settings/store-config', {
        method: 'PUT',
        headers: this.headers(),
        body: JSON.stringify(config.toApiRequest()),
      })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao salvar configuração da loja'))
      return StoreConfigEntity.fromApiResponse(await res.json())
    } catch {
      return err('Erro de rede')
    }
  }

  async listPaymentMethods(): Promise<Result<PaymentMethodEntity[]>> {
    try {
      const res = await fetch('/api/settings/payment-methods', { headers: this.headers() })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao listar formas de pagamento'))
      const data = (await res.json()) as Array<{
        id: string
        name: string
        surchargePercent: number
        isActive: boolean
        createdAt: string
        updatedAt: string
      }>
      const items: PaymentMethodEntity[] = []
      for (const row of data) {
        const mapped = PaymentMethodEntity.fromApi(row)
        if (mapped.ok) items.push(mapped.data)
      }
      return ok(items)
    } catch {
      return err('Erro de rede')
    }
  }

  async createPaymentMethod(input: PaymentMethodInput): Promise<Result<PaymentMethodEntity>> {
    try {
      const res = await fetch('/api/settings/payment-methods', {
        method: 'POST',
        headers: this.headers(),
        body: JSON.stringify(input),
      })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao criar forma de pagamento'))
      return PaymentMethodEntity.fromApi(await res.json())
    } catch {
      return err('Erro de rede')
    }
  }

  async updatePaymentMethod(
    id: string,
    input: Partial<PaymentMethodInput>,
  ): Promise<Result<PaymentMethodEntity>> {
    try {
      const res = await fetch(`/api/settings/payment-methods/${id}`, {
        method: 'PUT',
        headers: this.headers(),
        body: JSON.stringify(input),
      })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao atualizar forma de pagamento'))
      return PaymentMethodEntity.fromApi(await res.json())
    } catch {
      return err('Erro de rede')
    }
  }

  async deletePaymentMethod(id: string): Promise<Result<void>> {
    try {
      const res = await fetch(`/api/settings/payment-methods/${id}`, {
        method: 'DELETE',
        headers: this.headers(),
      })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao excluir forma de pagamento'))
      return ok(undefined)
    } catch {
      return err('Erro de rede')
    }
  }

  async listCashRegisters(): Promise<Result<CashRegisterTerminalEntity[]>> {
    try {
      const res = await fetch('/api/settings/cash-registers', { headers: this.headers() })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao listar caixas'))
      const data = (await res.json()) as Array<{
        id: string
        name: string
        status: string
        assignedOperatorId?: string | null
        assignedOperatorName?: string | null
        createdAt: string
        updatedAt: string
      }>
      const items: CashRegisterTerminalEntity[] = []
      for (const row of data) {
        const mapped = CashRegisterTerminalEntity.fromApi(row)
        if (mapped.ok) items.push(mapped.data)
      }
      return ok(items)
    } catch {
      return err('Erro de rede')
    }
  }

  async createCashRegister(input: CashRegisterInput): Promise<Result<CashRegisterTerminalEntity>> {
    try {
      const res = await fetch('/api/settings/cash-registers', {
        method: 'POST',
        headers: this.headers(),
        body: JSON.stringify(input),
      })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao criar caixa'))
      return CashRegisterTerminalEntity.fromApi(await res.json())
    } catch {
      return err('Erro de rede')
    }
  }

  async updateCashRegister(
    id: string,
    input: Partial<CashRegisterInput>,
  ): Promise<Result<CashRegisterTerminalEntity>> {
    try {
      const res = await fetch(`/api/settings/cash-registers/${id}`, {
        method: 'PUT',
        headers: this.headers(),
        body: JSON.stringify(input),
      })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao atualizar caixa'))
      return CashRegisterTerminalEntity.fromApi(await res.json())
    } catch {
      return err('Erro de rede')
    }
  }

  async deleteCashRegister(id: string): Promise<Result<void>> {
    try {
      const res = await fetch(`/api/settings/cash-registers/${id}`, {
        method: 'DELETE',
        headers: this.headers(),
      })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao excluir caixa'))
      return ok(undefined)
    } catch {
      return err('Erro de rede')
    }
  }
}

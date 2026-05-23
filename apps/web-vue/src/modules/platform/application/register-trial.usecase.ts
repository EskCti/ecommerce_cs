import { err, type Result } from '@/shared/result'
import type { ITrialRepository, RegisterTrialInput, RegisterTrialResult } from './platform.repository'

export class RegisterTrialUseCase {
  constructor(private readonly repo: ITrialRepository) {}

  async execute(input: RegisterTrialInput): Promise<Result<RegisterTrialResult>> {
    if (!input.companyName.trim()) return err('Informe o nome da empresa')
    if (!input.adminEmail.trim()) return err('Informe o e-mail do administrador')
    if (input.adminPassword.length < 6) return err('Senha deve ter ao menos 6 caracteres')
    return this.repo.registerTrial(input)
  }
}

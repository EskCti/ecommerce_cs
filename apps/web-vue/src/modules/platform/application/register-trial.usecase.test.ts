import { describe, expect, it, vi } from 'vitest'
import { RegisterTrialUseCase } from './register-trial.usecase'
import type { ITrialRepository } from './platform.repository'

describe('RegisterTrialUseCase', () => {
  it('rejects short password', async () => {
    const repo: ITrialRepository = { registerTrial: vi.fn() }
    const result = await new RegisterTrialUseCase(repo).execute({
      companyName: 'Loja',
      adminName: 'Admin',
      adminEmail: 'a@b.com',
      adminPassword: '123',
    })
    expect(result.ok).toBe(false)
  })
})

import { describe, expect, it } from 'vitest'
import { GradeVariantEntity } from './grade-variant.entity'

describe('GradeVariantEntity', () => {
  it('fromApi maps label and stock', () => {
    const entity = GradeVariantEntity.fromApi({
      id: 'v1',
      productId: 'p1',
      optionIds: ['o1', 'o2'],
      label: 'Azul / P',
      stock: 7,
    })

    expect(entity.label).toBe('Azul / P')
    expect(entity.stock).toBe(7)
    expect(entity.optionIds).toEqual(['o1', 'o2'])
  })

  it('matchesOptions compares sorted option ids', () => {
    const entity = GradeVariantEntity.fromApi({
      id: 'v1',
      productId: 'p1',
      optionIds: ['o2', 'o1'],
      label: 'Azul / P',
      stock: 1,
    })

    expect(entity.matchesOptions(['o1', 'o2'])).toBe(true)
    expect(entity.matchesOptions(['o1'])).toBe(false)
  })
})

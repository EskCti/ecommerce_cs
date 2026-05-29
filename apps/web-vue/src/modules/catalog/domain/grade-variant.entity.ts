export type GradeVariantApi = {
  id: string
  productId: string
  optionIds: string[]
  label: string
  stock: number
}

export class GradeVariantEntity {
  constructor(
    readonly id: string,
    readonly productId: string,
    readonly optionIds: readonly string[],
    readonly label: string,
    readonly stock: number,
  ) {}

  static fromApi(raw: GradeVariantApi): GradeVariantEntity {
    return new GradeVariantEntity(
      raw.id,
      raw.productId,
      raw.optionIds ?? [],
      raw.label ?? '',
      Number(raw.stock) || 0,
    )
  }

  matchesOptions(optionIds: string[]): boolean {
    const a = [...this.optionIds].sort().join(',')
    const b = [...optionIds].sort().join(',')
    return a === b && a.length > 0
  }
}

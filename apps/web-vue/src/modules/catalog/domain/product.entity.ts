import { err, ok, type Result } from '@/shared/result'

export type ProductData = {
  readonly id: string
  readonly barcode: string
  readonly name: string
  readonly description?: string
  readonly salePrice: number
  readonly costPrice: number
  readonly stock: number
  readonly profitMargin?: number
  readonly stockAlertLevel: number
  readonly categoryId: string
  readonly supplierId?: string
  readonly photoPath?: string
  readonly isActive: boolean
  readonly isOpenPrice: boolean
  readonly isLowStock?: boolean
  readonly createdAt?: string
  readonly updatedAt?: string
}

export function isValidBarcode(value: string): boolean {
  const trimmed = value.trim()
  return trimmed.length > 0 && trimmed.length <= 50
}

export function isOpenPriceSale(salePrice: number): boolean {
  return salePrice === 0
}

export class ProductEntity implements ProductData {
  readonly id: string
  readonly barcode: string
  readonly name: string
  readonly description?: string
  readonly salePrice: number
  readonly costPrice: number
  readonly stock: number
  readonly profitMargin?: number
  readonly stockAlertLevel: number
  readonly categoryId: string
  readonly supplierId?: string
  readonly photoPath?: string
  readonly isActive: boolean
  readonly isOpenPrice: boolean
  readonly isLowStock?: boolean
  readonly createdAt?: string
  readonly updatedAt?: string

  private constructor(data: ProductData) {
    this.id = data.id
    this.barcode = data.barcode
    this.name = data.name
    this.description = data.description
    this.salePrice = data.salePrice
    this.costPrice = data.costPrice
    this.stock = data.stock
    this.profitMargin = data.profitMargin
    this.stockAlertLevel = data.stockAlertLevel
    this.categoryId = data.categoryId
    this.supplierId = data.supplierId
    this.photoPath = data.photoPath
    this.isActive = data.isActive
    this.isOpenPrice = data.isOpenPrice
    this.isLowStock = data.isLowStock
    this.createdAt = data.createdAt
    this.updatedAt = data.updatedAt
  }

  static create(data: ProductData): Result<ProductEntity> {
    if (!data.id?.trim()) return err('Id do produto é obrigatório')
    if (!data.name?.trim()) return err('Nome do produto é obrigatório')
    if (!isValidBarcode(data.barcode)) return err('Código de barras inválido')
    if (!data.categoryId?.trim()) return err('Categoria é obrigatória')
    if (data.salePrice < 0 || data.costPrice < 0) return err('Preços não podem ser negativos')
    if (data.stock < 0) return err('Estoque não pode ser negativo')
    if (data.stockAlertLevel < 0) return err('Nível de alerta inválido')

    const openPrice = isOpenPriceSale(data.salePrice)
    if (openPrice !== data.isOpenPrice) {
      return err('Preço aberto exige valor de venda zero')
    }

    return ok(
      new ProductEntity({
        ...data,
        barcode: data.barcode.trim(),
        name: data.name.trim(),
        isOpenPrice: openPrice,
      }),
    )
  }

  static fromApi(data: ProductData): Result<ProductEntity> {
    return ProductEntity.create({
      ...data,
      isOpenPrice: data.isOpenPrice ?? isOpenPriceSale(data.salePrice),
    })
  }
}

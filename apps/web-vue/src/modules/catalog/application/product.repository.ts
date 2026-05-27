import type { ProductEntity } from '../domain/product.entity'
import type { Result } from '@/shared/result'

export type ProductInput = {
  barcode: string
  name: string
  description?: string
  salePrice: number
  costPrice: number
  initialStock: number
  stockAlertLevel: number
  categoryId: string
  supplierId?: string
  photoPath?: string
  isActive?: boolean
}

export type ProductUpdateInput = {
  name?: string
  description?: string
  salePrice?: number
  costPrice?: number
  stockAlertLevel?: number
  categoryId?: string
  supplierId?: string
  photoPath?: string
  isActive?: boolean
  clearSupplier?: boolean
}

export type ProductListFilter = {
  barcode?: string
  name?: string
  categoryId?: string
  isActive?: boolean
  page?: number
  pageSize?: number
}

export type ProductListPage = {
  items: ProductEntity[]
  total: number
  page: number
  pageSize: number
}

export type GenerateBarcodeOutput = {
  barcode: string
}

export interface IProductRepository {
  list(filter: ProductListFilter): Promise<Result<ProductListPage>>
  create(input: ProductInput): Promise<Result<ProductEntity>>
  update(id: string, input: ProductUpdateInput): Promise<Result<ProductEntity>>
  deactivate(id: string): Promise<Result<void>>
  generateBarcode(): Promise<Result<GenerateBarcodeOutput>>
  findByBarcode(code: string): Promise<Result<ProductEntity>>
}

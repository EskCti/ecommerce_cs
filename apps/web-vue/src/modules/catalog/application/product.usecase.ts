import { ok, type Result } from '@/shared/result'
import type {
  GenerateBarcodeOutput,
  IProductRepository,
  ProductInput,
  ProductListFilter,
  ProductListPage,
  ProductUpdateInput,
} from './product.repository'
import type { ProductEntity } from '../domain/product.entity'

export class ListProductsUseCase {
  constructor(private readonly repository: IProductRepository) {}
  execute(filter: ProductListFilter): Promise<Result<ProductListPage>> {
    return this.repository.list(filter)
  }
}

export class CreateProductUseCase {
  constructor(private readonly repository: IProductRepository) {}
  execute(input: ProductInput): Promise<Result<ProductEntity>> {
    return this.repository.create(input)
  }
}

export class UpdateProductUseCase {
  constructor(private readonly repository: IProductRepository) {}
  execute(id: string, input: ProductUpdateInput): Promise<Result<ProductEntity>> {
    return this.repository.update(id, input)
  }
}

export class DeactivateProductUseCase {
  constructor(private readonly repository: IProductRepository) {}
  async execute(id: string): Promise<Result<void>> {
    const result = await this.repository.deactivate(id)
    if (!result.ok) return result
    return ok(undefined)
  }
}

export class GenerateBarcodeUseCase {
  constructor(private readonly repository: IProductRepository) {}
  execute(): Promise<Result<GenerateBarcodeOutput>> {
    return this.repository.generateBarcode()
  }
}

export class FindByBarcodeUseCase {
  constructor(private readonly repository: IProductRepository) {}
  execute(code: string): Promise<Result<ProductEntity>> {
    return this.repository.findByBarcode(code)
  }
}

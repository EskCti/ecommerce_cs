import { parseApiError } from '@/shared/parse-api-error'
import { err, ok, type Result } from '@/shared/result'
import { ProductEntity } from '../domain/product.entity'
import type {
  GenerateBarcodeOutput,
  IProductRepository,
  ProductInput,
  ProductListFilter,
  ProductListPage,
  ProductUpdateInput,
} from '../application/product.repository'

type TokenProvider = () => string | null

type ProductListItemDto = {
  id: string
  barcode: string
  name: string
  salePrice: number
  stock: number
  categoryId: string
  isActive: boolean
  isLowStock: boolean
}

type ProductOutputDto = {
  id: string
  tenantId: number
  barcode: string
  name: string
  description?: string
  salePrice: number
  costPrice: number
  stock: number
  profitMargin: number
  stockAlertLevel: number
  categoryId: string
  supplierId?: string
  photoPath?: string
  isActive: boolean
  isOpenPrice: boolean
  isLowStock: boolean
  createdAt: string
  updatedAt: string
}

type PaginatedProductsDto = {
  items: ProductListItemDto[]
  page: number
  pageSize: number
  totalCount: number
}

function mapListItem(row: ProductListItemDto): Result<ProductEntity> {
  return ProductEntity.fromApi({
    id: row.id,
    barcode: row.barcode,
    name: row.name,
    salePrice: row.salePrice,
    costPrice: 0,
    stock: row.stock,
    stockAlertLevel: 0,
    categoryId: row.categoryId,
    isActive: row.isActive,
    isOpenPrice: row.salePrice === 0,
    isLowStock: row.isLowStock,
  })
}

function mapOutput(row: ProductOutputDto): Result<ProductEntity> {
  return ProductEntity.fromApi({
    id: row.id,
    barcode: row.barcode,
    name: row.name,
    description: row.description,
    salePrice: row.salePrice,
    costPrice: row.costPrice,
    stock: row.stock,
    profitMargin: row.profitMargin,
    stockAlertLevel: row.stockAlertLevel,
    categoryId: row.categoryId,
    supplierId: row.supplierId,
    photoPath: row.photoPath,
    isActive: row.isActive,
    isOpenPrice: row.isOpenPrice,
    isLowStock: row.isLowStock,
    createdAt: row.createdAt,
    updatedAt: row.updatedAt,
  })
}

export class ProductHttpRepository implements IProductRepository {
  constructor(private readonly getToken: TokenProvider) {}

  private headers() {
    const token = this.getToken()
    return {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    }
  }

  async list(filter: ProductListFilter): Promise<Result<ProductListPage>> {
    const params = new URLSearchParams()
    if (filter.barcode) params.set('barcode', filter.barcode)
    if (filter.name) params.set('name', filter.name)
    if (filter.categoryId) params.set('categoryId', filter.categoryId)
    if (filter.isActive !== undefined) params.set('isActive', String(filter.isActive))
    params.set('page', String(filter.page ?? 1))
    params.set('pageSize', String(filter.pageSize ?? 20))

    try {
      const res = await fetch(`/api/catalog/products?${params}`, { headers: this.headers() })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao listar produtos'))
      const body = (await res.json()) as PaginatedProductsDto
      const items: ProductEntity[] = []
      for (const row of body.items) {
        const mapped = mapListItem(row)
        if (mapped.ok) items.push(mapped.data)
      }
      return ok({
        items,
        total: body.totalCount,
        page: body.page,
        pageSize: body.pageSize,
      })
    } catch {
      return err('Erro de rede')
    }
  }

  async create(input: ProductInput): Promise<Result<ProductEntity>> {
    try {
      const res = await fetch('/api/catalog/products', {
        method: 'POST',
        headers: this.headers(),
        body: JSON.stringify(input),
      })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao criar produto'))
      return mapOutput((await res.json()) as ProductOutputDto)
    } catch {
      return err('Erro de rede')
    }
  }

  async update(id: string, input: ProductUpdateInput): Promise<Result<ProductEntity>> {
    try {
      const res = await fetch(`/api/catalog/products/${id}`, {
        method: 'PUT',
        headers: this.headers(),
        body: JSON.stringify(input),
      })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao atualizar produto'))
      return mapOutput((await res.json()) as ProductOutputDto)
    } catch {
      return err('Erro de rede')
    }
  }

  async deactivate(id: string): Promise<Result<void>> {
    try {
      const res = await fetch(`/api/catalog/products/${id}`, {
        method: 'DELETE',
        headers: this.headers(),
      })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao desativar produto'))
      return ok(undefined)
    } catch {
      return err('Erro de rede')
    }
  }

  async generateBarcode(): Promise<Result<GenerateBarcodeOutput>> {
    try {
      const res = await fetch('/api/catalog/products/generate-barcode', {
        method: 'POST',
        headers: this.headers(),
      })
      if (!res.ok) return err(await parseApiError(res, 'Falha ao gerar código de barras'))
      const body = (await res.json()) as { barcode: string }
      return ok({ barcode: body.barcode })
    } catch {
      return err('Erro de rede')
    }
  }

  async findByBarcode(code: string): Promise<Result<ProductEntity>> {
    try {
      const res = await fetch(`/api/catalog/products/by-barcode/${encodeURIComponent(code)}`, {
        headers: this.headers(),
      })
      if (res.status === 404) return err('Produto não encontrado')
      if (!res.ok) return err(await parseApiError(res, 'Falha ao buscar produto por código'))
      return mapOutput((await res.json()) as ProductOutputDto)
    } catch {
      return err('Erro de rede')
    }
  }
}

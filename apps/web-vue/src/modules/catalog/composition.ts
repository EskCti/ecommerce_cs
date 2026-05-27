import {
  CreateCategoryUseCase,
  DeactivateCategoryUseCase,
  ListCategoriesUseCase,
  UpdateCategoryUseCase,
} from './application/category.usecase'
import {
  CreateProductUseCase,
  DeactivateProductUseCase,
  FindByBarcodeUseCase,
  GenerateBarcodeUseCase,
  ListProductsUseCase,
  UpdateProductUseCase,
} from './application/product.usecase'
import {
  ListLowStockUseCase,
  PurchaseStockUseCase,
  RecordStockEntryUseCase,
  RecordStockExitUseCase,
} from './application/stock.usecase'
import { ConfigureGradeUseCase, ListGradesUseCase } from './application/grade.usecase'
import { CategoryHttpRepository } from './infrastructure/category-http.repository'
import { GradeHttpRepository } from './infrastructure/grade-http.repository'
import { ProductHttpRepository } from './infrastructure/product-http.repository'
import { StockHttpRepository } from './infrastructure/stock-http.repository'

export function createCatalogModule(getToken: () => string | null) {
  const productRepository = new ProductHttpRepository(getToken)
  const categoryRepository = new CategoryHttpRepository(getToken)
  const stockRepository = new StockHttpRepository(getToken)
  const gradeRepository = new GradeHttpRepository(getToken)

  return {
    listProductsUseCase: new ListProductsUseCase(productRepository),
    createProductUseCase: new CreateProductUseCase(productRepository),
    updateProductUseCase: new UpdateProductUseCase(productRepository),
    deactivateProductUseCase: new DeactivateProductUseCase(productRepository),
    generateBarcodeUseCase: new GenerateBarcodeUseCase(productRepository),
    findByBarcodeUseCase: new FindByBarcodeUseCase(productRepository),
    listCategoriesUseCase: new ListCategoriesUseCase(categoryRepository),
    createCategoryUseCase: new CreateCategoryUseCase(categoryRepository),
    updateCategoryUseCase: new UpdateCategoryUseCase(categoryRepository),
    deactivateCategoryUseCase: new DeactivateCategoryUseCase(categoryRepository),
    recordStockEntryUseCase: new RecordStockEntryUseCase(stockRepository),
    recordStockExitUseCase: new RecordStockExitUseCase(stockRepository),
    purchaseStockUseCase: new PurchaseStockUseCase(stockRepository),
    listLowStockUseCase: new ListLowStockUseCase(stockRepository),
    listGradesUseCase: new ListGradesUseCase(gradeRepository),
    configureGradeUseCase: new ConfigureGradeUseCase(gradeRepository),
  }
}

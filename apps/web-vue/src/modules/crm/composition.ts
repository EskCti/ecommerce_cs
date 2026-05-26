import {
  CreateCustomerUseCase,
  DeactivateCustomerUseCase,
  FindOrCreateByCpfUseCase,
  ListCustomersUseCase,
  UpdateCustomerUseCase,
} from './application/customer.usecase'
import {
  CreateSupplierUseCase,
  DeactivateSupplierUseCase,
  ListSuppliersUseCase,
  UpdateSupplierUseCase,
} from './application/supplier.usecase'
import { CustomerHttpRepository } from './infrastructure/customer-http.repository'
import { SupplierHttpRepository } from './infrastructure/supplier-http.repository'

export function createCrmModule(getToken: () => string | null) {
  const customerRepository = new CustomerHttpRepository(getToken)
  const supplierRepository = new SupplierHttpRepository(getToken)

  return {
    listCustomersUseCase: new ListCustomersUseCase(customerRepository),
    createCustomerUseCase: new CreateCustomerUseCase(customerRepository),
    updateCustomerUseCase: new UpdateCustomerUseCase(customerRepository),
    deactivateCustomerUseCase: new DeactivateCustomerUseCase(customerRepository),
    findOrCreateByCpfUseCase: new FindOrCreateByCpfUseCase(customerRepository),
    listSuppliersUseCase: new ListSuppliersUseCase(supplierRepository),
    createSupplierUseCase: new CreateSupplierUseCase(supplierRepository),
    updateSupplierUseCase: new UpdateSupplierUseCase(supplierRepository),
    deactivateSupplierUseCase: new DeactivateSupplierUseCase(supplierRepository),
  }
}

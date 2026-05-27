## ADDED Requirements

### Requirement: Product list on mobile

The Android app SHALL list and search products for field operators.

#### Scenario: Search product by barcode

- **WHEN** operator scans or enters barcode on mobile
- **THEN** app displays product details from catalog by-barcode API

### Requirement: Product detail screen

The Android app SHALL show stock level, price, and grade options when available.

#### Scenario: View graded product

- **WHEN** product has grade options
- **THEN** detail screen lists options with per-option stock quantities

### Requirement: Authenticated catalog access

Catalog mobile requests SHALL use tenant-scoped JWT like other modules.

#### Scenario: Unauthorized catalog request

- **WHEN** request lacks valid token
- **THEN** API returns 401 and app prompts login

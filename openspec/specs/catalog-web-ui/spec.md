## ADDED Requirements

### Requirement: Products and categories UI

The Vue app SHALL provide product and category management with PrimeVue DataTable and forms.

#### Scenario: Product list with filters

- **WHEN** manager opens catalog products page
- **THEN** DataTable shows paginated products with barcode and category filters

#### Scenario: Category management

- **WHEN** manager creates or deactivates category
- **THEN** changes persist via catalog categories API

### Requirement: Grade editor UI

The Vue app SHALL provide grade dimension and option editor for products per US-051.

#### Scenario: Configure grades in UI

- **WHEN** manager adds dimension and options in grade editor
- **THEN** UI saves via product grades API and displays updated stock per option

### Requirement: Stock movement UI

The Vue app SHALL provide forms for manual entry, exit, and purchase stock operations.

#### Scenario: Record stock entry from UI

- **WHEN** operator submits stock entry form
- **THEN** movement is created and product stock updates in UI

### Requirement: Low stock dashboard

The Vue app SHALL display low stock products dashboard per US-053.

#### Scenario: Low stock dashboard loads

- **WHEN** manager opens inventory alerts dashboard
- **THEN** list shows products below alert threshold with current and minimum levels

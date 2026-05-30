## ADDED Requirements

### Requirement: Legacy finance port

Infrastructure SHALL implement IFinanceLegacyPort mapping typed domain aggregates to legacy receber/pagar/comissoes tables.

#### Scenario: Manual receivable ACL mapping

- **WHEN** manual receivable is saved
- **THEN** LegacyReceivableMapper writes receber row with appropriate legacy discriminator without domain seeing column names

#### Scenario: Payable ACL mapping

- **WHEN** expense payable is saved
- **THEN** LegacyPayableMapper writes pagar row with correct tipo

### Requirement: Discriminator routing

ACL SHALL route legacy receber tipo Empresa to Platform port and tipo Venda coordination to Sales without creating duplicate Finance manual records.

#### Scenario: Empresa receivable routed

- **WHEN** legacy row has tipo Empresa
- **THEN** mapper delegates to Platform invoice port not ManualReceivable aggregate

### Requirement: Domain isolation

Finance domain projects SHALL NOT reference legacy table or column identifiers.

#### Scenario: Core layer inspection

- **WHEN** Finance.Core dependencies are analyzed
- **THEN** no Infrastructure.Legacy references exist

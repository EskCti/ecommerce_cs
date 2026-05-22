## ADDED Requirements

### Requirement: PDF renderer port

Reporting application SHALL render documents through IPdfRendererPort abstraction per RNF-008.

#### Scenario: PDF byte array returned

- **WHEN** query handler requests PDF render for a report model
- **THEN** port returns non-empty byte array with valid PDF header

### Requirement: QuestPDF adapter

Infrastructure SHALL implement IPdfRendererPort using QuestPDF library replacing legacy dompdf.

#### Scenario: Template-based render

- **WHEN** adapter receives templateId and model
- **THEN** QuestPDF document is composed and exported to bytes

### Requirement: Report format content type

Renderer SHALL honor ReportFormat from tenant Store Settings for response content type.

#### Scenario: PDF format

- **WHEN** tenant ReportFormat is PDF
- **THEN** API returns application/pdf

#### Scenario: HTML format

- **WHEN** tenant ReportFormat is HTML
- **THEN** API returns text/html equivalent content

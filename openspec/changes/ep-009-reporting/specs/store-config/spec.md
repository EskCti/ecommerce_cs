## MODIFIED Requirements

### Requirement: Store report format configuration

The system SHALL persist tenant report output format as PDF or HTML via ReportFormat value object. Reporting module SHALL read ReportFormat when rendering tenant reports and set HTTP content type accordingly.

#### Scenario: PDF format configured

- **WHEN** tenant ReportFormat is PDF and user exports any tenant report
- **THEN** response Content-Type is application/pdf

#### Scenario: HTML format configured

- **WHEN** tenant ReportFormat is HTML and user exports any tenant report
- **THEN** response Content-Type is text/html

## Why

Lojistas dependem do **digest diário WhatsApp** do legado (`api/mensagem.php`) para alertas de contas vencendo, estoque baixo e cobrança de mensalidade — disparado via `conexao.php` com API enviame (`RF-090`). Após Finance (EP-007) e Catalog operacionais, o **BC Notifications** (Generic) encapsula composição do resumo, idempotência diária (RN-070) e credenciais por tenant (RN-071) sem acoplar HTTP externo ao domínio.

## What Changes

- Novo módulo `RetailOps.Notifications` (Core, Application, Infrastructure)
- Entidades: `DailyDigestLog` (idempotência RN-070), opcional `NotificationSchedule` stub RF-091
- VOs: `PhoneNumber`, `MessageTemplate`, `DigestContent`, `WhatsAppCredentials`
- Serviço `DailyDigestComposer` agregando dados via ports read-only (Finance, Catalog, Platform)
- Handler `SendDailyTenantDigestHandler` orquestrando compose → send → log
- Port `IWhatsAppGatewayPort` + `EnviameWhatsAppAdapter` (substitui `file_get_contents`)
- ACL: `LegacyDigestQueryAdapter` durante parallel run
- Hosted service `DailyDigestBackgroundService` (substitui trigger em `conexao.php`)
- Config: token tenant sobrescreve global RN-071; `telefone_sistema` destino
- Testes unitários ≥95% + integração mock enviame

## Capabilities

### New Capabilities

- `daily-tenant-digest`: Envio digest diário WhatsApp por tenant RF-090
- `digest-composition`: Agregação contas vencendo hoje, estoque baixo, cobrança mensalidade
- `digest-scheduler`: Job hosted service com execução 1×/dia RN-070
- `whatsapp-gateway`: Adapter HTTP api.enviame.com.br
- `notifications-legacy-acl`: Queries read-only legado para composição durante parallel run

### Modified Capabilities

- `store-config`: Token WhatsApp e telefone sistema consumidos pelo digest; token tenant sobrescreve global RN-071

## Impact

- **Backend**: módulo Notifications pequeno (P); hosted service no ASP.NET Core host
- **Store Settings EP-003**: `ApiToken` / telefone para credenciais WhatsApp
- **Finance EP-007**: contas vencendo hoje via query port
- **Catalog EP-005**: produtos estoque baixo via query port
- **Platform EP-002**: cobrança mensalidade tenant no digest
- **RF-091** agendamento manual: fora do escopo MVP — change futuro
- **Dependências**: EP-000, EP-001, EP-003, EP-007 · **Release**: 3

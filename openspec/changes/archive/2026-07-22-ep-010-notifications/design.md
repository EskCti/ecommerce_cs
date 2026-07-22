## Context

**RetailOps** — BC **Notifications** (Generic). Release 3; depende de EP-007 Finance, EP-005 Catalog e EP-003 Store Settings. Legado: `api/mensagem.php` incluído em `conexao.php`; `acl-design.md` mapeia para handlers C#.

**Princípio:** Notifications **não muta** Finance/Catalog — conformist consumer via query ports. HTTP WhatsApp isolado em ACL.

Referências: `ddd-tactical-model.md` (BC Notifications), `acl-design.md` (BC Notifications), `requirements.md` M10.

## Goals / Non-Goals

**Goals:**

- Paridade US-100: digest diário WhatsApp RF-090
- Conteúdo: contas vencendo hoje, estoque baixo, cobrança mensalidade
- Idempotência 1×/dia por tenant RN-070
- Token tenant sobrescreve global RN-071
- `EnviameWhatsAppAdapter` substituindo `file_get_contents`
- Hosted service agendado (cron diário America/Sao_Paulo RNF-006)

**Non-Goals:**

- Agendamento manual de mensagens (`RF-091` / `api/agendar.php`) — follow-up
- Push notifications, email, SMS
- UI Vue admin para notifications (legado não tem tela dedicada)
- Retry complexo / dead letter queue (MVP: log + skip)

## Decisions

### 1. Módulo Notifications

**Decisão:** `RetailOps.Notifications.Core` (entities, VOs, DailyDigestComposer) + Application (handlers) + Infrastructure (ACL + HTTP).

### 2. Daily digest flow

```
DailyDigestBackgroundService (cron)
  → foreach active tenant
    → SendDailyTenantDigestHandler
      → check DailyDigestLog (RN-070)
      → DailyDigestComposer (ports)
      → IWhatsAppGatewayPort.SendAsync
      → persist DailyDigestLog + emit DigestSent
```

### 3. Digest composition

**Decisão:** `DailyDigestComposer` domain service chama ports:

| Port | Fonte | Conteúdo digest |
| ---- | ----- | --------------- |
| `IReceivablesDueTodayQueryPort` | Finance | Contas vencendo hoje |
| `ILowStockSummaryQueryPort` | Catalog | Count + top produtos estoque baixo |
| `ITenantBillingAlertQueryPort` | Platform | Mensalidade pendente/vencida |

**MVP parallel run:** `LegacyDigestQueryAdapter` implementa ports via SQL legado.

### 4. Idempotência RN-070

**Decisão:** `DailyDigestLog` entity keyed by `(TenantId, DigestDate)` — skip se já enviado hoje.

Legado usa `config.data` — mapear para `DailyDigestLog.SentAt` + não replicar update em `config.data` após cutover.

### 5. WhatsApp credentials RN-071

**Decisão:** `WhatsAppCredentials` VO resolvido por tenant:

1. Tenant `StoreConfig.ApiToken` se preenchido
2. Senão global `appsettings:WhatsApp:DefaultToken`
3. Destino: `StoreConfig.SystemPhone` (`telefone_sistema`)

### 6. Enviame adapter

**Decisão:** `IWhatsAppGatewayPort`:

```csharp
Task<Result<Unit>> SendTextAsync(PhoneNumber to, MessageTemplate body, WhatsAppCredentials creds);
```

`EnviameWhatsAppAdapter` — HttpClient tipado, timeout, log erros; **não** expor URL enviame ao Core.

### 7. Hosted service

**Decisão:** `DailyDigestBackgroundService` : BackgroundService

- Schedule: daily ~07:00 America/Sao_Paulo (configurável)
- Process tenants sequentially or bounded parallel (max 5)
- Skip tenant sem telefone ou sem token

**Alternativa rejeitada:** Hangfire — over-engineering para MVP.

### 8. API (optional admin)

**Decisão:** `POST /api/notifications/digest/trigger` admin-only para teste manual — não exposto a tenant users.

## Risks / Trade-offs

| Risk | Mitigation |
| ---- | ---------- |
| API enviame indisponível | Log failure; não marca DailyDigestLog — retry next run |
| Digest duplicado | DailyDigestLog unique constraint RN-070 |
| Token global vs tenant | Resolver documentado RN-071; testes unitários |
| Trigger conexao.php vs hosted | Cutover EP-011 desliga PHP job |

## Migration Plan

1. Deploy Notifications + hosted service em staging
2. Parallel run: comparar mensagens PHP vs C# para amostra tenants
3. Desligar include `mensagem.php` em `conexao.php` no cutover EP-011

## Open Questions

- Horário exato do job legado — inferido manhã; confirmar com operação
- Incluir RF-091 no mesmo épico ou change separado?

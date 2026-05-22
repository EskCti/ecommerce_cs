# Tasks — ep-010-notifications (EP-010 Notifications)

Referência: `docs/planning/loja-php/backlog.md` · US-100

## 1. Module setup

- [ ] 1.1 `infra:setup` Módulo Notifications (~1h)
  - **Agent:** `Config New Module (C#)`
  - **Prompt:** "Crie RetailOps.Notifications: Core, Application, Infrastructure; IWhatsAppGatewayPort, INotificationLogRepository, digest query ports."
  - **Spec:** `whatsapp-gateway`, `daily-tenant-digest`

## 2. Domain layer

- [ ] 2.1 `domain:vo` Notification VOs (~2h)
  - **Agent:** `Core Value Object (C#)`
  - **Prompt:** "PhoneNumber E.164 BR, MessageTemplate, DigestContent, WhatsAppCredentials; validações."
  - **Spec:** `daily-tenant-digest`, `whatsapp-gateway`

- [ ] 2.2 `domain:entity` DailyDigestLog (~1h)
  - **Agent:** `Core Entity (C#)`
  - **Prompt:** "DailyDigestLog AR TenantId+DigestDate unique; SentAt; idempotência RN-070."
  - **Spec:** `digest-scheduler`

- [ ] 2.3 `domain:service` DailyDigestComposer (~3h)
  - **Agent:** `Core Domain Service (C#)`
  - **Prompt:** "DailyDigestComposer agrega IReceivablesDueTodayQueryPort, ILowStockSummaryQueryPort, ITenantBillingAlertQueryPort → DigestContent → MessageTemplate RF-090."
  - **Spec:** `digest-composition`

## 3. Application layer

- [ ] 3.1 `app:usecase` SendDailyTenantDigestHandler (~3h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "SendDailyTenantDigestHandler: check DailyDigestLog RN-070; compose; IWhatsAppGatewayPort; persist log; DigestSent event."
  - **Spec:** `daily-tenant-digest`, `digest-scheduler`

## 4. Infrastructure

- [ ] 4.1 `infra:adapter` EnviameWhatsAppAdapter (~3h)
  - **Agent:** `Backend Data (C#)`
  - **Prompt:** "EnviameWhatsAppAdapter HttpClient api.enviame.com.br; token tenant/global RN-071; Result mapping."
  - **Spec:** `whatsapp-gateway`

- [ ] 4.2 `infra:adapter` LegacyDigestQueryAdapter (~2h)
  - **Agent:** `Backend Data (C#)`
  - **Prompt:** "LegacyDigestQueryAdapter read-only SELECT; receivables due today, low stock, billing alert; paridade mensagem.php ACL."
  - **Spec:** `notifications-legacy-acl`

- [ ] 4.3 `infra:hosted` DailyDigestBackgroundService (~2h)
  - **Agent:** `Backend Data (C#)`
  - **Prompt:** "BackgroundService cron diário America/Sao_Paulo; foreach tenant SendDailyTenantDigestHandler."
  - **Spec:** `digest-scheduler`

## 5. API

- [ ] 5.1 `interface:controller` NotificationsController (~1h)
  - **Agent:** `Backend Controller (C#)`
  - **Prompt:** "POST /api/notifications/digest/trigger admin-only; tenantId param; RBAC admin."
  - **Spec:** `digest-scheduler`

## 6. Tests

- [ ] 6.1 `test:unit` Notifications domain (~2h)
  - **Agent:** `Unit Tests (C#)`
  - **Prompt:** "xUnit: DailyDigestComposer mocks ports; RN-070 skip duplicate; RN-071 token resolution. Coverlet ≥95%."

- [ ] 6.2 `test:e2e` Digest flow mock gateway (~2h)
  - **Agent:** `E2E Tests (C#)`
  - **Prompt:** "WebApplicationFactory: trigger digest → mock IWhatsAppGatewayPort called; DailyDigestLog persisted; second trigger skipped RN-070."

## 7. Acceptance verification

- [ ] 7.1 Validar US-100: digest contém contas hoje, estoque baixo, cobrança
- [ ] 7.2 Validar RN-070: máximo 1 envio/dia/tenant
- [ ] 7.3 Validar RN-071: token tenant sobrescreve global
- [ ] 7.4 Comparar mensagem amostra vs legado mensagem.php

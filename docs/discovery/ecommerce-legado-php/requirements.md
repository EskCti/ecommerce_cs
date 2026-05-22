# Requisitos — Sistema de Vendas (Legado PHP)

**Fonte**: `/exemplos/php/` (código em `/exemplos/php/vendas/`) + `sas.sql`  
**Data da análise**: 2026-05-22  
**Método**: análise de código (codebase local)

## Visão Geral

O legado é um **Sistema de Vendas SaaS multi-tenant** para lojas físicas (PDV, estoque, financeiro), não um e-commerce B2C com vitrine pública. Uma única instalação PHP atende:

1. **Portal de login** (`vendas/index.php`) — autenticação unificada e cadastro de trial.
2. **Painel do tenant** (`vendas/sistema/`) — operação da loja (produtos, PDV, financeiro, relatórios).
3. **Painel SAS** (`vendas/sas/`) — operador da plataforma (empresas clientes, cobrança, contratos, permissões globais).

Todos os módulos compartilham o banco MariaDB `sas`, com isolamento lógico por coluna `empresa` (tenant). O operador SAS usa `empresa = 0` para dados globais.

```
┌─────────────────────────────────────────────────────────────┐
│                    ARQUITETURA GERAL                        │
├─────────────────────────────────────────────────────────────┤
│  vendas/index.php (login)                                   │
│       │                                                     │
│       ├── nivel SAS ──────▶ vendas/sas/ (Platform Admin)    │
│       └── demais níveis ──▶ vendas/sistema/ (Tenant App)    │
│                                                             │
│  Relatórios PDF: vendas/rel/ (SAS) | rel_sistema/ (tenant)  │
│  Integrações: api/mensagem.php, api/agendar.php (WhatsApp)  │
│                                                             │
│                    MariaDB (sas) — multi-tenant             │
└─────────────────────────────────────────────────────────────┘
```

## Stack Tecnológica

| Camada         | Tecnologia |
| -------------- | ---------- |
| Frontend       | PHP server-rendered + jQuery + Bootstrap 4 + DataTables |
| Backend        | PHP procedural (scripts por ação em `paginas/*/`) |
| Banco de Dados | MariaDB/MySQL (`sas.sql`) via PDO |
| Relatórios     | dompdf (`vendas/dompdf/`) |
| Sessão/Auth    | PHP `session` + MD5 (`senha_crip`) + localStorage |
| Infra          | Apache/Nginx + PHP 8.x [inferido do dump SQL] |

## Módulos Identificados

### M1 — Autenticação e Acesso

**Descrição**: Login central, recuperação de senha, trial self-service, RBAC por permissões granulares.

#### Requisitos Funcionais

- **RF-001**: Login com email ou CPF + senha (`autenticar.php`).
  - Endpoint: `POST vendas/autenticar.php`
  - Validações: usuário ativo; permissões atribuídas (exceto Administrador/SAS)
  - Ações: redireciona para `sas/` (nível SAS) ou `sistema/` (demais)

- **RF-002**: Cadastro de trial self-service (`cadastrar.php`).
  - Campos: nome, email, telefone, senha
  - Validações: email único; período de teste configurável em `config.dias_teste`
  - Ações: cria `empresas` (teste=Sim) + usuário Administrador

- **RF-003**: Recuperação de senha via modal (`recuperar-senha.php`).

- **RF-004**: Bootstrap automático de super-admin SAS e empresa teste (`index.php`).

- **RF-005**: Desativação automática de empresas com trial expirado (`index.php`, `autenticar.php`).

- **RF-006**: Controle de permissões por usuário (`usuarios/add-permissao.php`, tabela `usuarios_permissoes` + `acessos`).

- **RF-007**: Verificação de permissões em runtime (`sistema/verificar-permissoes.php`).

#### Regras de Negócio

- **RN-001**: Senha armazenada em MD5 (`senha_crip`).
- **RN-002**: Usuários não-Administrador sem registros em `usuarios_permissoes` não acessam o sistema.
- **RN-003**: Empresa com `teste=Sim` e `data_pgto < hoje` tem usuários e empresa desativados.
- **RN-004**: Níveis observados: `SAS`, `Administrador`, `Gerente`, `Operador`, `Vendedor` (+ cargos customizáveis).

---

### M2 — Plataforma SaaS (Painel SAS)

**Descrição**: Gestão de tenants, cobrança de mensalidade, contratos e configuração global.

#### Requisitos Funcionais

- **RF-010**: CRUD de empresas (`sas/paginas/empresas/`).
  - Campos: nome, telefone, email, CPF/CNPJ, endereço, valor mensalidade, data_pgto, flag teste
  - Ações: cria admin padrão (senha `123`) na inclusão

- **RF-011**: CRUD de usuários SAS (`sas/paginas/usuarios/`).

- **RF-012**: Gestão de grupos e acessos do menu (`grupos`, `acessos`, `grupo_acessos`).

- **RF-013**: CRUD de frequências de recorrência (`frequencias` — global e por tenant).

- **RF-014**: Contas a receber de empresas (`receber` com `empresa=0`, `tipo=Empresa`, `pessoa=id_empresa`).

- **RF-015**: Despesas da plataforma (`pagar` com `empresa=0`).

- **RF-016**: Contratos de locação de software por empresa (`contratos`, `empresas/salvar-contrato.php`).

- **RF-017**: Anexos por entidade (`arquivos` — tipos: Empresa, Cliente, Receber, Pagar).

- **RF-018**: Relatórios SAS (`rel/`): empresas, receber, pagar, lucro, contrato.

#### Regras de Negócio

- **RN-010**: Cobrança de tenant gera `receber` vinculada à empresa; inadimplência pode bloquear tenant após `config.dias_bloqueio` [inferido de `sistema/index.php`].
- **RN-011**: Mensagem de bloqueio configurável em `config.msg_bloqueio` (empresa 0).

---

### M3 — Cadastros Operacionais (Tenant)

**Descrição**: Master data da loja.

#### Requisitos Funcionais

- **RF-020**: CRUD clientes (`clientes/`) — nome, CPF, telefone, email, endereço; histórico vendas/contas/arquivos.

- **RF-021**: CRUD fornecedores (`fornecedores/`) — pessoa Física/Jurídica.

- **RF-022**: CRUD funcionários (`funcionarios/`) — cria usuário com senha padrão `123`, nível/cargo, comissão %.

- **RF-023**: CRUD usuários internos (`usuarios/`) — permissões granulares.

- **RF-024**: CRUD cargos (`cargos/`).

- **RF-025**: CRUD formas de pagamento (`forma_pgtos/`) — nome + percentual de acréscimo (parcelamento).

- **RF-026**: CRUD caixas físicos (`caixas/`) — nome, status Aberto/Fechado, operador vinculado.

- **RF-027**: Configurações da loja (`editar-config.php`, `carregar-dados-config.php`).
  - Campos: nome_sistema, contatos, CNPJ, endereço, tipo_rel (PDF/HTML), tipo_desconto (%/R$), comissão padrão, token WhatsApp, logo relatório

---

### M4 — Catálogo e Produtos

**Descrição**: Produtos com código de barras, categorias, grades (variações) e fotos.

#### Requisitos Funcionais

- **RF-030**: CRUD produtos (`produtos/salvar.php`, `listar.php`, `excluir.php`, `mudar-status.php`).
  - Campos: nome, código, categoria, fornecedor, valor_venda, nivel_estoque, foto
  - Validações: código único por empresa; extensões de imagem permitidas

- **RF-031**: Geração automática de código (`produtos/gerar-codigo.php`).

- **RF-032**: CRUD categorias (`categorias/`) — ativo Sim/Não.

- **RF-033**: Grades de produto — duas dimensões opcionais (`cat_grade`, `itens_grade`).
  - Ex.: Cor + Tamanho; estoque por combinação

- **RF-034**: Operações de grade (`inserir-grade.php`, `inserir-itens-grade.php`, `excluir-grade.php`).

#### Regras de Negócio

- **RN-020**: Produto pode ter `valor_venda = 0` — preço informado no PDV na hora da venda.
- **RN-021**: `nivel_estoque` define limite para alerta de estoque baixo.
- **RN-022**: Lucro calculado na compra: `(valor_venda - valor_compra) / valor_compra * 100`.

---

### M5 — Estoque

**Descrição**: Movimentações manuais, compras e rastreio por grade.

#### Requisitos Funcionais

- **RF-040**: Entrada manual de estoque (`produtos/entrada.php`, `estoque/entrada.php`) — motivo + quantidade.

- **RF-041**: Saída manual (`produtos/saida.php`, `estoque/saida.php`).

- **RF-042**: Compra de produto (`produtos/comprar.php`, `estoque/comprar.php`).
  - Atualiza estoque, valor_compra, lucro, fornecedor
  - Gera conta `pagar` tipo Compra
  - Registra `detalhes_grade` tipo Compra

- **RF-043**: Listagem de entradas/saídas históricas (`entradas/listar.php`, `saidas/listar.php`).

- **RF-044**: Dashboard estoque baixo (`estoque/listar.php`) — produtos com `estoque < nivel_estoque`.

#### Regras de Negócio

- **RN-030**: Cancelamento de venda devolve estoque via `entradas` motivo "Cancelamento da Venda".
- **RN-031**: Movimentações de grade registradas em `detalhes_grade` com tipos: Compra, Venda, Troca Entrada, Troca Saída.

---

### M6 — PDV (Ponto de Venda)

**Descrição**: Caixa aberto por operador, carrinho, finalização e sangria.

#### Requisitos Funcionais

- **RF-050**: Abertura de caixa (`abertura/salvar.php`).
  - Campos: caixa físico, valor inicial, gerente (validação senha plaintext)
  - Ações: cria registro `caixa` status Aberto; marca `caixas.status = Aberto`

- **RF-051**: PDV — leitura de código de barras (`pdv/buscar-codigo.php`).
  - Suporta prefixo quantidade: `2*12345678`
  - Adiciona item em `itens_venda` (venda=0 = carrinho aberto)
  - Abate estoque imediato (produto e/ou grade na confirmação)

- **RF-052**: Confirmação de grade na venda (`pdv/confirmar-grade.php`).

- **RF-053**: Finalização de venda (`pdv/buscar-codigo.php` quando `forma_pgto_input` preenchido).
  - Campos: forma pagamento, cliente, vendedor, desconto (% ou R$), acréscimo, valor recebido, troco, garantia (dias), data pagamento
  - Cria `receber` tipo Venda vinculado à abertura de caixa (`id_ref`)
  - Vincula itens do carrinho à venda
  - Gera comissão se vendedor informado

- **RF-054**: Exclusão de item do carrinho (`pdv/excluir-item.php`) — devolve estoque.

- **RF-055**: Sangria de caixa (`pdv/sangria.php`) — registra em `sangrias`.

- **RF-056**: Fechamento de caixa (`pdv/fechar-caixa.php`).
  - Calcula: valor_vendido, valor_sangrias, valor_quebra = valor_fechamento - (abertura + vendido - sangrias)
  - Valida senha gerente

- **RF-057**: Comprovante/recibo PDF (`rel_sistema/comprovante.php`, `recibo.php`).

#### Regras de Negócio

- **RN-040**: Operador só vende com caixa aberto (`caixa.status = Aberto` para o operador).
- **RN-041**: Venda fiada ou com vencimento futuro exige cliente selecionado; `pago = Não`.
- **RN-042**: Venda à vista ou vencimento ≤ hoje: `pago = Sim`, `data_pgto = hoje`.
- **RN-043**: Troco não pode ser negativo.
- **RN-044**: Venda sem itens é rejeitada.
- **RN-045**: Estoque insuficiente bloqueia adição (exceto produtos valor 0) [inferido].
- **RN-046**: Comissão = `total_sem_taxa * percentual / 100`; percentual do vendedor sobrescreve config da loja.
- **RN-047**: Acréscimo de forma de pagamento aplicado via campo `acrescimo` em `receber`.

---

### M7 — Trocas

**Descrição**: Troca de produto cliente — entrada de um SKU, saída de outro.

#### Requisitos Funcionais

- **RF-060**: Registrar troca (`trocas/salvar.php`).
  - Campos: cliente (ou CPF+nome auto-cadastro), produto_entrada/saída, grades opcionais
  - Ações: +1 estoque entrada, -1 estoque saída; registra `trocas` + 2× `detalhes_grade`

- **RF-061**: Listagem e exclusão de trocas.

#### Regras de Negócio

- **RN-050**: Quantidade fixa 1 unidade por lado da troca.
- **RN-051**: Cliente criado automaticamente se CPF não existir.

---

### M8 — Financeiro

**Descrição**: Contas a receber/pagar, fluxo de caixa, comissões.

#### Requisitos Funcionais

- **RF-070**: CRUD contas a receber (`receber/`) — tipos: Venda, Empresa, outros manuais.
  - Baixa de conta (`receber/baixar.php`)
  - Anexos (`inserir-arquivo.php`, `listar-arquivos.php`)

- **RF-071**: CRUD despesas (`pagar/`) — tipos: Compra, Conta, Pagamento (comissão), Empresa.
  - Baixa, anexos, recorrência via `frequencia`

- **RF-072**: Listagem de compras (`compras/listar.php`) — filtro `pagar.tipo = Compra`.

- **RF-073**: Listagem de vendas (`vendas/listar.php`) — filtro `receber.tipo = Venda`; detalhes itens; cancelamento.

- **RF-074**: Fluxo de caixa (`fluxo/listar.php`) — consolida recebimentos e pagamentos.

- **RF-075**: Comissões (`comissoes/`, `minhas_comissoes/`).
  - Baixa individual ou em lote; gera `pagar` tipo Pagamento

#### Regras de Negócio

- **RN-060**: Cancelamento de venda exclui `receber`, `comissoes` vinculadas e devolve estoque.
- **RN-061**: Conta com vencimento futuro na compra permanece `pago = Não`.

---

### M9 — Relatórios

**Descrição**: PDFs via dompdf, filtros por período.

#### Requisitos Funcionais

- **RF-080**: Relatórios tenant (`rel_sistema/`): vendas, clientes, receber, pagar, lucro, produtos, estoque baixo, entradas/saídas, caixas, comissões, trocas, sangrias, comprovante.

- **RF-081**: Relatórios SAS (`rel/`): empresas, receber, pagar, lucro, contrato.

- **RF-082**: Filtros por data início/fim, status, cliente, vendedor [via modais em `sistema/index.php`].

---

### M10 — Notificações

**Descrição**: Mensagens automáticas WhatsApp via API externa.

#### Requisitos Funcionais

- **RF-090**: Job diário em `api/mensagem.php` (incluído em `conexao.php`).
  - Envia resumo: contas vencendo hoje, estoque baixo, cobrança mensalidade
  - Usa `config.token` e API `api.enviame.com.br`

- **RF-091**: Agendamento de texto (`api/agendar.php`).

#### Regras de Negócio

- **RN-070**: Execução limitada a 1× por dia (`config.data` vs data atual).
- **RN-071**: Token por empresa sobrescreve token global quando preenchido.

---

## Requisitos Não-Funcionais

- **RNF-001**: Multi-tenancy lógico — todas as queries de tenant filtram por `empresa`.
- **RNF-002**: Autenticação baseada em sessão PHP + espelhamento em localStorage.
- **RNF-003**: Senhas em MD5 (legado — **não atender em reimplementação**).
- **RNF-004**: Validação de gerente por senha plaintext comparada a `usuarios.senha` (legado).
- **RNF-005**: Upload de arquivos com whitelist de extensões.
- **RNF-006**: Timezone `America/Sao_Paulo`.
- **RNF-007**: Interface responsiva básica (Bootstrap 4).
- **RNF-008**: Relatórios exportáveis PDF (dompdf).
- **RNF-009**: SQL concatenado direto — risco de injection (legado).

## Integrações

| Sistema | Tipo | Descrição |
| ------- | ---- | --------- |
| api.enviame.com.br | API REST | Envio/agendamento WhatsApp |
| dompdf | Biblioteca PHP | Geração de PDFs |
| [placeholder] | API pagamento | Cartão débito/crédito comentado em `buscar-codigo.php`, não implementado |

## Perfis de Acesso

| Perfil | Escopo | Permissões |
| ------ | ------ | ---------- |
| SAS | Plataforma (`empresa=0`) | Empresas, usuários SAS, cobrança tenants, grupos/acessos globais |
| Administrador | Tenant | Acesso total ao menu do tenant (bypass permissões) |
| Gerente | Tenant | Abre/fecha caixa (validação senha); operações supervisionadas |
| Operador | Tenant | PDV, caixa próprio |
| Vendedor | Tenant | Vendas associadas; comissões em `minhas_comissoes` |
| Custom (cargo) | Tenant | Permissões via `usuarios_permissoes` × `acessos.chave` |

Grupos de menu (`grupo_acessos`): Pessoas (2), Cadastros (3), Produtos (4), Financeiro (5), Relatórios (6).

## Fluxos Principais

### Fluxo 1: Trial → Operação da Loja

```
Login/Cadastro trial → sistema/ → Config loja → Cadastrar produtos → Abrir caixa → PDV → Fechar caixa
```

### Fluxo 2: Venda PDV Completa

```
Abrir caixa → Escanear produtos (itens_venda venda=0) → [Confirmar grades] → Informar pagamento/cliente/vendedor → Finalizar → receber + comissão → Recibo PDF
```

### Fluxo 3: Compra de Mercadoria

```
Produtos → Comprar → Informar qty/valor/fornecedor/vencimento → Estoque↑ + pagar(Compra) + detalhes_grade
```

### Fluxo 4: Cobrança SaaS

```
SAS cadastra empresa → Gera receber mensal (empresa=0) → Inadimplência → Alerta tenant → [dias_bloqueio] bloqueio
```

### Fluxo 5: Cancelamento de Venda

```
Vendas → Excluir → Devolve estoque (produto + grade) → entradas cancelamento → Remove receber e comissões
```

## Lacunas e Observações

- Não há vitrine e-commerce B2C (carrinho web, checkout online) — foco em PDV presencial.
- Integração de cartão está stub/comentada; pagamentos são registros manuais.
- Paths informados (`/exemplos/php/sas/`) mapeiam para `/exemplos/php/vendas/sas/` na árvore real.
- Tabela `vendas` não existe — vendas são registros em `receber` tipo `Venda` + `itens_venda`.
- Segurança frágil (MD5, SQL inline, senha gerente plaintext) — documentar para ACL na migração.
- `funcionarios` e `usuarios` compartilham tabela `usuarios` [inferido de estrutura].
- Arquivos `*:Zone.Identifier` são metadados Windows; ignorar na migração.

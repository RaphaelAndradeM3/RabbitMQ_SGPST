# SGPST Enterprise - Sistema de Gerenciamento de Pedidos de Suporte Técnico

Este projeto é um ecossistema completo e robusto de gerenciamento de chamados de suporte técnico, desenvolvido em **C# .NET 8.0**. Ele adota os princípios da **Clean Architecture**, padrões de design modernos, injeção de dependências e comentários internos escritos em **Português (sem acentuação)** para garantir compatibilidade universal.

---

## 🚀 Visão Geral do Sistema

O SGPST Enterprise simula um ambiente de TI corporativo onde:
1. **Clientes** ou o **Gerador de Carga** abrem chamados técnicos via API REST.
2. A **Web API** autentica as requisições via **JWT**, persiste os dados no **PostgreSQL** e publica um evento assíncrono na fila de triagem do **RabbitMQ**.
3. O **Background Service Worker** consome e processa as mensagens da fila.
4. As **Apresentações Web (MVC)** e **Desktop (WPF)** fornecem painéis administrativos ricos para triagem, designação de técnicos, registro de deslocamentos físicos, faturamento operacional e cancelamento.

---

## 🏗️ Arquitetura do Projeto

A solução está dividida em 9 projetos organizados segundo a **Clean Architecture**:

- **[SGPST.Domain](file:///H:/tmp/RSA/Mensageria/RabbitMQ/SGPST/RabbitMQ_SGPST/src/SGPST.Domain)**: O núcleo de regras de domínio. Contém entidades (`SupportTicket`, `Client`, `Technician`, `ServicePrice`, `User`, `DisplacementLog`), enums e interfaces de contrato.
- **[SGPST.Application](file:///H:/tmp/RSA/Mensageria/RabbitMQ/SGPST/RabbitMQ_SGPST/src/SGPST.Application)**: Camada de casos de uso, lógica de negócios, DTOs compartilhados, mapeamentos e interfaces de serviço.
- **[SGPST.Infrastructure](file:///H:/tmp/RSA/Mensageria/RabbitMQ/SGPST/RabbitMQ_SGPST/src/SGPST.Infrastructure)**: Contém as implementações técnicas:
  - **Data (EF Core)**: Persistência relacional utilizando **Entity Framework Core** configurado para **PostgreSQL**.
  - **Messaging**: Publicador de mensagens integrado ao **RabbitMQ**.
- **[SGPST.Presentation.Api](file:///H:/tmp/RSA/Mensageria/RabbitMQ/SGPST/RabbitMQ_SGPST/src/SGPST.Presentation.Api)**: Web API RESTful que centraliza o processamento e expõe endpoints seguros para todas as aplicações clientes.
- **[SGPST.Presentation.Web](file:///H:/tmp/RSA/Mensageria/RabbitMQ/SGPST/RabbitMQ_SGPST/src/SGPST.Presentation.Web)**: Painel administrativo Web desenvolvido em ASP.NET Core MVC com tema escuro e visual moderno.
- **[SGPST.Presentation.Desktop](file:///H:/tmp/RSA/Mensageria/RabbitMQ/SGPST/RabbitMQ_SGPST/src/SGPST.Presentation.Desktop)**: Cliente administrativo desktop desenvolvido em **WPF** (Windows Presentation Foundation) com tema escuro premium e controle dinâmico de permissões.
- **[SGPST.Service.Worker](file:///H:/tmp/RSA/Mensageria/RabbitMQ/SGPST/RabbitMQ_SGPST/src/SGPST.Service.Worker)**: Serviço de segundo plano (.NET Worker) que atua como Consumer do RabbitMQ para notificações e eventos assíncronos.
- **[SGPST.Client.Generator](file:///H:/tmp/RSA/Mensageria/RabbitMQ/SGPST/RabbitMQ_SGPST/src/SGPST.Client.Generator)**: Aplicativo console de simulação de estresse que gera carga de chamados, autenticando-se via JWT e cadastrando dados fictícios automaticamente se necessário.
- **[SGPST.Tests](file:///H:/tmp/RSA/Mensageria/RabbitMQ/SGPST/RabbitMQ_SGPST/src/SGPST.Tests)**: Suíte de testes automatizados com xUnit e Mocking para validação do domínio e regras de negócio.

---

## 🛠️ Tecnologias e Infraestrutura

- **Linguagem e Runtime**: C# com .NET 8.0 e WPF.
- **Banco de Dados**: PostgreSQL 16 (rodando em container Docker).
- **Mapeador Relacional**: Entity Framework Core (EF Core) com suporte a Migrations automáticas.
- **Broker de Mensageria**: RabbitMQ 3 com painel de gerenciamento habilitado.
- **Segurança**: Autenticação customizada via Tokens JWT (Bearer).
- **Aesthetics (UI)**: Cores escuras curadas, gradientes neon, fontes limpas e efeitos visuais premium.

---

## ⚙️ Como Executar o Ecossistema

Toda a inicialização do ambiente e dos módulos foi simplificada usando scripts batch (`.bat`) na raiz do repositório.

### 1. Pré-requisitos
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download) instalado.
- [Docker Desktop](https://www.docker.com/products/docker-desktop) instalado e em execução.

### 2. Inicializar a Infraestrutura (Docker)
Execute o script abaixo para subir os containers do PostgreSQL e do RabbitMQ em segundo plano:
```bash
start-environment.bat
```
- **RabbitMQ Management Dashboard**: Acesse em `http://localhost:15672` (Usuário: `guest` | Senha: `guest`).
- Para parar a infraestrutura, execute: `stop-environment.bat`.

### 3. Executar os Componentes
Abra terminais separados para rodar os serviços na seguinte ordem recomendada:

1. **Web API (Core)**:
   ```bash
   run-api.bat
   ```
2. **Background Consumer (Worker)**:
   ```bash
   run-worker.bat
   ```
3. **Gerador de Carga de Testes** (Para criar chamados automaticamente):
   ```bash
   run-generator.bat
   ```
4. **Desktop Client (WPF)**:
   ```bash
   run-desktop.bat
   ```
5. **Dashboard Web (MVC)**:
   ```bash
   run-web.bat
   ```

---

## 🧪 Credenciais de Demonstracao

Para testar as interfaces Web e Desktop, utilize qualquer uma das seguintes contas pré-configuradas:
- **Administrador:** Usuário `admin` / Senha `admin123`
- **Atendente:** Usuário `atendente` / Senha `atendente123`
- **Técnico:** Usuário `tecnico` / Senha `tecnico123`
- **Cliente:** Usuário `cliente` / Senha `cliente123`

---

## 🔬 Testes Automatizados

A suíte de testes unitários cobre as lógicas cruciais de negócio do Domínio e Casos de Uso. Execute via CLI de dentro do diretório `src/`:
```bash
dotnet test
```

---

## 📂 Pasta de Referência (demo-sgpst)

Na raiz da solução está localizada a pasta **[demo-sgpst](file:///H:/tmp/RSA/Mensageria/RabbitMQ/SGPST/RabbitMQ_SGPST/demo-sgpst)**. Esta pasta contém a versão anterior de referência do projeto didático, servindo como documentação base de diretrizes arquiteturais, exemplos de estruturação de código, arquivos originais de persistência e instruções de estilização. Ela é preservada para fins de consulta histórica e conformidade com as regras de design originais exigidas na construção do SGPST Enterprise.

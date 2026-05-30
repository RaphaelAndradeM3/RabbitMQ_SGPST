# SGPST - Sistema de Gerenciamento de Pedidos de Suporte Técnico

Este projeto é um sistema robusto de gerenciamento de pedidos de suporte técnico, desenvolvido em **C# .NET 8.0**. Ele utiliza **RabbitMQ** para mensageria assíncrona e **Dapper** com **SQLite** para persistência de dados, seguindo rigorosamente os princípios da **Clean Architecture** e **SOLID**.

## 🚀 Visão Geral

O SGPST (Sistema de Gerenciamento de Pedidos de Suporte Técnico) simula um ecossistema completo onde:
- **Clientes** geram pedidos de suporte (via API ou Gerador de Carga).
- **Mensageria** (RabbitMQ) orquestra a distribuição desses pedidos de forma resiliente.
- **Workers** processam os pedidos de forma assíncrona.
- **Interfaces Web e Desktop** permitem o monitoramento e a gestão administrativa do sistema em tempo real.

O foco principal deste projeto é demonstrar uma arquitetura altamente desacoplada, escalável e de fácil manutenção.

## 🏗️ Arquitetura do Sistema

O projeto está dividido em camadas seguindo a **Clean Architecture**:

- **SGPST.Domain**: O coração do sistema. Contém as entidades de negócio (`Order`, `ServiceProvider`, `User`), interfaces de contrato e objetos de valor.
- **SGPST.Application**: Contém a lógica de aplicação, casos de uso, DTOs e mapeamentos. Orquestra o fluxo de dados entre o domínio e as camadas externas.
- **SGPST.Infrastructure**: Implementação de detalhes técnicos.
    - **Data**: Persistência com **Dapper** e **SQLite**.
    - **Messaging**: Integração completa com **RabbitMQ**.
    - **Logging**: Configuração de logs estruturados com **Serilog** e logs em arquivo.
- **SGPST.Presentation**: Múltiplas portas de entrada e saída:
    - **SGPST.Presentation.Api**: Web API (Producer) para recepção de novos pedidos.
    - **SGPST.Presentation.Web**: Painel administrativo (Dashboard) em ASP.NET Core MVC.
    - **SGPST.Presentation.Desktop**: Aplicativo administrativo desenvolvido em **WPF**.
- **SGPST.Service.Worker**: Serviço de segundo plano (Consumer) que processa as filas do RabbitMQ.
- **SGPST.Client.Generator**: Aplicativo de console para simulação de carga e estresse, gerando pedidos aleatórios.

## 🛠️ Tecnologias e Ferramentas

- **Runtime**: .NET 8.0
- **Acesso a Dados**: Dapper (Micro-ORM)
- **Banco de Dados**: SQLite (embutido)
- **Mensageria**: RabbitMQ (rodando via Docker)
- **Logging**: Serilog (Console e Arquivo)
- **Injeção de Dependência**: Nativa do .NET 8
- **Testes**: xUnit e Moq
- **Containerização**: Docker e Docker Compose

## ⚙️ Como Executar

O projeto foi preparado para ser executado de forma simples através de scripts batch (.bat) na raiz do projeto.

### 1. Pré-requisitos
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

### 2. Preparando o Ambiente (RabbitMQ)
Execute o script para subir o container do RabbitMQ:
```bash
start-rabbitmq.bat
```
O painel de gerenciamento do RabbitMQ ficará disponível em: `http://localhost:15672` (Login: `guest` / Senha: `guest`)

### 3. Executando os Módulos
Você pode abrir múltiplos terminais e executar os componentes desejados:
- **API (Producer)**: `run-api.bat`
- **Worker (Consumer)**: `run-worker.bat`
- **Dashboard Web**: `run-web.bat`
- **Gestão Desktop (WPF)**: `run-desktop.bat`
- **Gerador de Carga**: `run-generator.bat` (Use para simular múltiplos pedidos rapidamente)

## 📐 Padrões de Desenvolvimento

- **Clean Architecture**: Desacoplamento total entre lógica de negócio e detalhes de infraestrutura.
- **Tratamento de Erros**: Blocos `try/catch` implementados de forma sistemática em todos os métodos críticos para garantir a resiliência do sistema.
- **Interfaces**: Uso extensivo de abstrações para facilitar a testabilidade e o intercâmbio de implementações.
- **Nomenclatura**: Segue os padrões oficiais da Microsoft (PascalCase para membros públicos, camelCase com `_` para privados).
- **Comentários**: Documentação interna em Português, escrita sem acentuação para evitar problemas de compatibilidade entre editores e sistemas operacionais.

## 🧪 Testes

A suíte de testes está localizada no projeto **SGPST.Tests** e pode ser executada via terminal:
```bash
dotnet test
```
Os testes cobrem as camadas de **Domain** e **Application**, garantindo que as regras de negócio e o fluxo de dados funcionem conforme o esperado.

---
Desenvolvido como um modelo de referência para sistemas de mensageria com RabbitMQ e C#.

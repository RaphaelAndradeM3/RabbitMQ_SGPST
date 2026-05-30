# SGPST - Sistema de Gerenciamento de Pedidos de Suporte Técnico

Este projeto é um sistema de gerenciamento de pedidos de suporte técnico, desenvolvido em C# .NET 8.0, utilizando RabbitMQ para mensageria e Dapper para persistência de dados. O sistema segue os princípios da **Clean Architecture** e **SOLID**.

## 🚀 Visão Geral

O SGPST permite que clientes simulem pedidos de suporte que são processados de forma assíncrona por múltiplos prestadores de serviço, com monitoramento em tempo real via interfaces Web e Desktop. O foco principal é demonstrar uma arquitetura desacoplada, escalável e de alta performance.

## 🏗️ Arquitetura e Padrões

- **Clean Architecture**: Divisão clara entre Domain, Application, Infrastructure e Presentation.
- **Desacoplamento Total**: Uso extensivo de interfaces e injeção de dependência.
- **Padrões Criacionais**: Utilização de Factory Method para instanciar objetos complexos.
- **Tratamento de Erros**: Implementação de blocos try/catch em todas as camadas para garantir estabilidade.
- **Nomenclatura**: Padrões C# (PascalCase/camelCase) com comentários em Português (sem acentos).

## 🛠️ Tecnologias Utilizadas

- **Linguagem**: C# .NET 8.0
- **Persistência**: Dapper (SQLite/SQL Server)
- **Mensageria**: RabbitMQ (Docker)
- **Containerização**: Docker (Configurações persistentes)
- **Testes**: xUnit e Moq para testes unitários.

## ⚙️ Plano de Desenvolvimento

O projeto será desenvolvido seguindo as seguintes fases:

### Fase 1: Infraestrutura e Estrutura Base
- Configuração do `docker-compose.yml` para RabbitMQ com volumes persistentes.
- Criação da Solution e Projetos seguindo a Clean Architecture:
  - `SGPST.Domain`, `SGPST.Application`, `SGPST.Infrastructure`, `SGPST.Presentation`, `SGPST.Tests`.
- Definição de interfaces base e do padrão de tratamento de erros global (`try/catch`).

### Fase 2: Camada de Domain (Domínio)
- Modelagem das entidades (Pedido, Usuário, Prestador).
- Definição das interfaces de contrato (Repositórios e Serviços de Mensageria).
- Implementação de **Factories** para criação de objetos de domínio.

### Fase 3: Camada de Infrastructure (Infraestrutura)
- Implementação da persistência de dados com **Dapper**.
- Desenvolvimento da biblioteca de integração com **RabbitMQ**.
- Configuração final dos containers Docker.

### Fase 4: Camada de Application (Aplicação)
- Desenvolvimento dos Casos de Uso (Envio de Pedido, Processamento, Listagem).
- Implementação da lógica de orquestração com tratamento de exceções rigoroso.
- Criação dos DTOs para tráfego de dados entre camadas.

### Fase 5: Apresentação - APIs e Workers
- Desenvolvimento da **API de Envio** (Producer).
- Desenvolvimento do **Worker Console** (Consumer) para processamento proporcional de pedidos.
- Desenvolvimento do **Gerador de Carga** (Console) para simulação de pedidos randômicos.

### Fase 6: Apresentação - Frontends
- Implementação da **Interface Web** (ASP.NET Core MVC) para monitoramento em tempo real.
- Implementação da **Interface Desktop** (WPF/WinForms) para controle administrativo.

### Fase 7: Validação e Testes
- Implementação de testes unitários com **xUnit** e **Moq** em todas as camadas.
- Testes de integração ponta a ponta no ambiente Docker.
- Documentação final de uso e operação.

---
Desenvolvido com foco em alta performance, desacoplamento e testabilidade.

# SGPST - Sistema de Gerenciamento de Pedidos de Suporte Técnico

Este projeto é um sistema de gerenciamento de pedidos de suporte técnico, desenvolvido em C# .NET 8.0, utilizando RabbitMQ para mensageria e Dapper para persistência de dados. O sistema segue os princípios da **Clean Architecture** e **SOLID**.

## 🚀 Visão Geral

O SGPST permite que clientes simulem pedidos de suporte que são processados de forma assíncrona por múltiplos prestadores de serviço, com monitoramento em tempo real via interfaces Web e Desktop.

## 📂 Estrutura do Projeto

Toda a documentação detalhada e definições do projeto estão centralizadas na pasta [`/docs`](./docs):

- [**Proposta do Projeto**](./docs/README.md): Visão geral original e escopo.
- [**Arquitetura**](./docs/arquitetura.md): Detalhamento das camadas da Clean Architecture.
- [**Regras de Desenvolvimento**](./docs/regras.md): Padrões de código, nomenclatura e diretrizes técnicas.
- [**Instruções Gemini**](./docs/Gemini.md): Mandatos específicos para o assistente de IA.
- [**Memória do Projeto**](./docs/MEMORY.md): Registro de decisões e progresso.

## 🛠️ Tecnologias Utilizadas

- **Linguagem**: C# .NET 8.0
- **Persistência**: Dapper (SQLite/SQL Server)
- **Mensageria**: RabbitMQ (via Docker)
- **Containerização**: Docker
- **Testes**: xUnit, Moq

## ⚙️ Como Executar (Em breve)

O projeto será orquestrado via Docker Compose. As instruções de execução serão adicionadas conforme o desenvolvimento progredir.

---
Desenvolvido com foco em alta performance, desacoplamento e testabilidade.

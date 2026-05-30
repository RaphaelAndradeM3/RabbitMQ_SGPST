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

## ⚙️ Como Executar (Em breve)

O projeto será orquestrado via Docker Compose, garantindo que o ambiente de mensageria e banco de dados subam de forma automatizada e persistente.

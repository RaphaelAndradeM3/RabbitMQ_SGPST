# Arquitetura do Projeto SGPST

O Sistema de Gerenciamento de Pedidos de Suporte Técnico segue os princípios da **Clean Architecture** (Arquitetura Limpa) para garantir manutenibilidade, testabilidade e independência de frameworks.

## Camadas

### 1. Domain (Domínio)
- Contém as entidades de negócio, interfaces de repositórios e interfaces de serviços.
- Não possui dependências de outras camadas.
- Define o "coração" do sistema.

### 2. Application (Aplicação)
- Contém os Casos de Uso (Use Cases).
- Implementa a lógica de aplicação e orquestra o fluxo de dados.
- Utiliza DTOs (Data Transfer Objects) para comunicação com as camadas externas.

### 3. Infrastructure (Infraestrutura)
- Implementações de acesso a dados usando **Dapper**.
- Integração com **RabbitMQ** para mensageria.
- Implementações de serviços externos (e-mail, logs, etc.).
- Persistência de dados (SQLite/SQL Server).

### 4. Presentation (Apresentação)
- **API de Envio**: Endpoint para receber pedidos.
- **API de Listagem**: Endpoint para consultar pedidos.
- **Interfaces Web**: ASP.NET Core MVC.
- **Interfaces Desktop**: WPF/WinForms.
- **Clientes Console**: Geradores de pedidos e Processadores (Workers).

## Padrões de Comunicação
- A comunicação entre camadas é feita através de **Interfaces**.
- O **RabbitMQ** atua como o mediador para o processamento assíncrono de pedidos, garantindo escalabilidade.

# Sistema de Gerenciamento de Pedidos de Suporte Técnico

Este projeto é um sistema de gerenciamento de pedidos de suporte técnico, desenvolvido em C# .NET 6.0. O sistema é dividido em várias camadas e utiliza RabbitMQ para gerenciamento de filas de mensagens. O Dapper é utilizado para o acesso a dados, seguindo os princípios SOLID. O sistema inclui APIs REST separadas, interfaces web e desktop, e clientes console para simulação de geração e processamento de pedidos.

## Arquitetura

O sistema é composto pelos seguintes componentes:

- **Biblioteca de Modelos de Dados**: Define as classes para troca de dados entre as camadas.
- **Biblioteca de Acesso a Dados (DAL)**: Implementada com Dapper, realiza as operações no banco de dados.
- **Biblioteca de Negócios (BLL)**: Contém a lógica de negócios para manipulação dos pedidos.
- **Biblioteca de Integração com RabbitMQ**: Gerencia a comunicação com RabbitMQ para envio e recebimento de mensagens.
- **API de Envio de Pedidos**: Recebe pedidos de suporte técnico e os coloca na fila do RabbitMQ.
- **API de Listagem de Pedidos**: Fornece informações sobre pedidos pendentes e realizados.
- **Clientes Consoles**:
  - **Geração de Pedidos**: Simula usuários enviando pedidos randômicos.
  - **Prestadores de Serviços**: Consome e processa pedidos das filas do RabbitMQ.
- **Interface Web**: Desenvolvida em ASP.NET Core MVC para exibição e controle de pedidos.
- **Interface Desktop**: Desenvolvida em WPF/WinForms para exibição e controle de pedidos.

## Funcionalidades

- **Geração e Envio de Pedidos**: Clientes consoles simulam a geração e envio de pedidos para a API.
- **Processamento de Pedidos**: Prestadores de serviços consomem os pedidos e os processam, com tempo de processamento configurável.
- **Autenticação**: A interface web possui sistema de login para um usuário master configurado.
- **Exibição de Pedidos**: As interfaces web e desktop mostram pedidos pendentes e realizados, indicando quem solicitou e quem está processando.

## Tecnologias Utilizadas

- **C# .NET 8.0**
- **ASP.NET Core MVC**
- **WPF/WinForms**
- **Dapper**
- **RabbitMQ**
- **SQLite/SQL Server**

## Estrutura do Projeto

```plaintext
/src
│
├── /SupportSystem.Models
│   ├── OrderDTO.cs
│
├── /SupportSystem.DAL
│   ├── OrderRepository.cs
│   ├── DbConnectionFactory.cs
│   ├── UserRepository.cs
│
├── /SupportSystem.BLL
│   ├── OrderService.cs
│   ├── UserService.cs
│
├── /SupportSystem.RabbitMQIntegration
│   ├── RabbitMQClient.cs
│
├── /SupportSystem.API.Orders
│   ├── /Controllers
│   │   ├── OrderController.cs
│   ├── Program.cs
│
├── /SupportSystem.API.Listing
│   ├── /Controllers
│   │   ├── ListingController.cs
│   ├── Program.cs
│
├── /SupportSystem.Client.Console
│   ├── Program.cs
│
├── /SupportSystem.Service.Console
│   ├── Program.cs
│
├── /SupportSystem.Web
│   ├── /Controllers
│   ├── /Views
│   ├── Program.cs
│
└── /SupportSystem.Desktop
    ├── MainWindow.xaml
    ├── Program.cs

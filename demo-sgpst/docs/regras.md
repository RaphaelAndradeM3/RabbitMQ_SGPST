# Regras de Desenvolvimento SGPST

Este documento define as regras de codificação e padrões técnicos para o projeto.

## 1. Nomenclatura (C#)
- **Classes e Métodos**: PascalCase (ex: `ProcessarPedido`).
- **Interfaces**: Prefixo 'I' seguido de PascalCase (ex: `IOrderRepository`).
- **Variáveis Locais e Parâmetros**: camelCase (ex: `pedidoId`).
- **Campos Privados**: camelCase com sublinhado (ex: `_rabbitMqClient`).

## 2. Tratamento de Erros
- **Try/Catch Obrigatório**: Todos os métodos devem ser envolvidos em blocos `try-catch`.
- **Tratamento**: Erros devem ser logados e, se necessário, propagados de forma controlada através de Result patterns ou exceções customizadas que não exponham detalhes da infraestrutura.

## 3. Comentários e Documentação
- **Idioma**: Português.
- **Restrição**: **NÃO utilizar acentos** ou caracteres especiais em comentários (ex: use "configuracao" em vez de "configuração").

## 4. Injeção de Dependência e Desacoplamento
- **Interface em Tudo**: Nenhum componente deve depender de uma implementação concreta.
- **Padrões Criacionais**: Use **Factory Method** ou outros padrões criacionais para a criação de instâncias complexas, facilitando a substituição e o teste.

## 5. Docker e RabbitMQ
- O ambiente de desenvolvimento deve ser orquestrado via `docker-compose.yml`.
- O container do RabbitMQ deve ter volumes configurados para persistir:
  - Definições de filas e exchanges.
  - Dados de mensagens (se necessário).
  - Configurações de usuários e permissões.

## 6. Testes Unitários
- Todo serviço, repositório e lógica de negócio deve ter cobertura de testes unitários.
- Utilize frameworks como xUnit, Moq e FluentAssertions.

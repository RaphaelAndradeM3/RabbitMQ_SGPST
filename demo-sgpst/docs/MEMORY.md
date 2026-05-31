# Memoria do Projeto SGPST

Este arquivo serve como um indice de decisoes, estados e progresso do projeto.

## Estado Atual
- Estrutura de documentacao inicial criada em /docs.
- Regras de arquitetura e desenvolvimento definidas.
- **Fase 1 Concluida**: Solution inicializada, projetos criados seguindo Clean Architecture, docker-compose configurado.
- **Fase 2 Concluida**: Modelagem do Domain finalizada (Order, User, ServiceProvider) com Factory Methods e Interfaces de contrato.
- **Fase 3 Concluida**: Implementacao da Infraestrutura finalizada com Dapper (SQLite) e RabbitMQ. Repositorios e MessageBroker prontos para uso.
- **Fase 4 Concluida**: Camada de Application finalizada. Casos de uso (OrderService), DTOs e Interfaces de servico implementados com tratamento de erros global.
- **Fase 5 Concluida**: Camada de Apresentacao finalizada. Web API com Swagger configurada (acessivel em /swagger), Worker de processamento e Gerador de carga implementados.
- **Fase 6 Concluida**: Frontends finalizados. Dashboard Web (MVC) com Bootstrap e aplicacao Desktop (WPF) para monitoramento administrativo implementados.
- **Fase 7 Concluida**: Validacao e Testes Unitarios finalizados. Cobertura de testes para Domain e Application garantindo a integridade dos fluxos de negocio.
- **CI/CD**: Configurado GitHub Action (windows-latest) para compilacao e execucao automatica de testes.
- **Melhorias de Resiliencia**: Implementado mecanismo de retry na conexao com RabbitMQ.

## Decisoes Arquiteturais
- **Linguagem**: C# .NET 10.0 (detectado via SDK).
- **CI/CD**: GitHub Actions (Windows-latest).
- **ORM**: Dapper (para performance e controle).
- **Banco de Dados**: SQLite (escolhido para persistencia local e simplicidade no prototipo).
- **Mensageria**: RabbitMQ (Docker exposto em localhost:5672).
- **Estilo**: Clean Architecture.
- **Tratamento de Erros**: Padronizado com IAppResult e AppResult.

## Status Final
O prototipo funcional do SGPST esta completo e resiliente.

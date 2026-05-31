# Instruções para Gemini CLI - Projeto SGPST

Este arquivo contém as diretrizes que o Gemini CLI deve seguir ao desenvolver o Sistema de Gerenciamento de Pedidos de Suporte Técnico (SGPST).

## Mandatos Principais

1. **Arquitetura**: Utilize estritamente **Clean Architecture**. Divida o projeto em camadas claras: Domain, Application, Infrastructure e Presentation.
2. **Padrões de Projeto**: 
   - Utilize **interfaces em tudo** para garantir o desacoplamento.
   - Priorize **Padrões de Projeto Criacionais**, especialmente o **Factory Method**, para instanciar objetos e evitar acoplamento direto.
3. **Tratamento de Erros**: Implemente blocos `try/catch` em todas as funções e métodos para capturar e tratar erros, evitando travamentos e exceções não tratadas.
4. **Qualidade de Código e Comunicação**:
   - **Idioma**: Todas as respostas do assistente devem ser obrigatoriamente em **Português do Brasil**, mesmo que a pergunta tenha sido feita em inglês.
   - **Nomenclatura**: Siga os padrões padrão de C# (PascalCase para classes, interfaces e métodos; camelCase para variáveis locais e campos privados com prefixo `_`).
   - **Testes**: Todo novo código deve vir acompanhado de testes unitários.
   - **Comentários**: Devem ser em Português, mas **sem acentos** para evitar problemas de codificação.
5. **Desenvolvimento e Infraestrutura**:
   - O desenvolvimento deve ser feito com **Docker**.
   - O **RabbitMQ** deve rodar em um container com pastas e configurações persistentes (volumes).
   - Utilize **Dapper** para acesso a dados.

## Fluxo de Trabalho

- Sempre valide as mudanças com testes.
- Mantenha a documentação atualizada na pasta `/docs`.
- A raiz do projeto deve conter apenas o `README.md` bem documentado e arquivos de configuração essenciais.

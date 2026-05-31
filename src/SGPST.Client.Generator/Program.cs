using System.Net.Http.Headers;
using System.Net.Http.Json;
using SGPST.Application.DTOs.Auth;
using SGPST.Application.DTOs.Client;
using SGPST.Application.DTOs.SupportTicket;

Console.Title = "SGPST Enterprise - Gerador de Carga de Testes";

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("=========================================================");
Console.WriteLine("    SGPST ENTERPRISE - GERADOR DE CARGA DE TESTES        ");
Console.WriteLine("=========================================================");
Console.ResetColor();

const string apiBaseUrl = "http://localhost:5196/api/";
using var httpClient = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };

string? token = null;
List<ClientDto> clients = new();
var random = new Random();

int totalSent = 0;
int totalSuccess = 0;
int totalFailed = 0;

string[] templatesTitulo = {
    "Instalacao de software corporativo",
    "Lentidao extrema no sistema de banco de dados",
    "Falha de autenticacao no portal do parceiro",
    "Configuracao de VPN para home office",
    "Falha na impressora do financeiro",
    "Substituicao de mouse e teclado",
    "Configurar conta de email corporativo",
    "Queda constante de conexao com a rede local",
    "Restauracao de backup de arquivos perdidos",
    "Problema ao acessar compartilhamento de rede"
};

string[] templatesDescricao = {
    "O usuario relata que nao consegue prosseguir devido a um erro inesperado na tela inicial.",
    "O banco de dados apresenta alto consumo de CPU e memoria, travando as operacoes comerciais.",
    "Credenciais corretas digitadas mas o sistema retorna erro 403 proibido.",
    "Conexao cai apos alguns minutos de uso, impedindo o trabalho remoto estável.",
    "Impressora nao responde aos comandos da rede mesmo ligada e com papel.",
    "Equipamentos antigos apresentando mau funcionamento e travamentos físicos.",
    "Necessidade de configurar no Outlook com os novos protocolos de segurança SSL.",
    "A placa de rede do computador perde o IP de tempos em tempos de forma intermitente.",
    "Foi apagada uma pasta importante por engano e precisa recuperar o estado de ontem.",
    "Mensagem de erro diz que o caminho de rede nao foi localizado."
};

// Fluxo 1: Autenticacao
await AutenticarAsync();

// Fluxo 2: Obter Clientes
await CarregarClientesAsync();

// Fluxo 3: Loop de Geração de Carga
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("\n[INICIANDO] Gerador operacional. Iniciando envio de chamados em loop...\n");
Console.ResetColor();

while (true)
{
    try
    {
        if (clients.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[ERRO] Nenhum cliente disponivel para gerar chamados. Tentando recarregar...");
            Console.ResetColor();
            await Task.Delay(5000);
            await CarregarClientesAsync();
            continue;
        }

        // Escolhe dados aleatorios
        var client = clients[random.Next(clients.Count)];
        var title = $"{templatesTitulo[random.Next(templatesTitulo.Length)]} #{random.Next(1000, 9999)}";
        var description = templatesDescricao[random.Next(templatesDescricao.Length)];
        var priority = random.Next(1, 5); // 1-Baixa, 2-Media, 3-Alta, 4-Urgente
        var type = random.Next(1, 3);     // 1-Remoto, 2-Presencial

        var createDto = new CreateSupportTicketDto(client.Id, title, description, priority, type);

        totalSent++;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[ENVIANDO #{totalSent}] Chamado para {client.Name} | Prioridade: {priority} | Tipo: {type}");
        Console.ResetColor();

        var response = await httpClient.PostAsJsonAsync("tickets", createDto);

        if (response.IsSuccessStatusCode)
        {
            totalSuccess++;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[SUCESSO #{totalSuccess}] Chamado criado com sucesso via API.");
            Console.ResetColor();
        }
        else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            totalFailed++;
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("[EXPIRADO] Token JWT expirado ou invalido. Reautenticando...");
            Console.ResetColor();
            await AutenticarAsync();
        }
        else
        {
            totalFailed++;
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[FALHA #{totalFailed}] API respondeu com erro {response.StatusCode}: {errorContent}");
            Console.ResetColor();
        }

        // Exibe estatísticas parciais
        double successRate = totalSent > 0 ? ((double)totalSuccess / totalSent) * 100 : 0;
        Console.Title = $"SGPST Load Generator | Total: {totalSent} | Sucesso: {totalSuccess} ({successRate:0.0}%)";

        // Intervalo aleatorio de delay para simular comportamento humano (1.5 a 4 segundos)
        var delayMs = random.Next(1500, 4001);
        await Task.Delay(delayMs);
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[ERRO DE CONEXAO] Erro ao enviar chamado: {ex.Message}");
        Console.ResetColor();
        await Task.Delay(5000);
    }
}

async Task AutenticarAsync()
{
    while (true)
    {
        try
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[AUTENTICACAO] Tentando autenticar na API como admin...");
            Console.ResetColor();

            var loginDto = new LoginDto("admin", "admin123");
            var response = await httpClient.PostAsJsonAsync("auth/login", loginDto);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TokenResultDto>();
                if (result != null && !string.IsNullOrEmpty(result.Token))
                {
                    token = result.Token;
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("[AUTENTICACAO] Autenticado com sucesso! Token JWT carregado.");
                    Console.ResetColor();
                    break;
                }
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[AUTENTICACAO] Falha ao logar na API. Status: {response.StatusCode}. Tentando novamente em 5 segundos...");
            Console.ResetColor();
            await Task.Delay(5000);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[AUTENTICACAO ERRO] Falha de conexao com a API: {ex.Message}. Tentando novamente em 5 segundos...");
            Console.ResetColor();
            await Task.Delay(5000);
        }
    }
}

async Task CarregarClientesAsync()
{
    while (true)
    {
        try
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[CLIENTES] Carregando clientes da base de dados...");
            Console.ResetColor();

            var fetched = await httpClient.GetFromJsonAsync<IEnumerable<ClientDto>>("clients");
            if (fetched != null)
            {
                clients = fetched.ToList();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[CLIENTES] {clients.Count} clientes carregados com sucesso.");
                Console.ResetColor();

                if (clients.Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("[CLIENTES] Nenhum cliente encontrado na base. Cadastrando clientes mock de teste...");
                    Console.ResetColor();
                    await CadastrarClientesPadraoAsync();
                    continue; // Repete para carregar os clientes cadastrados
                }
                break;
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[CLIENTES ERRO] Erro ao carregar clientes: {ex.Message}. Re-tentando em 5 segundos...");
            Console.ResetColor();
            await Task.Delay(5000);
        }
    }
}

async Task CadastrarClientesPadraoAsync()
{
    var novosClientes = new[] {
        new CreateClientDto("Empresa Alfa Ltda", "11222333000101", "contato@alfa.com", "11988887777", "Av. Paulista, 1000", "Bela Vista", "Sao Paulo", "SP", "01311000"),
        new CreateClientDto("Empresa Beta Corp", "44555666000102", "suporte@beta.com", "21977776666", "Av. Rio Branco, 500", "Centro", "Rio de Janeiro", "RJ", "20040000"),
        new CreateClientDto("Empresa Gama Sistemas", "77888999000103", "financeiro@gama.com", "31966665555", "Rua da Bahia, 1200", "Lourdes", "Belo Horizonte", "MG", "30160011")
    };

    foreach (var clientDto in novosClientes)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("clients", clientDto);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[CLIENTES] Cliente '{clientDto.Name}' cadastrado com sucesso.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[CLIENTES] Falha ao cadastrar cliente '{clientDto.Name}': {response.StatusCode}");
                Console.ResetColor();
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[CLIENTES ERRO] Falha de conexao ao cadastrar cliente: {ex.Message}");
            Console.ResetColor();
        }
    }
}

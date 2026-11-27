using Chapter.WebApi.Contexts;
using Chapter.WebApi.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Chapter.WebApi.Tests;

/// <summary>
/// Sobe a API Chapter inteira em memória (TestServer) para os testes de integração.
/// Usa a configuração real do Startup (rotas, controllers, autenticação JWT, repositórios),
/// trocando apenas o SQL Server por um banco EF Core InMemory.
/// </summary>
/// <remarks>
/// O tutorial monta o TestServer com "new WebHostBuilder().UseStartup&lt;Startup&gt;()".
/// No .NET 10 essa forma é obsoleta (avisos ASPDEPR004 e ASPDEPR008). A WebApplicationFactory,
/// do pacote Microsoft.AspNetCore.Mvc.Testing instalado no Passo 13, faz o mesmo:
/// cria um TestServer a partir do Program/Startup da API.
/// </remarks>
public class ChapterApiFactory : WebApplicationFactory<Program>
{
    // Nome único: cada instância (ou seja, cada teste) tem o seu próprio banco, sempre começando vazio.
    private readonly string _nomeDoBanco = "ChapterTestes_" + Guid.NewGuid();

    // Massa de dados padrão (ver "Massa de dados" na Especificação de Casos de Teste).
    public const string EmailValido = "admin@chapter.com";
    public const string SenhaValida = "123456";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureTestServices(services =>
        {
            // Registra as opções do contexto apontando para o banco em memória.
            // Com as opções registradas, o ChapterContext é criado pelo construtor que as recebe,
            // e o OnConfiguring não aplica a connection string do SQL Server (IsConfigured = true).
            services.AddDbContext<ChapterContext>(options => options.UseInMemoryDatabase(_nomeDoBanco));
        });
    }

    /// <summary>
    /// Grava a massa de dados no banco em memória deste teste.
    /// </summary>
    /// <param name="incluirLivros">false para deixar a tabela Livros vazia (CT02).</param>
    public void PopularBanco(bool incluirLivros = true)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ChapterContext>();

        if (incluirLivros)
        {
            context.Livros.AddRange(
                new Livro { Id = 1, Titulo = "Dom Casmurro", QuantidadePaginas = 256, Disponivel = true },
                new Livro { Id = 2, Titulo = "O Cortiço", QuantidadePaginas = 304, Disponivel = false });
        }

        context.Usuarios.Add(new Usuario { Id = 1, Email = EmailValido, Senha = SenhaValida });
        context.SaveChanges();
    }
}

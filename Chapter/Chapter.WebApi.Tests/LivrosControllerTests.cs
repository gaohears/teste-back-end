using System.Net;
using System.Net.Http.Headers;
using Chapter.WebApi.Models;
using Newtonsoft.Json.Linq;

namespace Chapter.WebApi.Tests;

/// <summary>
/// Testes de integração dos endpoints /api/livros.
/// Cada requisição percorre a API real: rota, autenticação, controller, repositório, EF Core e banco (InMemory).
/// Cada caso está ligado à "Especificação de Casos de Teste" pelo ID (CT01 a CT13).
/// </summary>
public class LivrosControllerTests : IDisposable
{
    private readonly ChapterApiFactory _factory;
    private readonly HttpClient _client;

    // O xUnit cria uma instância nova da classe para cada teste,
    // então cada teste recebe uma API e um banco novos (testes independentes).
    public LivrosControllerTests()
    {
        _factory = new ChapterApiFactory();
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    // ------------------------------------------------------------------
    // GET /api/livros (RF01)
    // ------------------------------------------------------------------

    [Fact]
    public async Task CT01_Listar_ComLivrosCadastrados_Retorna200EOsDoisLivros()
    {
        // Arrange
        _factory.PopularBanco();

        // Act
        var resposta = await _client.GetAsync("/api/livros");

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var livros = await HttpJson.LerAsync<List<Livro>>(resposta);
        Assert.Equal(2, livros.Count);
        Assert.Contains(livros, l => l.Titulo == "Dom Casmurro");
        Assert.Contains(livros, l => l.Titulo == "O Cortiço");
    }

    [Fact]
    public async Task CT02_Listar_BancoVazio_Retorna200EListaVazia()
    {
        // Arrange
        _factory.PopularBanco(incluirLivros: false);

        // Act
        var resposta = await _client.GetAsync("/api/livros");

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var livros = await HttpJson.LerAsync<List<Livro>>(resposta);
        Assert.Empty(livros);
    }

    // ------------------------------------------------------------------
    // GET /api/livros/{id} (RF02 e RF07)
    // ------------------------------------------------------------------

    [Fact]
    public async Task CT03_BuscarPorId_IdExistente_Retorna200EOLivro()
    {
        // Arrange
        _factory.PopularBanco();

        // Act
        var resposta = await _client.GetAsync("/api/livros/1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var livro = await HttpJson.LerAsync<Livro>(resposta);
        Assert.Equal(1, livro.Id);
        Assert.Equal("Dom Casmurro", livro.Titulo);
        Assert.Equal(256, livro.QuantidadePaginas);
        Assert.True(livro.Disponivel);
    }

    [Fact]
    public async Task CT04_BuscarPorId_IdInexistente_Retorna404()
    {
        // Arrange
        _factory.PopularBanco();

        // Act
        var resposta = await _client.GetAsync("/api/livros/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task CT05_BuscarPorId_IdNaoNumerico_Retorna400()
    {
        // Arrange
        _factory.PopularBanco();

        // Act
        var resposta = await _client.GetAsync("/api/livros/abc");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    // ------------------------------------------------------------------
    // POST /api/livros (RF03 e RF07)
    // ------------------------------------------------------------------

    [Fact]
    public async Task CT06_Cadastrar_LivroValido_Retorna201EOLivroPassaAConstarNaLista()
    {
        // Arrange
        _factory.PopularBanco();
        var novoLivro = new { titulo = "Iracema", quantidadePaginas = 200, disponivel = true };

        // Act
        var resposta = await _client.PostAsync("/api/livros", HttpJson.Corpo(novoLivro));

        // Assert: status da resposta
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);

        // Assert: o livro foi realmente gravado no banco
        var livros = await HttpJson.LerAsync<List<Livro>>(await _client.GetAsync("/api/livros"));
        Assert.Equal(3, livros.Count);
        Assert.Contains(livros, l => l.Titulo == "Iracema" && l.QuantidadePaginas == 200 && l.Disponivel);
    }

    [Fact]
    public async Task CT07_Cadastrar_CorpoVazio_Retorna400()
    {
        // Arrange
        _factory.PopularBanco();
        var corpoVazio = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");

        // Act
        var resposta = await _client.PostAsync("/api/livros", corpoVazio);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    // ------------------------------------------------------------------
    // PUT /api/livros/{id} (RF04)
    // ------------------------------------------------------------------

    [Fact]
    public async Task CT08_Atualizar_IdExistente_Retorna204EOsDadosSaoAlterados()
    {
        // Arrange
        _factory.PopularBanco();
        var dadosNovos = new { titulo = "Dom Casmurro - 2ª ed.", quantidadePaginas = 300, disponivel = false };

        // Act
        var resposta = await _client.PutAsync("/api/livros/1", HttpJson.Corpo(dadosNovos));

        // Assert: status da resposta
        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);

        // Assert: os dados foram realmente alterados no banco
        var livro = await HttpJson.LerAsync<Livro>(await _client.GetAsync("/api/livros/1"));
        Assert.Equal("Dom Casmurro - 2ª ed.", livro.Titulo);
        Assert.Equal(300, livro.QuantidadePaginas);
        Assert.False(livro.Disponivel);
    }

    [Fact]
    public async Task CT09_Atualizar_IdInexistente_Retorna404()
    {
        // Arrange
        _factory.PopularBanco();
        var dados = new { titulo = "Inexistente", quantidadePaginas = 100, disponivel = true };

        // Act
        var resposta = await _client.PutAsync("/api/livros/999", HttpJson.Corpo(dados));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    // ------------------------------------------------------------------
    // DELETE /api/livros/{id} (RF05)
    // ------------------------------------------------------------------

    [Fact]
    public async Task CT10_Deletar_SemToken_Retorna401EOLivroNaoEExcluido()
    {
        // Arrange
        _factory.PopularBanco();

        // Act
        var resposta = await _client.DeleteAsync("/api/livros/1");

        // Assert: acesso negado
        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);

        // Assert: o livro continua existindo
        var busca = await _client.GetAsync("/api/livros/1");
        Assert.Equal(HttpStatusCode.OK, busca.StatusCode);
    }

    [Fact]
    public async Task CT11_Deletar_TokenInvalido_Retorna401()
    {
        // Arrange
        _factory.PopularBanco();
        var requisicao = CriarDeleteAutenticado("/api/livros/1", "token-falso");

        // Act
        var resposta = await _client.SendAsync(requisicao);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task CT12_Deletar_IdExistenteAutenticado_Retorna204EOLivroDeixaDeExistir()
    {
        // Arrange
        _factory.PopularBanco();
        string token = await ObterTokenAsync();
        var requisicao = CriarDeleteAutenticado("/api/livros/1", token);

        // Act
        var resposta = await _client.SendAsync(requisicao);

        // Assert: status da resposta
        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);

        // Assert: o livro foi realmente excluído do banco
        var busca = await _client.GetAsync("/api/livros/1");
        Assert.Equal(HttpStatusCode.NotFound, busca.StatusCode);
    }

    [Fact]
    public async Task CT13_Deletar_IdInexistenteAutenticado_Retorna404()
    {
        // Arrange
        _factory.PopularBanco();
        string token = await ObterTokenAsync();
        var requisicao = CriarDeleteAutenticado("/api/livros/999", token);

        // Act
        var resposta = await _client.SendAsync(requisicao);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    // ------------------------------------------------------------------
    // Métodos auxiliares
    // ------------------------------------------------------------------

    /// <summary>
    /// Faz login com o usuário da massa de dados e devolve o token JWT.
    /// Falha com mensagem clara se o login não funcionar (pré-condição do CT12 e do CT13).
    /// </summary>
    private async Task<string> ObterTokenAsync()
    {
        var login = new { email = ChapterApiFactory.EmailValido, senha = ChapterApiFactory.SenhaValida };
        var resposta = await _client.PostAsync("/api/login", HttpJson.Corpo(login));

        Assert.True(resposta.StatusCode == HttpStatusCode.OK,
            $"Pré-condição falhou: o login (CT14) deveria retornar 200, mas retornou {(int)resposta.StatusCode}.");

        var corpo = JObject.Parse(await resposta.Content.ReadAsStringAsync());
        return corpo["token"]!.ToString();
    }

    private static HttpRequestMessage CriarDeleteAutenticado(string url, string token)
    {
        var requisicao = new HttpRequestMessage(HttpMethod.Delete, url);
        requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return requisicao;
    }
}

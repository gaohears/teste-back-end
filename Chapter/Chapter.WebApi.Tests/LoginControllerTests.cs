using System.Net;
using Newtonsoft.Json.Linq;

namespace Chapter.WebApi.Tests;

/// <summary>
/// Testes de integração do endpoint /api/login (RF06).
/// Cada caso está ligado à "Especificação de Casos de Teste" pelo ID (CT14 e CT15).
/// </summary>
public class LoginControllerTests : IDisposable
{
    private readonly ChapterApiFactory _factory;
    private readonly HttpClient _client;

    public LoginControllerTests()
    {
        _factory = new ChapterApiFactory();
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task CT14_Login_CredenciaisValidas_Retorna200EToken()
    {
        // Arrange
        _factory.PopularBanco();
        var login = new { email = ChapterApiFactory.EmailValido, senha = ChapterApiFactory.SenhaValida };

        // Act
        var resposta = await _client.PostAsync("/api/login", HttpJson.Corpo(login));

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var corpo = JObject.Parse(await resposta.Content.ReadAsStringAsync());
        Assert.False(string.IsNullOrWhiteSpace(corpo["token"]?.ToString()));
    }

    [Fact]
    public async Task CT15_Login_SenhaErrada_Retorna404EMensagem()
    {
        // Arrange
        _factory.PopularBanco();
        var login = new { email = ChapterApiFactory.EmailValido, senha = "errada" };

        // Act
        var resposta = await _client.PostAsync("/api/login", HttpJson.Corpo(login));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        string mensagem = await HttpJson.LerAsync<string>(resposta);
        Assert.Equal("E-mail ou senha inválidos!", mensagem);
    }
}

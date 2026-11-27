using System.Text;
using Newtonsoft.Json;

namespace Chapter.WebApi.Tests;

/// <summary>
/// Atalhos para enviar e ler JSON nas requisições dos testes (Newtonsoft.Json, como no tutorial).
/// </summary>
internal static class HttpJson
{
    /// <summary>Serializa o objeto como corpo JSON da requisição.</summary>
    public static StringContent Corpo(object objeto) =>
        new(JsonConvert.SerializeObject(objeto), Encoding.UTF8, "application/json");

    /// <summary>Lê o corpo da resposta e o converte para o tipo informado.</summary>
    public static async Task<T> LerAsync<T>(HttpResponseMessage resposta)
    {
        string json = await resposta.Content.ReadAsStringAsync();
        T? valor = JsonConvert.DeserializeObject<T>(json);
        Assert.NotNull(valor);
        return valor;
    }
}

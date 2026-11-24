using CalculadoraImc.Core;

namespace CalculadoraImc.Tests;

/// <summary>
/// Testes de unidade da calculadora de IMC (técnica caixa preta).
/// Cada caso está ligado ao documento "Especificação de Casos de Teste" pelo ID (CT01 a CT28).
/// </summary>
public class CalculadoraDeImcTests
{
    // ------------------------------------------------------------------
    // CalcularImc: fluxo básico (RF01 e RF02)
    // ------------------------------------------------------------------

    [Fact]
    public void CT01_CalcularImc_ValoresTipicos_Retorna22Virgula9()
    {
        // Arrange
        double peso = 70;
        double altura = 1.75;

        // Act
        double resultado = CalculadoraDeImc.CalcularImc(peso, altura);

        // Assert
        Assert.Equal(22.9, resultado);
    }

    // Obs.: usamos Assert.Equal sem o parâmetro "precision" de propósito.
    // Com precision, o xUnit arredondaria o valor obtido e esconderia um erro de arredondamento (RF02).
    [Theory]
    [InlineData(80.0, 2.0, 20.0)]    // CT02: resultado exato (RF01)
    [InlineData(75.0, 1.8, 23.1)]    // CT03: 23,148... arredonda para baixo (RF02)
    [InlineData(24.96, 1.0, 25.0)]   // CT04: arredonda para cima, cruzando a fronteira de faixa (RF02)
    [InlineData(24.94, 1.0, 24.9)]   // CT05: arredonda para baixo, sem cruzar a fronteira (RF02)
    public void CalcularImc_EntradasValidas_RetornaImcArredondado(double peso, double altura, double esperado)
    {
        // Act
        double resultado = CalculadoraDeImc.CalcularImc(peso, altura);

        // Assert
        Assert.Equal(esperado, resultado);
    }

    // ------------------------------------------------------------------
    // CalcularImc: fluxos alternativos A1 e A2 (RF04)
    // ------------------------------------------------------------------

    [Theory]
    [InlineData(0.0, 1.75)]     // CT06: peso igual a zero
    [InlineData(-70.0, 1.75)]   // CT07: peso negativo
    [InlineData(70.0, 0.0)]     // CT08: altura igual a zero
    [InlineData(70.0, -1.75)]   // CT09: altura negativa
    [InlineData(0.0, 0.0)]      // CT10: peso e altura iguais a zero
    public void CalcularImc_EntradaInvalida_LancaArgumentOutOfRangeException(double peso, double altura)
    {
        // Act + Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => CalculadoraDeImc.CalcularImc(peso, altura));
    }

    // ------------------------------------------------------------------
    // Classificar: fluxo básico (RF03), valores-limite e classes de equivalência
    // ------------------------------------------------------------------

    [Theory]
    [InlineData(18.4, "Baixo peso")]           // CT11
    [InlineData(18.5, "Eutrofia")]             // CT12
    [InlineData(18.6, "Eutrofia")]             // CT13
    [InlineData(24.9, "Eutrofia")]             // CT14
    [InlineData(25.0, "Sobrepeso")]            // CT15
    [InlineData(25.1, "Sobrepeso")]            // CT16
    [InlineData(29.9, "Sobrepeso")]            // CT17
    [InlineData(30.0, "Obesidade grau I")]     // CT18
    [InlineData(30.1, "Obesidade grau I")]     // CT19
    [InlineData(34.9, "Obesidade grau I")]     // CT20
    [InlineData(35.0, "Obesidade grau II")]    // CT21
    [InlineData(35.1, "Obesidade grau II")]    // CT22
    [InlineData(39.9, "Obesidade grau II")]    // CT23
    [InlineData(40.0, "Obesidade grau III")]   // CT24
    [InlineData(40.1, "Obesidade grau III")]   // CT25
    [InlineData(0.1, "Baixo peso")]            // CT26: menor IMC válido
    public void Classificar_ImcValido_RetornaClassificacaoDaAbeso(double imc, string esperado)
    {
        // Act
        string resultado = CalculadoraDeImc.Classificar(imc);

        // Assert
        Assert.Equal(esperado, resultado);
    }

    // ------------------------------------------------------------------
    // Classificar: fluxo alternativo A1 (RF05)
    // ------------------------------------------------------------------

    [Theory]
    [InlineData(0.0)]    // CT27: IMC igual a zero
    [InlineData(-5.0)]   // CT28: IMC negativo
    public void Classificar_ImcInvalido_LancaArgumentOutOfRangeException(double imc)
    {
        // Act + Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => CalculadoraDeImc.Classificar(imc));
    }
}
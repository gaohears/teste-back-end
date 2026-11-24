using CalculadoraImc.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CalculadoraImc.Tests.MSTest;

/// <summary>
/// Testes de unidade da calculadora de IMC em MSTest (técnica caixa preta).
/// Mesmos 28 casos do xUnit; cada caso está ligado ao documento de casos de teste pelo ID (CT01 a CT28).
/// </summary>
[TestClass]
public class CalculadoraDeImcMSTests
{
    // ------------------------------------------------------------------
    // CalcularImc: fluxo básico (RF01 e RF02)
    // ------------------------------------------------------------------

    [TestMethod]
    public void CT01_CalcularImc_ValoresTipicos_Retorna22Virgula9()
    {
        // Arrange
        double peso = 70;
        double altura = 1.75;

        // Act
        double resultado = CalculadoraDeImc.CalcularImc(peso, altura);

        // Assert
        Assert.AreEqual(22.9, resultado);
    }

    // Obs.: Assert.AreEqual sem "delta" de propósito: com tolerância, um erro de arredondamento (RF02) passaria despercebido.
    [TestMethod]
    [DataRow(80.0, 2.0, 20.0)]    // CT02: resultado exato (RF01)
    [DataRow(75.0, 1.8, 23.1)]    // CT03: 23,148... arredonda para baixo (RF02)
    [DataRow(24.96, 1.0, 25.0)]   // CT04: arredonda para cima, cruzando a fronteira de faixa (RF02)
    [DataRow(24.94, 1.0, 24.9)]   // CT05: arredonda para baixo, sem cruzar a fronteira (RF02)
    public void CalcularImc_EntradasValidas_RetornaImcArredondado(double peso, double altura, double esperado)
    {
        // Act
        double resultado = CalculadoraDeImc.CalcularImc(peso, altura);

        // Assert
        Assert.AreEqual(esperado, resultado);
    }

    // ------------------------------------------------------------------
    // CalcularImc: fluxos alternativos A1 e A2 (RF04)
    // ------------------------------------------------------------------

    [TestMethod]
    [DataRow(0.0, 1.75)]     // CT06: peso igual a zero
    [DataRow(-70.0, 1.75)]   // CT07: peso negativo
    [DataRow(70.0, 0.0)]     // CT08: altura igual a zero
    [DataRow(70.0, -1.75)]   // CT09: altura negativa
    [DataRow(0.0, 0.0)]      // CT10: peso e altura iguais a zero
    public void CalcularImc_EntradaInvalida_LancaArgumentOutOfRangeException(double peso, double altura)
    {
        // Act + Assert
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => CalculadoraDeImc.CalcularImc(peso, altura));
    }

    // ------------------------------------------------------------------
    // Classificar: fluxo básico (RF03), valores-limite e classes de equivalência
    // ------------------------------------------------------------------

    [TestMethod]
    [DataRow(18.4, "Baixo peso")]           // CT11
    [DataRow(18.5, "Eutrofia")]             // CT12
    [DataRow(18.6, "Eutrofia")]             // CT13
    [DataRow(24.9, "Eutrofia")]             // CT14
    [DataRow(25.0, "Sobrepeso")]            // CT15
    [DataRow(25.1, "Sobrepeso")]            // CT16
    [DataRow(29.9, "Sobrepeso")]            // CT17
    [DataRow(30.0, "Obesidade grau I")]     // CT18
    [DataRow(30.1, "Obesidade grau I")]     // CT19
    [DataRow(34.9, "Obesidade grau I")]     // CT20
    [DataRow(35.0, "Obesidade grau II")]    // CT21
    [DataRow(35.1, "Obesidade grau II")]    // CT22
    [DataRow(39.9, "Obesidade grau II")]    // CT23
    [DataRow(40.0, "Obesidade grau III")]   // CT24
    [DataRow(40.1, "Obesidade grau III")]   // CT25
    [DataRow(0.1, "Baixo peso")]            // CT26: menor IMC válido
    public void Classificar_ImcValido_RetornaClassificacaoDaAbeso(double imc, string esperado)
    {
        // Act
        string resultado = CalculadoraDeImc.Classificar(imc);

        // Assert
        Assert.AreEqual(esperado, resultado);
    }

    // ------------------------------------------------------------------
    // Classificar: fluxo alternativo A1 (RF05)
    // ------------------------------------------------------------------

    [TestMethod]
    [DataRow(0.0)]    // CT27: IMC igual a zero
    [DataRow(-5.0)]   // CT28: IMC negativo
    public void Classificar_ImcInvalido_LancaArgumentOutOfRangeException(double imc)
    {
        // Act + Assert
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => CalculadoraDeImc.Classificar(imc));
    }
}

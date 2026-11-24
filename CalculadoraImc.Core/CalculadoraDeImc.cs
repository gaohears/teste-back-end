using System;

namespace CalculadoraImc.Core
{
    /// <summary>
    /// Calculadora de IMC com classificação conforme a tabela da Abeso.
    /// Requisitos: RF01 a RF05 (Especificação de Requisitos da Calculadora de IMC).
    /// </summary>
    public static class CalculadoraDeImc
    {
        /// <summary>
        /// RF01, RF02 e RF04: calcula peso ÷ altura², arredondado para 1 casa decimal.
        /// </summary>
        /// <param name="peso">Peso em quilogramas (deve ser maior que zero).</param>
        /// <param name="altura">Altura em metros (deve ser maior que zero).</param>
        /// <returns>IMC arredondado para 1 casa decimal.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Peso ou altura menor ou igual a zero.</exception>
        public static double CalcularImc(double peso, double altura)
        {
            // RF04: rejeita entradas inválidas.
            // A forma !(x > 0) também rejeita NaN, que não é maior nem menor que zero.
            if (!(peso > 0))
                throw new ArgumentOutOfRangeException(nameof(peso), "O peso deve ser maior que zero.");

            if (!(altura > 0))
                throw new ArgumentOutOfRangeException(nameof(altura), "A altura deve ser maior que zero.");

            // RF01: fórmula do IMC.
            double imc = peso / (altura * altura);

            // RF02: 1 casa decimal; valor exatamente no meio sobe (ex.: 24,95 -> 25,0).
            return Math.Round(imc, 1, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// RF03 e RF05: devolve a classificação da Abeso para o IMC informado.
        /// </summary>
        /// <param name="imc">IMC (deve ser maior que zero).</param>
        /// <returns>Nome da classificação.</returns>
        /// <exception cref="ArgumentOutOfRangeException">IMC menor ou igual a zero.</exception>
        public static string Classificar(double imc)
        {
            // RF05: rejeita IMC inválido.
            if (!(imc > 0))
                throw new ArgumentOutOfRangeException(nameof(imc), "O IMC deve ser maior que zero.");

            // RF03: faixas da tabela, da menor para a maior.
            if (imc < 18.5) return "Baixo peso";
            if (imc <= 25.0) return "Eutrofia";
            if (imc < 30.0) return "Sobrepeso";
            if (imc < 35.0) return "Obesidade grau I";
            if (imc < 40.0) return "Obesidade grau II";
            return "Obesidade grau III";
        }
    }
}

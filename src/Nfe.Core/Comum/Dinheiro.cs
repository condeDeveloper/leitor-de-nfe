using System.Globalization;

namespace Nfe.Core.Comum;

/// <summary>
/// Conversão e arredondamento dos valores monetários do XML. O layout da NF-e
/// sempre usa ponto como separador decimal e duas casas nos totais, então a
/// leitura é feita com cultura invariante para não depender da máquina.
/// </summary>
public static class Dinheiro
{
    /// <summary>Casas decimais dos campos de valor da NF-e.</summary>
    public const int Casas = 2;

    /// <summary>Lê um valor do XML, devolvendo zero quando o campo está ausente.</summary>
    public static decimal Ler(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return 0m;
        }

        return decimal.TryParse(texto.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var valor)
            ? valor
            : throw new FormatException($"Valor numérico inválido: '{texto}'.");
    }

    /// <summary>Arredonda para duas casas usando o critério comercial.</summary>
    public static decimal Arredondar(decimal valor) => Math.Round(valor, Casas, MidpointRounding.AwayFromZero);

    /// <summary>Arredonda para a quantidade de casas informada.</summary>
    public static decimal Arredondar(decimal valor, int casas) => Math.Round(valor, casas, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Compara dois valores tolerando a diferença de um centavo, que é o que a
    /// própria SEFAZ aceita entre o somatório dos itens e o total declarado.
    /// </summary>
    public static bool Equivalem(decimal esquerda, decimal direita, decimal tolerancia = 0.01m)
        => Math.Abs(Arredondar(esquerda) - Arredondar(direita)) <= tolerancia;

    /// <summary>
    /// Formata o valor no padrão brasileiro, sem o símbolo da moeda. O formato
    /// é montado à mão porque a API roda com globalização invariante e não tem
    /// a cultura pt-BR disponível.
    /// </summary>
    public static string Formatar(decimal valor) => Formatar(valor, Casas);

    /// <summary>
    /// Formata o valor com a quantidade de casas informada, que nas quantidades
    /// comerciais chega a quatro.
    /// </summary>
    public static string Formatar(decimal valor, int casas)
    {
        var formato = new NumberFormatInfo
        {
            NumberDecimalSeparator = ",",
            NumberGroupSeparator = ".",
            NumberGroupSizes = [3],
            NumberNegativePattern = 1,
        };

        return Arredondar(valor, casas).ToString($"N{casas}", formato);
    }
}

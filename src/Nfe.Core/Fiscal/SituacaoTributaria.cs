namespace Nfe.Core.Fiscal;

/// <summary>Regime em que o emitente se enquadra, campo CRT do XML.</summary>
public enum RegimeTributario
{
    /// <summary>Simples Nacional.</summary>
    SimplesNacional = 1,

    /// <summary>Simples Nacional com excesso de sublimite de receita bruta.</summary>
    SimplesNacionalExcesso = 2,

    /// <summary>Regime normal, lucro presumido ou real.</summary>
    Normal = 3,
}

/// <summary>
/// Situação tributária do ICMS do item. Quem está no Simples Nacional usa
/// CSOSN, os demais usam CST, e as duas tabelas convivem no mesmo campo do XML,
/// então o tipo guarda qual delas foi usada.
/// </summary>
public readonly record struct SituacaoTributaria
{
    private static readonly int[] CstValidos =
        [0, 10, 20, 30, 40, 41, 50, 51, 60, 70, 90];

    private static readonly int[] CsosnValidos =
        [101, 102, 103, 201, 202, 203, 300, 400, 500, 900];

    private SituacaoTributaria(string codigo, bool csosn)
    {
        Codigo = codigo;
        EhCsosn = csosn;
    }

    /// <summary>O código como aparece no XML.</summary>
    public string Codigo { get; }

    /// <summary>Indica se o código pertence à tabela do Simples Nacional.</summary>
    public bool EhCsosn { get; }

    /// <summary>Indica se a situação prevê tributação integral pelo emitente.</summary>
    public bool TributaIntegralmente => !EhCsosn && Codigo is "00" or "0";

    /// <summary>Indica se a situação é de isenção, não tributação ou suspensão.</summary>
    public bool Desonera => EhCsosn
        ? Codigo is "300" or "400"
        : Codigo is "30" or "40" or "41" or "50";

    /// <summary>Indica se o imposto foi retido antes por substituição tributária.</summary>
    public bool TemSubstituicao => EhCsosn
        ? Codigo is "201" or "202" or "203" or "500"
        : Codigo is "10" or "30" or "60" or "70";

    /// <summary>Tenta interpretar o código do campo CST ou CSOSN.</summary>
    public static bool TentarAnalisar(string? entrada, out SituacaoTributaria situacao)
    {
        situacao = default;

        if (string.IsNullOrWhiteSpace(entrada))
        {
            return false;
        }

        var texto = entrada.Trim();
        if (!int.TryParse(texto, out var numero))
        {
            return false;
        }

        if (texto.Length <= 2 && CstValidos.Contains(numero))
        {
            situacao = new SituacaoTributaria(numero.ToString("D2"), csosn: false);
            return true;
        }

        if (texto.Length == 3 && CsosnValidos.Contains(numero))
        {
            situacao = new SituacaoTributaria(numero.ToString("D3"), csosn: true);
            return true;
        }

        return false;
    }

    /// <summary>Interpreta o código ou lança se ele for inválido.</summary>
    public static SituacaoTributaria Analisar(string? entrada)
    {
        if (!TentarAnalisar(entrada, out var situacao))
        {
            throw new FormatException($"CST ou CSOSN inválido: '{entrada}'.");
        }

        return situacao;
    }

    /// <summary>
    /// Confere se a tabela usada combina com o regime declarado no CRT: quem
    /// está no Simples usa CSOSN, quem está no regime normal usa CST.
    /// </summary>
    public bool CombinaCom(RegimeTributario regime) => regime switch
    {
        RegimeTributario.Normal => !EhCsosn,
        _ => EhCsosn,
    };

    /// <inheritdoc />
    public override string ToString() => Codigo;
}

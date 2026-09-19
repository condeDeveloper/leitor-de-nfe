using Nfe.Core.Comum;
using Nfe.Core.Fiscal;

namespace Nfe.Core.Modelo;

/// <summary>ICMS de um item.</summary>
public sealed record Icms
{
    /// <summary>Origem da mercadoria, campo orig.</summary>
    public int Origem { get; init; }

    /// <summary>CST ou CSOSN.</summary>
    public SituacaoTributaria Situacao { get; init; }

    /// <summary>Base de cálculo.</summary>
    public decimal BaseDeCalculo { get; init; }

    /// <summary>Alíquota em pontos percentuais.</summary>
    public decimal Aliquota { get; init; }

    /// <summary>Valor do imposto.</summary>
    public decimal Valor { get; init; }

    /// <summary>Base de cálculo da substituição tributária.</summary>
    public decimal BaseDeSubstituicao { get; init; }

    /// <summary>Valor do ICMS retido por substituição tributária.</summary>
    public decimal ValorDeSubstituicao { get; init; }

    /// <summary>Valor do ICMS desonerado.</summary>
    public decimal ValorDesonerado { get; init; }

    /// <summary>Recalcula o valor a partir da base e da alíquota.</summary>
    public decimal ValorCalculado() => Dinheiro.Arredondar(BaseDeCalculo * Aliquota / 100m);

    /// <summary>Indica se o valor declarado confere com base vezes alíquota.</summary>
    public bool ValorConfere() => Dinheiro.Equivalem(Valor, ValorCalculado());
}

/// <summary>Tributo de cálculo simples, com base, alíquota e valor.</summary>
public sealed record TributoSimples
{
    /// <summary>Código da situação tributária.</summary>
    public string Situacao { get; init; } = string.Empty;

    /// <summary>Base de cálculo.</summary>
    public decimal BaseDeCalculo { get; init; }

    /// <summary>Alíquota em pontos percentuais.</summary>
    public decimal Aliquota { get; init; }

    /// <summary>Valor do imposto.</summary>
    public decimal Valor { get; init; }

    /// <summary>Recalcula o valor a partir da base e da alíquota.</summary>
    public decimal ValorCalculado() => Dinheiro.Arredondar(BaseDeCalculo * Aliquota / 100m);

    /// <summary>Indica se o valor declarado confere com base vezes alíquota.</summary>
    public bool ValorConfere() => Dinheiro.Equivalem(Valor, ValorCalculado());
}

/// <summary>Conjunto de tributos incidentes sobre um item.</summary>
public sealed record Tributos
{
    /// <summary>ICMS do item.</summary>
    public Icms Icms { get; init; } = new();

    /// <summary>IPI do item, quando houver.</summary>
    public TributoSimples? Ipi { get; init; }

    /// <summary>PIS do item.</summary>
    public TributoSimples Pis { get; init; } = new();

    /// <summary>COFINS do item.</summary>
    public TributoSimples Cofins { get; init; } = new();

    /// <summary>Soma de tudo que incide sobre o item.</summary>
    public decimal Total() => Dinheiro.Arredondar(
        Icms.Valor + Icms.ValorDeSubstituicao + (Ipi?.Valor ?? 0m) + Pis.Valor + Cofins.Valor);
}

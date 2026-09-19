using Nfe.Core.Comum;
using Nfe.Core.Fiscal;

namespace Nfe.Core.Modelo;

/// <summary>Item de uma nota fiscal, o elemento det do XML.</summary>
public sealed record Item
{
    /// <summary>Número sequencial do item na nota.</summary>
    public int Numero { get; init; }

    /// <summary>Código interno do produto.</summary>
    public string Codigo { get; init; } = string.Empty;

    /// <summary>Código de barras GTIN, quando informado.</summary>
    public string? Gtin { get; init; }

    /// <summary>Descrição do produto.</summary>
    public string Descricao { get; init; } = string.Empty;

    /// <summary>NCM do produto.</summary>
    public Ncm Ncm { get; init; }

    /// <summary>CFOP da operação.</summary>
    public Cfop Cfop { get; init; }

    /// <summary>Unidade comercial.</summary>
    public string Unidade { get; init; } = string.Empty;

    /// <summary>Quantidade comercial, com até quatro casas.</summary>
    public decimal Quantidade { get; init; }

    /// <summary>Valor unitário comercial.</summary>
    public decimal ValorUnitario { get; init; }

    /// <summary>Valor bruto declarado do item.</summary>
    public decimal ValorBruto { get; init; }

    /// <summary>Desconto do item.</summary>
    public decimal Desconto { get; init; }

    /// <summary>Frete rateado no item.</summary>
    public decimal Frete { get; init; }

    /// <summary>Seguro rateado no item.</summary>
    public decimal Seguro { get; init; }

    /// <summary>Outras despesas acessórias do item.</summary>
    public decimal OutrasDespesas { get; init; }

    /// <summary>Indica se o valor do item entra no total da nota, campo indTot.</summary>
    public bool CompoeOTotal { get; init; } = true;

    /// <summary>Tributos incidentes sobre o item.</summary>
    public Tributos Tributos { get; init; } = new();

    /// <summary>Valor bruto recalculado a partir de quantidade e valor unitário.</summary>
    public decimal ValorBrutoCalculado() => Dinheiro.Arredondar(Quantidade * ValorUnitario);

    /// <summary>Indica se o valor bruto declarado confere com o recalculado.</summary>
    public bool ValorBrutoConfere() => Dinheiro.Equivalem(ValorBruto, ValorBrutoCalculado());

    /// <summary>Valor líquido do item, já com desconto e despesas acessórias.</summary>
    public decimal ValorLiquido() => Dinheiro.Arredondar(
        ValorBruto - Desconto + Frete + Seguro + OutrasDespesas);
}

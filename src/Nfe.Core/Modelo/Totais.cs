using Nfe.Core.Comum;

namespace Nfe.Core.Modelo;

/// <summary>
/// Totais declarados no grupo ICMSTot. É o bloco que mais costuma divergir da
/// soma dos itens quando o XML foi gerado à mão ou por integração malfeita.
/// </summary>
public sealed record Totais
{
    /// <summary>Soma das bases de cálculo do ICMS.</summary>
    public decimal BaseDeIcms { get; init; }

    /// <summary>Soma do ICMS.</summary>
    public decimal Icms { get; init; }

    /// <summary>Soma das bases da substituição tributária.</summary>
    public decimal BaseDeSubstituicao { get; init; }

    /// <summary>Soma do ICMS retido por substituição tributária.</summary>
    public decimal IcmsDeSubstituicao { get; init; }

    /// <summary>Soma dos valores brutos dos produtos.</summary>
    public decimal Produtos { get; init; }

    /// <summary>Frete total.</summary>
    public decimal Frete { get; init; }

    /// <summary>Seguro total.</summary>
    public decimal Seguro { get; init; }

    /// <summary>Desconto total.</summary>
    public decimal Desconto { get; init; }

    /// <summary>Outras despesas acessórias.</summary>
    public decimal OutrasDespesas { get; init; }

    /// <summary>IPI total.</summary>
    public decimal Ipi { get; init; }

    /// <summary>PIS total.</summary>
    public decimal Pis { get; init; }

    /// <summary>COFINS total.</summary>
    public decimal Cofins { get; init; }

    /// <summary>Valor total da nota.</summary>
    public decimal NotaFiscal { get; init; }

    /// <summary>
    /// Fórmula do valor total da nota conforme o manual: produtos, menos
    /// desconto, mais frete, seguro, despesas, ST e IPI.
    /// </summary>
    public decimal TotalCalculado() => Dinheiro.Arredondar(
        Produtos - Desconto + Frete + Seguro + OutrasDespesas + IcmsDeSubstituicao + Ipi);

    /// <summary>Indica se o total declarado bate com a fórmula do manual.</summary>
    public bool TotalConfere() => Dinheiro.Equivalem(NotaFiscal, TotalCalculado());

    /// <summary>Monta os totais somando os itens, para comparar com o declarado.</summary>
    public static Totais APartirDosItens(IEnumerable<Item> itens)
    {
        ArgumentNullException.ThrowIfNull(itens);

        var considerados = itens.Where(item => item.CompoeOTotal).ToList();

        var totais = new Totais
        {
            Produtos = Dinheiro.Arredondar(considerados.Sum(item => item.ValorBruto)),
            Desconto = Dinheiro.Arredondar(considerados.Sum(item => item.Desconto)),
            Frete = Dinheiro.Arredondar(considerados.Sum(item => item.Frete)),
            Seguro = Dinheiro.Arredondar(considerados.Sum(item => item.Seguro)),
            OutrasDespesas = Dinheiro.Arredondar(considerados.Sum(item => item.OutrasDespesas)),
            BaseDeIcms = Dinheiro.Arredondar(considerados.Sum(item => item.Tributos.Icms.BaseDeCalculo)),
            Icms = Dinheiro.Arredondar(considerados.Sum(item => item.Tributos.Icms.Valor)),
            BaseDeSubstituicao = Dinheiro.Arredondar(considerados.Sum(item => item.Tributos.Icms.BaseDeSubstituicao)),
            IcmsDeSubstituicao = Dinheiro.Arredondar(considerados.Sum(item => item.Tributos.Icms.ValorDeSubstituicao)),
            Ipi = Dinheiro.Arredondar(considerados.Sum(item => item.Tributos.Ipi?.Valor ?? 0m)),
            Pis = Dinheiro.Arredondar(considerados.Sum(item => item.Tributos.Pis.Valor)),
            Cofins = Dinheiro.Arredondar(considerados.Sum(item => item.Tributos.Cofins.Valor)),
        };

        return totais with { NotaFiscal = totais.TotalCalculado() };
    }
}

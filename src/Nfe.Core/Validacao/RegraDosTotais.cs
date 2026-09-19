using Nfe.Core.Comum;
using Nfe.Core.Modelo;

namespace Nfe.Core.Validacao;

/// <summary>
/// Confere se os totais declarados batem com a soma dos itens e se cada item
/// fecha quantidade vezes valor unitário. É a regra que mais pega erro de
/// integração, porque muita gente monta o ICMSTot na mão.
/// </summary>
public sealed class RegraDosTotais : IRegra
{
    /// <inheritdoc />
    public string Nome => "totais";

    /// <inheritdoc />
    public IEnumerable<Problema> Conferir(NotaFiscal nota)
    {
        ArgumentNullException.ThrowIfNull(nota);

        foreach (var item in nota.Itens)
        {
            if (!item.ValorBrutoConfere())
            {
                yield return Problema.Erro(
                    "TOT-001",
                    $"det[{item.Numero}]/prod/vProd",
                    $"O valor do item ({Dinheiro.Formatar(item.ValorBruto)}) não é a quantidade vezes o " +
                    $"unitário ({Dinheiro.Formatar(item.ValorBrutoCalculado())}).");
            }

            if (!item.Tributos.Icms.ValorConfere())
            {
                yield return Problema.Alerta(
                    "TOT-002",
                    $"det[{item.Numero}]/imposto/ICMS/vICMS",
                    $"O ICMS do item ({Dinheiro.Formatar(item.Tributos.Icms.Valor)}) não é a base vezes a " +
                    $"alíquota ({Dinheiro.Formatar(item.Tributos.Icms.ValorCalculado())}).");
            }

            if (item.Desconto > item.ValorBruto)
            {
                yield return Problema.Erro(
                    "TOT-003",
                    $"det[{item.Numero}]/prod/vDesc",
                    "O desconto do item é maior que o próprio valor do item.");
            }
        }

        var somados = nota.TotaisDosItens();
        var declarados = nota.Totais;

        foreach (var divergencia in Comparar(declarados, somados))
        {
            yield return divergencia;
        }

        if (!declarados.TotalConfere())
        {
            yield return Problema.Erro(
                "TOT-010",
                "total/ICMSTot/vNF",
                $"O total declarado ({Dinheiro.Formatar(declarados.NotaFiscal)}) não fecha com a fórmula do " +
                $"manual ({Dinheiro.Formatar(declarados.TotalCalculado())}).");
        }

        if (nota.Pagamentos.Formas.Count > 0 && !nota.Pagamentos.CobreOTotalDaNota(declarados.NotaFiscal))
        {
            yield return Problema.Alerta(
                "TOT-011",
                "pag",
                $"A soma dos pagamentos ({Dinheiro.Formatar(nota.Pagamentos.Total())}) não cobre o total da " +
                $"nota ({Dinheiro.Formatar(declarados.NotaFiscal)}).");
        }
    }

    private static IEnumerable<Problema> Comparar(Totais declarados, Totais somados)
    {
        var campos = new (string Codigo, string Campo, decimal Declarado, decimal Somado)[]
        {
            ("TOT-004", "total/ICMSTot/vProd", declarados.Produtos, somados.Produtos),
            ("TOT-005", "total/ICMSTot/vDesc", declarados.Desconto, somados.Desconto),
            ("TOT-006", "total/ICMSTot/vBC", declarados.BaseDeIcms, somados.BaseDeIcms),
            ("TOT-007", "total/ICMSTot/vICMS", declarados.Icms, somados.Icms),
            ("TOT-008", "total/ICMSTot/vST", declarados.IcmsDeSubstituicao, somados.IcmsDeSubstituicao),
            ("TOT-009", "total/ICMSTot/vIPI", declarados.Ipi, somados.Ipi),
        };

        foreach (var (codigo, campo, declarado, somado) in campos)
        {
            if (!Dinheiro.Equivalem(declarado, somado))
            {
                yield return Problema.Erro(
                    codigo,
                    campo,
                    $"Declarado {Dinheiro.Formatar(declarado)}, soma dos itens {Dinheiro.Formatar(somado)}.");
            }
        }
    }
}

using Nfe.Core.Fiscal;
using Nfe.Core.Modelo;

namespace Nfe.Core.Validacao;

/// <summary>
/// Confere se o CFOP de cada item combina com as UFs envolvidas e com o tipo da
/// operação, e se a nota não mistura entrada e saída no mesmo documento.
/// </summary>
public sealed class RegraDoCfop : IRegra
{
    /// <inheritdoc />
    public string Nome => "cfop";

    /// <inheritdoc />
    public IEnumerable<Problema> Conferir(NotaFiscal nota)
    {
        ArgumentNullException.ThrowIfNull(nota);

        var ufEmitente = nota.Emitente.Endereco.Uf;
        var ufDestinatario = nota.Destinatario.Endereco.Uf;
        var esperado = Cfop.AmbitoEsperado(ufEmitente, ufDestinatario);

        var sentidoDaNota = nota.Operacao == TipoDeOperacao.Entrada
            ? SentidoDaOperacao.Entrada
            : SentidoDaOperacao.Saida;

        foreach (var item in nota.Itens)
        {
            var caminho = $"det[{item.Numero}]/prod/CFOP";

            if (item.Cfop.Ambito != esperado)
            {
                yield return Problema.Erro(
                    "CFO-001",
                    caminho,
                    $"O CFOP {item.Cfop} é {Descrever(item.Cfop.Ambito)}, mas a operação {ufEmitente} para " +
                    $"{ufDestinatario} é {Descrever(esperado)}.");
            }

            if (item.Cfop.Sentido != sentidoDaNota)
            {
                yield return Problema.Erro(
                    "CFO-002",
                    caminho,
                    $"O CFOP {item.Cfop} é de {Descrever(item.Cfop.Sentido)}, mas a nota é de " +
                    $"{Descrever(sentidoDaNota)}.");
            }

            if (item.Cfop.EhDevolucao && nota.Finalidade != FinalidadeDaEmissao.Devolucao)
            {
                yield return Problema.Alerta(
                    "CFO-003",
                    caminho,
                    $"O CFOP {item.Cfop} é de devolução, mas a finalidade da nota é {nota.Finalidade}.");
            }

            if (item.Ncm.EhGenerico)
            {
                yield return Problema.Alerta(
                    "CFO-004",
                    $"det[{item.Numero}]/prod/NCM",
                    "O item usa o NCM genérico '00', que só vale para casos específicos do layout.");
            }
        }

        if (nota.Itens.Select(item => item.Cfop.Sentido).Distinct().Count() > 1)
        {
            yield return Problema.Erro(
                "CFO-005",
                "det",
                "A nota mistura CFOP de entrada e de saída no mesmo documento.");
        }
    }

    private static string Descrever(AmbitoDaOperacao ambito) => ambito switch
    {
        AmbitoDaOperacao.Estadual => "operação dentro do estado",
        AmbitoDaOperacao.Interestadual => "operação interestadual",
        _ => "operação com o exterior",
    };

    private static string Descrever(SentidoDaOperacao sentido)
        => sentido == SentidoDaOperacao.Entrada ? "entrada" : "saída";
}

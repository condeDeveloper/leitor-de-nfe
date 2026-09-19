using Nfe.Core.Comum;
using Nfe.Core.Modelo;

namespace Nfe.Core.Validacao;

/// <summary>
/// Confere os dados cadastrais de emitente e destinatário: UF conhecida, CEP
/// com oito dígitos, inscrição estadual coerente com o indicador e regime
/// tributário coerente com a tabela de CST usada nos itens.
/// </summary>
public sealed class RegraDosCadastros : IRegra
{
    /// <inheritdoc />
    public string Nome => "cadastros";

    /// <inheritdoc />
    public IEnumerable<Problema> Conferir(NotaFiscal nota)
    {
        ArgumentNullException.ThrowIfNull(nota);

        foreach (var problema in ConferirEndereco(nota.Emitente.Endereco, "emit/enderEmit"))
        {
            yield return problema;
        }

        foreach (var problema in ConferirEndereco(nota.Destinatario.Endereco, "dest/enderDest"))
        {
            yield return problema;
        }

        if (string.IsNullOrWhiteSpace(nota.Emitente.InscricaoEstadual))
        {
            yield return Problema.Erro("CAD-003", "emit/IE", "O emitente está sem inscrição estadual.");
        }

        var destinatario = nota.Destinatario;
        var temInscricao = !string.IsNullOrWhiteSpace(destinatario.InscricaoEstadual)
            && !string.Equals(destinatario.InscricaoEstadual, "ISENTO", StringComparison.OrdinalIgnoreCase);

        if (destinatario.Indicador == IndicadorDeInscricao.Contribuinte && !temInscricao)
        {
            yield return Problema.Erro(
                "CAD-004",
                "dest/IE",
                "O destinatário está marcado como contribuinte mas não tem inscrição estadual.");
        }

        if (destinatario.Indicador == IndicadorDeInscricao.Isento && temInscricao)
        {
            yield return Problema.Alerta(
                "CAD-005",
                "dest/IE",
                "O destinatário está marcado como isento mas informou inscrição estadual.");
        }

        if (destinatario.EhPessoaFisica && destinatario.Indicador == IndicadorDeInscricao.Contribuinte)
        {
            yield return Problema.Alerta(
                "CAD-006",
                "dest/indIEDest",
                "Destinatário pessoa física declarado como contribuinte de ICMS.");
        }

        foreach (var item in nota.Itens)
        {
            if (!item.Tributos.Icms.Situacao.CombinaCom(nota.Emitente.Regime))
            {
                yield return Problema.Erro(
                    "CAD-007",
                    $"det[{item.Numero}]/imposto/ICMS",
                    $"O regime do emitente ({nota.Emitente.Regime}) não combina com o código " +
                    $"{item.Tributos.Icms.Situacao} usado no item.");
            }
        }
    }

    private static IEnumerable<Problema> ConferirEndereco(Endereco endereco, string caminho)
    {
        if (!Uf.SiglaEhValida(endereco.Uf))
        {
            yield return Problema.Erro("CAD-001", $"{caminho}/UF", $"UF desconhecida: '{endereco.Uf}'.");
        }

        if (endereco.Cep.Length != 8)
        {
            yield return Problema.Alerta("CAD-002", $"{caminho}/CEP", $"CEP fora do padrão: '{endereco.Cep}'.");
        }
    }
}

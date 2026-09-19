using Nfe.Core.Chave;
using Nfe.Core.Modelo;

namespace Nfe.Core.Validacao;

/// <summary>
/// Confere se a chave de acesso combina com o conteúdo da nota. A chave repete
/// UF, mês, CNPJ, modelo, série e número, então qualquer divergência aqui é
/// sinal de XML montado errado ou adulterado.
/// </summary>
public sealed class RegraDaChave : IRegra
{
    /// <inheritdoc />
    public string Nome => "chave";

    /// <inheritdoc />
    public IEnumerable<Problema> Conferir(NotaFiscal nota)
    {
        ArgumentNullException.ThrowIfNull(nota);

        var chave = nota.Chave;

        if (chave.Cnpj != nota.Emitente.Documento.Numero)
        {
            yield return Problema.Erro(
                "CHV-001",
                "infNFe/@Id",
                $"O CNPJ da chave ({chave.Cnpj}) não é o do emitente ({nota.Emitente.Documento.Numero}).");
        }

        if (chave.Numero != nota.Numero)
        {
            yield return Problema.Erro(
                "CHV-002",
                "ide/nNF",
                $"O número da chave ({chave.Numero}) não é o da nota ({nota.Numero}).");
        }

        if (chave.Serie != nota.Serie)
        {
            yield return Problema.Erro(
                "CHV-003",
                "ide/serie",
                $"A série da chave ({chave.Serie}) não é a da nota ({nota.Serie}).");
        }

        if (chave.Modelo != nota.Modelo)
        {
            yield return Problema.Erro(
                "CHV-004",
                "ide/mod",
                $"O modelo da chave ({chave.Modelo}) não é o da nota ({nota.Modelo}).");
        }

        if (chave.Ano != nota.Emissao.Year || chave.Mes != nota.Emissao.Month)
        {
            yield return Problema.Erro(
                "CHV-005",
                "ide/dhEmi",
                $"A competência da chave ({chave.Mes:D2}/{chave.Ano}) não é a da emissão " +
                $"({nota.Emissao.Month:D2}/{nota.Emissao.Year}).");
        }

        if (!Comum.Uf.TentarCodigo(nota.Emitente.Endereco.Uf, out var codigoDaUf))
        {
            yield return Problema.Erro(
                "CHV-006",
                "emit/enderEmit/UF",
                $"UF do emitente desconhecida: '{nota.Emitente.Endereco.Uf}'.");
        }
        else if (codigoDaUf != chave.CodigoDaUf)
        {
            yield return Problema.Erro(
                "CHV-007",
                "infNFe/@Id",
                $"A UF da chave ({chave.Uf}) não é a do emitente ({nota.Emitente.Endereco.Uf}).");
        }

        if (chave.TipoDeEmissao != TipoDeEmissao.Normal)
        {
            yield return Problema.Alerta(
                "CHV-008",
                "infNFe/@Id",
                $"A nota foi emitida em contingência ({chave.TipoDeEmissao}).");
        }
    }
}

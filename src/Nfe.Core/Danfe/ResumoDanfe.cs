using System.Globalization;
using Nfe.Core.Comum;
using Nfe.Core.Modelo;

namespace Nfe.Core.Danfe;

/// <summary>Uma linha da tabela de produtos do DANFE.</summary>
/// <param name="Numero">Número do item.</param>
/// <param name="Codigo">Código do produto.</param>
/// <param name="Descricao">Descrição do produto.</param>
/// <param name="Ncm">NCM formatado.</param>
/// <param name="Cfop">CFOP com quatro dígitos.</param>
/// <param name="Unidade">Unidade comercial.</param>
/// <param name="Quantidade">Quantidade formatada.</param>
/// <param name="ValorUnitario">Valor unitário formatado.</param>
/// <param name="ValorTotal">Valor total do item formatado.</param>
public sealed record LinhaDeProduto(
    int Numero,
    string Codigo,
    string Descricao,
    string Ncm,
    string Cfop,
    string Unidade,
    string Quantidade,
    string ValorUnitario,
    string ValorTotal);

/// <summary>
/// Projeção da nota no formato em que o DANFE precisa dela: campos já
/// formatados, chave em grupos de quatro e a descrição da natureza da operação
/// pronta para o cabeçalho. Não desenha nada, só organiza o conteúdo.
/// </summary>
public sealed class ResumoDanfe
{
    private ResumoDanfe(NotaFiscal nota)
    {
        Nota = nota;
    }

    /// <summary>A nota que originou o resumo.</summary>
    public NotaFiscal Nota { get; }

    /// <summary>Chave em grupos de quatro dígitos, como impressa no DANFE.</summary>
    public string ChaveFormatada => Nota.Chave.Formatada();

    /// <summary>Identificação curta da nota, no formato número/série.</summary>
    public string Identificacao =>
        $"Nº {Nota.Numero.ToString("N0", CultureInfo.InvariantCulture).Replace(",", ".", StringComparison.Ordinal)}" +
        $" - Série {Nota.Serie}";

    /// <summary>Descrição do tipo de operação para o cabeçalho.</summary>
    public string TipoDeOperacao => Nota.Operacao == Modelo.TipoDeOperacao.Entrada ? "0 - ENTRADA" : "1 - SAÍDA";

    /// <summary>Data de emissão no formato brasileiro.</summary>
    public string DataDeEmissao => Nota.Emissao.ToString("dd'/'MM'/'yyyy", CultureInfo.InvariantCulture);

    /// <summary>Data de saída ou entrada, vazia quando não informada.</summary>
    public string DataDeSaida =>
        Nota.SaidaOuEntrada?.ToString("dd'/'MM'/'yyyy", CultureInfo.InvariantCulture) ?? string.Empty;

    /// <summary>Emitente em uma linha, com documento e endereço.</summary>
    public string EmitenteEmUmaLinha =>
        $"{Nota.Emitente.RazaoSocial} - CNPJ {Nota.Emitente.Documento.Formatado()} - " +
        Nota.Emitente.Endereco.EmUmaLinha();

    /// <summary>Destinatário em uma linha, com documento e endereço.</summary>
    public string DestinatarioEmUmaLinha =>
        $"{Nota.Destinatario.RazaoSocial} - {Nota.Destinatario.Documento.Formatado()} - " +
        Nota.Destinatario.Endereco.EmUmaLinha();

    /// <summary>Total da nota formatado.</summary>
    public string TotalDaNota => Dinheiro.Formatar(Nota.Totais.NotaFiscal);

    /// <summary>Linhas da tabela de produtos.</summary>
    public IReadOnlyList<LinhaDeProduto> Produtos { get; private set; } = [];

    /// <summary>Monta o resumo a partir da nota lida.</summary>
    public static ResumoDanfe De(NotaFiscal nota)
    {
        ArgumentNullException.ThrowIfNull(nota);

        return new ResumoDanfe(nota)
        {
            Produtos = nota.Itens.Select(item => new LinhaDeProduto(
                item.Numero,
                item.Codigo,
                item.Descricao,
                item.Ncm.Formatado(),
                item.Cfop.ToString(),
                item.Unidade,
                Dinheiro.Formatar(item.Quantidade, 4),
                Dinheiro.Formatar(item.ValorUnitario),
                Dinheiro.Formatar(item.ValorBruto))).ToList(),
        };
    }

    /// <summary>
    /// Texto do bloco de dados adicionais, juntando o que veio do XML com o
    /// aviso de contingência quando for o caso.
    /// </summary>
    public string DadosAdicionais()
    {
        var partes = new List<string>();

        if (!string.IsNullOrWhiteSpace(Nota.InformacoesComplementares))
        {
            partes.Add(Nota.InformacoesComplementares.Trim());
        }

        if (Nota.Chave.TipoDeEmissao != Chave.TipoDeEmissao.Normal)
        {
            partes.Add($"EMITIDA EM CONTINGÊNCIA ({Nota.Chave.TipoDeEmissao}).");
        }

        if (!Nota.EstaAutorizada)
        {
            partes.Add("XML SEM PROTOCOLO DE AUTORIZAÇÃO.");
        }

        return string.Join(" ", partes);
    }
}

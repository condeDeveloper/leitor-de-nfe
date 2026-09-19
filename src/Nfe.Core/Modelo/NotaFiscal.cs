using Nfe.Core.Chave;

namespace Nfe.Core.Modelo;

/// <summary>Tipo da operação, campo tpNF.</summary>
public enum TipoDeOperacao
{
    /// <summary>Nota de entrada.</summary>
    Entrada = 0,

    /// <summary>Nota de saída.</summary>
    Saida = 1,
}

/// <summary>Finalidade da emissão, campo finNFe.</summary>
public enum FinalidadeDaEmissao
{
    /// <summary>Nota normal.</summary>
    Normal = 1,

    /// <summary>Nota complementar.</summary>
    Complementar = 2,

    /// <summary>Nota de ajuste.</summary>
    Ajuste = 3,

    /// <summary>Devolução de mercadoria.</summary>
    Devolucao = 4,
}

/// <summary>Uma NF-e completa, já lida do XML e com os campos tipados.</summary>
public sealed record NotaFiscal
{
    /// <summary>Chave de acesso declarada no atributo Id da infNFe.</summary>
    public required ChaveDeAcesso Chave { get; init; }

    /// <summary>Número do documento.</summary>
    public int Numero { get; init; }

    /// <summary>Série do documento.</summary>
    public int Serie { get; init; }

    /// <summary>Modelo: 55 para NF-e, 65 para NFC-e.</summary>
    public int Modelo { get; init; }

    /// <summary>Data e hora de emissão, com o fuso declarado no XML.</summary>
    public DateTimeOffset Emissao { get; init; }

    /// <summary>Data e hora de saída ou entrada, quando informada.</summary>
    public DateTimeOffset? SaidaOuEntrada { get; init; }

    /// <summary>Natureza da operação em texto livre.</summary>
    public string NaturezaDaOperacao { get; init; } = string.Empty;

    /// <summary>Entrada ou saída.</summary>
    public TipoDeOperacao Operacao { get; init; }

    /// <summary>Finalidade da emissão.</summary>
    public FinalidadeDaEmissao Finalidade { get; init; } = FinalidadeDaEmissao.Normal;

    /// <summary>Emitente da nota.</summary>
    public required Emitente Emitente { get; init; }

    /// <summary>Destinatário da nota.</summary>
    public required Destinatario Destinatario { get; init; }

    /// <summary>Itens da nota, na ordem em que aparecem no XML.</summary>
    public IReadOnlyList<Item> Itens { get; init; } = [];

    /// <summary>Totais declarados no XML.</summary>
    public Totais Totais { get; init; } = new();

    /// <summary>Dados de transporte.</summary>
    public Transporte Transporte { get; init; } = new();

    /// <summary>Formas de pagamento.</summary>
    public Pagamentos Pagamentos { get; init; } = new();

    /// <summary>Informações complementares de interesse do contribuinte.</summary>
    public string? InformacoesComplementares { get; init; }

    /// <summary>Protocolo de autorização, quando o XML já veio processado.</summary>
    public string? Protocolo { get; init; }

    /// <summary>Indica se o XML lido continha o protocolo de autorização.</summary>
    public bool EstaAutorizada => !string.IsNullOrWhiteSpace(Protocolo);

    /// <summary>Quantidade de itens da nota.</summary>
    public int QuantidadeDeItens => Itens.Count;

    /// <summary>Totais recalculados a partir dos itens.</summary>
    public Totais TotaisDosItens() => Totais.APartirDosItens(Itens);
}

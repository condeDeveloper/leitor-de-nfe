using Nfe.Core.Chave;
using Nfe.Core.Danfe;
using Nfe.Core.Modelo;
using Nfe.Core.Validacao;

namespace Nfe.Api;

/// <summary>Corpo das requisições que recebem um XML.</summary>
/// <param name="Xml">Conteúdo do arquivo da NF-e ou do procNFe.</param>
public sealed record PedidoDeXml(string Xml);

/// <summary>Chave de acesso decomposta nos campos que a formam.</summary>
public sealed record ChaveAberta(
    string Chave,
    string ChaveFormatada,
    string Uf,
    int Ano,
    int Mes,
    string Cnpj,
    int Modelo,
    int Serie,
    int Numero,
    string TipoDeEmissao,
    string CodigoNumerico,
    int DigitoVerificador)
{
    /// <summary>Monta a resposta a partir da chave lida.</summary>
    public static ChaveAberta De(ChaveDeAcesso chave) => new(
        chave.Digitos,
        chave.Formatada(),
        chave.Uf,
        chave.Ano,
        chave.Mes,
        chave.Cnpj,
        chave.Modelo,
        chave.Serie,
        chave.Numero,
        chave.TipoDeEmissao.ToString(),
        chave.CodigoNumerico,
        chave.DigitoVerificador);
}

/// <summary>Resumo da nota devolvido pela leitura.</summary>
public sealed record ResumoDaNota(
    ChaveAberta Chave,
    int Numero,
    int Serie,
    int Modelo,
    DateTimeOffset Emissao,
    string NaturezaDaOperacao,
    string Operacao,
    string Emitente,
    string Destinatario,
    int QuantidadeDeItens,
    decimal TotalDosProdutos,
    decimal TotalDaNota,
    bool EstaAutorizada,
    IReadOnlyList<LinhaDeProduto> Produtos)
{
    /// <summary>Monta a resposta a partir da nota lida.</summary>
    public static ResumoDaNota De(NotaFiscal nota)
    {
        var danfe = ResumoDanfe.De(nota);

        return new ResumoDaNota(
            ChaveAberta.De(nota.Chave),
            nota.Numero,
            nota.Serie,
            nota.Modelo,
            nota.Emissao,
            nota.NaturezaDaOperacao,
            nota.Operacao.ToString(),
            danfe.EmitenteEmUmaLinha,
            danfe.DestinatarioEmUmaLinha,
            nota.QuantidadeDeItens,
            nota.Totais.Produtos,
            nota.Totais.NotaFiscal,
            nota.EstaAutorizada,
            danfe.Produtos);
    }
}

/// <summary>Resposta da conferência de uma nota.</summary>
public sealed record RespostaDaValidacao(
    string Chave,
    bool EhValida,
    int QuantidadeDeErros,
    int QuantidadeDeAlertas,
    IReadOnlyList<Problema> Problemas)
{
    /// <summary>Monta a resposta a partir do resultado da validação.</summary>
    public static RespostaDaValidacao De(NotaFiscal nota, Resultado resultado) => new(
        nota.Chave.Digitos,
        resultado.EhValida,
        resultado.Erros.Count(),
        resultado.Alertas.Count(),
        resultado.Problemas);
}

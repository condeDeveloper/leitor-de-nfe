using Nfe.Core.Comum;
using Nfe.Core.Fiscal;

namespace Nfe.Core.Modelo;

/// <summary>Indicador da inscrição estadual do destinatário, campo indIEDest.</summary>
public enum IndicadorDeInscricao
{
    /// <summary>Contribuinte de ICMS com inscrição estadual.</summary>
    Contribuinte = 1,

    /// <summary>Isento de inscrição no cadastro de contribuintes.</summary>
    Isento = 2,

    /// <summary>Não contribuinte, que pode ou não ter inscrição.</summary>
    NaoContribuinte = 9,
}

/// <summary>Emitente da nota fiscal.</summary>
public sealed record Emitente
{
    /// <summary>CNPJ ou CPF do emitente.</summary>
    public Documento Documento { get; init; }

    /// <summary>Razão social.</summary>
    public string RazaoSocial { get; init; } = string.Empty;

    /// <summary>Nome fantasia, quando informado.</summary>
    public string? NomeFantasia { get; init; }

    /// <summary>Inscrição estadual.</summary>
    public string InscricaoEstadual { get; init; } = string.Empty;

    /// <summary>Endereço do emitente.</summary>
    public Endereco Endereco { get; init; } = new();

    /// <summary>Regime tributário declarado no campo CRT.</summary>
    public RegimeTributario Regime { get; init; } = RegimeTributario.Normal;
}

/// <summary>Destinatário da nota fiscal.</summary>
public sealed record Destinatario
{
    /// <summary>CNPJ ou CPF do destinatário.</summary>
    public Documento Documento { get; init; }

    /// <summary>Razão social ou nome.</summary>
    public string RazaoSocial { get; init; } = string.Empty;

    /// <summary>Inscrição estadual, quando houver.</summary>
    public string? InscricaoEstadual { get; init; }

    /// <summary>Indicador da inscrição estadual.</summary>
    public IndicadorDeInscricao Indicador { get; init; } = IndicadorDeInscricao.NaoContribuinte;

    /// <summary>Endereço do destinatário.</summary>
    public Endereco Endereco { get; init; } = new();

    /// <summary>Endereço de e-mail, quando informado.</summary>
    public string? Email { get; init; }

    /// <summary>Indica se o destinatário é consumidor final pessoa física.</summary>
    public bool EhPessoaFisica => Documento.Tipo == TipoDeDocumento.Cpf;
}

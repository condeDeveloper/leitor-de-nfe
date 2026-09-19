namespace Nfe.Core.Fiscal;

/// <summary>Sentido da operação indicado pelo primeiro dígito do CFOP.</summary>
public enum SentidoDaOperacao
{
    /// <summary>Entrada de mercadoria ou serviço.</summary>
    Entrada,

    /// <summary>Saída de mercadoria ou serviço.</summary>
    Saida,
}

/// <summary>Âmbito territorial da operação, também dado pelo primeiro dígito.</summary>
public enum AmbitoDaOperacao
{
    /// <summary>Operação dentro do próprio estado.</summary>
    Estadual,

    /// <summary>Operação entre estados diferentes.</summary>
    Interestadual,

    /// <summary>Importação ou exportação.</summary>
    Exterior,
}

/// <summary>
/// Código Fiscal de Operações e Prestações. O primeiro dígito já diz se é
/// entrada ou saída e qual o âmbito, o que permite cruzar o CFOP com as UFs do
/// emitente e do destinatário sem precisar da tabela completa.
/// </summary>
public readonly record struct Cfop
{
    private Cfop(int codigo)
    {
        Codigo = codigo;
    }

    /// <summary>O código com quatro dígitos.</summary>
    public int Codigo { get; }

    /// <summary>Entrada ou saída.</summary>
    public SentidoDaOperacao Sentido => Codigo / 1000 <= 3 ? SentidoDaOperacao.Entrada : SentidoDaOperacao.Saida;

    /// <summary>Âmbito territorial da operação.</summary>
    public AmbitoDaOperacao Ambito => (Codigo / 1000) switch
    {
        1 or 5 => AmbitoDaOperacao.Estadual,
        2 or 6 => AmbitoDaOperacao.Interestadual,
        _ => AmbitoDaOperacao.Exterior,
    };

    /// <summary>Indica se o CFOP é de devolução ou retorno.</summary>
    public bool EhDevolucao => Codigo % 1000 is >= 200 and < 300;

    /// <summary>Indica se o CFOP é de remessa ou retorno de industrialização.</summary>
    public bool EhIndustrializacao => Codigo % 1000 is >= 900 and < 950;

    /// <summary>Tenta interpretar o código, aceitando pontos e espaços.</summary>
    public static bool TentarAnalisar(string? entrada, out Cfop cfop)
    {
        cfop = default;
        var digitos = Comum.Modulo11.SomenteDigitos(entrada);

        if (digitos.Length != 4 || !int.TryParse(digitos, out var codigo))
        {
            return false;
        }

        var grupo = codigo / 1000;
        if (grupo is < 1 or > 7 || grupo is 4)
        {
            return false;
        }

        cfop = new Cfop(codigo);
        return true;
    }

    /// <summary>Interpreta o código ou lança se ele for inválido.</summary>
    public static Cfop Analisar(string? entrada)
    {
        if (!TentarAnalisar(entrada, out var cfop))
        {
            throw new FormatException($"CFOP inválido: '{entrada}'.");
        }

        return cfop;
    }

    /// <summary>
    /// Deduz o âmbito esperado a partir das UFs envolvidas, para comparar com o
    /// que o CFOP declara.
    /// </summary>
    public static AmbitoDaOperacao AmbitoEsperado(string? ufEmitente, string? ufDestinatario)
    {
        if (string.Equals(ufDestinatario, "EX", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(ufEmitente, "EX", StringComparison.OrdinalIgnoreCase))
        {
            return AmbitoDaOperacao.Exterior;
        }

        return string.Equals(ufEmitente, ufDestinatario, StringComparison.OrdinalIgnoreCase)
            ? AmbitoDaOperacao.Estadual
            : AmbitoDaOperacao.Interestadual;
    }

    /// <inheritdoc />
    public override string ToString() => Codigo.ToString("D4");
}

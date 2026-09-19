namespace Nfe.Core.Comum;

/// <summary>Tipo de documento do emitente ou do destinatário.</summary>
public enum TipoDeDocumento
{
    /// <summary>Pessoa física.</summary>
    Cpf,

    /// <summary>Pessoa jurídica.</summary>
    Cnpj,
}

/// <summary>
/// CPF ou CNPJ já normalizado (somente dígitos) e com o dígito verificador
/// conferido. A nota fiscal usa os dois de forma intercambiável, então vale a
/// pena ter um tipo só que sabe qual dos dois está segurando.
/// </summary>
public readonly record struct Documento
{
    private Documento(string numero, TipoDeDocumento tipo)
    {
        Numero = numero;
        Tipo = tipo;
    }

    /// <summary>Somente os dígitos, sem máscara.</summary>
    public string Numero { get; }

    /// <summary>CPF ou CNPJ.</summary>
    public TipoDeDocumento Tipo { get; }

    /// <summary>Tenta interpretar a entrada como CPF ou CNPJ válido.</summary>
    public static bool TentarAnalisar(string? entrada, out Documento documento)
    {
        documento = default;
        var digitos = Modulo11.SomenteDigitos(entrada);

        return digitos.Length switch
        {
            11 when CpfEhValido(digitos) => Aceitar(digitos, TipoDeDocumento.Cpf, out documento),
            14 when CnpjEhValido(digitos) => Aceitar(digitos, TipoDeDocumento.Cnpj, out documento),
            _ => false,
        };
    }

    /// <summary>Interpreta a entrada ou lança se o documento for inválido.</summary>
    public static Documento Analisar(string? entrada)
    {
        if (!TentarAnalisar(entrada, out var documento))
        {
            throw new FormatException($"Documento inválido: '{entrada}'.");
        }

        return documento;
    }

    /// <summary>Confere o dígito verificador de um CPF.</summary>
    public static bool CpfEhValido(string digitos)
    {
        if (digitos.Length != 11 || TodosIguais(digitos))
        {
            return false;
        }

        var primeiro = Modulo11.DigitoDeDocumento(digitos.AsSpan(0, 9), 10);
        var segundo = Modulo11.DigitoDeDocumento(digitos.AsSpan(0, 10), 11);

        return digitos[9] - '0' == primeiro && digitos[10] - '0' == segundo;
    }

    /// <summary>Confere o dígito verificador de um CNPJ.</summary>
    public static bool CnpjEhValido(string digitos)
    {
        if (digitos.Length != 14 || TodosIguais(digitos))
        {
            return false;
        }

        var primeiro = Modulo11.DigitoDeDocumento(digitos.AsSpan(0, 12), 5);
        var segundo = Modulo11.DigitoDeDocumento(digitos.AsSpan(0, 13), 6);

        return digitos[12] - '0' == primeiro && digitos[13] - '0' == segundo;
    }

    /// <summary>Devolve o documento com a máscara usual.</summary>
    public string Formatado() => Tipo == TipoDeDocumento.Cpf
        ? $"{Numero[..3]}.{Numero.Substring(3, 3)}.{Numero.Substring(6, 3)}-{Numero[9..]}"
        : $"{Numero[..2]}.{Numero.Substring(2, 3)}.{Numero.Substring(5, 3)}/{Numero.Substring(8, 4)}-{Numero[12..]}";

    /// <inheritdoc />
    public override string ToString() => Numero;

    private static bool Aceitar(string digitos, TipoDeDocumento tipo, out Documento documento)
    {
        documento = new Documento(digitos, tipo);
        return true;
    }

    private static bool TodosIguais(string digitos)
    {
        for (var i = 1; i < digitos.Length; i++)
        {
            if (digitos[i] != digitos[0])
            {
                return false;
            }
        }

        return true;
    }
}

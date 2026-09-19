namespace Nfe.Core.Modelo;

/// <summary>Endereço de emitente, destinatário ou local de entrega.</summary>
public sealed record Endereco
{
    /// <summary>Logradouro.</summary>
    public string Logradouro { get; init; } = string.Empty;

    /// <summary>Número, que pode vir como "S/N".</summary>
    public string Numero { get; init; } = string.Empty;

    /// <summary>Complemento, quando informado.</summary>
    public string? Complemento { get; init; }

    /// <summary>Bairro.</summary>
    public string Bairro { get; init; } = string.Empty;

    /// <summary>Código do município no IBGE.</summary>
    public int CodigoDoMunicipio { get; init; }

    /// <summary>Nome do município.</summary>
    public string Municipio { get; init; } = string.Empty;

    /// <summary>Sigla da UF.</summary>
    public string Uf { get; init; } = string.Empty;

    /// <summary>CEP, somente dígitos.</summary>
    public string Cep { get; init; } = string.Empty;

    /// <summary>Telefone, quando informado.</summary>
    public string? Telefone { get; init; }

    /// <summary>Monta o endereço em uma linha, como sai no DANFE.</summary>
    public string EmUmaLinha()
    {
        var inicio = string.IsNullOrWhiteSpace(Numero) ? Logradouro : $"{Logradouro}, {Numero}";
        var partes = new List<string> { inicio };

        if (!string.IsNullOrWhiteSpace(Complemento))
        {
            partes.Add(Complemento);
        }

        if (!string.IsNullOrWhiteSpace(Bairro))
        {
            partes.Add(Bairro);
        }

        partes.Add($"{Municipio}/{Uf}");
        return string.Join(" - ", partes);
    }
}

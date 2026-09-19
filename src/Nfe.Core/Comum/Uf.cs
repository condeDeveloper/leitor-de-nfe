namespace Nfe.Core.Comum;

/// <summary>
/// Unidades federativas com o código do IBGE, que é o que aparece nos dois
/// primeiros dígitos da chave de acesso e no campo cUF do XML.
/// </summary>
public static class Uf
{
    private static readonly IReadOnlyDictionary<int, string> PorCodigo = new Dictionary<int, string>
    {
        [11] = "RO", [12] = "AC", [13] = "AM", [14] = "RR", [15] = "PA", [16] = "AP", [17] = "TO",
        [21] = "MA", [22] = "PI", [23] = "CE", [24] = "RN", [25] = "PB", [26] = "PE", [27] = "AL",
        [28] = "SE", [29] = "BA",
        [31] = "MG", [32] = "ES", [33] = "RJ", [35] = "SP",
        [41] = "PR", [42] = "SC", [43] = "RS",
        [50] = "MS", [51] = "MT", [52] = "GO", [53] = "DF",
    };

    private static readonly IReadOnlyDictionary<string, int> PorSigla =
        PorCodigo.ToDictionary(par => par.Value, par => par.Key, StringComparer.OrdinalIgnoreCase);

    /// <summary>Todas as siglas conhecidas, em ordem alfabética.</summary>
    public static IReadOnlyCollection<string> Siglas { get; } = PorCodigo.Values.OrderBy(s => s, StringComparer.Ordinal).ToArray();

    /// <summary>Converte o código do IBGE na sigla da UF.</summary>
    public static bool TentarSigla(int codigo, out string sigla) => PorCodigo.TryGetValue(codigo, out sigla!);

    /// <summary>Converte a sigla da UF no código do IBGE.</summary>
    public static bool TentarCodigo(string? sigla, out int codigo)
    {
        codigo = 0;
        return !string.IsNullOrWhiteSpace(sigla) && PorSigla.TryGetValue(sigla.Trim(), out codigo);
    }

    /// <summary>Indica se o código do IBGE corresponde a alguma UF.</summary>
    public static bool CodigoEhValido(int codigo) => PorCodigo.ContainsKey(codigo);

    /// <summary>Indica se a sigla corresponde a alguma UF.</summary>
    public static bool SiglaEhValida(string? sigla) => TentarCodigo(sigla, out _);
}

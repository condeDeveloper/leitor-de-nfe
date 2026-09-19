namespace Nfe.Core.Fiscal;

/// <summary>
/// Nomenclatura Comum do Mercosul. São oito dígitos hierárquicos: capítulo,
/// posição, subposição, item e subitem. O layout aceita o valor "00" genérico
/// em alguns casos, e isso é tratado à parte.
/// </summary>
public readonly record struct Ncm
{
    private Ncm(string digitos, bool generico)
    {
        Digitos = digitos;
        EhGenerico = generico;
    }

    /// <summary>Os oito dígitos, ou "00" no caso genérico.</summary>
    public string Digitos { get; }

    /// <summary>Indica se o item usou o código genérico permitido pelo layout.</summary>
    public bool EhGenerico { get; }

    /// <summary>Capítulo, os dois primeiros dígitos.</summary>
    public int Capitulo => EhGenerico ? 0 : int.Parse(Digitos.AsSpan(0, 2));

    /// <summary>Posição, os quatro primeiros dígitos.</summary>
    public int Posicao => EhGenerico ? 0 : int.Parse(Digitos.AsSpan(0, 4));

    /// <summary>Tenta interpretar o código, aceitando pontos.</summary>
    public static bool TentarAnalisar(string? entrada, out Ncm ncm)
    {
        ncm = default;
        var digitos = Comum.Modulo11.SomenteDigitos(entrada);

        if (digitos == "00")
        {
            ncm = new Ncm(digitos, generico: true);
            return true;
        }

        if (digitos.Length != 8)
        {
            return false;
        }

        var capitulo = int.Parse(digitos.AsSpan(0, 2));
        if (capitulo is < 1 or > 97)
        {
            return false;
        }

        ncm = new Ncm(digitos, generico: false);
        return true;
    }

    /// <summary>Interpreta o código ou lança se ele for inválido.</summary>
    public static Ncm Analisar(string? entrada)
    {
        if (!TentarAnalisar(entrada, out var ncm))
        {
            throw new FormatException($"NCM inválido: '{entrada}'.");
        }

        return ncm;
    }

    /// <summary>Devolve o código no formato 0000.00.00.</summary>
    public string Formatado() => EhGenerico
        ? Digitos
        : $"{Digitos[..4]}.{Digitos.Substring(4, 2)}.{Digitos[6..]}";

    /// <inheritdoc />
    public override string ToString() => Digitos;
}

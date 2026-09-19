namespace Nfe.Core.Comum;

/// <summary>
/// Cálculo de dígito verificador pelo módulo 11, usado na chave de acesso da
/// NF-e, no CNPJ e no CPF. Cada documento tem a sua variação de pesos, então o
/// núcleo aqui recebe a sequência de pesos já pronta.
/// </summary>
public static class Modulo11
{
    /// <summary>
    /// Soma ponderada dos dígitos, da direita para a esquerda, com os pesos
    /// ciclando entre <paramref name="pesoInicial"/> e <paramref name="pesoFinal"/>.
    /// </summary>
    public static int SomaPonderada(ReadOnlySpan<char> digitos, int pesoInicial, int pesoFinal)
    {
        if (pesoInicial < 2 || pesoFinal <= pesoInicial)
        {
            throw new ArgumentOutOfRangeException(nameof(pesoInicial), "Faixa de pesos inválida.");
        }

        var soma = 0;
        var peso = pesoInicial;

        for (var i = digitos.Length - 1; i >= 0; i--)
        {
            var c = digitos[i];
            if (c is < '0' or > '9')
            {
                throw new FormatException($"Caractere '{c}' não é um dígito.");
            }

            soma += (c - '0') * peso;
            peso = peso == pesoFinal ? pesoInicial : peso + 1;
        }

        return soma;
    }

    /// <summary>
    /// Dígito verificador da chave de acesso da NF-e: pesos de 2 a 9 e resto
    /// 0 ou 1 resultando em dígito zero.
    /// </summary>
    public static int DigitoDaChave(ReadOnlySpan<char> digitos)
    {
        var resto = SomaPonderada(digitos, 2, 9) % 11;
        return resto is 0 or 1 ? 0 : 11 - resto;
    }

    /// <summary>
    /// Dígito verificador usado em CPF e CNPJ, onde os pesos são decrescentes
    /// a partir do tamanho da sequência.
    /// </summary>
    public static int DigitoDeDocumento(ReadOnlySpan<char> digitos, int pesoMaximo)
    {
        var soma = 0;
        var peso = pesoMaximo;

        foreach (var c in digitos)
        {
            if (c is < '0' or > '9')
            {
                throw new FormatException($"Caractere '{c}' não é um dígito.");
            }

            soma += (c - '0') * peso;
            peso = peso == 2 ? 9 : peso - 1;
        }

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    /// <summary>
    /// Mantém apenas os dígitos de uma entrada com máscara.
    /// </summary>
    public static string SomenteDigitos(string? valor)
    {
        if (string.IsNullOrEmpty(valor))
        {
            return string.Empty;
        }

        Span<char> destino = valor.Length <= 64 ? stackalloc char[valor.Length] : new char[valor.Length];
        var n = 0;

        foreach (var c in valor)
        {
            if (c is >= '0' and <= '9')
            {
                destino[n++] = c;
            }
        }

        return new string(destino[..n]);
    }
}

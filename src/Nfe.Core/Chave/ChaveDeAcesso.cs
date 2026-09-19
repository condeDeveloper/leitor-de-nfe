using Nfe.Core.Comum;

namespace Nfe.Core.Chave;

/// <summary>Forma de emissão declarada na chave de acesso (campo tpEmis).</summary>
public enum TipoDeEmissao
{
    /// <summary>Emissão normal, com autorização on-line.</summary>
    Normal = 1,

    /// <summary>Contingência em formulário de segurança.</summary>
    ContingenciaFs = 2,

    /// <summary>Contingência com SCAN (desativado, mantido por compatibilidade).</summary>
    ContingenciaScan = 3,

    /// <summary>Contingência em papel via DPEC.</summary>
    ContingenciaDpec = 4,

    /// <summary>Contingência em formulário de segurança para impressão auxiliar.</summary>
    ContingenciaFsda = 5,

    /// <summary>Contingência com autorização pela SVC-AN.</summary>
    ContingenciaSvcAn = 6,

    /// <summary>Contingência com autorização pela SVC-RS.</summary>
    ContingenciaSvcRs = 7,

    /// <summary>Contingência off-line, exclusiva da NFC-e.</summary>
    ContingenciaOffline = 9,
}

/// <summary>
/// Chave de acesso de 44 dígitos da NF-e, já decomposta nos campos que a
/// compõem. A chave carrega quase toda a identificação da nota, então validá-la
/// e abri-la resolve boa parte da conferência antes mesmo de ler o XML.
/// </summary>
public sealed class ChaveDeAcesso
{
    /// <summary>Quantidade de dígitos de uma chave de acesso.</summary>
    public const int Tamanho = 44;

    private ChaveDeAcesso(string digitos)
    {
        Digitos = digitos;
        CodigoDaUf = int.Parse(digitos.AsSpan(0, 2));
        Ano = 2000 + int.Parse(digitos.AsSpan(2, 2));
        Mes = int.Parse(digitos.AsSpan(4, 2));
        Cnpj = digitos.Substring(6, 14);
        Modelo = int.Parse(digitos.AsSpan(20, 2));
        Serie = int.Parse(digitos.AsSpan(22, 3));
        Numero = int.Parse(digitos.AsSpan(25, 9));
        TipoDeEmissao = (TipoDeEmissao)int.Parse(digitos.AsSpan(34, 1));
        CodigoNumerico = digitos.Substring(35, 8);
        DigitoVerificador = digitos[43] - '0';
        Uf = Comum.Uf.TentarSigla(CodigoDaUf, out var sigla) ? sigla : string.Empty;
    }

    /// <summary>Os 44 dígitos, sem separadores.</summary>
    public string Digitos { get; }

    /// <summary>Código do IBGE da UF do emitente.</summary>
    public int CodigoDaUf { get; }

    /// <summary>Sigla da UF do emitente, vazia se o código for desconhecido.</summary>
    public string Uf { get; }

    /// <summary>Ano de emissão, com os quatro dígitos.</summary>
    public int Ano { get; }

    /// <summary>Mês de emissão.</summary>
    public int Mes { get; }

    /// <summary>CNPJ do emitente embutido na chave.</summary>
    public string Cnpj { get; }

    /// <summary>Modelo do documento: 55 para NF-e e 65 para NFC-e.</summary>
    public int Modelo { get; }

    /// <summary>Série do documento.</summary>
    public int Serie { get; }

    /// <summary>Número do documento.</summary>
    public int Numero { get; }

    /// <summary>Forma de emissão.</summary>
    public TipoDeEmissao TipoDeEmissao { get; }

    /// <summary>Código numérico aleatório gerado pelo emitente.</summary>
    public string CodigoNumerico { get; }

    /// <summary>Dígito verificador, calculado pelo módulo 11.</summary>
    public int DigitoVerificador { get; }

    /// <summary>Indica se o modelo é o de nota ao consumidor.</summary>
    public bool EhNfce => Modelo == 65;

    /// <summary>Tenta interpretar uma chave, aceitando máscara e espaços.</summary>
    public static bool TentarAnalisar(string? entrada, out ChaveDeAcesso chave)
    {
        chave = null!;
        var digitos = Modulo11.SomenteDigitos(entrada);

        if (digitos.Length != Tamanho)
        {
            return false;
        }

        if (Modulo11.DigitoDaChave(digitos.AsSpan(0, 43)) != digitos[43] - '0')
        {
            return false;
        }

        chave = new ChaveDeAcesso(digitos);
        return true;
    }

    /// <summary>Interpreta a chave ou lança se ela for inválida.</summary>
    public static ChaveDeAcesso Analisar(string? entrada)
    {
        if (!TentarAnalisar(entrada, out var chave))
        {
            throw new FormatException("Chave de acesso inválida: precisa ter 44 dígitos e dígito verificador correto.");
        }

        return chave;
    }

    /// <summary>
    /// Monta a chave a partir dos campos e calcula o dígito verificador. Útil
    /// para conferir se a chave declarada no XML bate com o conteúdo da nota.
    /// </summary>
    public static ChaveDeAcesso Montar(
        int codigoDaUf,
        int ano,
        int mes,
        string cnpj,
        int modelo,
        int serie,
        int numero,
        TipoDeEmissao tipoDeEmissao,
        string codigoNumerico)
    {
        var cnpjLimpo = Modulo11.SomenteDigitos(cnpj);
        if (cnpjLimpo.Length != 14)
        {
            throw new ArgumentException("O CNPJ precisa ter 14 dígitos.", nameof(cnpj));
        }

        var codigo = Modulo11.SomenteDigitos(codigoNumerico);
        if (codigo.Length != 8)
        {
            throw new ArgumentException("O código numérico precisa ter 8 dígitos.", nameof(codigoNumerico));
        }

        var anoCurto = ano >= 2000 ? ano - 2000 : ano;

        var sem = string.Concat(
            codigoDaUf.ToString("D2"),
            anoCurto.ToString("D2"),
            mes.ToString("D2"),
            cnpjLimpo,
            modelo.ToString("D2"),
            serie.ToString("D3"),
            numero.ToString("D9"),
            ((int)tipoDeEmissao).ToString("D1"),
            codigo);

        return new ChaveDeAcesso(sem + Modulo11.DigitoDaChave(sem));
    }

    /// <summary>Devolve a chave em grupos de quatro dígitos, como no DANFE.</summary>
    public string Formatada()
    {
        Span<char> destino = stackalloc char[Tamanho + 10];
        var n = 0;

        for (var i = 0; i < Tamanho; i++)
        {
            if (i > 0 && i % 4 == 0)
            {
                destino[n++] = ' ';
            }

            destino[n++] = Digitos[i];
        }

        return new string(destino[..n]);
    }

    /// <inheritdoc />
    public override string ToString() => Digitos;
}

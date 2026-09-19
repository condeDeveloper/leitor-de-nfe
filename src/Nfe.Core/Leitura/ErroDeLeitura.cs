namespace Nfe.Core.Leitura;

/// <summary>
/// Falha ao interpretar o XML. Guarda o caminho do elemento para que o erro
/// aponte direto para o campo problemático, em vez de só dizer que o XML é
/// inválido.
/// </summary>
public sealed class ErroDeLeitura : Exception
{
    /// <summary>Cria o erro apontando o caminho do elemento.</summary>
    public ErroDeLeitura(string caminho, string mensagem)
        : base($"{caminho}: {mensagem}")
    {
        Caminho = caminho;
    }

    /// <summary>Cria o erro apontando o caminho e preservando a causa.</summary>
    public ErroDeLeitura(string caminho, string mensagem, Exception causa)
        : base($"{caminho}: {mensagem}", causa)
    {
        Caminho = caminho;
    }

    /// <summary>Caminho do elemento ou atributo que causou a falha.</summary>
    public string Caminho { get; }
}

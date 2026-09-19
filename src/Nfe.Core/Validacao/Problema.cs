namespace Nfe.Core.Validacao;

/// <summary>Gravidade de um problema encontrado na nota.</summary>
public enum Gravidade
{
    /// <summary>Divergência que não impede o uso da nota, mas merece atenção.</summary>
    Alerta,

    /// <summary>Divergência que rejeitaria a nota ou invalidaria a escrituração.</summary>
    Erro,
}

/// <summary>
/// Um problema encontrado pela validação. O código segue o estilo das rejeições
/// da SEFAZ para facilitar a busca, e o campo aponta onde olhar no XML.
/// </summary>
/// <param name="Codigo">Código curto e estável do problema.</param>
/// <param name="Gravidade">Se é erro ou apenas alerta.</param>
/// <param name="Campo">Caminho do campo envolvido.</param>
/// <param name="Mensagem">Descrição legível do que está errado.</param>
public sealed record Problema(string Codigo, Gravidade Gravidade, string Campo, string Mensagem)
{
    /// <summary>Cria um problema de gravidade alta.</summary>
    public static Problema Erro(string codigo, string campo, string mensagem)
        => new(codigo, Gravidade.Erro, campo, mensagem);

    /// <summary>Cria um problema apenas informativo.</summary>
    public static Problema Alerta(string codigo, string campo, string mensagem)
        => new(codigo, Gravidade.Alerta, campo, mensagem);

    /// <inheritdoc />
    public override string ToString() => $"[{Codigo}] {Campo}: {Mensagem}";
}

/// <summary>Resultado consolidado de uma validação.</summary>
/// <param name="Problemas">Tudo o que foi encontrado, na ordem das regras.</param>
public sealed record Resultado(IReadOnlyList<Problema> Problemas)
{
    /// <summary>Resultado sem nenhum problema.</summary>
    public static Resultado Limpo { get; } = new([]);

    /// <summary>Indica que a nota passou sem nenhum erro de gravidade alta.</summary>
    public bool EhValida => Problemas.All(p => p.Gravidade != Gravidade.Erro);

    /// <summary>Somente os erros.</summary>
    public IEnumerable<Problema> Erros => Problemas.Where(p => p.Gravidade == Gravidade.Erro);

    /// <summary>Somente os alertas.</summary>
    public IEnumerable<Problema> Alertas => Problemas.Where(p => p.Gravidade == Gravidade.Alerta);
}

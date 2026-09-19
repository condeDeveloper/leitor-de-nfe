using Nfe.Core.Modelo;

namespace Nfe.Core.Validacao;

/// <summary>
/// Roda todas as regras sobre a nota e junta os problemas em um resultado só.
/// As regras são independentes, então dá para trocar o conjunto conforme o uso:
/// uma conferência de recebimento pode querer menos regras que uma auditoria.
/// </summary>
public sealed class Validador
{
    private readonly IReadOnlyList<IRegra> regras;

    /// <summary>Cria o validador com o conjunto padrão de regras.</summary>
    public Validador()
        : this(RegrasPadrao())
    {
    }

    /// <summary>Cria o validador com um conjunto específico de regras.</summary>
    public Validador(IEnumerable<IRegra> regras)
    {
        ArgumentNullException.ThrowIfNull(regras);
        this.regras = regras.ToList();

        if (this.regras.Count == 0)
        {
            throw new ArgumentException("Informe ao menos uma regra.", nameof(regras));
        }
    }

    /// <summary>Nomes das regras configuradas.</summary>
    public IReadOnlyCollection<string> Regras => regras.Select(regra => regra.Nome).ToList();

    /// <summary>O conjunto de regras usado quando nada é informado.</summary>
    public static IReadOnlyList<IRegra> RegrasPadrao() =>
    [
        new RegraDaChave(),
        new RegraDosCadastros(),
        new RegraDosTotais(),
        new RegraDoCfop(),
    ];

    /// <summary>Confere a nota e devolve tudo o que foi encontrado.</summary>
    public Resultado Conferir(NotaFiscal nota)
    {
        ArgumentNullException.ThrowIfNull(nota);

        var problemas = new List<Problema>();

        foreach (var regra in regras)
        {
            problemas.AddRange(regra.Conferir(nota));
        }

        return problemas.Count == 0 ? Resultado.Limpo : new Resultado(problemas);
    }
}

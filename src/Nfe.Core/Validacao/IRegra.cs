using Nfe.Core.Modelo;

namespace Nfe.Core.Validacao;

/// <summary>
/// Uma regra de conferência da nota. Cada regra olha um aspecto e devolve os
/// problemas que encontrar, sem interromper as demais, para que uma única
/// passagem mostre tudo o que está errado.
/// </summary>
public interface IRegra
{
    /// <summary>Nome curto da regra, usado em log e em teste.</summary>
    string Nome { get; }

    /// <summary>Roda a regra sobre a nota.</summary>
    IEnumerable<Problema> Conferir(NotaFiscal nota);
}

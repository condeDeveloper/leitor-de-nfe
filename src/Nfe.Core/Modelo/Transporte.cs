using Nfe.Core.Comum;

namespace Nfe.Core.Modelo;

/// <summary>Responsável pelo frete, campo modFrete.</summary>
public enum ResponsavelPeloFrete
{
    /// <summary>Contratação por conta do remetente, CIF.</summary>
    Remetente = 0,

    /// <summary>Contratação por conta do destinatário, FOB.</summary>
    Destinatario = 1,

    /// <summary>Contratação por conta de terceiros.</summary>
    Terceiros = 2,

    /// <summary>Transporte próprio por conta do remetente.</summary>
    ProprioDoRemetente = 3,

    /// <summary>Transporte próprio por conta do destinatário.</summary>
    ProprioDoDestinatario = 4,

    /// <summary>Sem ocorrência de transporte.</summary>
    SemTransporte = 9,
}

/// <summary>Volume transportado.</summary>
public sealed record Volume
{
    /// <summary>Quantidade de volumes.</summary>
    public int Quantidade { get; init; }

    /// <summary>Espécie dos volumes.</summary>
    public string? Especie { get; init; }

    /// <summary>Marca dos volumes.</summary>
    public string? Marca { get; init; }

    /// <summary>Peso líquido em quilos.</summary>
    public decimal PesoLiquido { get; init; }

    /// <summary>Peso bruto em quilos.</summary>
    public decimal PesoBruto { get; init; }
}

/// <summary>Dados de transporte da nota, o grupo transp do XML.</summary>
public sealed record Transporte
{
    /// <summary>Quem responde pelo frete.</summary>
    public ResponsavelPeloFrete Responsavel { get; init; } = ResponsavelPeloFrete.SemTransporte;

    /// <summary>Documento da transportadora, quando houver.</summary>
    public Documento? Transportadora { get; init; }

    /// <summary>Razão social da transportadora.</summary>
    public string? RazaoSocial { get; init; }

    /// <summary>Placa do veículo, quando informada.</summary>
    public string? Placa { get; init; }

    /// <summary>UF do veículo.</summary>
    public string? UfDoVeiculo { get; init; }

    /// <summary>Volumes transportados.</summary>
    public IReadOnlyList<Volume> Volumes { get; init; } = [];

    /// <summary>Peso bruto somado de todos os volumes.</summary>
    public decimal PesoBrutoTotal() => Dinheiro.Arredondar(Volumes.Sum(v => v.PesoBruto), 3);

    /// <summary>Quantidade somada de todos os volumes.</summary>
    public int QuantidadeDeVolumes() => Volumes.Sum(v => v.Quantidade);
}

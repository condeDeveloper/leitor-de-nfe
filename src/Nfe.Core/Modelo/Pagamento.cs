using Nfe.Core.Comum;

namespace Nfe.Core.Modelo;

/// <summary>Meio de pagamento informado no grupo detPag.</summary>
public enum MeioDePagamento
{
    /// <summary>Dinheiro.</summary>
    Dinheiro = 1,

    /// <summary>Cheque.</summary>
    Cheque = 2,

    /// <summary>Cartão de crédito.</summary>
    CartaoDeCredito = 3,

    /// <summary>Cartão de débito.</summary>
    CartaoDeDebito = 4,

    /// <summary>Crédito na loja.</summary>
    CreditoNaLoja = 5,

    /// <summary>Vale alimentação.</summary>
    ValeAlimentacao = 10,

    /// <summary>Vale refeição.</summary>
    ValeRefeicao = 11,

    /// <summary>Boleto bancário.</summary>
    Boleto = 15,

    /// <summary>Depósito bancário.</summary>
    Deposito = 16,

    /// <summary>Pix com pagamento dinâmico.</summary>
    Pix = 17,

    /// <summary>Transferência bancária ou carteira digital.</summary>
    Transferencia = 18,

    /// <summary>Sem pagamento, usado em remessas e bonificações.</summary>
    SemPagamento = 90,

    /// <summary>Outros meios.</summary>
    Outros = 99,
}

/// <summary>Uma forma de pagamento declarada na nota.</summary>
public sealed record Pagamento
{
    /// <summary>Meio utilizado.</summary>
    public MeioDePagamento Meio { get; init; }

    /// <summary>Valor pago por esse meio.</summary>
    public decimal Valor { get; init; }

    /// <summary>Indica se o pagamento foi a prazo.</summary>
    public bool APrazo { get; init; }

    /// <summary>Bandeira do cartão, quando houver.</summary>
    public string? Bandeira { get; init; }

    /// <summary>Indica se o meio dispensa informação de valor.</summary>
    public bool DispensaValor => Meio == MeioDePagamento.SemPagamento;
}

/// <summary>Conjunto de pagamentos da nota, o grupo pag do XML.</summary>
public sealed record Pagamentos
{
    /// <summary>Formas de pagamento informadas.</summary>
    public IReadOnlyList<Pagamento> Formas { get; init; } = [];

    /// <summary>Troco, quando informado.</summary>
    public decimal Troco { get; init; }

    /// <summary>Soma dos valores pagos, desconsiderando as formas sem valor.</summary>
    public decimal Total() => Dinheiro.Arredondar(
        Formas.Where(forma => !forma.DispensaValor).Sum(forma => forma.Valor));

    /// <summary>Indica se o total pago cobre o valor da nota.</summary>
    public bool CobreOTotalDaNota(decimal totalDaNota)
        => Formas.Any(forma => forma.DispensaValor) || Dinheiro.Equivalem(Total() - Troco, totalDaNota);
}

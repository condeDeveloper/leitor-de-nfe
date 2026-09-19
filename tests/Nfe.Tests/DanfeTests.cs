using Nfe.Core.Comum;
using Nfe.Core.Danfe;
using Nfe.Core.Leitura;

namespace Nfe.Tests;

public class DanfeTests
{
    private readonly LeitorDeNfe leitor = new();

    [Fact]
    public void Monta_o_cabecalho_do_danfe()
    {
        var danfe = ResumoDanfe.De(leitor.Ler(Exemplos.NotaCompleta));

        danfe.ChaveFormatada.Should().Be("3526 0911 2223 3300 0181 5500 1000 0123 4511 2345 6784");
        danfe.Identificacao.Should().Be("Nº 12.345 - Série 1");
        danfe.TipoDeOperacao.Should().Be("1 - SAÍDA");
        danfe.DataDeEmissao.Should().Be("15/09/2026");
        danfe.DataDeSaida.Should().Be("15/09/2026");
        danfe.TotalDaNota.Should().Be("557,75");
    }

    [Fact]
    public void Monta_as_linhas_de_emitente_e_destinatario()
    {
        var danfe = ResumoDanfe.De(leitor.Ler(Exemplos.NotaCompleta));

        danfe.EmitenteEmUmaLinha.Should().Be(
            "COMERCIO DE FERRAGENS CONDE LTDA - CNPJ 11.222.333/0001-81 - " +
            "RUA DAS OFICINAS, 1200 - BRAS - SAO PAULO/SP");

        danfe.DestinatarioEmUmaLinha.Should().Contain("529.982.247-25");
        danfe.DestinatarioEmUmaLinha.Should().Contain("APTO 51");
    }

    [Fact]
    public void Monta_a_tabela_de_produtos_com_os_valores_formatados()
    {
        var produtos = ResumoDanfe.De(leitor.Ler(Exemplos.NotaCompleta)).Produtos;

        produtos.Should().HaveCount(2);
        produtos[0].Codigo.Should().Be("PAR-001");
        produtos[0].Ncm.Should().Be("7318.15.00");
        produtos[0].Cfop.Should().Be("5102");
        produtos[0].Quantidade.Should().Be("10,0000");
        produtos[0].ValorUnitario.Should().Be("25,50");
        produtos[0].ValorTotal.Should().Be("255,00");
    }

    [Fact]
    public void Leva_as_informacoes_complementares_para_os_dados_adicionais()
    {
        var danfe = ResumoDanfe.De(leitor.Ler(Exemplos.NotaCompleta));

        danfe.DadosAdicionais().Should().Be("PEDIDO 4471. PAGAMENTO EM 30 DIAS.");
    }

    [Fact]
    public void Avisa_nos_dados_adicionais_quando_falta_o_protocolo()
    {
        var inicio = Exemplos.NotaCompleta.IndexOf("<NFe>", StringComparison.Ordinal);
        var fim = Exemplos.NotaCompleta.IndexOf("</NFe>", StringComparison.Ordinal) + "</NFe>".Length;
        var xml = Exemplos.NotaCompleta[inicio..fim].Replace(
            "<NFe>",
            """<NFe xmlns="http://www.portalfiscal.inf.br/nfe">""",
            StringComparison.Ordinal);

        var danfe = ResumoDanfe.De(leitor.Ler(xml));

        danfe.DadosAdicionais().Should().Contain("SEM PROTOCOLO");
    }

    [Theory]
    [InlineData(0, "0,00")]
    [InlineData(1234.5, "1.234,50")]
    [InlineData(1234567.891, "1.234.567,89")]
    [InlineData(-45.678, "-45,68")]
    public void Formata_valores_no_padrao_brasileiro(decimal valor, string esperado)
    {
        Dinheiro.Formatar(valor).Should().Be(esperado);
    }

    [Theory]
    [InlineData("1.50", 1.50)]
    [InlineData("0", 0)]
    [InlineData("", 0)]
    [InlineData(null, 0)]
    public void Le_valores_do_xml_com_ponto_decimal(string? texto, decimal esperado)
    {
        Dinheiro.Ler(texto).Should().Be(esperado);
    }

    [Fact]
    public void Recusa_valor_que_nao_e_numero()
    {
        var acao = () => Dinheiro.Ler("mil reais");

        acao.Should().Throw<FormatException>();
    }

    [Theory]
    [InlineData(10.00, 10.01, true)]
    [InlineData(10.00, 10.02, false)]
    [InlineData(10.004, 10.00, true)]
    public void Compara_valores_tolerando_um_centavo(decimal esquerda, decimal direita, bool equivalem)
    {
        Dinheiro.Equivalem(esquerda, direita).Should().Be(equivalem);
    }

    [Fact]
    public void Arredonda_meio_centavo_para_cima()
    {
        Dinheiro.Arredondar(1.005m).Should().Be(1.01m);
        Dinheiro.Arredondar(2.675m).Should().Be(2.68m);
    }
}

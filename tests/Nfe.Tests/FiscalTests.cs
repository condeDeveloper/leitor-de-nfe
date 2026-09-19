using Nfe.Core.Comum;
using Nfe.Core.Fiscal;

namespace Nfe.Tests;

public class CfopTests
{
    [Theory]
    [InlineData("1102", SentidoDaOperacao.Entrada, AmbitoDaOperacao.Estadual)]
    [InlineData("2102", SentidoDaOperacao.Entrada, AmbitoDaOperacao.Interestadual)]
    [InlineData("3102", SentidoDaOperacao.Entrada, AmbitoDaOperacao.Exterior)]
    [InlineData("5102", SentidoDaOperacao.Saida, AmbitoDaOperacao.Estadual)]
    [InlineData("6102", SentidoDaOperacao.Saida, AmbitoDaOperacao.Interestadual)]
    [InlineData("7102", SentidoDaOperacao.Saida, AmbitoDaOperacao.Exterior)]
    public void Deduz_sentido_e_ambito_pelo_primeiro_digito(
        string codigo, SentidoDaOperacao sentido, AmbitoDaOperacao ambito)
    {
        var cfop = Cfop.Analisar(codigo);

        cfop.Sentido.Should().Be(sentido);
        cfop.Ambito.Should().Be(ambito);
    }

    [Theory]
    [InlineData("5202")]
    [InlineData("1202")]
    public void Reconhece_cfop_de_devolucao(string codigo)
    {
        Cfop.Analisar(codigo).EhDevolucao.Should().BeTrue();
    }

    [Fact]
    public void Reconhece_cfop_de_industrializacao()
    {
        Cfop.Analisar("5901").EhIndustrializacao.Should().BeTrue();
        Cfop.Analisar("5102").EhIndustrializacao.Should().BeFalse();
    }

    [Theory]
    [InlineData("4102")]
    [InlineData("8102")]
    [InlineData("0102")]
    [InlineData("510")]
    [InlineData("")]
    [InlineData(null)]
    public void Recusa_codigo_invalido(string? codigo)
    {
        Cfop.TentarAnalisar(codigo, out _).Should().BeFalse();
    }

    [Theory]
    [InlineData("SP", "SP", AmbitoDaOperacao.Estadual)]
    [InlineData("SP", "MG", AmbitoDaOperacao.Interestadual)]
    [InlineData("SP", "EX", AmbitoDaOperacao.Exterior)]
    public void Deduz_o_ambito_esperado_pelas_ufs(string origem, string destino, AmbitoDaOperacao esperado)
    {
        Cfop.AmbitoEsperado(origem, destino).Should().Be(esperado);
    }

    [Fact]
    public void Formata_com_quatro_digitos()
    {
        Cfop.Analisar("5.102").ToString().Should().Be("5102");
    }
}

public class NcmTests
{
    [Fact]
    public void Abre_capitulo_e_posicao()
    {
        var ncm = Ncm.Analisar("73181500");

        ncm.Capitulo.Should().Be(73);
        ncm.Posicao.Should().Be(7318);
        ncm.EhGenerico.Should().BeFalse();
    }

    [Fact]
    public void Formata_no_padrao_com_pontos()
    {
        Ncm.Analisar("73181500").Formatado().Should().Be("7318.15.00");
    }

    [Fact]
    public void Aceita_o_codigo_generico()
    {
        var ncm = Ncm.Analisar("00");

        ncm.EhGenerico.Should().BeTrue();
        ncm.Formatado().Should().Be("00");
    }

    [Theory]
    [InlineData("1234567")]
    [InlineData("98181500")]
    [InlineData("00181500")]
    [InlineData(null)]
    public void Recusa_codigo_invalido(string? codigo)
    {
        Ncm.TentarAnalisar(codigo, out _).Should().BeFalse();
    }
}

public class SituacaoTributariaTests
{
    [Theory]
    [InlineData("00", false)]
    [InlineData("0", false)]
    [InlineData("60", false)]
    [InlineData("102", true)]
    [InlineData("500", true)]
    public void Distingue_cst_de_csosn(string codigo, bool ehCsosn)
    {
        SituacaoTributaria.Analisar(codigo).EhCsosn.Should().Be(ehCsosn);
    }

    [Fact]
    public void Normaliza_o_cst_com_dois_digitos()
    {
        SituacaoTributaria.Analisar("0").Codigo.Should().Be("00");
    }

    [Theory]
    [InlineData("40")]
    [InlineData("41")]
    [InlineData("300")]
    public void Reconhece_situacoes_desoneradas(string codigo)
    {
        SituacaoTributaria.Analisar(codigo).Desonera.Should().BeTrue();
    }

    [Theory]
    [InlineData("10")]
    [InlineData("60")]
    [InlineData("201")]
    public void Reconhece_situacoes_com_substituicao(string codigo)
    {
        SituacaoTributaria.Analisar(codigo).TemSubstituicao.Should().BeTrue();
    }

    [Fact]
    public void Cst_combina_com_regime_normal_e_csosn_com_o_simples()
    {
        SituacaoTributaria.Analisar("00").CombinaCom(RegimeTributario.Normal).Should().BeTrue();
        SituacaoTributaria.Analisar("00").CombinaCom(RegimeTributario.SimplesNacional).Should().BeFalse();
        SituacaoTributaria.Analisar("102").CombinaCom(RegimeTributario.SimplesNacional).Should().BeTrue();
        SituacaoTributaria.Analisar("102").CombinaCom(RegimeTributario.Normal).Should().BeFalse();
    }

    [Theory]
    [InlineData("99")]
    [InlineData("777")]
    [InlineData("abc")]
    [InlineData(null)]
    public void Recusa_codigo_fora_das_tabelas(string? codigo)
    {
        SituacaoTributaria.TentarAnalisar(codigo, out _).Should().BeFalse();
    }
}

public class UfTests
{
    [Fact]
    public void Converte_codigo_do_ibge_em_sigla()
    {
        Uf.TentarSigla(35, out var sigla).Should().BeTrue();
        sigla.Should().Be("SP");
    }

    [Fact]
    public void Converte_sigla_em_codigo_do_ibge()
    {
        Uf.TentarCodigo("mg", out var codigo).Should().BeTrue();
        codigo.Should().Be(31);
    }

    [Fact]
    public void Conhece_as_vinte_e_sete_unidades_federativas()
    {
        Uf.Siglas.Should().HaveCount(27);
    }

    [Theory]
    [InlineData(34)]
    [InlineData(0)]
    public void Recusa_codigo_desconhecido(int codigo)
    {
        Uf.CodigoEhValido(codigo).Should().BeFalse();
    }

    [Theory]
    [InlineData("XX")]
    [InlineData("")]
    [InlineData(null)]
    public void Recusa_sigla_desconhecida(string? sigla)
    {
        Uf.SiglaEhValida(sigla).Should().BeFalse();
    }
}

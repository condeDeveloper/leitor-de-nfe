using Nfe.Core.Chave;

namespace Nfe.Tests;

public class ChaveDeAcessoTests
{
    [Fact]
    public void Abre_todos_os_campos_da_chave()
    {
        var chave = ChaveDeAcesso.Analisar(Exemplos.Chave);

        chave.CodigoDaUf.Should().Be(35);
        chave.Uf.Should().Be("SP");
        chave.Ano.Should().Be(2026);
        chave.Mes.Should().Be(9);
        chave.Cnpj.Should().Be(Exemplos.CnpjDoEmitente);
        chave.Modelo.Should().Be(55);
        chave.Serie.Should().Be(1);
        chave.Numero.Should().Be(12345);
        chave.TipoDeEmissao.Should().Be(TipoDeEmissao.Normal);
        chave.CodigoNumerico.Should().Be("12345678");
        chave.EhNfce.Should().BeFalse();
    }

    [Fact]
    public void Aceita_chave_com_mascara_e_espacos()
    {
        var comEspacos = ChaveDeAcesso.Analisar(ChaveDeAcesso.Analisar(Exemplos.Chave).Formatada());

        comEspacos.Digitos.Should().Be(Exemplos.Chave);
    }

    [Fact]
    public void Formata_a_chave_em_grupos_de_quatro()
    {
        var formatada = ChaveDeAcesso.Analisar(Exemplos.Chave).Formatada();

        formatada.Should().StartWith("3526 0911 2223 3300");
        formatada.Split(' ').Should().HaveCount(11).And.OnlyContain(grupo => grupo.Length == 4);
    }

    [Fact]
    public void Recusa_chave_com_digito_verificador_errado()
    {
        var digitoTrocado = Exemplos.Chave[..43] + (Exemplos.Chave[43] == '0' ? '1' : '0');

        ChaveDeAcesso.TentarAnalisar(digitoTrocado, out _).Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("352609112223330001815500100001234511234567")]
    [InlineData("3526091122233300018155001000012345112345678400")]
    [InlineData("abc")]
    public void Recusa_entradas_fora_do_formato(string? entrada)
    {
        ChaveDeAcesso.TentarAnalisar(entrada, out _).Should().BeFalse();
    }

    [Fact]
    public void Analisar_lanca_quando_a_chave_e_invalida()
    {
        var acao = () => ChaveDeAcesso.Analisar("123");

        acao.Should().Throw<FormatException>();
    }

    [Fact]
    public void Monta_a_chave_calculando_o_digito_verificador()
    {
        var chave = ChaveDeAcesso.Montar(
            codigoDaUf: 35,
            ano: 2026,
            mes: 9,
            cnpj: Exemplos.CnpjDoEmitente,
            modelo: 55,
            serie: 1,
            numero: 12345,
            tipoDeEmissao: TipoDeEmissao.Normal,
            codigoNumerico: "12345678");

        chave.Digitos.Should().Be(Exemplos.Chave);
    }

    [Fact]
    public void Montar_recusa_cnpj_fora_do_tamanho()
    {
        var acao = () => ChaveDeAcesso.Montar(35, 2026, 9, "123", 55, 1, 1, TipoDeEmissao.Normal, "12345678");

        acao.Should().Throw<ArgumentException>().WithParameterName("cnpj");
    }

    [Fact]
    public void Montar_recusa_codigo_numerico_fora_do_tamanho()
    {
        var acao = () => ChaveDeAcesso.Montar(35, 2026, 9, Exemplos.CnpjDoEmitente, 55, 1, 1, TipoDeEmissao.Normal, "1");

        acao.Should().Throw<ArgumentException>().WithParameterName("codigoNumerico");
    }

    [Fact]
    public void Reconhece_a_nota_ao_consumidor_pelo_modelo()
    {
        var chave = ChaveDeAcesso.Montar(
            35, 2026, 9, Exemplos.CnpjDoEmitente, 65, 1, 1, TipoDeEmissao.ContingenciaOffline, "00000001");

        chave.EhNfce.Should().BeTrue();
        chave.TipoDeEmissao.Should().Be(TipoDeEmissao.ContingenciaOffline);
    }
}

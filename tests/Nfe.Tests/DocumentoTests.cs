using Nfe.Core.Comum;

namespace Nfe.Tests;

public class DocumentoTests
{
    [Theory]
    [InlineData("52998224725")]
    [InlineData("529.982.247-25")]
    [InlineData("11144477735")]
    public void Aceita_cpf_valido(string entrada)
    {
        Documento.TentarAnalisar(entrada, out var documento).Should().BeTrue();
        documento.Tipo.Should().Be(TipoDeDocumento.Cpf);
    }

    [Theory]
    [InlineData("11222333000181")]
    [InlineData("11.222.333/0001-81")]
    public void Aceita_cnpj_valido(string entrada)
    {
        Documento.TentarAnalisar(entrada, out var documento).Should().BeTrue();
        documento.Tipo.Should().Be(TipoDeDocumento.Cnpj);
        documento.Numero.Should().Be("11222333000181");
    }

    [Theory]
    [InlineData("52998224724")]
    [InlineData("11111111111")]
    [InlineData("00000000000")]
    [InlineData("1234567890")]
    public void Recusa_cpf_invalido(string entrada)
    {
        Documento.TentarAnalisar(entrada, out _).Should().BeFalse();
    }

    [Theory]
    [InlineData("11222333000182")]
    [InlineData("11111111111111")]
    [InlineData("")]
    [InlineData(null)]
    public void Recusa_cnpj_invalido(string? entrada)
    {
        Documento.TentarAnalisar(entrada, out _).Should().BeFalse();
    }

    [Fact]
    public void Formata_cpf_com_mascara()
    {
        Documento.Analisar("52998224725").Formatado().Should().Be("529.982.247-25");
    }

    [Fact]
    public void Formata_cnpj_com_mascara()
    {
        Documento.Analisar("11222333000181").Formatado().Should().Be("11.222.333/0001-81");
    }

    [Fact]
    public void Analisar_lanca_quando_o_documento_e_invalido()
    {
        var acao = () => Documento.Analisar("99999999999");

        acao.Should().Throw<FormatException>();
    }

    [Fact]
    public void Dois_documentos_com_o_mesmo_numero_sao_iguais()
    {
        Documento.Analisar("52998224725").Should().Be(Documento.Analisar("529.982.247-25"));
    }

    [Theory]
    [InlineData("12.345-6", "123456")]
    [InlineData("abc123", "123")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void Limpa_a_mascara_mantendo_so_os_digitos(string? entrada, string esperado)
    {
        Modulo11.SomenteDigitos(entrada).Should().Be(esperado);
    }

    [Fact]
    public void Soma_ponderada_recusa_faixa_de_pesos_invalida()
    {
        var acao = () => Modulo11.SomaPonderada("123", 9, 2);

        acao.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Soma_ponderada_recusa_caractere_que_nao_e_digito()
    {
        var acao = () => Modulo11.SomaPonderada("12a", 2, 9);

        acao.Should().Throw<FormatException>();
    }
}

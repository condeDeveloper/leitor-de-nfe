using Nfe.Core.Comum;
using Nfe.Core.Leitura;
using Nfe.Core.Modelo;

namespace Nfe.Tests;

public class LeituraTests
{
    private readonly LeitorDeNfe leitor = new();

    [Fact]
    public void Le_a_identificacao_da_nota()
    {
        var nota = leitor.Ler(Exemplos.NotaCompleta);

        nota.Chave.Digitos.Should().Be(Exemplos.Chave);
        nota.Numero.Should().Be(12345);
        nota.Serie.Should().Be(1);
        nota.Modelo.Should().Be(55);
        nota.NaturezaDaOperacao.Should().Be("VENDA DE MERCADORIA");
        nota.Operacao.Should().Be(TipoDeOperacao.Saida);
        nota.Finalidade.Should().Be(FinalidadeDaEmissao.Normal);
        nota.Emissao.Should().Be(new DateTimeOffset(2026, 9, 15, 10, 30, 0, TimeSpan.FromHours(-3)));
        nota.SaidaOuEntrada.Should().NotBeNull();
    }

    [Fact]
    public void Le_o_emitente_com_endereco_e_regime()
    {
        var emitente = leitor.Ler(Exemplos.NotaCompleta).Emitente;

        emitente.Documento.Numero.Should().Be(Exemplos.CnpjDoEmitente);
        emitente.RazaoSocial.Should().Be("COMERCIO DE FERRAGENS CONDE LTDA");
        emitente.NomeFantasia.Should().Be("FERRAGENS CONDE");
        emitente.InscricaoEstadual.Should().Be("112233445566");
        emitente.Regime.Should().Be(Core.Fiscal.RegimeTributario.Normal);
        emitente.Endereco.Uf.Should().Be("SP");
        emitente.Endereco.Cep.Should().Be("03043000");
        emitente.Endereco.EmUmaLinha().Should().Be("RUA DAS OFICINAS, 1200 - BRAS - SAO PAULO/SP");
    }

    [Fact]
    public void Le_o_destinatario_pessoa_fisica()
    {
        var destinatario = leitor.Ler(Exemplos.NotaCompleta).Destinatario;

        destinatario.Documento.Tipo.Should().Be(TipoDeDocumento.Cpf);
        destinatario.Documento.Numero.Should().Be(Exemplos.CpfDoDestinatario);
        destinatario.EhPessoaFisica.Should().BeTrue();
        destinatario.Indicador.Should().Be(IndicadorDeInscricao.NaoContribuinte);
        destinatario.Email.Should().Be("maria@exemplo.com.br");
        destinatario.Endereco.Complemento.Should().Be("APTO 51");
    }

    [Fact]
    public void Le_os_itens_na_ordem_do_xml()
    {
        var itens = leitor.Ler(Exemplos.NotaCompleta).Itens;

        itens.Should().HaveCount(2);
        itens[0].Numero.Should().Be(1);
        itens[0].Codigo.Should().Be("PAR-001");
        itens[0].Descricao.Should().Be("PARAFUSO SEXTAVADO 10MM");
        itens[0].Ncm.Formatado().Should().Be("7318.15.00");
        itens[0].Cfop.ToString().Should().Be("5102");
        itens[0].Quantidade.Should().Be(10m);
        itens[0].ValorUnitario.Should().Be(25.50m);
        itens[0].ValorBruto.Should().Be(255.00m);
        itens[1].Codigo.Should().Be("FUR-014");
        itens[1].Desconto.Should().Be(9.80m);
    }

    [Fact]
    public void Trata_sem_gtin_como_ausencia_de_codigo_de_barras()
    {
        var itens = leitor.Ler(Exemplos.NotaCompleta).Itens;

        itens[0].Gtin.Should().BeNull();
        itens[1].Gtin.Should().Be("7891234567895");
    }

    [Fact]
    public void Le_os_tributos_do_item()
    {
        var tributos = leitor.Ler(Exemplos.NotaCompleta).Itens[0].Tributos;

        tributos.Icms.Situacao.Codigo.Should().Be("00");
        tributos.Icms.BaseDeCalculo.Should().Be(255.00m);
        tributos.Icms.Aliquota.Should().Be(18.00m);
        tributos.Icms.Valor.Should().Be(45.90m);
        tributos.Pis.Valor.Should().Be(4.21m);
        tributos.Cofins.Valor.Should().Be(19.38m);
        tributos.Total().Should().Be(82.24m);
    }

    [Fact]
    public void Pula_o_c_enq_e_le_o_grupo_do_ipi()
    {
        var itens = leitor.Ler(Exemplos.NotaCompleta).Itens;

        itens[0].Tributos.Ipi.Should().NotBeNull();
        itens[0].Tributos.Ipi!.Valor.Should().Be(12.75m);
        itens[0].Tributos.Ipi!.Aliquota.Should().Be(5.00m);
        itens[1].Tributos.Ipi.Should().BeNull();
    }

    [Fact]
    public void Le_os_totais_declarados()
    {
        var totais = leitor.Ler(Exemplos.NotaCompleta).Totais;

        totais.Produtos.Should().Be(554.80m);
        totais.Desconto.Should().Be(9.80m);
        totais.Icms.Should().Be(98.10m);
        totais.Ipi.Should().Be(12.75m);
        totais.NotaFiscal.Should().Be(557.75m);
    }

    [Fact]
    public void Le_o_transporte_e_os_volumes()
    {
        var transporte = leitor.Ler(Exemplos.NotaCompleta).Transporte;

        transporte.Responsavel.Should().Be(ResponsavelPeloFrete.Destinatario);
        transporte.RazaoSocial.Should().Be("TRANSPORTES CONDE");
        transporte.Placa.Should().Be("ABC1D23");
        transporte.QuantidadeDeVolumes().Should().Be(3);
        transporte.PesoBrutoTotal().Should().Be(20.000m);
    }

    [Fact]
    public void Le_os_pagamentos()
    {
        var pagamentos = leitor.Ler(Exemplos.NotaCompleta).Pagamentos;

        pagamentos.Formas.Should().ContainSingle();
        pagamentos.Formas[0].Meio.Should().Be(MeioDePagamento.Boleto);
        pagamentos.Formas[0].APrazo.Should().BeTrue();
        pagamentos.Total().Should().Be(557.75m);
        pagamentos.CobreOTotalDaNota(557.75m).Should().BeTrue();
    }

    [Fact]
    public void Le_o_protocolo_e_as_informacoes_complementares()
    {
        var nota = leitor.Ler(Exemplos.NotaCompleta);

        nota.EstaAutorizada.Should().BeTrue();
        nota.Protocolo.Should().Be("135260011223344");
        nota.InformacoesComplementares.Should().Contain("PEDIDO 4471");
    }

    [Fact]
    public void Le_o_xml_sem_o_envelope_do_protocolo()
    {
        var soANota = Exemplos.NotaCompleta;
        var inicio = soANota.IndexOf("<NFe>", StringComparison.Ordinal);
        var fim = soANota.IndexOf("</NFe>", StringComparison.Ordinal) + "</NFe>".Length;
        var xml = soANota[inicio..fim].Replace(
            "<NFe>",
            """<NFe xmlns="http://www.portalfiscal.inf.br/nfe">""",
            StringComparison.Ordinal);

        var nota = leitor.Ler(xml);

        nota.EstaAutorizada.Should().BeFalse();
        nota.Numero.Should().Be(12345);
    }

    [Fact]
    public void Le_a_partir_de_um_fluxo()
    {
        using var fluxo = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(Exemplos.NotaCompleta));

        leitor.Ler(fluxo).Numero.Should().Be(12345);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Recusa_conteudo_vazio(string xml)
    {
        var acao = () => leitor.Ler(xml);

        acao.Should().Throw<ErroDeLeitura>().Which.Caminho.Should().Be("xml");
    }

    [Fact]
    public void Recusa_xml_malformado()
    {
        var acao = () => leitor.Ler("<nfeProc><NFe>");

        acao.Should().Throw<ErroDeLeitura>().WithMessage("*malformado*");
    }

    [Fact]
    public void Recusa_documento_sem_o_elemento_nfe()
    {
        var acao = () => leitor.Ler("""<qualquer xmlns="http://www.portalfiscal.inf.br/nfe" />""");

        acao.Should().Throw<ErroDeLeitura>().WithMessage("*NFe*");
    }

    [Fact]
    public void Aponta_o_campo_quando_a_chave_do_id_e_invalida()
    {
        var xml = Exemplos.NotaCom($"Id=\"NFe{Exemplos.Chave}\"", "Id=\"NFe123\"");

        var acao = () => leitor.Ler(xml);

        acao.Should().Throw<ErroDeLeitura>().Which.Caminho.Should().Be("infNFe/@Id");
    }

    [Fact]
    public void Aponta_o_item_quando_o_cfop_e_invalido()
    {
        var xml = Exemplos.NotaComPrimeiro("<CFOP>5102</CFOP>", "<CFOP>9999</CFOP>");

        var acao = () => leitor.Ler(xml);

        acao.Should().Throw<ErroDeLeitura>().Which.Caminho.Should().Be("det[1]/prod/CFOP");
    }

    [Fact]
    public void Aponta_o_grupo_quando_falta_o_total()
    {
        var xml = Exemplos.NotaCom("<vNF>557.75</vNF>", "<vNF>abc</vNF>");

        var acao = () => leitor.Ler(xml);

        acao.Should().Throw<FormatException>();
    }
}

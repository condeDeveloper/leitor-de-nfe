using Nfe.Core.Leitura;
using Nfe.Core.Modelo;
using Nfe.Core.Validacao;

namespace Nfe.Tests;

public class ValidacaoTests
{
    private readonly LeitorDeNfe leitor = new();
    private readonly Validador validador = new();

    [Fact]
    public void Nota_consistente_passa_sem_nenhum_problema()
    {
        var resultado = Conferir(Exemplos.NotaCompleta);

        resultado.Problemas.Should().BeEmpty();
        resultado.EhValida.Should().BeTrue();
    }

    [Fact]
    public void Acusa_quando_o_numero_da_nota_nao_bate_com_a_chave()
    {
        var resultado = Conferir(Exemplos.NotaCom("<nNF>12345</nNF>", "<nNF>99999</nNF>"));

        resultado.EhValida.Should().BeFalse();
        resultado.Erros.Should().Contain(problema => problema.Codigo == "CHV-002");
    }

    [Fact]
    public void Acusa_quando_a_competencia_da_chave_nao_bate_com_a_emissao()
    {
        var resultado = Conferir(Exemplos.NotaCom("2026-09-15T10:30:00-03:00", "2026-11-15T10:30:00-03:00"));

        resultado.Erros.Should().Contain(problema => problema.Codigo == "CHV-005");
    }

    [Fact]
    public void Acusa_quando_a_uf_do_emitente_nao_bate_com_a_chave()
    {
        var resultado = Conferir(Exemplos.NotaComPrimeiro("<UF>SP</UF>", "<UF>MG</UF>"));

        resultado.Erros.Should().Contain(problema => problema.Codigo == "CHV-007");
    }

    [Fact]
    public void Avisa_quando_a_nota_foi_emitida_em_contingencia()
    {
        var chave = Nfe.Core.Chave.ChaveDeAcesso.Montar(
            35, 2026, 9, Exemplos.CnpjDoEmitente, 55, 1, 12345,
            Nfe.Core.Chave.TipoDeEmissao.ContingenciaSvcAn, "12345678");

        var xml = Exemplos.NotaCompleta
            .Replace(Exemplos.Chave, chave.Digitos, StringComparison.Ordinal)
            .Replace("<tpEmis>1</tpEmis>", "<tpEmis>6</tpEmis>", StringComparison.Ordinal);

        var resultado = Conferir(xml);

        resultado.EhValida.Should().BeTrue();
        resultado.Alertas.Should().Contain(problema => problema.Codigo == "CHV-008");
    }

    [Fact]
    public void Acusa_item_cujo_valor_nao_e_quantidade_vezes_unitario()
    {
        var resultado = Conferir(Exemplos.NotaComPrimeiro("<vUnCom>25.5000</vUnCom>", "<vUnCom>30.0000</vUnCom>"));

        resultado.Erros.Should().Contain(problema => problema.Codigo == "TOT-001");
    }

    [Fact]
    public void Acusa_total_de_produtos_diferente_da_soma_dos_itens()
    {
        var resultado = Conferir(Exemplos.NotaCom("<vProd>554.80</vProd>", "<vProd>600.00</vProd>"));

        resultado.Erros.Should().Contain(problema => problema.Codigo == "TOT-004");
    }

    [Fact]
    public void Acusa_total_da_nota_fora_da_formula_do_manual()
    {
        var resultado = Conferir(Exemplos.NotaCom("<vNF>557.75</vNF>", "<vNF>500.00</vNF>"));

        resultado.Erros.Should().Contain(problema => problema.Codigo == "TOT-010");
    }

    [Fact]
    public void Avisa_quando_o_icms_do_item_nao_bate_com_a_aliquota()
    {
        var resultado = Conferir(Exemplos.NotaComPrimeiro("<vICMS>45.90</vICMS>", "<vICMS>40.00</vICMS>"));

        resultado.Alertas.Should().Contain(problema => problema.Codigo == "TOT-002");
    }

    [Fact]
    public void Avisa_quando_os_pagamentos_nao_cobrem_o_total()
    {
        var resultado = Conferir(Exemplos.NotaCom("<vPag>557.75</vPag>", "<vPag>100.00</vPag>"));

        resultado.Alertas.Should().Contain(problema => problema.Codigo == "TOT-011");
    }

    [Fact]
    public void Acusa_cfop_interestadual_em_operacao_dentro_do_estado()
    {
        var resultado = Conferir(Exemplos.NotaCompleta.Replace("<CFOP>5102</CFOP>", "<CFOP>6102</CFOP>", StringComparison.Ordinal));

        resultado.Erros.Should().Contain(problema => problema.Codigo == "CFO-001");
    }

    [Fact]
    public void Acusa_cfop_de_entrada_em_nota_de_saida()
    {
        var resultado = Conferir(Exemplos.NotaCompleta.Replace("<CFOP>5102</CFOP>", "<CFOP>1102</CFOP>", StringComparison.Ordinal));

        resultado.Erros.Should().Contain(problema => problema.Codigo == "CFO-002");
    }

    [Fact]
    public void Acusa_nota_que_mistura_entrada_e_saida()
    {
        var resultado = Conferir(Exemplos.NotaComPrimeiro("<CFOP>5102</CFOP>", "<CFOP>1102</CFOP>"));

        resultado.Erros.Should().Contain(problema => problema.Codigo == "CFO-005");
    }

    [Fact]
    public void Avisa_quando_o_item_usa_ncm_generico()
    {
        var resultado = Conferir(Exemplos.NotaComPrimeiro("<NCM>73181500</NCM>", "<NCM>00</NCM>"));

        resultado.Alertas.Should().Contain(problema => problema.Codigo == "CFO-004");
    }

    [Fact]
    public void Acusa_emitente_sem_inscricao_estadual()
    {
        var resultado = Conferir(Exemplos.NotaCom("<IE>112233445566</IE>", "<IE></IE>"));

        resultado.Erros.Should().Contain(problema => problema.Codigo == "CAD-003");
    }

    [Fact]
    public void Acusa_destinatario_contribuinte_sem_inscricao()
    {
        var resultado = Conferir(Exemplos.NotaCom("<indIEDest>9</indIEDest>", "<indIEDest>1</indIEDest>"));

        resultado.Erros.Should().Contain(problema => problema.Codigo == "CAD-004");
    }

    [Fact]
    public void Acusa_csosn_em_emitente_do_regime_normal()
    {
        var resultado = Conferir(Exemplos.NotaComPrimeiro("<CST>00</CST>", "<CSOSN>102</CSOSN>"));

        resultado.Erros.Should().Contain(problema => problema.Codigo == "CAD-007");
    }

    [Fact]
    public void Avisa_quando_o_cep_esta_fora_do_padrao()
    {
        var resultado = Conferir(Exemplos.NotaCom("<CEP>03043000</CEP>", "<CEP>3043</CEP>"));

        resultado.Alertas.Should().Contain(problema => problema.Codigo == "CAD-002");
    }

    [Fact]
    public void O_validador_expoe_as_regras_configuradas()
    {
        validador.Regras.Should().BeEquivalentTo(new[] { "chave", "cadastros", "totais", "cfop" });
    }

    [Fact]
    public void Aceita_um_conjunto_reduzido_de_regras()
    {
        var soChave = new Validador([new RegraDaChave()]);

        soChave.Regras.Should().ContainSingle().Which.Should().Be("chave");
        soChave.Conferir(leitor.Ler(Exemplos.NotaCom("<vNF>557.75</vNF>", "<vNF>1.00</vNF>")))
            .Problemas.Should().BeEmpty();
    }

    [Fact]
    public void Recusa_ser_criado_sem_nenhuma_regra()
    {
        var acao = () => new Validador([]);

        acao.Should().Throw<ArgumentException>().WithParameterName("regras");
    }

    [Fact]
    public void O_problema_se_descreve_em_uma_linha()
    {
        var problema = Problema.Erro("XYZ-001", "ide/nNF", "Número divergente.");

        problema.ToString().Should().Be("[XYZ-001] ide/nNF: Número divergente.");
        problema.Gravidade.Should().Be(Gravidade.Erro);
    }

    [Fact]
    public void Totais_montados_a_partir_dos_itens_ignoram_quem_nao_compoe_o_total()
    {
        var nota = leitor.Ler(Exemplos.NotaComPrimeiro("<indTot>1</indTot>", "<indTot>0</indTot>"));

        var somados = Totais.APartirDosItens(nota.Itens);

        somados.Produtos.Should().Be(299.80m);
        nota.Itens[0].CompoeOTotal.Should().BeFalse();
    }

    private Resultado Conferir(string xml) => validador.Conferir(leitor.Ler(xml));
}

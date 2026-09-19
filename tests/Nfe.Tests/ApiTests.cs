using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Nfe.Tests;

public class ApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> fabrica;

    public ApiTests(WebApplicationFactory<Program> fabrica)
    {
        this.fabrica = fabrica;
    }

    [Fact]
    public async Task Responde_a_verificacao_de_saude()
    {
        var cliente = fabrica.CreateClient();

        var resposta = await cliente.GetAsync("/saude");

        resposta.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Abre_a_chave_de_acesso()
    {
        var cliente = fabrica.CreateClient();

        var resposta = await cliente.GetAsync($"/chaves/{Exemplos.Chave}");
        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>();

        resposta.StatusCode.Should().Be(HttpStatusCode.OK);
        corpo.GetProperty("uf").GetString().Should().Be("SP");
        corpo.GetProperty("numero").GetInt32().Should().Be(12345);
        corpo.GetProperty("cnpj").GetString().Should().Be(Exemplos.CnpjDoEmitente);
    }

    [Fact]
    public async Task Recusa_chave_invalida_com_quatrocentos()
    {
        var cliente = fabrica.CreateClient();

        var resposta = await cliente.GetAsync("/chaves/123");

        resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Le_a_nota_e_devolve_o_resumo()
    {
        var cliente = fabrica.CreateClient();

        var resposta = await cliente.PostAsJsonAsync("/notas/leitura", new { xml = Exemplos.NotaCompleta });
        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>();

        resposta.StatusCode.Should().Be(HttpStatusCode.OK);
        corpo.GetProperty("numero").GetInt32().Should().Be(12345);
        corpo.GetProperty("quantidadeDeItens").GetInt32().Should().Be(2);
        corpo.GetProperty("totalDaNota").GetDecimal().Should().Be(557.75m);
        corpo.GetProperty("estaAutorizada").GetBoolean().Should().BeTrue();
        corpo.GetProperty("produtos").GetArrayLength().Should().Be(2);
    }

    [Fact]
    public async Task Valida_a_nota_e_devolve_o_resultado_limpo()
    {
        var cliente = fabrica.CreateClient();

        var resposta = await cliente.PostAsJsonAsync("/notas/validacao", new { xml = Exemplos.NotaCompleta });
        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>();

        resposta.StatusCode.Should().Be(HttpStatusCode.OK);
        corpo.GetProperty("ehValida").GetBoolean().Should().BeTrue();
        corpo.GetProperty("quantidadeDeErros").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task Valida_a_nota_e_lista_os_problemas()
    {
        var cliente = fabrica.CreateClient();
        var xml = Exemplos.NotaCom("<vNF>557.75</vNF>", "<vNF>500.00</vNF>");

        var resposta = await cliente.PostAsJsonAsync("/notas/validacao", new { xml });
        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>();

        corpo.GetProperty("ehValida").GetBoolean().Should().BeFalse();
        corpo.GetProperty("quantidadeDeErros").GetInt32().Should().BeGreaterThan(0);
        corpo.GetProperty("problemas").EnumerateArray()
            .Select(problema => problema.GetProperty("codigo").GetString())
            .Should().Contain("TOT-010");
    }

    [Fact]
    public async Task Devolve_o_resumo_para_o_danfe()
    {
        var cliente = fabrica.CreateClient();

        var resposta = await cliente.PostAsJsonAsync("/notas/danfe", new { xml = Exemplos.NotaCompleta });
        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>();

        resposta.StatusCode.Should().Be(HttpStatusCode.OK);
        corpo.GetProperty("chave").GetString().Should().StartWith("3526 0911");
        corpo.GetProperty("total").GetString().Should().Be("557,75");
        corpo.GetProperty("produtos").GetArrayLength().Should().Be(2);
    }

    [Fact]
    public async Task Devolve_quatrocentos_com_o_campo_quando_o_xml_esta_errado()
    {
        var cliente = fabrica.CreateClient();
        var xml = Exemplos.NotaCom($"Id=\"NFe{Exemplos.Chave}\"", "Id=\"NFe123\"");

        var resposta = await cliente.PostAsJsonAsync("/notas/leitura", new { xml });
        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>();

        resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        corpo.GetProperty("campo").GetString().Should().Be("infNFe/@Id");
    }

    [Fact]
    public async Task Devolve_quatrocentos_quando_o_xml_vem_vazio()
    {
        var cliente = fabrica.CreateClient();

        var resposta = await cliente.PostAsJsonAsync("/notas/leitura", new { xml = "" });

        resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

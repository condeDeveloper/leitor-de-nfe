using Microsoft.AspNetCore.Mvc;
using Nfe.Api;
using Nfe.Core.Chave;
using Nfe.Core.Danfe;
using Nfe.Core.Leitura;
using Nfe.Core.Validacao;

var construtor = WebApplication.CreateBuilder(args);

construtor.Services.AddEndpointsApiExplorer();
construtor.Services.AddSwaggerGen();
construtor.Services.AddSingleton<LeitorDeNfe>();

// O validador tem um construtor que recebe as regras, e o container escolhe
// justamente esse. Registrar cada regra deixa o conjunto explícito aqui e
// permite trocá-lo sem mexer no núcleo.
foreach (var regra in Validador.RegrasPadrao())
{
    construtor.Services.AddSingleton(regra);
}

construtor.Services.AddSingleton<Validador>();

var aplicacao = construtor.Build();

if (aplicacao.Environment.IsDevelopment())
{
    aplicacao.UseSwagger();
    aplicacao.UseSwaggerUI();
}

aplicacao.MapGet("/saude", () => Results.Ok(new { estado = "ok" }))
    .WithName("Saude")
    .WithTags("Serviço");

aplicacao.MapGet("/chaves/{chave}", (string chave) =>
        ChaveDeAcesso.TentarAnalisar(chave, out var lida)
            ? Results.Ok(ChaveAberta.De(lida))
            : Results.BadRequest(new ProblemDetails
            {
                Title = "Chave de acesso inválida",
                Detail = "A chave precisa ter 44 dígitos e dígito verificador correto.",
                Status = StatusCodes.Status400BadRequest,
            }))
    .WithName("AbrirChave")
    .WithTags("Chave");

aplicacao.MapPost("/notas/leitura", ([FromBody] PedidoDeXml pedido, LeitorDeNfe leitor) =>
        Executar(() => Results.Ok(ResumoDaNota.De(leitor.Ler(pedido.Xml)))))
    .WithName("LerNota")
    .WithTags("Nota");

aplicacao.MapPost("/notas/validacao", ([FromBody] PedidoDeXml pedido, LeitorDeNfe leitor, Validador validador) =>
        Executar(() =>
        {
            var nota = leitor.Ler(pedido.Xml);
            return Results.Ok(RespostaDaValidacao.De(nota, validador.Conferir(nota)));
        }))
    .WithName("ValidarNota")
    .WithTags("Nota");

aplicacao.MapPost("/notas/danfe", ([FromBody] PedidoDeXml pedido, LeitorDeNfe leitor) =>
        Executar(() =>
        {
            var danfe = ResumoDanfe.De(leitor.Ler(pedido.Xml));

            return Results.Ok(new
            {
                chave = danfe.ChaveFormatada,
                identificacao = danfe.Identificacao,
                tipoDeOperacao = danfe.TipoDeOperacao,
                emissao = danfe.DataDeEmissao,
                saida = danfe.DataDeSaida,
                emitente = danfe.EmitenteEmUmaLinha,
                destinatario = danfe.DestinatarioEmUmaLinha,
                total = danfe.TotalDaNota,
                produtos = danfe.Produtos,
                dadosAdicionais = danfe.DadosAdicionais(),
            });
        }))
    .WithName("ResumoDoDanfe")
    .WithTags("Nota");

aplicacao.Run();

// A leitura falha com um erro que já sabe qual campo do XML causou o problema,
// então vale a pena devolvê-lo como 400 com o caminho junto em vez de deixar
// virar 500.
static IResult Executar(Func<IResult> acao)
{
    try
    {
        return acao();
    }
    catch (ErroDeLeitura erro)
    {
        return Results.BadRequest(new ProblemDetails
        {
            Title = "Não foi possível ler o XML",
            Detail = erro.Message,
            Status = StatusCodes.Status400BadRequest,
            Extensions = { ["campo"] = erro.Caminho },
        });
    }
    catch (FormatException erro)
    {
        return Results.BadRequest(new ProblemDetails
        {
            Title = "Conteúdo inválido no XML",
            Detail = erro.Message,
            Status = StatusCodes.Status400BadRequest,
        });
    }
}

/// <summary>Exposta para que os testes de integração possam subir a API.</summary>
public partial class Program;

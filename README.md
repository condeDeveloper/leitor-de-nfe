# leitor-de-nfe

Leitor e conferente de NF-e no layout 4.00, em .NET 8. Recebe o XML da nota
(ou o `nfeProc`, já com o protocolo de autorização), devolve o conteúdo tipado
e aponta as divergências que costumam derrubar a escrituração: chave que não
bate com o corpo da nota, totais que não fecham com a soma dos itens, CFOP
incompatível com as UFs envolvidas e CST usado fora do regime do emitente.

## Por que existe

Quem integra emissor com ERP passa mais tempo caçando divergência de centavo do
que escrevendo regra de negócio. O XML da NF-e repete a mesma informação em três
lugares — na chave de acesso, no grupo `ide` e no `ICMSTot` — e nada garante que
os três concordem. Este projeto faz essa conferência cruzada de uma vez só e
diz exatamente qual campo está errado.

## O que ele confere

| Código | Gravidade | O que pega |
| --- | --- | --- |
| `CHV-001` a `CHV-007` | erro | CNPJ, número, série, modelo, competência e UF da chave divergindo do corpo da nota |
| `CHV-008` | alerta | nota emitida em contingência |
| `CAD-001` a `CAD-007` | erro/alerta | UF desconhecida, CEP fora do padrão, IE ausente, indicador de IE incoerente, CST fora do regime |
| `TOT-001` a `TOT-011` | erro/alerta | item que não fecha quantidade × unitário, ICMS fora da alíquota, totais divergindo da soma dos itens, `vNF` fora da fórmula do manual, pagamento que não cobre a nota |
| `CFO-001` a `CFO-005` | erro/alerta | CFOP com âmbito ou sentido errado, devolução sem finalidade de devolução, NCM genérico, nota misturando entrada e saída |

## Estrutura

```
src/Nfe.Core     núcleo: chave de acesso, documentos, leitura do XML, regras e resumo do DANFE
src/Nfe.Api      API mínima que expõe leitura, validação e resumo
tests/Nfe.Tests  testes de unidade e de integração
```

O núcleo não depende de nada além da BCL. A leitura usa `XmlReader` com DTD
desabilitado e sem resolver externo, porque o arquivo quase sempre vem de fora.

## Como rodar

```bash
dotnet test                       # roda a suíte inteira
dotnet run --project src/Nfe.Api  # sobe a API em http://localhost:5000
```

Em desenvolvimento a API publica o Swagger em `/swagger`.

## Endpoints

| Método | Rota | O que faz |
| --- | --- | --- |
| `GET` | `/saude` | verificação de disponibilidade |
| `GET` | `/chaves/{chave}` | abre os 44 dígitos nos campos que os formam |
| `POST` | `/notas/leitura` | devolve o resumo da nota lida |
| `POST` | `/notas/validacao` | devolve os problemas encontrados |
| `POST` | `/notas/danfe` | devolve os campos já formatados para impressão |

Os três `POST` recebem `{ "xml": "<conteúdo do arquivo>" }`.

### Exemplo

```bash
curl -s http://localhost:5000/chaves/35260911222333000181550010000123451123456784
```

```json
{
  "chave": "35260911222333000181550010000123451123456784",
  "chaveFormatada": "3526 0911 2223 3300 0181 5500 1000 0123 4511 2345 6784",
  "uf": "SP",
  "ano": 2026,
  "mes": 9,
  "cnpj": "11222333000181",
  "modelo": 55,
  "serie": 1,
  "numero": 12345,
  "tipoDeEmissao": "Normal",
  "codigoNumerico": "12345678",
  "digitoVerificador": 4
}
```

## Uso como biblioteca

```csharp
var nota = new LeitorDeNfe().Ler(File.ReadAllText("nota.xml"));
var resultado = new Validador().Conferir(nota);

foreach (var problema in resultado.Erros)
{
    Console.WriteLine(problema); // [TOT-004] total/ICMSTot/vProd: Declarado 600,00, soma dos itens 554,80.
}
```

As regras são independentes e implementam `IRegra`, então dá para montar um
conjunto sob medida — uma conferência de recebimento costuma querer menos regras
que uma auditoria:

```csharp
var validador = new Validador([new RegraDaChave(), new RegraDosTotais()]);
```

## Limites conhecidos

- Não valida assinatura digital nem consulta a SEFAZ; a conferência é toda
  offline, sobre o conteúdo do arquivo.
- Não carrega a tabela completa de CFOP nem de NCM: o que é conferido é a
  estrutura do código e a coerência com o resto da nota.
- Cobre os grupos mais comuns de ICMS, IPI, PIS e COFINS. Partilha de ICMS,
  FCP e ISSQN ainda não são lidos.

## Licença

MIT.

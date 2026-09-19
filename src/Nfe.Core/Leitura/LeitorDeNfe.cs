using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using Nfe.Core.Chave;
using Nfe.Core.Comum;
using Nfe.Core.Fiscal;
using Nfe.Core.Modelo;

namespace Nfe.Core.Leitura;

/// <summary>
/// Lê o XML da NF-e no layout 4.00 e devolve o modelo tipado. Aceita tanto o
/// XML puro da nota quanto o procNFe, que é o arquivo distribuído já com o
/// protocolo de autorização.
/// </summary>
public sealed class LeitorDeNfe
{
    /// <summary>Espaço de nomes do portal fiscal, usado em todo o layout.</summary>
    public static readonly XNamespace Ns = "http://www.portalfiscal.inf.br/nfe";

    /// <summary>Lê o XML a partir de um texto.</summary>
    public NotaFiscal Ler(string xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
        {
            throw new ErroDeLeitura("xml", "Conteúdo vazio.");
        }

        XDocument documento;
        try
        {
            var configuracao = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                IgnoreWhitespace = true,
                IgnoreComments = true,
            };

            using var texto = new StringReader(xml);
            using var leitor = XmlReader.Create(texto, configuracao);
            documento = XDocument.Load(leitor);
        }
        catch (XmlException erro)
        {
            throw new ErroDeLeitura("xml", "XML malformado.", erro);
        }

        return Ler(documento);
    }

    /// <summary>Lê o XML a partir de um fluxo.</summary>
    public NotaFiscal Ler(Stream fluxo)
    {
        ArgumentNullException.ThrowIfNull(fluxo);

        using var leitor = new StreamReader(fluxo);
        return Ler(leitor.ReadToEnd());
    }

    /// <summary>Lê o XML já carregado em memória.</summary>
    public NotaFiscal Ler(XDocument documento)
    {
        ArgumentNullException.ThrowIfNull(documento);

        var raiz = documento.Root ?? throw new ErroDeLeitura("xml", "Documento sem elemento raiz.");
        var nfe = raiz.Name == Ns + "NFe" ? raiz : raiz.Element(Ns + "NFe");

        if (nfe is null)
        {
            throw new ErroDeLeitura(raiz.Name.LocalName, "Não encontrei o elemento NFe.");
        }

        var info = Obrigatorio(nfe, "infNFe", "NFe");
        var chave = LerChave(info);
        var ide = Obrigatorio(info, "ide", "infNFe");

        return new NotaFiscal
        {
            Chave = chave,
            Numero = Inteiro(ide, "nNF", "ide"),
            Serie = Inteiro(ide, "serie", "ide"),
            Modelo = Inteiro(ide, "mod", "ide"),
            Emissao = DataHora(ide, "dhEmi", "ide"),
            SaidaOuEntrada = DataHoraOpcional(ide, "dhSaiEnt"),
            NaturezaDaOperacao = Texto(ide, "natOp") ?? string.Empty,
            Operacao = (TipoDeOperacao)Inteiro(ide, "tpNF", "ide"),
            Finalidade = (FinalidadeDaEmissao)(InteiroOpcional(ide, "finNFe") ?? 1),
            Emitente = LerEmitente(Obrigatorio(info, "emit", "infNFe")),
            Destinatario = LerDestinatario(Obrigatorio(info, "dest", "infNFe")),
            Itens = LerItens(info),
            Totais = LerTotais(info),
            Transporte = LerTransporte(info.Element(Ns + "transp")),
            Pagamentos = LerPagamentos(info.Element(Ns + "pag")),
            InformacoesComplementares = Texto(info.Element(Ns + "infAdic"), "infCpl"),
            Protocolo = Texto(raiz.Element(Ns + "protNFe")?.Element(Ns + "infProt"), "nProt"),
        };
    }

    private static ChaveDeAcesso LerChave(XElement info)
    {
        var id = info.Attribute("Id")?.Value
            ?? throw new ErroDeLeitura("infNFe/@Id", "Atributo Id ausente.");

        var digitos = id.StartsWith("NFe", StringComparison.OrdinalIgnoreCase) ? id[3..] : id;

        return ChaveDeAcesso.TentarAnalisar(digitos, out var chave)
            ? chave
            : throw new ErroDeLeitura("infNFe/@Id", $"Chave de acesso inválida: '{id}'.");
    }

    private static Emitente LerEmitente(XElement emit)
    {
        var documento = Texto(emit, "CNPJ") ?? Texto(emit, "CPF");

        return new Emitente
        {
            Documento = Documento.TentarAnalisar(documento, out var doc)
                ? doc
                : throw new ErroDeLeitura("emit", $"Documento inválido: '{documento}'."),
            RazaoSocial = Texto(emit, "xNome") ?? string.Empty,
            NomeFantasia = Texto(emit, "xFant"),
            InscricaoEstadual = Texto(emit, "IE") ?? string.Empty,
            Endereco = LerEndereco(Obrigatorio(emit, "enderEmit", "emit")),
            Regime = (RegimeTributario)(InteiroOpcional(emit, "CRT") ?? 3),
        };
    }

    private static Destinatario LerDestinatario(XElement dest)
    {
        var documento = Texto(dest, "CNPJ") ?? Texto(dest, "CPF");

        return new Destinatario
        {
            Documento = Documento.TentarAnalisar(documento, out var doc)
                ? doc
                : throw new ErroDeLeitura("dest", $"Documento inválido: '{documento}'."),
            RazaoSocial = Texto(dest, "xNome") ?? string.Empty,
            InscricaoEstadual = Texto(dest, "IE"),
            Indicador = (IndicadorDeInscricao)(InteiroOpcional(dest, "indIEDest") ?? 9),
            Endereco = LerEndereco(Obrigatorio(dest, "enderDest", "dest")),
            Email = Texto(dest, "email"),
        };
    }

    private static Endereco LerEndereco(XElement ender) => new()
    {
        Logradouro = Texto(ender, "xLgr") ?? string.Empty,
        Numero = Texto(ender, "nro") ?? string.Empty,
        Complemento = Texto(ender, "xCpl"),
        Bairro = Texto(ender, "xBairro") ?? string.Empty,
        CodigoDoMunicipio = InteiroOpcional(ender, "cMun") ?? 0,
        Municipio = Texto(ender, "xMun") ?? string.Empty,
        Uf = Texto(ender, "UF") ?? string.Empty,
        Cep = Modulo11.SomenteDigitos(Texto(ender, "CEP")),
        Telefone = Texto(ender, "fone"),
    };

    private static List<Item> LerItens(XElement info)
    {
        var itens = new List<Item>();

        foreach (var det in info.Elements(Ns + "det"))
        {
            var numero = int.TryParse(det.Attribute("nItem")?.Value, out var n) ? n : itens.Count + 1;
            var prod = Obrigatorio(det, "prod", $"det[{numero}]");
            var caminho = $"det[{numero}]";

            var ncmTexto = Texto(prod, "NCM");
            var cfopTexto = Texto(prod, "CFOP");

            itens.Add(new Item
            {
                Numero = numero,
                Codigo = Texto(prod, "cProd") ?? string.Empty,
                Gtin = NormalizarGtin(Texto(prod, "cEAN")),
                Descricao = Texto(prod, "xProd") ?? string.Empty,
                Ncm = Ncm.TentarAnalisar(ncmTexto, out var ncm)
                    ? ncm
                    : throw new ErroDeLeitura($"{caminho}/prod/NCM", $"NCM inválido: '{ncmTexto}'."),
                Cfop = Cfop.TentarAnalisar(cfopTexto, out var cfop)
                    ? cfop
                    : throw new ErroDeLeitura($"{caminho}/prod/CFOP", $"CFOP inválido: '{cfopTexto}'."),
                Unidade = Texto(prod, "uCom") ?? string.Empty,
                Quantidade = LerDecimal(prod, "qCom"),
                ValorUnitario = LerDecimal(prod, "vUnCom"),
                ValorBruto = LerDecimal(prod, "vProd"),
                Desconto = LerDecimal(prod, "vDesc"),
                Frete = LerDecimal(prod, "vFrete"),
                Seguro = LerDecimal(prod, "vSeg"),
                OutrasDespesas = LerDecimal(prod, "vOutro"),
                CompoeOTotal = (InteiroOpcional(prod, "indTot") ?? 1) == 1,
                Tributos = LerTributos(det.Element(Ns + "imposto"), caminho),
            });
        }

        if (itens.Count == 0)
        {
            throw new ErroDeLeitura("infNFe", "A nota não tem nenhum item.");
        }

        return itens;
    }

    private static Tributos LerTributos(XElement? imposto, string caminho)
    {
        if (imposto is null)
        {
            return new Tributos();
        }

        return new Tributos
        {
            Icms = LerIcms(imposto.Element(Ns + "ICMS"), caminho),
            Ipi = LerTributoSimples(Modalidade(imposto, "IPI"), "vIPI"),
            Pis = LerTributoSimples(Modalidade(imposto, "PIS"), "vPIS") ?? new TributoSimples(),
            Cofins = LerTributoSimples(Modalidade(imposto, "COFINS"), "vCOFINS") ?? new TributoSimples(),
        };
    }

    // Cada tributo vem embrulhado em um grupo cujo nome indica a modalidade,
    // como IPITrib ou PISAliq. No IPI ainda aparece o cEnq solto antes do
    // grupo, então não dá para pegar simplesmente o primeiro filho.
    private static XElement? Modalidade(XElement imposto, string tributo)
        => imposto.Element(Ns + tributo)?.Elements().FirstOrDefault(filho => filho.HasElements);

    private static Icms LerIcms(XElement? icms, string caminho)
    {
        // O grupo ICMS envolve um único filho cujo nome indica a modalidade,
        // como ICMS00, ICMS60 ou ICMSSN102. O conteúdo é que interessa.
        var grupo = icms?.Elements().FirstOrDefault();
        if (grupo is null)
        {
            return new Icms();
        }

        var codigo = Texto(grupo, "CST") ?? Texto(grupo, "CSOSN");

        return new Icms
        {
            Origem = InteiroOpcional(grupo, "orig") ?? 0,
            Situacao = SituacaoTributaria.TentarAnalisar(codigo, out var situacao)
                ? situacao
                : throw new ErroDeLeitura($"{caminho}/imposto/ICMS", $"CST ou CSOSN inválido: '{codigo}'."),
            BaseDeCalculo = LerDecimal(grupo, "vBC"),
            Aliquota = LerDecimal(grupo, "pICMS"),
            Valor = LerDecimal(grupo, "vICMS"),
            BaseDeSubstituicao = LerDecimal(grupo, "vBCST"),
            ValorDeSubstituicao = LerDecimal(grupo, "vICMSST"),
            ValorDesonerado = LerDecimal(grupo, "vICMSDeson"),
        };
    }

    private static TributoSimples? LerTributoSimples(XElement? grupo, string campoDoValor)
    {
        if (grupo is null)
        {
            return null;
        }

        return new TributoSimples
        {
            Situacao = Texto(grupo, "CST") ?? string.Empty,
            BaseDeCalculo = LerDecimal(grupo, "vBC"),
            Aliquota = LerDecimal(grupo, "pPIS") + LerDecimal(grupo, "pCOFINS") + LerDecimal(grupo, "pIPI"),
            Valor = LerDecimal(grupo, campoDoValor),
        };
    }

    private static Totais LerTotais(XElement info)
    {
        var total = info.Element(Ns + "total")?.Element(Ns + "ICMSTot");
        if (total is null)
        {
            throw new ErroDeLeitura("infNFe/total", "Grupo ICMSTot ausente.");
        }

        return new Totais
        {
            BaseDeIcms = LerDecimal(total, "vBC"),
            Icms = LerDecimal(total, "vICMS"),
            BaseDeSubstituicao = LerDecimal(total, "vBCST"),
            IcmsDeSubstituicao = LerDecimal(total, "vST"),
            Produtos = LerDecimal(total, "vProd"),
            Frete = LerDecimal(total, "vFrete"),
            Seguro = LerDecimal(total, "vSeg"),
            Desconto = LerDecimal(total, "vDesc"),
            OutrasDespesas = LerDecimal(total, "vOutro"),
            Ipi = LerDecimal(total, "vIPI"),
            Pis = LerDecimal(total, "vPIS"),
            Cofins = LerDecimal(total, "vCOFINS"),
            NotaFiscal = LerDecimal(total, "vNF"),
        };
    }

    private static Transporte LerTransporte(XElement? transp)
    {
        if (transp is null)
        {
            return new Transporte();
        }

        var transporta = transp.Element(Ns + "transporta");
        var documento = Texto(transporta, "CNPJ") ?? Texto(transporta, "CPF");
        var veiculo = transp.Element(Ns + "veicTransp");

        return new Transporte
        {
            Responsavel = (ResponsavelPeloFrete)(InteiroOpcional(transp, "modFrete") ?? 9),
            Transportadora = Documento.TentarAnalisar(documento, out var doc) ? doc : null,
            RazaoSocial = Texto(transporta, "xNome"),
            Placa = Texto(veiculo, "placa"),
            UfDoVeiculo = Texto(veiculo, "UF"),
            Volumes = transp.Elements(Ns + "vol").Select(vol => new Volume
            {
                Quantidade = InteiroOpcional(vol, "qVol") ?? 0,
                Especie = Texto(vol, "esp"),
                Marca = Texto(vol, "marca"),
                PesoLiquido = LerDecimal(vol, "pesoL"),
                PesoBruto = LerDecimal(vol, "pesoB"),
            }).ToList(),
        };
    }

    private static Pagamentos LerPagamentos(XElement? pag)
    {
        if (pag is null)
        {
            return new Pagamentos();
        }

        return new Pagamentos
        {
            Troco = LerDecimal(pag, "vTroco"),
            Formas = pag.Elements(Ns + "detPag").Select(det => new Pagamento
            {
                Meio = (MeioDePagamento)(InteiroOpcional(det, "tPag") ?? 99),
                Valor = LerDecimal(det, "vPag"),
                APrazo = (InteiroOpcional(det, "indPag") ?? 0) == 1,
                Bandeira = Texto(det.Element(Ns + "card"), "tBand"),
            }).ToList(),
        };
    }

    private static string? NormalizarGtin(string? valor)
        => string.IsNullOrWhiteSpace(valor) || valor.Equals("SEM GTIN", StringComparison.OrdinalIgnoreCase)
            ? null
            : valor.Trim();

    private static XElement Obrigatorio(XElement pai, string nome, string caminho)
        => pai.Element(Ns + nome) ?? throw new ErroDeLeitura($"{caminho}/{nome}", "Elemento obrigatório ausente.");

    private static string? Texto(XElement? pai, string nome)
    {
        var valor = pai?.Element(Ns + nome)?.Value.Trim();
        return string.IsNullOrEmpty(valor) ? null : valor;
    }

    private static int Inteiro(XElement pai, string nome, string caminho)
        => InteiroOpcional(pai, nome)
           ?? throw new ErroDeLeitura($"{caminho}/{nome}", "Campo numérico obrigatório ausente ou inválido.");

    private static int? InteiroOpcional(XElement? pai, string nome)
        => int.TryParse(Texto(pai, nome), NumberStyles.Integer, CultureInfo.InvariantCulture, out var valor)
            ? valor
            : null;

    private static decimal LerDecimal(XElement? pai, string nome) => Dinheiro.Ler(Texto(pai, nome));

    private static DateTimeOffset DataHora(XElement pai, string nome, string caminho)
        => DataHoraOpcional(pai, nome)
           ?? throw new ErroDeLeitura($"{caminho}/{nome}", "Data obrigatória ausente ou inválida.");

    private static DateTimeOffset? DataHoraOpcional(XElement? pai, string nome)
        => DateTimeOffset.TryParse(
            Texto(pai, nome),
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var valor)
            ? valor
            : null;
}

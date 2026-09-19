namespace Nfe.Tests;

/// <summary>
/// XML de apoio aos testes. É uma nota de saída dentro de São Paulo, com dois
/// itens, totais que fecham e emitente no regime normal, para servir de base
/// tanto para os testes de leitura quanto para os de validação, que a estragam
/// de propósito em cada cenário.
/// </summary>
public static class Exemplos
{
    /// <summary>Chave de acesso usada no XML de exemplo.</summary>
    public const string Chave = "35260911222333000181550010000123451123456784";

    /// <summary>CNPJ do emitente do XML de exemplo.</summary>
    public const string CnpjDoEmitente = "11222333000181";

    /// <summary>CPF do destinatário do XML de exemplo.</summary>
    public const string CpfDoDestinatario = "52998224725";

    /// <summary>Nota completa, já com o protocolo de autorização.</summary>
    public static string NotaCompleta => $"""
        <?xml version="1.0" encoding="UTF-8"?>
        <nfeProc xmlns="http://www.portalfiscal.inf.br/nfe" versao="4.00">
          <NFe>
            <infNFe Id="NFe{Chave}" versao="4.00">
              <ide>
                <cUF>35</cUF>
                <cNF>12345678</cNF>
                <natOp>VENDA DE MERCADORIA</natOp>
                <mod>55</mod>
                <serie>1</serie>
                <nNF>12345</nNF>
                <dhEmi>2026-09-15T10:30:00-03:00</dhEmi>
                <dhSaiEnt>2026-09-15T14:00:00-03:00</dhSaiEnt>
                <tpNF>1</tpNF>
                <idDest>1</idDest>
                <tpEmis>1</tpEmis>
                <finNFe>1</finNFe>
              </ide>
              <emit>
                <CNPJ>{CnpjDoEmitente}</CNPJ>
                <xNome>COMERCIO DE FERRAGENS CONDE LTDA</xNome>
                <xFant>FERRAGENS CONDE</xFant>
                <enderEmit>
                  <xLgr>RUA DAS OFICINAS</xLgr>
                  <nro>1200</nro>
                  <xBairro>BRAS</xBairro>
                  <cMun>3550308</cMun>
                  <xMun>SAO PAULO</xMun>
                  <UF>SP</UF>
                  <CEP>03043000</CEP>
                  <fone>1133334444</fone>
                </enderEmit>
                <IE>112233445566</IE>
                <CRT>3</CRT>
              </emit>
              <dest>
                <CPF>{CpfDoDestinatario}</CPF>
                <xNome>MARIA DE SOUZA</xNome>
                <enderDest>
                  <xLgr>AVENIDA PAULISTA</xLgr>
                  <nro>900</nro>
                  <xCpl>APTO 51</xCpl>
                  <xBairro>BELA VISTA</xBairro>
                  <cMun>3550308</cMun>
                  <xMun>SAO PAULO</xMun>
                  <UF>SP</UF>
                  <CEP>01310100</CEP>
                </enderDest>
                <indIEDest>9</indIEDest>
                <email>maria@exemplo.com.br</email>
              </dest>
              <det nItem="1">
                <prod>
                  <cProd>PAR-001</cProd>
                  <cEAN>SEM GTIN</cEAN>
                  <xProd>PARAFUSO SEXTAVADO 10MM</xProd>
                  <NCM>73181500</NCM>
                  <CFOP>5102</CFOP>
                  <uCom>CX</uCom>
                  <qCom>10.0000</qCom>
                  <vUnCom>25.5000</vUnCom>
                  <vProd>255.00</vProd>
                  <indTot>1</indTot>
                </prod>
                <imposto>
                  <ICMS>
                    <ICMS00>
                      <orig>0</orig>
                      <CST>00</CST>
                      <modBC>3</modBC>
                      <vBC>255.00</vBC>
                      <pICMS>18.00</pICMS>
                      <vICMS>45.90</vICMS>
                    </ICMS00>
                  </ICMS>
                  <IPI>
                    <cEnq>999</cEnq>
                    <IPITrib>
                      <CST>50</CST>
                      <vBC>255.00</vBC>
                      <pIPI>5.00</pIPI>
                      <vIPI>12.75</vIPI>
                    </IPITrib>
                  </IPI>
                  <PIS>
                    <PISAliq>
                      <CST>01</CST>
                      <vBC>255.00</vBC>
                      <pPIS>1.65</pPIS>
                      <vPIS>4.21</vPIS>
                    </PISAliq>
                  </PIS>
                  <COFINS>
                    <COFINSAliq>
                      <CST>01</CST>
                      <vBC>255.00</vBC>
                      <pCOFINS>7.60</pCOFINS>
                      <vCOFINS>19.38</vCOFINS>
                    </COFINSAliq>
                  </COFINS>
                </imposto>
              </det>
              <det nItem="2">
                <prod>
                  <cProd>FUR-014</cProd>
                  <cEAN>7891234567895</cEAN>
                  <xProd>FURADEIRA DE IMPACTO 650W</xProd>
                  <NCM>84672100</NCM>
                  <CFOP>5102</CFOP>
                  <uCom>UN</uCom>
                  <qCom>2.0000</qCom>
                  <vUnCom>149.9000</vUnCom>
                  <vProd>299.80</vProd>
                  <vDesc>9.80</vDesc>
                  <indTot>1</indTot>
                </prod>
                <imposto>
                  <ICMS>
                    <ICMS00>
                      <orig>0</orig>
                      <CST>00</CST>
                      <modBC>3</modBC>
                      <vBC>290.00</vBC>
                      <pICMS>18.00</pICMS>
                      <vICMS>52.20</vICMS>
                    </ICMS00>
                  </ICMS>
                  <PIS>
                    <PISAliq>
                      <CST>01</CST>
                      <vBC>290.00</vBC>
                      <pPIS>1.65</pPIS>
                      <vPIS>4.79</vPIS>
                    </PISAliq>
                  </PIS>
                  <COFINS>
                    <COFINSAliq>
                      <CST>01</CST>
                      <vBC>290.00</vBC>
                      <pCOFINS>7.60</pCOFINS>
                      <vCOFINS>22.04</vCOFINS>
                    </COFINSAliq>
                  </COFINS>
                </imposto>
              </det>
              <total>
                <ICMSTot>
                  <vBC>545.00</vBC>
                  <vICMS>98.10</vICMS>
                  <vBCST>0.00</vBCST>
                  <vST>0.00</vST>
                  <vProd>554.80</vProd>
                  <vFrete>0.00</vFrete>
                  <vSeg>0.00</vSeg>
                  <vDesc>9.80</vDesc>
                  <vIPI>12.75</vIPI>
                  <vPIS>9.00</vPIS>
                  <vCOFINS>41.42</vCOFINS>
                  <vOutro>0.00</vOutro>
                  <vNF>557.75</vNF>
                </ICMSTot>
              </total>
              <transp>
                <modFrete>1</modFrete>
                <transporta>
                  <CNPJ>11222333000181</CNPJ>
                  <xNome>TRANSPORTES CONDE</xNome>
                </transporta>
                <veicTransp>
                  <placa>ABC1D23</placa>
                  <UF>SP</UF>
                </veicTransp>
                <vol>
                  <qVol>3</qVol>
                  <esp>CAIXA</esp>
                  <marca>CONDE</marca>
                  <pesoL>18.500</pesoL>
                  <pesoB>20.000</pesoB>
                </vol>
              </transp>
              <pag>
                <detPag>
                  <indPag>1</indPag>
                  <tPag>15</tPag>
                  <vPag>557.75</vPag>
                </detPag>
              </pag>
              <infAdic>
                <infCpl>PEDIDO 4471. PAGAMENTO EM 30 DIAS.</infCpl>
              </infAdic>
            </infNFe>
          </NFe>
          <protNFe versao="4.00">
            <infProt>
              <chNFe>{Chave}</chNFe>
              <dhRecbto>2026-09-15T10:31:12-03:00</dhRecbto>
              <nProt>135260011223344</nProt>
              <cStat>100</cStat>
              <xMotivo>Autorizado o uso da NF-e</xMotivo>
            </infProt>
          </protNFe>
        </nfeProc>
        """;

    /// <summary>Devolve o XML de exemplo com um trecho trocado por outro.</summary>
    public static string NotaCom(string original, string substituto)
    {
        var xml = NotaCompleta;

        if (!xml.Contains(original, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Trecho não encontrado no XML de exemplo: '{original}'.");
        }

        return xml.Replace(original, substituto, StringComparison.Ordinal);
    }

    /// <summary>
    /// Troca somente a primeira ocorrência do trecho, para mexer em um item sem
    /// afetar o outro.
    /// </summary>
    public static string NotaComPrimeiro(string original, string substituto)
    {
        var xml = NotaCompleta;
        var posicao = xml.IndexOf(original, StringComparison.Ordinal);

        if (posicao < 0)
        {
            throw new InvalidOperationException($"Trecho não encontrado no XML de exemplo: '{original}'.");
        }

        return string.Concat(xml.AsSpan(0, posicao), substituto, xml.AsSpan(posicao + original.Length));
    }
}

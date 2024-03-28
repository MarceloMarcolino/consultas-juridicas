using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Authenticators;
using Newtonsoft.Json;
using System.Web.UI.WebControls.WebParts;
using Newtonsoft.Json.Linq;
using System.Text.Json.Nodes;

namespace CasoDeUso
{
    class Pesquisa
    {
        public string NomeRelacional { get; set; }
        public string Token { get; set; }
        public string TokenJWT { get; set; }
        public int[] CodsPesquisa { get; set; }
        public int CodPesquisa { get; set; }
        public int Instancia { get; set; }
        public string[] ListaNumProcessos { get; set; }
        public bool EntregarPublicacoes { get; set; }
        public bool EntregarDocIniciais { get; set; }
        public string[] Abrangencias { get; set; }
        public int CodStatus { get; set; }
        public string Descricao { get; set; }
        public int CodStatusMovimento { get; set; }
        public string DescricaoMovimento { get; set; }
        public int CodStatusPublicacao { get; set; }
        public string DescricaoPublicacao { get; set; }
        public int CodStatusDocIniciais { get; set; }
        public string DescricaoDocIniciais { get; set; }
        public int CodProcesso { get; set; }
        public string NumeroProcessoFormatado { get; set; }
        public string NumeroNaoCnj { get; set; }
        public double ValorCausa { get; set; }
        public List<string> Assuntos { get; set; }
        public CapaProcesso CapaProcessoPesquisa { get; set; }
        public List<Parte> Partes { get; set; }
        public List<Advogado> Advogados {  get; set; }
        public bool DadosProcessoEncontrado { get; set; }
    }

    class CapaProcesso
    {
        public string SiglaTribunal { get; set; }
        public string Relator { get; set; }
        public DateTime? DataDistribuicao { get; set; }
        public DateTime? DataAutuacao { get; set; }
        public string OrgaoJulgador { get; set; }
        public string ClasseCnj { get; set; }
        public string Segmento { get; set; }
        public string Uf { get; set; }
        public string UnidadeOrigem { get; set; }
        public string StatusProcesso { get; set; }
        public DateTime? DataArquivamento { get; set; }
        public string RamoDireito { get; set; }
        public bool ESegredoJustica { get; set; }
    }

    class Sujeito
    {
        public int CodParte { get; set; }
        public string Polo { get; set; }
        public string Nome { get; set; }
    }

    class Parte : Sujeito
    {
        public string Tipo { get; set; }
        public string Cnpj { get; set; }
        public string Cpf { get; set; }
    }

    class Advogado : Sujeito
    {
        public string UfOab { get; set; }
        public int NumeroOAB { get; set; }
    }

    //Nome da empresa que detém a API
    internal class SolucionareLJ
    {
        //Execução principal do programa
        static void Main(string[] args)
        {
            Pesquisa pesquisa = new();

            //URL providenciada para acesso à API
            var baseURL = "http://online.solucionarelj.com.br:9090/WebApiDiscoveryFullV2/api/DiscoveryFull/";

            //Nome do banco de dados relacional
            Console.WriteLine("Digite o nome do banco de dados relacional:");
            pesquisa.NomeRelacional = Console.ReadLine();

            //Token de acesso inicial
            Console.WriteLine("Digite o token de acesso inicial:");
            pesquisa.Token = Console.ReadLine();

            AutenticaAPI(baseURL, pesquisa);

            Console.WriteLine("Digite o 1º código da pesquisa:");
            pesquisa.CodPesquisa = Convert.ToInt32(Console.ReadLine());
            pesquisa.CodsPesquisa = [pesquisa.CodPesquisa];

            Console.WriteLine("Digite os outros códigos de pesquisa separados por vírgula:");
            string input = Console.ReadLine();
            string[] values = input.Split(',');
            List<int> tempList = new(pesquisa.CodsPesquisa);
            foreach (string value in values)
            {
                if (int.TryParse(value, out int parsedValue))
                {
                    tempList.Add(parsedValue);
                }
                else
                {
                    Console.WriteLine($"Invalid input: '{value}'. Skipping...");
                }
            }
            pesquisa.CodsPesquisa = [.. tempList];

            //Instância em que deseja realizar a consulta conforme descrito abaixo
            Console.WriteLine("Digite a instância:");
            pesquisa.Instancia = Convert.ToInt32(Console.ReadLine());

            //Números de processo que serão utilizados como filtro na pesquisa dos processos.
            Console.WriteLine("Digite os números de processos separados por vírgula:");
            pesquisa.ListaNumProcessos = Console.ReadLine().Split(',');

            //Para consultas de publicações enviar valor verdadeiro, caso não enviado esse parâmetro, não serão retornadas publicações.
            Console.WriteLine("Deseja retornar publicações? (true/false):");
            pesquisa.EntregarPublicacoes = Convert.ToBoolean(Console.ReadLine());

            //Para busca de documentos iniciais enviar valor verdadeiro, caso não enviado esse parâmetro, não serão retornados documentos.
            Console.WriteLine("Deseja retornar documentos iniciais? (true/false):");
            pesquisa.EntregarDocIniciais = Convert.ToBoolean(Console.ReadLine());

            Console.WriteLine("Digite as abrangências separadas por vírgula:");
            pesquisa.Abrangencias = Console.ReadLine().Split(',');

            //Retorna o status das pesquisas cadastradas anteriormente.
            BuscaStatusPesquisa(baseURL, pesquisa, pesquisa.CodsPesquisa);

            //Este método permite o cadastro de uma pesquisa por números de processo.
            InserirPesquisaPorNumeroProcesso(baseURL, pesquisa);

            //Este método retorna todos os dados e processos encontrados na pesquisa solicitada.
            BuscaDadosResultadoPesquisa(baseURL, pesquisa);
        }

        static void AutenticaAPI(string baseURL, Pesquisa pesquisa)
        {
            //Nome do método, ou endpoint, na API
            var endpoint = "autenticaAPI";

            //Criação de Client Rest a partir da URL providenciada
            var client = new RestClient(baseURL);

            //Request de método POST
            var request = new RestRequest(endpoint, Method.Post);

            //Adiciona parâmetros de body ao JSON
            request.AddJsonBody(new
            {
                pesquisa.NomeRelacional,
                pesquisa.Token
            });

            try
            {
                var response = client.Execute(request);
                pesquisa.TokenJWT = JsonConvert.DeserializeObject<string>(response.Content);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ocorreu um erro: " + ex.Message);
                throw;
            }
            finally
            {
                Console.WriteLine("Token JWT: " + pesquisa.TokenJWT);
            }
        }

        static void BuscaStatusPesquisa(string baseURL, Pesquisa pesquisa, int[] codPesquisa)
        {
            // Endpoint da API
            var endpoint = "buscaStatusPesquisa";

            //Cria o cliente RestSharp
            var client = new RestClient(baseURL);

            //Cria o objeto da chamada
            var request = new RestRequest(endpoint, Method.Post);

            //Adiciona token JWT ao header da chamada
            request.AddHeader("Authorization", pesquisa.TokenJWT);

            //Adiciona body JSON aos parâmetros
            request.AddJsonBody(new
            {
                codPesquisa
            });

            _ = new RestResponse();

            try
            {
                //Executa a chamada
                RestResponse response = client.Execute(request);
                JArray jsonArray = JArray.Parse(response.Content);
                foreach (JObject obj in jsonArray.Cast<JObject>())
                {
                    pesquisa.CodPesquisa = (int)obj["codPesquisa"];
                    pesquisa.CodStatus = (int)obj["codStatus"];
                    pesquisa.Descricao = (string)obj["descricao"];
                    pesquisa.CodStatusMovimento = (int)obj["codStatusMovimento"];
                    pesquisa.DescricaoMovimento = (string)obj["descricaoMovimento"];
                    pesquisa.CodStatusPublicacao = (int)obj["codStatusPublicacao"];
                    pesquisa.DescricaoPublicacao = (string)obj["descricaoPublicacao"];
                    pesquisa.CodStatusDocIniciais = (int)obj["codStatusDocIniciais"];
                    pesquisa.DescricaoDocIniciais = (string)obj["descricaoDocIniciais"];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ocorreu um erro: " + ex.Message);
            }
            finally
            {
                Console.WriteLine("Código de Pesquisa: " + pesquisa.CodPesquisa);
                Console.WriteLine("- Código de Status: " + pesquisa.CodStatus);
                Console.WriteLine("- Descrição: " + pesquisa.Descricao);
                Console.WriteLine("- Código de Status de Movimento: " + pesquisa.CodStatusMovimento);
                Console.WriteLine("- Descrição de Movimento: " + pesquisa.DescricaoMovimento);
                Console.WriteLine("- Código de Status de Publicação: " + pesquisa.CodStatusPublicacao);
                Console.WriteLine("- Descrição de Publicação: " + pesquisa.DescricaoPublicacao);
                Console.WriteLine("- Código de Status dos Documentos Iniciais: " + pesquisa.CodStatusDocIniciais);
                Console.WriteLine("- Descrição dos Documentos Iniciais: " + pesquisa.DescricaoDocIniciais);
            }
        }

        //cadastraPesquisa_NumProcessos
        static void InserirPesquisaPorNumeroProcesso(string baseURL, Pesquisa pesquisa) { 

            // Endpoint da API
            var endpoint = "cadastraPesquisa_NumProcessos";

            //Cria o cliente RestSharp
            var client = new RestClient(baseURL);

            //Cria o objeto da chamada
            var request = new RestRequest(endpoint, Method.Post);

            //Adiciona token JWT ao header da chamada
            request.AddHeader("Authorization", pesquisa.TokenJWT);

            //Adiciona body JSON aos parâmetros
            request.AddJsonBody(new
            {
                pesquisa.Instancia,
                pesquisa.ListaNumProcessos,
                pesquisa.EntregarPublicacoes,
                pesquisa.EntregarDocIniciais,
                pesquisa.Abrangencias
            });

            RestResponse response = new();

            try
            {
                //Executa a chamada
                response = client.Execute(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ocorreu um erro: " + ex.Message);
            }
            finally
            {
                if (response.Content == "Foi cadastrado uma requisição com os mesmos parâmetros recentemente.")
                {
                    Console.WriteLine(response.Content);
                } else
                {
                    pesquisa.CodPesquisa = JsonConvert.DeserializeObject<int>(response.Content);
                    Console.WriteLine("Código de Pesquisa: " + pesquisa.CodPesquisa);
                }
            }
        }

        static void BuscaDadosResultadoPesquisa(string baseURL, Pesquisa pesquisa)
        {
            // Endpoint da API
            var endpoint = "buscaDadosResultadoPesquisa";

            //Cria o cliente RestSharp
            var client = new RestClient(baseURL);

            //Cria o objeto da chamada
            var request = new RestRequest(endpoint, Method.Post);

            //Adiciona token JWT ao header da chamada
            request.AddHeader("Authorization", pesquisa.TokenJWT);

            //Adiciona body JSON aos parâmetros
            request.AddJsonBody(new
            {
                pesquisa.CodPesquisa
            });

            _ = new RestResponse();

            try
            {
                //Executa a chamada
                RestResponse response = client.Execute(request);
                JArray jsonArray = JArray.Parse(response.Content);
                foreach (JObject obj in jsonArray.Cast<JObject>())
                {
                    pesquisa.CodProcesso = (int)obj["codProcesso"];
                    pesquisa.NumeroProcessoFormatado = (string)obj["numeroProcessoFormatado"];
                    pesquisa.NumeroNaoCnj = (string)obj["numeroNaoCnj"];
                    pesquisa.Instancia = (int)obj["instancia"];
                    pesquisa.ValorCausa = (double)obj["valorCausa"];
                    pesquisa.Assuntos = obj["assuntos"].ToObject<List<string>>();
                    pesquisa.CapaProcessoPesquisa = JsonConvert.DeserializeObject<CapaProcesso>(obj["capaProcesso"].ToString());
                    pesquisa.Partes = obj["partes"].ToObject<List<Parte>>();
                    pesquisa.Advogados = obj["advogados"].ToObject<List<Advogado>>();
                    pesquisa.DadosProcessoEncontrado = (bool)obj["dadosProcessoEncontrado"];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ocorreu um erro: " + ex.Message);
            }
            finally
            {
                Console.WriteLine("Código do Processo: " + pesquisa.CodProcesso);
                Console.WriteLine("Número do Processo Formatado: " + pesquisa.NumeroProcessoFormatado);
                Console.WriteLine("Número Não do CNJ: " + pesquisa.NumeroNaoCnj);
                Console.WriteLine("Instância: " + pesquisa.Instancia);
                Console.WriteLine("Valor da Causa: " + pesquisa.ValorCausa);
                Console.WriteLine("Assuntos: ");
                foreach (var assunto in pesquisa.Assuntos)
                {
                    Console.WriteLine("- " + assunto);
                }
                Console.WriteLine("Capa do Processo: ");
                Console.WriteLine("- Sigla do Tribunal: " + pesquisa.CapaProcessoPesquisa.SiglaTribunal);
                Console.WriteLine("- Relator: " + pesquisa.CapaProcessoPesquisa.Relator);
                Console.WriteLine("- Data de Distribuição: " + pesquisa.CapaProcessoPesquisa.DataDistribuicao);
                Console.WriteLine("- Data de Autuação: " + pesquisa.CapaProcessoPesquisa.DataAutuacao);
                Console.WriteLine("- Órgão Julgador: " + pesquisa.CapaProcessoPesquisa.OrgaoJulgador);
                Console.WriteLine("- Classe do CNJ: " + pesquisa.CapaProcessoPesquisa.ClasseCnj);
                Console.WriteLine("- Segmento: " + pesquisa.CapaProcessoPesquisa.Segmento);
                Console.WriteLine("- Unidade Federativa: " + pesquisa.CapaProcessoPesquisa.Uf);
                Console.WriteLine("- Unidade de Origem: " + pesquisa.CapaProcessoPesquisa.UnidadeOrigem);
                Console.WriteLine("- Status do Processo: " + pesquisa.CapaProcessoPesquisa.StatusProcesso);
                Console.WriteLine("- Data de Arquivamento: " + pesquisa.CapaProcessoPesquisa.DataArquivamento);
                Console.WriteLine("- Ramo do Direito: " + pesquisa.CapaProcessoPesquisa.RamoDireito);
                Console.WriteLine("- É Segredo de Justiça?: " + pesquisa.CapaProcessoPesquisa.ESegredoJustica);
                Console.WriteLine("Partes: ");
                int i = 1;
                foreach (var parte in pesquisa.Partes)
                {
                    Console.WriteLine("- " + i + "ª parte:");
                    Console.WriteLine("+ Código da Parte: " + parte.CodParte);
                    Console.WriteLine("+ Tipo: " + parte.Tipo);
                    Console.WriteLine("+ Polo: " + parte.Polo);
                    Console.WriteLine("+ Nome: " + parte.Nome);
                    Console.WriteLine("+ CNPJ: " + parte.Cnpj);
                    Console.WriteLine("+ CPF: " + parte.Cpf);
                    i++;
                }
                i = 1;
                foreach (var advogado in pesquisa.Advogados)
                {
                    Console.WriteLine("- " + i + "º advogado:");
                    Console.WriteLine("+ Código da Parte: " + advogado.CodParte);
                    Console.WriteLine("+ Nome: " + advogado.Nome);
                    Console.WriteLine("+ Unidade Federativa da OAB: " + advogado.UfOab);
                    Console.WriteLine("+ Número da OAB: " + advogado.NumeroOAB);
                    Console.WriteLine("+ Polo: " + advogado.Polo);
                    i++;
                }
                Console.WriteLine("Dados do Processo Encontrado: " + pesquisa.DadosProcessoEncontrado);
                Console.ReadKey();
            }
        }


    }
}

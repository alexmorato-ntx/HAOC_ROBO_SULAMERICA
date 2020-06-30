using HAOC_ROBO_SULAMERICA.Domain;
using HAOC_ROBO_SULAMERICA.Helpers;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using DapperExtensions;
using Oracle.ManagedDataAccess.Client;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.IO;
using System.Reflection;

namespace HAOC_ROBO_SULAMERICA
{
    class Program
    {
        static FileCreator fileCreator = new FileCreator();
        static List<RequestAtendimentoURI> ListaURIs = new List<RequestAtendimentoURI>();
        static List<RequestAtendimentoURI> ListaURIsErro = new List<RequestAtendimentoURI>();

        static void Main(string[] args)
        {
            //Log Data/Hora > Versão do Robô
            fileCreator.WriteTxtLog(String.Format("[{0}] - {1}", DateTime.Now.ToString("dd/MM/yyyy 'às' HH:mm"), "Versão: " + Assembly.GetEntryAssembly().GetName().Version.ToString()));
            Console.WriteLine(String.Format("[{0}] - {1}", DateTime.Now.ToString("dd/MM/yyyy 'às' HH:mm"), "Versão: " + Assembly.GetEntryAssembly().GetName().Version.ToString()));


            if (args == null || args.Length < 2)
            {

                Console.WriteLine("É necessário passar dois parametros de datas, no formato (yyyy-MM-dd HH:mm)!");
                Console.ReadLine();

            }
            else
            {

                string inputA = args[0];
                DateTime dateTimeA;
                bool fValidaA = false;
                if (DateTime.TryParse(inputA, out dateTimeA))
                {
                    fValidaA = true;
                    Console.WriteLine(dateTimeA);
                }


                string inputB = args[1];
                DateTime dateTimeB;
                bool fValidaB = false;
                if (DateTime.TryParse(inputB, out dateTimeB))
                {
                    fValidaB = true;
                    Console.WriteLine(dateTimeB);
                }




                if (fValidaA && fValidaB)
                {
                    //Log Data/Hora > Início de execução do ROBÔ
                    fileCreator.WriteTxtLog(String.Format("[{0}] - {1}", DateTime.Now.ToString("dd/MM/yyyy 'às' HH:mm"), "INICIO ROBÔ CONSULTA DE ATENDIMENTOS."));
                    Console.WriteLine(String.Format("[{0}] - {1}", DateTime.Now.ToString("dd/MM/yyyy 'às' HH:mm"), "INICIO ROBÔ CONSULTA DE ATENDIMENTOS."));

                    ConsultaAtendimento_Robo1(dateTimeA.ToString("yyyy-MM-dd HH:mm:ss"), dateTimeB.ToString("yyyy-MM-dd HH:mm:ss")).Wait();
                    AtendimentoUri_Robo3(ListaURIs, ListaURIsErro).Wait();


                }
                else
                {
                    Console.WriteLine("Parametro com datas inválidas!");
                    Console.ReadLine();
                }

            }


        }

        static async Task<IEnumerable<ResponseAtendimento>> ConsultaAtendimento_Robo1(string dtInicio, string dtFim)
        {

            try
            {
                string pathPDFSave = System.Configuration.ConfigurationManager.AppSettings["pathPDFSave"];

                string sUrlConnection = System.Configuration.ConfigurationManager.AppSettings["sUrlConnection"];

                string sTiposDocumento = System.Configuration.ConfigurationManager.AppSettings["TiposDocumento"];

                string bGeraLogArquivos = System.Configuration.ConfigurationManager.AppSettings["GeraLogArquivos"];

                var spltArr = sTiposDocumento.Split(';');
                List<HelperTiposDocumentos> listaTipoAtendimentoDocumentos = new List<HelperTiposDocumentos>();
                if (spltArr.Count() > 0)
                {
                    foreach (var item in spltArr)
                    {

                        var spltItem = item.Split(':');

                        HelperTiposDocumentos obj = new HelperTiposDocumentos()
                        {
                            TipoOperadora = "136", //spltItem[0],
                            TipoAtendimento = spltItem[1],
                            TipoDocumento = spltItem[2]
                        };

                        listaTipoAtendimentoDocumentos.Add(obj);
                    }
                }






                WebServices services;
                services = new WebServices();
                IEnumerable<ResponseAtendimento> lista;

                IRestResponse response = await services.PostFormDataAtendimentoRestAsync<IEnumerable<ResponseAtendimento>>("/atendimento.php", dtInicio, dtFim);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {

                    Console.WriteLine(response.Content);

                    lista = JsonConvert.DeserializeObject<IEnumerable<ResponseAtendimento>>(response.Content);

                    var retorno = new ResponseJson();
                    retorno.statusRetorno = response.StatusCode.ToString();
                    retorno.objetoRetorno = lista;
                    retorno.possuiDados = true;

                    //Log Data/Hora > Qtdade de Atendimentos encontrados
                    fileCreator.WriteTxtLog(String.Format("[{0}] - {1}", DateTime.Now.ToString("dd/MM/yyyy 'às' HH:mm"), "Foram encontrados " + lista.Count() + " atendimentos"));
                    Console.WriteLine(String.Format("[{0}] - {1}", DateTime.Now.ToString("dd/MM/yyyy 'às' HH:mm"), "Foram encontrados " + lista.Count() + " atendimentos"));





                    using (var dbConn = new OracleConnection(sUrlConnection))
                    {

                        dbConn.Open();



                        foreach (var item in lista)
                        {
                            Console.WriteLine(String.Format("[{0}] - {1}", DateTime.Now.ToString("dd/MM/yyyy 'às' HH:mm"), "MedicalCareName: " + item.MedicalCareName));
                            Console.WriteLine(String.Format("[{0}] - {1}", DateTime.Now.ToString("dd/MM/yyyy 'às' HH:mm"), "MedicalCareName: " + item.MedicalCareCode));
                            //Console.WriteLine(String.Format("[{0}] - {1}", DateTime.Now.ToString("dd/MM/yyyy 'às' HH:mm"), "MedicalCareName: " + item.DeliveryDate));


                            var strQuery = @"select U.URS_PATHFISICO ,  P.PAS_Registro , DD.DIV_CODIGOREDUZIDO   , P.PAS_CODIGOPASSAGEM , D.DOC_NOMEARQUIVO , F.FMT_EXTENSAO from GEDPassagens P
                                             inner join GEDDOCUMENTOS D on D.DOC_IDPASSAGEM = P.PAS_IDPASSAGEM
                                             inner join GEDSUBTIPOSDOCUMENTOS S on S.STD_IDSUBTIPOSDOCUMENTOS = D.DOC_IDSUBTIPODOCUMENTO
                                             inner join GEDTIPOSDOCUMENTOS T on T.TPD_IDTIPODOCUMENTO = S.STD_IDTIPOSDOCUMENTOS
                                             inner join GEDDIVISOES DD on DD.DIV_IDDIVISAO = T.TPD_IDDIVISAO
                                             inner join GEDFORMATOS F on F.FMT_IDFORMATO = D.DOC_IDFORMATO
                                             left join GEDURLSTORAGE U on U.URS_IDURLSTORAGE = D.DOC_IDURLSTORAGE
                                             where PAS_CODIGOPASSAGEM = " + item.MedicalCareCode;


                            //       var sTipoDocFilter = listaTipoAtendimentoDocumentos.Where(c => c.TipoAtendimento == item.MedicalCareType && c.TipoOperadora == item.HealthCareCode).FirstOrDefault();
                            var sTipoDocFilter = listaTipoAtendimentoDocumentos.Where(c => c.TipoAtendimento == item.MedicalCareType).FirstOrDefault();

                            if (sTipoDocFilter != null)
                            {
                                strQuery += " and STD_CODIGOBARRA in (" + sTipoDocFilter.TipoDocumento + ")";
                            }

                            strQuery += "order by D.DOC_ORDEM_VISUALIZACAO";

                            var listagemDocumentos = dbConn.Query<HelperUrl>(strQuery, new { PASSAGEM = item.MedicalCareCode }).AsEnumerable();



                            if (listagemDocumentos != null && listagemDocumentos.Count() > 0)
                            {
                                //Log Data/Hora - Documentos encontrados referente ao atendimento
                                // ORIGINAL - fileCreator.WriteTxtLog(String.Format("[{0}] - {1}", DateTime.Now.ToString("dd/MM/yyyy 'às' HH:mm"), "Encontrou os documentos do atendimento: " + item.MedicalCareCode + ". Documentos encontrados: " + listagemDocumentos.Count()));
                                fileCreator.WriteTxtLog(String.Format("[{0}] - {1}", DateTime.Now.ToString("ddMMyyyy HH:mm"), "Encontrou os docs do atend: " + item.MedicalCareCode + ". Docs encontrados: " + listagemDocumentos.Count()));
                                Console.WriteLine(String.Format("[{0}] - {1}", DateTime.Now.ToString("dd/MM/yyyy 'às' HH:mm"), "Encontrou os documentos do atendimento: " + item.MedicalCareCode + ". Documentos encontrados: " + listagemDocumentos.Count()));


                                //Log Data/Hora - Path dos Documentos encontrados referente ao atendimento
                                // ORIGINAL - fileCreator.WriteTxtLog(String.Format("[{0}] - {1}", DateTime.Now.ToString("dd/MM/yyyy 'às' HH:mm"), "Caminho das imagens do atendimento: " + item.MedicalCareCode));
                                fileCreator.WriteTxtLog(String.Format("[{0}] - {1}", DateTime.Now.ToString("ddMMyyyy HH:mm"), "Path img atend: " + item.MedicalCareCode));
                                Console.WriteLine(String.Format("[{0}] - {1}", DateTime.Now.ToString("dd/MM/yyyy 'às' HH:mm"), "Caminho das imagens do atendimento: " + item.MedicalCareCode));
                                //pegar as imagens para montar o PDF



                                PdfDocument doc = new PdfDocument();
                                int i = 1;

                                bool fGerando = true;
                                try
                                {
                                    foreach (var itemDoc in listagemDocumentos)
                                    {
                                        //Log Data/Hora - Path
                                        fileCreator.WriteTxtLog(String.Format("[{0}] - {1}", DateTime.Now.ToString("dd/MM/yyyy 'às' HH:mm"), "Path: " +
                                        itemDoc.URS_PATHFISICO + "\\" + itemDoc.PAS_Registro + "\\" + itemDoc.PAS_Registro + itemDoc.DIV_CODIGOREDUZIDO + "\\" + itemDoc.PAS_CODIGOPASSAGEM + "\\" + itemDoc.DOC_NOMEARQUIVO + "." + itemDoc.FMT_EXTENSAO
                                            ));

                                        Console.WriteLine(String.Format("[{0}] - {1}", DateTime.Now.ToString("dd/MM/yyyy 'às' HH:mm"), "Path: " +
                                        itemDoc.URS_PATHFISICO + "\\" + itemDoc.PAS_Registro + "\\" + itemDoc.PAS_Registro + itemDoc.DIV_CODIGOREDUZIDO + "\\" + itemDoc.PAS_CODIGOPASSAGEM + "\\" + itemDoc.DOC_NOMEARQUIVO + "." + itemDoc.FMT_EXTENSAO
                                            ));


                                        var scaminho = itemDoc.URS_PATHFISICO + "\\" + itemDoc.PAS_Registro + "\\" + itemDoc.PAS_Registro + itemDoc.DIV_CODIGOREDUZIDO + "\\" + itemDoc.PAS_CODIGOPASSAGEM + "\\" + itemDoc.DOC_NOMEARQUIVO + "." + itemDoc.FMT_EXTENSAO;


                                        // each source on separate page
                                        doc.Pages.Add(new PdfPage());
                                        XGraphics xgr = XGraphics.FromPdfPage(doc.Pages[i - 1]);
                                        XImage img = XImage.FromFile(scaminho);
                                        xgr.DrawImage(img, 0, 0);
                                        img.Dispose();
                                        xgr.Dispose();

                                        i++;

                                    }
                                }
                                catch (Exception ex)
                                {
                                    fGerando = false;
                                    fileCreator.WriteTxtLog(String.Format("[{0}] - {1}", DateTime.Now.ToString("dd/MM/yyyy 'às' HH:mm"), "Erro ao na chamada montar PDF. Erro: " + ex.Message));
                                    Console.WriteLine(String.Format("[{0}] - {1}", DateTime.Now.ToString("dd/MM/yyyy 'às' HH:mm"), "Ocorreu um erro ao realizar  a chamada montar PDF. Erro: " + ex.Message));
                                }



                                if (fGerando)
                                {
                                    //Log Data/Hora - Info de geração de arquivo pdf
                                    fileCreator.WriteTxtLog(String.Format("[{0}] - {1}", DateTime.Now.ToString("ddMMyyyy HH:mm"), "Gerando o arq. PDF"));
                                    Console.WriteLine(String.Format("[{0}] - {1}", DateTime.Now.ToString("dd/MM/yyyy 'às' HH:mm"), "Gerando o arquivo PDF"));
                                    //  save to destination file

                                    var nomeDocumento = item.MedicalCareCode + "_" + DateTime.Now.ToString("ddMMyyyyHHmmss");

                                    try
                                    {
                                        using (var nsa = NetworkShareAccesser.Access(pathPDFSave, "HAOC", "gedcaptura", "Captura@123"))
                                        {

                                            doc.Save(pathPDFSave + @"\" + nomeDocumento + ".pdf");
                                            doc.Close();


                                            ListaURIs.Add(new RequestAtendimentoURI { MedicalCareCode = item.MedicalCareCode, URI = nomeDocumento + ".pdf", MedicalCareType = item.MedicalCareType });

                                            // ORIGINAL - fileCreator.WriteTxtLog(String.Format("[{0}] - {1}", DateTime.Now.ToString("dd/MM/yyyy 'às' HH:mm"), "----------------"));
                                            fileCreator.WriteTxtLog(String.Format("[{0}] - {1}", DateTime.Now.ToString("ddMMyyyy HH:mm"), "----------------"));
                                            Console.WriteLine(String.Format("[{0}] - {1}", DateTime.Now.ToString("dd/MM/yyyy 'às' HH:mm"), "----------------"));

                                            DirectoryInfo Dir = new DirectoryInfo(pathPDFSave);
                                            // Busca automaticamente todos os arquivos em todos os subdiretórios
                                            FileInfo[] Files = Dir.GetFiles("*", SearchOption.AllDirectories);
                                            foreach (FileInfo File in Files)
                                            {
                                                // ORIGINAL - fileCreator.WriteTxtLog(String.Format("[{0}] - {1}", DateTime.Now.ToString("dd/MM/yyyy 'às' HH:mm"), "Arquivo: " + File.Name));

                                                if (bGeraLogArquivos == "1")
                                                {
                                                    fileCreator.WriteTxtLog(String.Format("[{0}] - {1}", DateTime.Now.ToString("ddMMyyyy HH:mm"), "Arq: " + File.Name));
                                                }

                                                Console.WriteLine(String.Format("[{0}] - {1}", DateTime.Now.ToString("dd/MM/yyyy 'às' HH:mm"), "Arquivo: " + File.Name));
                                            }

                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        ListaURIsErro.Add(new RequestAtendimentoURI() { URI = nomeDocumento, MedicalCareCode = item.MedicalCareCode, Error = ex.Message, MedicalCareType = item.MedicalCareType });
                                        fileCreator.WriteTxtLog(String.Format("[{0}] - {1}", DateTime.Now.ToString("dd/MM/yyyy 'às' HH:mm"), "Erro na chamada salvar PDF. Erro: " + ex.Message));
                                        Console.WriteLine(String.Format("[{0}] - {1}", DateTime.Now.ToString("dd/MM/yyyy 'às' HH:mm"), "Ocorreu um erro ao realizar  a chamada salvar PDF. Erro: " + ex.Message));
                                    }

                                }

                            }
                            else
                            {
                                //Log Data/Hora - Registro de falha ao encontrar documentos de passagem
                                fileCreator.WriteTxtLog(String.Format("[{0}] - {1}", DateTime.Now.ToString("dd/MM/yyyy 'às' HH:mm"), "NAO foi possivel encontrar os docs da passagem: " + item.MedicalCareCode));
                                Console.WriteLine(String.Format("[{0}] - {1}", DateTime.Now.ToString("dd/MM/yyyy 'às' HH:mm"), "NAO foi possivel encontrar os documentos da passagem: " + item.MedicalCareCode));
                            }

                        }

                        dbConn.Close();
                    }

                    return lista;

                }
                else
                {
                    fileCreator.WriteTxtLog(String.Format("[{0}] - {1}", DateTime.Now.ToString("ddMMyyyy HH:mm"), "content : " + response.Content));
                    fileCreator.WriteTxtLog(String.Format("[{0}] - {1}", DateTime.Now.ToString("ddMMyyyy HH:mm"), "Ocorreu um erro na chamada 1 Método. Erro: " + response.Content));
                    Console.WriteLine(String.Format("[{0}] - {1}", DateTime.Now.ToString("dd/MM/yyyy 'às' HH:mm"), "Ocorreu um erro ao realizar a chamada 1 Método. Erro: " + response.Content));

                    return null;
                }


            }
            catch (Exception ex)
            {
                fileCreator.WriteTxtLog(String.Format("[{0}] - {1}", DateTime.Now.ToString("ddMMyyyy HH:mm"), "Erro 1 Método: " + ex.Message));
                Console.WriteLine(String.Format("[{0}] - {1}", DateTime.Now.ToString("dd/MM/yyyy 'às' HH:mm"), "Erro 1 Método: " + ex.Message));
                return null;
            }

        }

        static async Task<bool> AtendimentoUri_Robo3(List<RequestAtendimentoURI> ListaURIs, List<RequestAtendimentoURI> ListaURIsErro)
        {

            string bGeraLogJson = System.Configuration.ConfigurationManager.AppSettings["GeraLogJson"];

            Console.WriteLine(String.Format("[{0}] - {1}", DateTime.Now.ToString("dd/MM/yyyy 'às' HH:mm"), "Iniciando 3 Método. "));
            fileCreator.WriteTxtLog(String.Format("[{0}] - {1}", DateTime.Now.ToString("ddMMyyyy HH:mm"), "Início 3 Método."));

            var ksd = JsonConvert.SerializeObject(ListaURIs);
            if (bGeraLogJson == "1")
            {
                fileCreator.WriteTxtLog(String.Format("[{0}] - {1}", DateTime.Now.ToString("ddMMyyyy HH:mm"), "json: " + ksd));
            }

            Console.WriteLine(String.Format("[{0}] - {1}", DateTime.Now.ToString("dd/MM/yyyy 'às' HH:mm"), "json: " + ksd));

            var ksd_erro = JsonConvert.SerializeObject(ListaURIsErro);
            fileCreator.WriteTxtLog(String.Format("[{0}] - {1}", DateTime.Now.ToString("ddMMyyyy HH:mm"), "json lista com erros: " + ksd_erro));
            Console.WriteLine(String.Format("[{0}] - {1}", DateTime.Now.ToString("dd/MM/yyyy 'às' HH:mm"), "json listagem com erros: " + ksd));
            try
            {
                WebServices services = new WebServices();



                IRestResponse response = await services.PostObjectAtendimentoURIRestAsync<List<RequestAtendimentoURI>>("/tiss", ListaURIs);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    fileCreator.WriteTxtLog(String.Format("[{0}] - {1}", DateTime.Now.ToString("ddMMyyyy HH:mm"), "Método 3 realizado com sucesso: " + response.Content));
                    Console.WriteLine(String.Format("[{0}] - {1}", DateTime.Now.ToString("dd/MM/yyyy 'às' HH:mm"), "Método 3 realizado com sucesso: " + response.Content));
                }
                else
                {
                    fileCreator.WriteTxtLog(String.Format("[{0}] - {1}", DateTime.Now.ToString("dd/MM/yyyy 'às' HH:mm"), "Erro na chamada 3 Método. Erro: " + response.Content));
                    Console.WriteLine(String.Format("[{0}] - {1}", DateTime.Now.ToString("dd/MM/yyyy 'às' HH:mm"), "Ocorreu um erro ao realizar a chamada 3 Método. Erro: " + response.Content));
                }

                return true;
            }
            catch (Exception ex)
            {

                fileCreator.WriteTxtLog(String.Format("[{0}] - {1}", DateTime.Now.ToString("ddMMyyyy HH:mm"), "Erro 3 Método: " + ex.Message));
                Console.WriteLine(String.Format("[{0}] - {1}", DateTime.Now.ToString("dd/MM/yyyy 'às' HH:mm"), "Erro 3 Método: " + ex.Message));
                return false;
            }



        }
    }

    public class HelperUrl
    {
        public string URS_PATHFISICO { get; set; }
        public string PAS_Registro { get; set; }
        public string DIV_CODIGOREDUZIDO { get; set; }
        public string PAS_CODIGOPASSAGEM { get; set; }
        public string DOC_NOMEARQUIVO { get; set; }
        public string FMT_EXTENSAO { get; set; }
    }


    public class HelperTiposDocumentos
    {
        public string TipoOperadora { get; set; }
        public string TipoAtendimento { get; set; }
        public string TipoDocumento { get; set; }
    }
}

using Crawler.DTO;
using Crawler.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Crawler
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string urlBase = GetURLBase();

            WebClient webClient = new WebClient();

            string historicalDataString = webClient.DownloadString(urlBase);
            IndexEuroDTO historicalJson = JsonConvert.DeserializeObject<IndexEuroDTO>(historicalDataString);

            var desiredValues = historicalJson.Value.Where(x => x.tipoBoletim == "Fechamento");
            int currentAverageMonth = desiredValues.FirstOrDefault().dataHoraCotacao.Month;

            List<IndexEuroDefault> averages = new List<IndexEuroDefault>();
            List<IndexEuroDefault> monthlyValues = new List<IndexEuroDefault>();

            foreach (var value in desiredValues)
            {
                int valueMonth = value.dataHoraCotacao.Month;
                int valueYear = value.dataHoraCotacao.Year;
                if (currentAverageMonth != valueMonth)
                {
                    averages.Add(new IndexEuroDefault
                    {
                        Year = valueYear,
                        Month = currentAverageMonth,
                        Price = BuildMonthlyAverage(monthlyValues)
                    });
                    currentAverageMonth = valueMonth;
                    monthlyValues.Clear();
                }

                IndexEuroDefault obj = new IndexEuroDefault
                {
                    Year = value.dataHoraCotacao.Year,
                    Month = value.dataHoraCotacao.Month,
                    Price = value.cotacaoVenda
                };
                monthlyValues.Add(obj);
            }
            averages.Add(new IndexEuroDefault
            {
                Year = desiredValues.LastOrDefault().dataHoraCotacao.Year,
                Month = desiredValues.LastOrDefault().dataHoraCotacao.Month,
                Price = BuildMonthlyAverage(monthlyValues)
            });//last month, if it ain't complete.

            IndexEuroDefault lastDate = averages.LastOrDefault();

            if (lastDate.Month < 12) CompleteRemainingYear(lastDate, averages);
            GenerateProjections(lastDate, averages);

            averages.ForEach(x => Console.WriteLine($"{x.Month}/{x.Year} - {x.Price}"));
            Console.ReadKey();
        }

        private static void GenerateProjections(IndexEuroDefault lastDate, List<IndexEuroDefault> finalProjections)
        {
            for (int futureYear = lastDate.Year + 1; futureYear <= 2040; futureYear++)
            {
                for (int month = 1; month <= 12; month++)
                {
                    IndexEuroDefault averagesEqualToLastYear = new IndexEuroDefault
                    {
                        Year = futureYear,
                        Month = month,
                        Price = lastDate.Price
                    };

                    finalProjections.Add(averagesEqualToLastYear);
                }
            }
        }

        private static void CompleteRemainingYear(IndexEuroDefault lastDate, List<IndexEuroDefault> indexes)
        {
            for (int month = lastDate.Month; month <= 12; month++)
            {
                IndexEuroDefault monthValue = new IndexEuroDefault
                {
                    Year = lastDate.Year,
                    Month = month,
                    Price = lastDate.Price
                };

                indexes.Add(monthValue);
            }
        }

        private static double BuildMonthlyAverage(List<IndexEuroDefault> values)
        {
            double average = 0;
            int count = 0;
            foreach (IndexEuroDefault day in values)
            {
                count++;
                average += day.Price;
            }
            average = average / values.Count;
            double defaultMonth = Math.Round(average, 2, MidpointRounding.ToEven);
            return defaultMonth;
        }

        public static string GetURLBase()
        {
            DateTime dateNow = DateTime.Now;
            string now = dateNow.Month.ToString() + "-" + dateNow.Day.ToString() + "-" + dateNow.Year.ToString();
            string baseURL = "https://olinda.bcb.gov.br/olinda/servico/PTAX/versao/v1/odata/CotacaoMoedaPeriodo(moeda=@moeda,dataInicial=@dataInicial,dataFinalCotacao=@dataFinalCotacao)?@moeda=%27EUR%27&@dataInicial=%2701-01-2022%27&@dataFinalCotacao=%27" + now + "%27&$format=json&$select=cotacaoVenda,dataHoraCotacao,tipoBoletim&$orderby=dataHoraCotacao%20asc";

            return baseURL;
        }

    }
}

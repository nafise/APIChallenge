using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AutoConsole
{
    class Program
    {
        static HttpClient client = new HttpClient();
        static SAutoDataSet AutoResult = new SAutoDataSet();
        static int counter = 0;
        static List<Dealer> RequestedDealers = new List<Dealer>();
        static List<Vehicle> RequestedVehicles = new List<Vehicle>();
        static List<int> DealerIds = new List<int>();
        static void Main(string[] args)
        {
            //Console.WriteLine("Started");
            //syncRead();
            AsyncRead();

        }
        private static void syncRead()
        {
            string GetURL = "";
            string stResult = "";

            //AutoDataSet
            GetURL = "https://vautointerview.azurewebsites.net/api/datasetId";
            stResult = Task.Run(() => GetHttpClientASync(GetURL)).Result;
            AutoDataSet Autodataset = JsonConvert.DeserializeObject<AutoDataSet>(stResult);


            //Vehicles
            GetURL = "https://vautointerview.azurewebsites.net/api/" + Autodataset.datasetId + "/vehicles";
            stResult = Task.Run(() => GetHttpClientASync(GetURL)).Result;
            List<int> lsVehicles = JObject.Parse(stResult)["vehicleIds"].Select(x => (int)x).ToList();

            //Dealers
            for (int i = 0; i < lsVehicles.Count; i++)
            {
                GetURL = "https://vautointerview.azurewebsites.net/api/" + Autodataset.datasetId + "/vehicles/" + lsVehicles[i];
                stResult = Task.Run(() => GetHttpClientASync(GetURL)).Result;
                Vehicle CurrVehicle = JsonConvert.DeserializeObject<Vehicle>(stResult);

                if (AutoResult.dealers == null || AutoResult.dealers.Count == 0)
                {
                    GetURL = "https://vautointerview.azurewebsites.net/api/" + Autodataset.datasetId + "/dealers/" + CurrVehicle.dealerId;
                    stResult = Task.Run(() => GetHttpClientASync(GetURL)).Result;
                    Dealer CurrDealer = JsonConvert.DeserializeObject<Dealer>(stResult);
                    CurrDealer.vehicles = new List<SVehicle>();
                    SVehicle NewVehicle = new SVehicle();
                    NewVehicle.vehicleId = CurrVehicle.vehicleId;
                    NewVehicle.year = CurrVehicle.year;
                    NewVehicle.model = CurrVehicle.model;
                    NewVehicle.make = CurrVehicle.make;
                    CurrDealer.vehicles.Add(NewVehicle);
                    AutoResult.dealers = new List<Dealer>();
                    AutoResult.dealers.Add(CurrDealer);
                }
                else
                {
                    Dealer findDealer = AutoResult.dealers.Where(x => x.dealerId == CurrVehicle.dealerId).FirstOrDefault();
                    if (findDealer != null)
                    {
                        SVehicle NewVehicle = new SVehicle();
                        NewVehicle.vehicleId = CurrVehicle.vehicleId;
                        NewVehicle.year = CurrVehicle.year;
                        NewVehicle.model = CurrVehicle.model;
                        NewVehicle.make = CurrVehicle.make;
                        findDealer.vehicles.Add(NewVehicle);
                    }
                    else
                    {
                        GetURL = "https://vautointerview.azurewebsites.net/api/" + Autodataset.datasetId + "/dealers/" + CurrVehicle.dealerId;
                        stResult = Task.Run(() => GetHttpClientASync(GetURL)).Result;
                        Dealer CurrDealer = JsonConvert.DeserializeObject<Dealer>(stResult);
                        CurrDealer.vehicles = new List<SVehicle>();
                        SVehicle NewVehicle = new SVehicle();
                        NewVehicle.vehicleId = CurrVehicle.vehicleId;
                        NewVehicle.year = CurrVehicle.year;
                        NewVehicle.model = CurrVehicle.model;
                        NewVehicle.make = CurrVehicle.make;
                        CurrDealer.vehicles.Add(NewVehicle);
                        AutoResult.dealers.Add(CurrDealer);
                    }
                }
            }
            string PostURL = "https://vautointerview.azurewebsites.net/api/" + Autodataset.datasetId + "/answer";
            string PostData = JsonConvert.SerializeObject(AutoResult);
            Console.WriteLine(Task.Run(() => POSTHttpClientASync(PostURL, PostData)).Result);
            Console.Read();
        }
        private static void AsyncRead()
        {
            string GetURL = "";
            string stResult = "";

            // Get DataSet ID
            GetURL = "https://vautointerview.azurewebsites.net/api/datasetId";
            stResult = Task.Run(() => GetHttpClientASync(GetURL)).Result;
            AutoDataSet Autodataset = JsonConvert.DeserializeObject<AutoDataSet>(stResult);


            //Get Vehicles List
            GetURL = "https://vautointerview.azurewebsites.net/api/" + Autodataset.datasetId + "/vehicles";
            stResult = Task.Run(() => GetHttpClientASync(GetURL)).Result;
            List<int> lsVehicles = JObject.Parse(stResult)["vehicleIds"].Select(x => (int)x).ToList();

            //Get Vehicles Details
            for (int i = 0; i < lsVehicles.Count; i++)
            {
                //Console.WriteLine("Vehicle " + counter);
                //Console.WriteLine(lsVehicles[i]);
                int Vid = lsVehicles[i];
                Task.Run(() => ReadVehicle(Autodataset.datasetId, Vid));
                //Console.WriteLine(i);
            }
            Console.Read();
        }
        /// <summary>
        /// Read Vehicle Info
        /// </summary>
        /// <param name="datasetId"></param>
        /// <param name="vehicleId"></param>
        private static async void ReadVehicle(string datasetId, int vehicleId)
        {
            try
            {
                counter++;
                string GetURL = "https://vautointerview.azurewebsites.net/api/" + datasetId + "/vehicles/" + vehicleId;
                string stResult = await GetHttpClientASync(GetURL);
                Vehicle CurrVehicle = JsonConvert.DeserializeObject<Vehicle>(stResult);
                RequestedVehicles.Add(CurrVehicle);
                if (DealerIds.Count == 0 ||
                    !DealerIds.Contains(CurrVehicle.dealerId))
                {
                    DealerIds.Add(CurrVehicle.dealerId);
                }
                counter--;
                // Get Dealers Info
                if (counter == 0)
                {
                    Task.Run(() => ReadDealerList(datasetId));
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// Call Read each Dealers Info and Post result
        /// </summary>
        /// <param name="datasetId"></param>
        private static async void ReadDealerList(string datasetId)
        {
            for (int i = 0; i < DealerIds.Count; i++)
            {
                await ReadDealer(datasetId, DealerIds[i]);
            }
            if (counter == 0)
            {
                PostData(datasetId);
            }
            return;
        }

        /// <summary>
        /// Read Dealer Info
        /// </summary>
        /// <param name="datasetId"></param>
        /// <param name="dealerId"></param>
        /// <returns></returns>
        private static async Task ReadDealer(string datasetId, int dealerId)
        {
            counter++;
            string GetURL = "https://vautointerview.azurewebsites.net/api/" + datasetId + "/dealers/" + dealerId;
            string stResult = await GetHttpClientASync(GetURL);
            Dealer CurrDealer = JsonConvert.DeserializeObject<Dealer>(stResult);
            RequestedDealers.Add(CurrDealer);
            counter--;
            
        }

        /// <summary>
        /// Build relation between Dealers and Vehicles
        /// </summary>
        /// <param name="datasetId"></param>
        private static void PostData(string datasetId)
        {
            AutoResult = new SAutoDataSet();
            AutoResult.dealers = new List<Dealer>();
            for (int i = 0; i < RequestedDealers.Count; i++)
            {
                AutoResult.dealers.Add(RequestedDealers[i]);
                AutoResult.dealers[i].vehicles = new List<SVehicle>();
            }
            for (int i = 0; i < RequestedVehicles.Count; i++)
            {
                for (int j = 0; j < AutoResult.dealers.Count; j++)
                {
                    if (AutoResult.dealers[j].dealerId == RequestedVehicles[i].dealerId)
                    {
                        SVehicle Temp = new SVehicle();
                        Temp.make = RequestedVehicles[i].make;
                        Temp.model = RequestedVehicles[i].model;
                        Temp.vehicleId = RequestedVehicles[i].vehicleId;
                        Temp.year = RequestedVehicles[i].year;
                        AutoResult.dealers[j].vehicles.Add(Temp);
                        break;
                    }
                }
            }
            string PostURL = "https://vautointerview.azurewebsites.net/api/" + datasetId + "/answer";
            string PostData = JsonConvert.SerializeObject(AutoResult);
            Console.WriteLine(Task.Run(() => POSTHttpClientASync(PostURL, PostData)).Result);
        }

        /// <summary>
        /// HttpClient Post
        /// </summary>
        /// <param name="URL"></param>
        /// <param name="PostData"></param>
        /// <returns></returns>
        private static async Task<string> POSTHttpClientASync(string URL, string PostData)
        {
            var content = new StringContent(PostData, Encoding.UTF8, "application/json");

            var result = await client.PostAsync(URL, content);
            string resultContent = await result.Content.ReadAsStringAsync();
            return resultContent;
        }

        /// <summary>
        /// HttpClient Get
        /// </summary>
        /// <param name="URL"></param>
        /// <returns></returns>
        private static async Task<string> GetHttpClientASync(string URL)
        {
            string content1 = "";
            try
            {
                HttpResponseMessage result = await client.GetAsync(URL);
                if (result.IsSuccessStatusCode)
                {
                    content1 = await result.Content.ReadAsStringAsync();
                    Console.WriteLine(URL);
                }
                return content1;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

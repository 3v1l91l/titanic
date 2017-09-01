using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Text;
using src.Models;
using System.Globalization;

namespace src.Controllers
{
    [Route("/")]
    public class RootController : Controller
    {
        [HttpPost("Submit")]
        public ActionResult Submit(Form model)
        {
            bool prediction = false;
            try
            {
                prediction = Predict(model.Fare, model.Age).Result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            ViewData["Prediction"] = prediction ? "Alive" : "Dead";
            return View("Submit");
        }

        // GET /
        [HttpGet("")]
        public IActionResult Index() {
            return View();
        }
        static async Task<bool> Predict(int fare, int age) {
            using (var client = new HttpClient())
            {
                var scoreRequest = new 
                {
                    Inputs = new Dictionary<string, List<Dictionary<string, string>>> () {
                        {
                            "input1",
                            new List<Dictionary<string, string>>(){new Dictionary<string, string>(){
                                            {
                                                "Fare", fare.ToString()
                                            },
                                            {
                                                "Age", age.ToString()
                                            },
                                            {
                                                "Survived", "1"
                                            },

                                }
                            }
                        },
                    },
                    GlobalParameters = new Dictionary<string, string>() {
                    }
                };

                const string apiKey = "b0Oh0djzVdMnyUCXALJ5ndCNr7tZrW21ZTF78b+pIjPRSbqrk1AxlBuTmstve5C1h7FRVheIV5U1h7uODW86Og==";
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Bearer", apiKey);
                client.BaseAddress = new Uri("https://ussouthcentral.services.azureml.net/workspaces/c9d75f957bf54c309caf9d580620beb4/services/897099567b3e47559ab51a4de5693d69/execute?api-version=2.0&format=swagger");
                
                HttpResponseMessage response = await client
                    .PostAsync("", new StringContent(JsonConvert.SerializeObject(scoreRequest), Encoding.UTF8, "application/json"))
                    .ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    string score = await response.Content.ReadAsStringAsync();
                    Data z = JsonConvert.DeserializeObject<Data>(score);
                
                    int result;
                    int.TryParse(z.Results.output1.First()["Scored Labels"], out result);
                    return result == 1;
                }
                else
                {
                    return false;
                }

            }
        }

        public class Data {
                public Results Results { get; set; }
        }

        public class Results {
             public IEnumerable<IDictionary<string, string>> output1 { get; set; }
        }
    }
}

using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace OpenAI_Sales_Query
{
    class Program
    {
        static readonly string API_URL = "https://api.openai.com/v1/chat/completions";

        static void Main(string[] args)
        {
            string API_KEY;

            if (args.Length == 0)
            {
                Console.WriteLine("Error: Please provide the API key as an argument.");
                return;
            }
            else
            {
                API_KEY = args[0];
            }

            while (true) // This will keep the program running until explicitly told to exit
            {
                Console.WriteLine("Enter your sales query (or type 'exit' to quit):");
                var inputQuery = Console.ReadLine();

                if (string.Equals(inputQuery, "exit", StringComparison.OrdinalIgnoreCase))
                {
                    // Exit the loop and end the program if the user types 'exit'
                    break;
                }

                if (string.IsNullOrWhiteSpace(inputQuery))
                {
                    Console.WriteLine("Please provide a valid input.");
                    continue;  // Skip to the next iteration without calling QueryOpenAI
                }

                var result = QueryOpenAI(inputQuery, API_KEY);
                Console.WriteLine(result);
                Console.WriteLine(); // Just to give a space before the next iteration
            }
        }

        static string QueryOpenAI(string inputQuery, string apiKey)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                string baseIntro = "Given a sales database with dimensions: product, product category, salesperson, region. ";
                string measures = "And the measures: quantity, price, total value. ";
                string examples = "Here are some examples. User input: top 5 sales reps by value sold. -> Output: salesperson, total value, descending, limit 5. ";
                string querySpecs = "Each query, such as 'top 5 regions by value sold' will specify: ";
                string dimensionDetail = "- the dimension that would be used to group the results, in this case 'region' ";
                string measureDetail = "- the measure that would be aggregated, in this case 'value' ";
                string orderDetail = "- an indication of whether the results would be returned in ascending or descending order, in this case descending ";
                string limitDetail = "- optionally, a limit for the number of results that would be returned, in this case 5. ";
                string examples2 = "Here are some examples. User input: lowest selling products by count. -> Output: product, quantity, descending, no limit. ";
                string querySpecs2 = "Each query, such as 'lowest selling products by count' will specify: ";
                string dimensionDetail2 = "- the dimension that would be used to group the results, in this case 'product' ";
                string measureDetail2 = "- the measure that would be aggregated, in this case 'quantity' ";
                string orderDetail2 = "- an indication of whether the results would be returned in ascending or descending order, in this case ascending ";
                string limitDetail2 = "- optionally, a limit for the number of results that would be returned, in this case no limit. ";
                string exampleFollowUp = "Please following the above example. ";
                string patternFollowUp = "Please following this json pattern to output the result. {'dimension': _, 'measure': _, 'order': _, 'limit': _ }";

                var prompt = $"{baseIntro}{measures}{examples}{querySpecs}{dimensionDetail}{measureDetail}{orderDetail}{limitDetail}{examples2}{querySpecs2}{dimensionDetail2}{measureDetail2}{orderDetail2}{limitDetail2}{exampleFollowUp}{patternFollowUp}\"{inputQuery}\".";

                
                var data = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[]
                    {
                        new { role = "system", content = "You are a helpful assistant that translates natural language sales queries into structured data." },
                        new { role = "user", content = prompt }
                    }
                };
                
                var response = client.PostAsync(API_URL, new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")).Result;

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"OpenAI API call failed with status: {response.StatusCode}");
                }

                var responseContent = response.Content.ReadAsStringAsync().Result;

                if (string.IsNullOrWhiteSpace(responseContent))
                {
                    throw new Exception("Received empty response from OpenAI.");
                }

                var responseObject = JObject.Parse(responseContent);
                if (!responseObject.ContainsKey("choices"))
                {
                    throw new Exception($"Unexpected API response: {responseContent}");
                }
                var reply = responseObject["choices"]?[0]?["message"]?["content"]?.ToString();
                if (string.IsNullOrWhiteSpace(reply))
                {
                    throw new Exception("Unexpected response structure from OpenAI.");
                }

                // Convert the string to lowercase
                reply = reply.ToLower();

                string correctedReply = reply.Replace("'", "\"");
                correctedReply = Regex.Replace(correctedReply, @"\bnone\b", "null", RegexOptions.IgnoreCase);
                
                Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(correctedReply);

                if (dictionary.Count > 4)
                {
                    return "Too many elements in the result. Please enter your question again.";
                }

                if (dictionary.Count < 4)
                {
                    return "Too less elements in the result. Please enter your question again.";
                }

                if (dictionary == null || dictionary.Count == 0)
                {
                    return "No valid output received. Please rephrase your question.";
                }

                var keys = new List<string>(dictionary.Keys);
                string dimension = dictionary[keys[0]].ToString();
                string measure = dictionary[keys[1]].ToString();
                string order = dictionary[keys[2]].ToString();
                object limitObj = dictionary[keys[3]];
                string limit = limitObj == null ? null : limitObj.ToString();

                if (measure.Contains("value"))
                {
                    measure = "total value";
                }

                if (limit == null)
                {
                    return $"{dimension}, {measure}, {order}, no limit";
                }
                else if (limit.ToLower().Contains("all") || limit.ToLower().Contains("specifi") || limit.ToLower().Contains("no"))
                {
                    return $"{dimension}, {measure}, {order}, no limit";
                }
                else
                {
                    return $"{dimension}, {measure}, {order}, limit {limit}";
                }
            }
        }

    }
}
using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq;

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
                var prompt = $"Given a sales database with dimensions: product, product category, salesperson, region. And the measures: quantity, price, total value. Here are some examples. User input: top 5 sales reps by value sold. -> Output: salesperson, total value, descending, limit 5. Each query, such as 'top 5 regions by value sold' will specify: - the dimension that would be used to group the results, in this case 'region' - the measure that would be aggregated, in this case 'value' - an indication of whether the results would be returned in ascending or descending order, in this case descending - optionally, a limit for the number of results that would be returned, in this case 5. Please following the above example. Please following the above pattern and print the dimension, measure, an indication of whether the results would be returned in ascending or descending order and a limit for the number of results that would be returned: \"{inputQuery}\".";
                
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

                // Split the string by newline to get each line separately
                string[] lines = reply.Split('\n');

                // Use a dictionary to map each keyword to its corresponding value
                Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

                foreach (var line in lines)
                {
                    var parts = line.Split(':');
                    if (parts.Length == 2)
                    {
                        // Trim whitespace and add to the dictionary
                        keyValuePairs[parts[0].Trim()] = parts[1].Trim();
                    }
                }

                // Print the required values with comma separation
                List<string> values = new List<string>();
                foreach (var key in keyValuePairs.Keys)
                {
                    var value = keyValuePairs[key];
                    if (key == "limit")
                    {
                        if (value.Contains("all") || value.Contains("specifi") || value.Contains("no"))
                        {
                            values.Add("no limit");
                        }
                        else
                        {
                            values.Add($"{key} {value}");
                        }
                    }
                    else
                    {
                        if (value.Contains("value"))
                        {
                            values.Add("total value");
                        }
                        else
                        {
                            values.Add(value);
                        }
                    }
                }
                
                return string.Join(", ", values.Where(v => !string.IsNullOrWhiteSpace(v)));
            }
        }

    }
}
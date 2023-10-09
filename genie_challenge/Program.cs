﻿using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace OpenAI_Sales_Query
{
    class Program
    {
        static readonly string API_URL = "https://api.openai.com/v1/chat/completions";
        static readonly string API_KEY = "Your-API-Key"; // Replace with your key and never share it publicly.

        static void Main(string[] args)
        {
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

                var result = QueryOpenAI(inputQuery);
                Console.WriteLine(result);
                Console.WriteLine(); // Just to give a space before the next iteration
            }
        }

        static string ExtractFromNaturalLanguage(string reply)
        {
            var dimensionPatterns = new (string Pattern, string Value)[]
            {
                (@"sales (representatives|people|reps)", "salesperson"),
                (@"products", "product"),
                (@"regions", "region"),
                // ... Add more patterns for other dimensions
            };

            var measurePatterns = new (string Pattern, string Value)[]
            {
                (@"total value|sales by value", "total value"),
                (@"count|quantity", "quantity"),
                (@"price", "price"),
                // ... Add more patterns for other measures
            };

            var sortOrderPatterns = new (string Pattern, string Value)[]
            {
                (@"highest", "descending"),
                (@"lowest", "ascending"),
                // ... Add more patterns for other sorts
            };

            var limitPatterns = new (string Pattern, string Value)[]
            {
                (@"top (\d+)", "limit {0}"),  // This captures "top N" pattern and returns "limit N"
                // ... Add more patterns for other limits
            };

            var dimension = "unknown";
            var measure = "unknown";
            var sortOrder = "unknown";
            var limit = "no limit";

            foreach (var (Pattern, Value) in dimensionPatterns)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(reply, Pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    dimension = Value;
                    break;
                }
            }

            foreach (var (Pattern, Value) in measurePatterns)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(reply, Pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    measure = Value;
                    break;
                }
            }

            foreach (var (Pattern, Value) in sortOrderPatterns)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(reply, Pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    sortOrder = Value;
                    break;
                }
            }

            foreach (var (Pattern, Value) in limitPatterns)
            {
                var match = System.Text.RegularExpressions.Regex.Match(reply, Pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    limit = string.Format(Value, match.Groups[1].Value);  // This uses the captured value in the pattern
                    break;
                }
            }

            return $"{dimension}, {measure}, {sortOrder}, {limit}";
        }



        static string QueryOpenAI(string inputQuery)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {API_KEY}");
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
                var keys = new List<string> { "dimension", "measure", "order", "limit" };
                List<string> values = new List<string>();
                
                foreach (var key in keys)
                {
                    if (keyValuePairs.TryGetValue(key, out var value))
                    {
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
                }
                
                return string.Join(", ", values);
            }
        }

        static string ParseReply(string reply)
        {
            if (string.IsNullOrWhiteSpace(reply) || (!reply.StartsWith("{") && !reply.StartsWith("[")))
            {
                return ExtractFromNaturalLanguage(reply);
            }
            var json = JObject.Parse(reply);

            var dimension = json["dimension"]?.ToString();
            var measure = json["measure"]?.ToString();
            var sortOrder = json["sortOrder"]?.ToString();
            var limit = json["limit"]?.ToString() ?? "no limit"; // Defaulting to "no limit" if it's not provided

            if (string.IsNullOrWhiteSpace(dimension) || string.IsNullOrWhiteSpace(measure) || string.IsNullOrWhiteSpace(sortOrder))
            {
                throw new Exception("Invalid or incomplete data received from OpenAI.");
            }

            return $"{dimension}, {measure}, {sortOrder}, limit {limit}";
        }
    }
}
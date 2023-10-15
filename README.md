# Qiming_genie_challenge

Please clone the project using `git clone https://github.com/14H034160212/Qiming_genie_challenge.git`.

### Run the program
After you clone your project, using your own openai api key and you can use either python version or C# version to ask question and conduct query separately to run the project under `Qiming_genie_challenge/genie_challenge` folder and then you can interact the program and type question in natural langauge format if you want.

#### Python Version
You need to create an virtual environment and enter into the environment.

`python -v venv venv`

` .\venv\Scripts\activate`

And install the following packages.

`pip install openai, pandas`

Run the program.

`python openai_example.py --api_key your-api-key-here` 

#### C# Version
`dotnet run your-api-key-here` 

### Basic test cases
Here are some examples input and output include:

Input: `"top 5 sales reps by value sold"` -> Output： `salesperson, total value, descending, limit 5`

Input: `"highest selling products by count"` -> Output: `product, quantity, descending, no limit`

Input: `"10 regions with the lowest sales by value"` -> Output: `region, total value, ascending, limit 10`

### More complex test cases 
Input: `lowest 5 sales reps by value sold` -> Output： `salesperson, total value, ascending, limit 5`

Input: `worst 5 sales reps by value sold` -> Output： `salesperson, total value, ascending, limit 5`

Input: `lowest selling products by count` -> Output： `product, quantity, ascending, no limit`

Input: `worst selling products by count` -> Output： `product, quantity, ascending, no limit`

Input: `Worst performing categories by value` -> Output： `product category, total value, ascending, no limit`

Input: `Best performing categories by value` -> Output： `product category, total value, descending, no limit`

Input: `highest performing categories by value` -> Output： `product category, total value, descending, no limit`

### Prompt Desgin
We designed the prompt based on the following aspects.
1) Provide a base introduction for the sales database with specific dimensions `- product (string) - product category (string) - salesperson (string) - region (string)` and measures `- quantity (integer) - price (float) - total value (float)`.
2) Provide two specific examples, along with a step-by-step explanation corresponding to the database's dimensions and measures. These examples should be representative and cover different dimensions and measures. The examples I've given are `top 5 sales reps by value sold` and `lowest selling products by count`.
3) Provide a standard format for output: `Please following the above example and using json pattern to output the result. {'dimension': _, 'measure': _, 'order': _, 'limit': _ }`

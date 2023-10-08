# Qiming_genie_challenge

Please clone the project using `git clone https://github.com/14H034160212/Qiming_genie_challenge.git`.

### Run the program
After you clone your project, using your own openai api key and you can use `python openai_example.py` and `dotnet run` separately to run the project under `Qiming_genie_challenge/genie_challenge` folder and then you can interact the program and type question in natural langauge format if you want.

Here are some examples input and output include:
### Basic test cases
Input: `- "top 5 sales reps by value sold"` -> Output： `salesperson, total value, descending, limit 5`

Input: `- "highest selling products by count"` -> Output: `product, quantity, descending, no limit`

Input: `- "10 regions with the lowest sales by value"` -> Output: `region, total value, ascending, limit 10`

### More complex test cases
Input: `- "10 regions with the high sales by value"` -> Output： `salesperson, total value, descending, limit 10`

Input: `- "10 regions with the low sales by value"` -> Output： `salesperson, total value, ascending, limit 10`

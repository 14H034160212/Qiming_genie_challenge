# Qiming_genie_challenge

Please clone the project using `git clone https://github.com/14H034160212/Qiming_genie_challenge.git`.

## Create virtual environment and install your environment packages
### Create your virtual environment and activate your virtual environment
`python -m venv spacy_env`

`spacy_env\Scripts\activate`


### Install required packages
`pip install spacy`

`python -m spacy download en_core_web_sm`

`python -m spacy download en_core_web_md`

### Run the program
After you clone your project, you can use `dotnet run` to run the project under `Qiming_genie_challenge/genie_challenge` folder and then you can interact the program and type question in natural langauge format if you want.

Here are some examples input and output include:

Input: `- "top 5 sales reps by value sold"` -> Output： `salesperson, total value, descending, limit 5`

Input: `- "highest selling products by count"` -> Output: `product, quantity, descending, no limit`

Input: `- "10 regions with the lowest sales by value"` -> Output: `region, total value, ascending, limit 10`

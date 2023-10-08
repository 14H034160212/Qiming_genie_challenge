import os
import openai
import json
import pandas as pd
from tqdm import tqdm
import argparse

parser = argparse.ArgumentParser()
parser.add_argument('--model_name', default="gpt-3.5-turbo",
                    help='the name of the openai model that been used')
parser.add_argument('--temperature', type=float, default=0.7,
                    help='A parameter that controls the creativity of the generated text. A higher temperature value will result in more unexpected and diverse output, while a lower value will result in more conservative and predictable output.')
parser.add_argument('--max_tokens', type=int, default=512,
                    help='The maximum number of tokens (words or subwords) that the completion should contain.')
parser.add_argument('--top_p', type=float, default=1,
                    help='A parameter that controls the diversity of the generated text by selecting from the most probable tokens according to their cumulative probability until the top_p probability mass is reached. A value of 1 means that all tokens are considered.')
parser.add_argument('--frequency_penalty', type=float, default=0,
                    help='A parameter that penalizes words that have already been generated in the response to encourage the model to generate new words.')
parser.add_argument('--presence_penalty', type=float, default=0,
                    help='A parameter that penalizes words that were present in the input messages to encourage the model to generate new words.')
parser.add_argument('--api_key', default="Your-api-key​",
                    help='Type your openai api key')


args = parser.parse_args()
openai.api_key = "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"

def clean_string(s):
    return ''.join(char for char in s if ord(char) < 256)

def extract_value(input_str, key):
    for line in input_str.split('\n'):
        if line.startswith(key):
            return line.split(': ')[1]
    return None

# Interactive loop for user input
while True:
    chat_history = []
    query = input("Enter your query (or type 'exit' to quit): ")
    
    if query.lower() in ['exit', 'quit']:
        break

    prompt = "Given a sales database with dimensions: product, product category, salesperson, region. And the measures: quantity, price, total value. Here are some examples. User input: top 5 sales reps by value sold. -> Output: salesperson, total value, descending, limit 5. Each query, such as 'top 5 regions by value sold' will specify: - the dimension that would be used to group the results, in this case 'region' - the measure that would be aggregated, in this case 'value' - an indication of whether the results would be returned in ascending or descending order, in this case descending - optionally, a limit for the number of results that would be returned, in this case 5. Please following the above example. Please following the above pattern and print the dimension, measure, an indication of whether the results would be returned in ascending or descending order and a limit for the number of results that would be returned:"
    chat_history.append({"role": "user", "content": prompt+clean_string(query)})

    response = openai.ChatCompletion.create(
        model=args.model_name,
        messages=chat_history,
        temperature=args.temperature,
        max_tokens=args.max_tokens,
        top_p=args.top_p,
        frequency_penalty=args.frequency_penalty,
        presence_penalty=args.presence_penalty
    )

    reply = response["choices"][0]["message"]["content"]
    reply = reply.lower()
    dimension = extract_value(reply, 'dimension')
    measure = extract_value(reply, 'measure')
    order = extract_value(reply, 'order')
    limit = extract_value(reply, 'limit')
    if "value" in measure:
        measure = "total value"
    
    if "all" in limit.lower() or "not specified" in limit.lower():
        print(f"{dimension}, {measure}, {order}, {'no limit'}")
    else:
        print(f"{dimension}, {measure}, {order}, limit {limit}")
    # print(reply)

    # Add the model's response to the chat history
    chat_history.append({"role": "system", "content": clean_string(reply)})
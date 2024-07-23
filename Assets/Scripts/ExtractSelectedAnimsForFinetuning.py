import json
import os

# File paths
first_file_path = './gpt3-5-Finetuning.jsonl'
second_file_path = './gpt3-5-Finetuning2.jsonl'
temp_output_file_path = './temp_output.jsonl'  # Temporary file for safe writing

# Step 1: Extract strings from the first file
user_contents = set()  # Using a set to avoid duplicates

with open(first_file_path, 'r') as file:
    for line in file:
        data = json.loads(line)
        for message in data['messages']:
            if message['role'] == 'user':
                user_contents.add(message['content'])
                print("Extracted content:", message['content'])  # Display extracted user content

# Step 2: Filter the second file based on these strings
matches_found = False  # Flag to check if any matches were found
with open(second_file_path, 'r') as file, open(temp_output_file_path, 'w') as outfile:
    for line in file:
        if any(user_content in line for user_content in user_contents):
            outfile.write(line)
            matches_found = True

# Replace the original file with the filtered content if any matches were found
if matches_found:
    os.replace(temp_output_file_path, second_file_path)
    print(f"Filtered data written to {second_file_path}")
else:
    os.remove(temp_output_file_path)
    print("No matches found. No changes made to the original file.")

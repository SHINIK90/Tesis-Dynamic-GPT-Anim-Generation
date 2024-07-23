import pandas as pd
import json
import os
import re

# Specify the path to the folder containing your JSON files
folder_path = './JsonAnims5-108k'

# Path to the text file containing your pre-prompt text
pre_prompt_file_path = './GPT_Creation_Prompt4.txt'

# Read the pre-prompt text from the .txt file
with open(pre_prompt_file_path, 'r', encoding='utf-8') as file:
    pre_prompt = file.read().strip().replace("\n", "")

# Initialize lists to hold column data
concepts = []
input = []
descriptions = []
texts = []

# List all files in the specified folder that are JSON files and not metadata
json_files = [f for f in os.listdir(folder_path) if f.endswith('.json') and not f.endswith('.meta')]

# Iterate over each JSON file
for file_name in json_files:
    # Construct the full path to the file
    file_path = os.path.join(folder_path, file_name)
    
    # Read the JSON file content
    with open(file_path, 'r') as file:
        description = json.load(file)
    
    # Convert the JSON object to a string for the Description column
    description_str = json.dumps(description, ensure_ascii=False).replace("\n", "")
    
    # Extract the concept from the file name (without the .json extension)
    pattern = r"\(\d\)"
    concept = re.sub(pattern, "", os.path.splitext(file_name)[0])
    
    # Format the text for the Text column
    text = f"human: {pre_prompt} based on these rules generate a json animation for {concept} \\n bot: {description_str}"
    
    # Append data to the lists
    concepts.append(concept)
    input.append("")
    descriptions.append(description_str)
    texts.append(text)

# Create a DataFrame with the collected data
df = pd.DataFrame({
    # 'prompt': concepts,
    # 'input':input,
    # 'output': descriptions,
    'text': texts
})

# Specify the path where you want to save the CSV file
csv_file_path = './train.csv'

# Save the DataFrame to a CSV file (uncomment the next line in actual use)
df.to_csv(csv_file_path, index=False)

# Print a message indicating completion (or remove for silent operation)
print(f"CSV file has been prepared and ready to be saved at: {csv_file_path}")

# Load your dataset
df = pd.read_csv('./train.csv')

# Strip whitespace from column names
print(df.columns)
print(df.columns.str.strip())

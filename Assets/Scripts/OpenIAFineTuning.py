import os
import json

# Path to the JSON file containing the system message
system_message_file = './GPT_Creation_Prompt9.txt'
user_content_file = './FileNames2048.txt'
# Path to the folder containing files for user and assistant messages
folder_path = './JsonAnims6-2048-DEG'
# Output .jsonl file path
# output_file_path = './gpt3-5-Finetuning.jsonl'
gpt_output = './gpt3-5-Finetuning2.jsonl'
babbage_output = './babbage-Finetuning.jsonl'

# Load the system message from the .txt file
with open(system_message_file, 'r') as f:
    system_message = f.read().strip()  # .strip() removes any leading/trailing whitespace

user_contents = []
with open(user_content_file, 'r') as file:
    user_contents = [line.strip() for line in file if line.strip()]  # Read each line, stripping whitespace

# Prepare a list to hold all conversation objects
conversations = []
conversation_babage = []

# Iterate over each file in the folder
i = 0
for filename in os.listdir(folder_path):
    if filename.endswith('.json'):
        
        # Load the assistant content from the file's JSON content
        file_path = os.path.join(folder_path, filename)
        with open(file_path, 'r') as f:
            assistant_content_json = json.load(f)
            assistant_content = json.dumps(assistant_content_json, ensure_ascii=False)
        
        # Construct the conversation object
        conversation = {
            "messages": [
                {"role": "system", "content": system_message},
                {"role": "user", "content": user_contents[i]},
                {"role": "assistant", "content": assistant_content}
            ]
        }
        
        # babbage
        conversation2 = {"prompt": user_contents[i], "completion": assistant_content}   #{"prompt": "<prompt text>", "completion": "<ideal generated text>"}

        # Append the conversation object to the list
        conversations.append(conversation)
        conversation_babage.append(conversation2)
        i = i+1

# Write the conversation objects to the .jsonl file
with open(gpt_output, 'w') as outfile:
    for conversation in conversations:
        json.dump(conversation, outfile)
        outfile.write('\n')

with open(babbage_output, 'w') as outfile:
    for conversation in conversation_babage:
        json.dump(conversation, outfile)
        outfile.write('\n')
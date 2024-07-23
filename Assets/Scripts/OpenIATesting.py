from openai import OpenAI

client = OpenAI(api_key="sk-proj-Jaido8hb9CKE3Afa6am3T3BlbkFJfJhAk8MdAmJEHk86a2SC")
# Replace "your_api_key_here" with your actual OpenAI API key
openai.api_key = 'sk-proj-Jaido8hb9CKE3Afa6am3T3BlbkFJfJhAk8MdAmJEHk86a2SC'

try:
       # Fetch the list of available models
    models = openai.Model.list()

    # Print the list of models
    for model in models.data:
        print(f"Model ID: {model.id} - Name: {model.name}")
except Exception as e:
    print(f"An error occurred: {str(e)}")

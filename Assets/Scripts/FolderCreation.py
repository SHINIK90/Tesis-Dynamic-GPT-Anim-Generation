import os

def create_folders_with_files(path):
    for i in range(1, 11):
        folder_name = str(i)
        folder_path = os.path.join(path, folder_name)
        os.makedirs(folder_path, exist_ok=True)
        
        for j in range(1, 41):
            file_name = f"{j}.json"
            file_path = os.path.join(folder_path, file_name)
            with open(file_path, 'w') as f:
                pass  # Create an empty file

# Replace 'your_path_here' with the actual path where you want to create the folders and files
path = 'D:/UnityProjects1Tb/Tesis Dynamic GPT Anim Generation/Assets/GPTsFiles/Scoring/PromptRepetition'
create_folders_with_files(path)

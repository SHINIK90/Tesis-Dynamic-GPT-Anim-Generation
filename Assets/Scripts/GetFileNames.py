import os
import csv

def generate_csv_from_folder(directory, csv_file_path):
    # List only .json files in the directory
    files = [f for f in os.listdir(directory) if os.path.isfile(os.path.join(directory, f)) and f.endswith('.json')]
    
    # Write file names to a CSV file, removing the '.json' extension
    with open(csv_file_path, 'w', newline='') as file:
        writer = csv.writer(file)
        for filename in files:
            writer.writerow([filename[:-5]])  # Slice off the last 5 characters (".json")

# Usage example
directory_path = './JsonAnims6-2048'  # Replace with the path to your directory
csv_output_path = './FileNames2048.csv'  # Path where the CSV will be saved
generate_csv_from_folder(directory_path, csv_output_path)

import json
import os

def get_size(obj):
    """Return the size of a JSON object in bytes."""
    return len(json.dumps(obj).encode('utf-8'))

def compile_json_files(folder_path, destination_path, max_size_mb=9):
    max_size_bytes = max_size_mb * 1024 * 1024  # Convert MB to bytes
    files = [f for f in os.listdir(folder_path) if f.endswith('.json')]
    
    compiled_objects = []
    current_size = 0
    file_counter = 1
    
    for file_name in files:
        file_path = os.path.join(folder_path, file_name)
        
        with open(file_path, 'r', encoding='utf-8') as file:
            obj = json.load(file)
            obj_size = get_size(obj)
            
            # Check if adding the current object would exceed the size limit
            if current_size + obj_size > max_size_bytes:
                # Write the current list of objects to a file
                output_file_path = os.path.join(destination_path, f'compiled_{file_counter}.json')
                with open(output_file_path, 'w', encoding='utf-8') as output_file:
                    # json.dump(compiled_objects, output_file, ensure_ascii=False, indent=4)
                    json.dump()
                    file.write()
                
                compiled_objects = [obj]
                current_size = obj_size
                file_counter += 1
            else:
                compiled_objects.append(obj)
                current_size += obj_size
    
    # Write any remaining objects to a file
    if compiled_objects:
        output_file_path = os.path.join(destination_path, f'compiled_{file_counter}.json')
        with open(output_file_path, 'w', encoding='utf-8') as output_file:
            json.dump(compiled_objects, output_file, ensure_ascii=False)

# Example usage
folder_path = './JsonAnims4'
destination_path = './CompressedAnims'
compile_json_files(folder_path,destination_path)

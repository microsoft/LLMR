import os
import shutil
def CleanUp(folder_path, file_name):


    # check if the file exists and delete it
    if os.path.isfile(file_name):
        os.remove(file_name)
        print(f"Deleted {file_name}")

    if os.path.isfile("TARGET.jpg"):
        os.remove("TARGET.jpg")
        print(f"Deleted TARGET.jpg")
  

    # check if the folder is empty and delete it
    if os.path.isdir(folder_path):
        shutil.rmtree(folder_path)
        print(f"Deleted {folder_path}")
    
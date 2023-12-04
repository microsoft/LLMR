import requests
import json

# the flask app url
url = "http://127.0.0.1:5000"

# the user prompt
user_prompt = "a siamese cat"

# create a JSON object with the user prompt as a field
data = {"user_prompt": user_prompt}

# send a POST request to the /get_image_from_prompt endpoint
response = requests.post(url + "/get_image_from_prompt", json=data)

# check the status code and the response content
if response.status_code == 200:
    print("Success!")
    print(response.json())
else:
    print("Error!")
    print(response.status_code)
    print(response.text)

# get the DALLE image base64 string from the response
dalle_image_base64 = response.json().get("dalle_image_base64")

# add the DALLE image base64 string to the JSON object
data["dalle_image_base64"] = dalle_image_base64

# send a POST request to the /get_closest_skfb_model endpoint
response = requests.post(url + "/get_closest_skfb_model", json=data)

# check the status code and the response content
if response.status_code == 200:
    print("Success!")
    print(response.json())
else:
    print("Error!")
    print(response.status_code)
    print(response.text)

# get the closest UID from the response
closest_uid = response.json().get("closest_uid")
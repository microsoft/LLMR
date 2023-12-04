import os
import openai
import requests



def prompt_dalle(userprompt):
    finalprompt = "a realistic 3D rendered image of " + userprompt + " with no background from a viewpoint where you can see the whole object"
    response = openai.Image.create(
    prompt=finalprompt,
    n=1,
    size="1024x1024"
    )
    image_url = response['data'][0]['url'] # get the url of the generated image
    image_name = "TARGET.jpg" # choose a name for the image file
    r = requests.get(image_url) # send a GET request to the image url
    with open(image_name, 'wb') as f: # open a file in write binary mode
        f.write(r.content) # write the content of the response to the file

        
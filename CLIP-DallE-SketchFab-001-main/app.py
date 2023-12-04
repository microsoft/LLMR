from flask import Flask, request, jsonify, render_template
from helpers.DownloadSketchFabModels import query_and_download_sketchfab_models
from helpers.PromptDALLE import prompt_dalle
from helpers.PromptDALLEScene import prompt_dalle_scene
from helpers.SeemAPI import get_sketchfab_model_descriptions
from helpers.CLIPFindClosestImage import CLIPFindClosestImage
from helpers.CLIPFindClosestText import CLIPFindClosestText
from helpers.Clean import CleanUp

import logging
import os
import base64
from io import BytesIO

app = Flask(__name__)
app.logger.setLevel(logging.DEBUG)
app.logger.handlers[0].setFormatter(logging.Formatter('%(asctime)s - %(levelname)s - %(message)s'))

@app.route('/')
def index():
   print('Request for index page received')
   return render_template('index.html')


@app.route('/get_image_from_prompt', methods=['POST'])
def get_image_from_prompt():
    # get the user prompt from the request body
    app.logger.info('Received a request from %s', request.remote_addr) # log the request source
    data = request.get_json()
    user_prompt = data.get('user_prompt')
    app.logger.debug('User prompt is %s', user_prompt) # log the user prompt
    print("***", request.get_json()["user_prompt"])
    data = request.get_json()
    
  
    user_prompt = data["user_prompt"]

    # generate the DALLE image from the prompt
    prompt_dalle(user_prompt)
    # encode the DALLE image as a base64 string
    # import base64
    # read the image file as binary data
    with open("TARGET.jpg", 'rb') as f:
        image_data = f.read()
    # encode the image data as a base64 string
    image_base64 = base64.b64encode(image_data).decode()
    # return the image base64 string as a response
    return jsonify({'dalle_image_base64': image_base64})



@app.route('/get_scene_from_prompt', methods=['POST'])
def get_scene_from_prompt():
    # get the user prompt from the request body
    app.logger.info('Received a request from %s', request.remote_addr) # log the request source
    data = request.get_json()
    user_prompt = data.get('user_prompt')
    app.logger.debug('User prompt is %s', user_prompt) # log the user prompt
    print("***", request.get_json()["user_prompt"])
    data = request.get_json()
    
  
    user_prompt = data["user_prompt"]

    # generate the DALLE image from the prompt
    prompt_dalle_scene(user_prompt)
    # encode the DALLE image as a base64 string
    # import base64
    # read the image file as binary data
    with open("SCENE.jpg", 'rb') as f:
        image_data = f.read()
    # encode the image data as a base64 string
    image_base64 = base64.b64encode(image_data).decode()
    # return the image base64 string as a response
    return jsonify({'dalle_scene_base64': image_base64})





@app.route('/get_closest_skfb_model', methods=['POST'])
def get_closest_skfb_model():
    # get the user prompt and the DALLE image filename from the request body
    data = request.get_json()
    user_prompt = data.get('user_prompt')
    dalle_image_filename = data.get('dalle_image_filename')
    # query and download the SketchFab models for the user prompt
    query_and_download_sketchfab_models(user_prompt)
    # get the SketchFab model descriptions from the Seem API
    captions_filename, done = get_sketchfab_model_descriptions(user_prompt)
    # find the closest text match for the DALLE image using CLIP
    labels, image_names = CLIPFindClosestText(captions_filename)
    # find the closest image match for the DALLE image using CLIP
    closest_uid = CLIPFindClosestImage(foldername=user_prompt, listofimages=image_names)
    # clean up the temporary files
    CleanUp(user_prompt, captions_filename)
    # return the UID of the closest model as a response
    return jsonify({'closest_uid': closest_uid})


if __name__ == '__main__':
   app.run()
# Import the required modules
import http.client, urllib.parse, io, os, json

def bbox_area(d):
    # d is a dictionary with keys x, y, w, h
    return d['w'] * d['h']

def get_sketchfab_model_descriptions(path_folder):
    # Define the request headers and parameters
    headers = {
        # Request headers
        'Content-Type': 'application/octet-stream',
        'Ocp-Apim-Subscription-Key': 'YOUR_API_KEY_GOES_HERE', #e.g 'c573...'
    }

    params = urllib.parse.urlencode({
        # Request parameters
        'features': 'denseCaptions',
        'model-name': '',
        'language': 'en',
        'smartcrops-aspect-ratios': '',
        'gender-neutral-caption': 'False',
    })

    # Initialize an empty dictionary to store the results
    results = {}

    # Loop through all the images in the table_images folder
    for filename in os.listdir(path_folder):
        
        # Skip any non-image files
        if not filename.endswith('.jpg'):
            continue
        # Use the io module to read the image as a byte stream
        with io.open(os.path.join(path_folder,filename), 'rb') as image:
            body = image.read()
        try:
            conn = http.client.HTTPSConnection('eastusa.cognitiveservices.azure.com')
            conn.request("POST", "/computervision/imageanalysis:analyze?api-version=2023-02-01-preview&%s" % params, body, headers)
            response = conn.getresponse()
            
            data = response.read()
            data = json.loads(data)
            print("****",data)
            if "denseCaptionsResult" in data:
                lst = data["denseCaptionsResult"]["values"]
                # Assuming the list is stored in a variable called lst
                highest_area = max(lst, key=lambda d: d['boundingBox']['w'] * d['boundingBox']['h']) # find the dictionary with the largest bounding box area
                # highest_confidence = max(lst, key=lambda d: d['confidence']) # find the dictionary with the highest confidence value
                print(highest_area) # print the result
                # Decode the data as a json object and store it in the results dictionary with the filename as the key
                results[filename] = highest_area
            conn.close()
        except Exception as e:
            print("[Errno {0}] {1}".format(e.errno, e.strerror))

    # Save the results dictionary as a json file
    fname = filename.replace(".jpg","") + 'results.json'
    with open(fname, 'w') as outfile:
        json.dump(results, outfile, indent=4)

    return fname, True

# get_sketchfab_model_descriptions("table_images")
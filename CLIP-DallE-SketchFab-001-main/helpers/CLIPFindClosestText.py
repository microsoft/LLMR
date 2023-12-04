import torch
import clip
from PIL import Image

# Import the json module
import json


def CLIPFindClosestText(captionfilename):

    # Open the json file and load the data
    with open(captionfilename) as f:
        data = json.load(f)
    listofcaptions = []
    listofimages = []

    for image_name, info in data.items():
        text = info['text']
        listofcaptions.append(text)
        listofimages.append(image_name)
 
    # Load the model
    device = "cuda" if torch.cuda.is_available() else "cpu"
    model, preprocess = clip.load('ViT-B/32', device)

    # Download the dataset


    # Prepare the inputs
    image = preprocess(Image.open("TARGET.jpg")).unsqueeze(0).to(device)
    # Calculate features
    text = clip.tokenize(listofcaptions).to(device)

    with torch.no_grad():
        image_features = model.encode_image(image)
        text_features = model.encode_text(text)
        
        logits_per_image, logits_per_text = model(image, text)
        print(image_features.size())
        print(text_features.size())
        probs = logits_per_image.softmax(dim=-1).cpu().numpy()

    
    # Find the indices of the top two probabilities
    indices = probs.squeeze().argsort()[-2:][::-1] # squeeze the tensor
    print("*****", indices)
    indices = indices.tolist()

    # Find the corresponding labels and image names
    labels = [listofcaptions[i] for i in indices]
    image_names = [listofimages[i] for i in indices]

    # Print the results
    print(f"The top two labels for the image are: {labels}")
    print(f"The image names are: {image_names}")

    return labels, image_names
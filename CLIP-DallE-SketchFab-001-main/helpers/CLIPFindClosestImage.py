import torch
import clip
from PIL import Image
from pathlib import Path


def CLIPFindClosestImage(foldername, listofimages):
    print(listofimages)

    listofimages = [Path(foldername).joinpath(image) for image in listofimages]
    # Load the model
    device = "cuda" if torch.cuda.is_available() else "cpu"
    model, preprocess = clip.load('ViT-B/32', device)

    # Prepare the inputs
    target_image = preprocess(Image.open("TARGET.jpg")).unsqueeze(0).to(device)
    # Calculate features
    images = torch.stack([preprocess(Image.open(image)).to(device) for image in listofimages])

    with torch.no_grad():
        target_image_features = model.encode_image(target_image)
        image_features = model.encode_image(images)
        
        logits_per_image = (target_image_features @ image_features.T).squeeze()
        print(target_image_features.size())
        print(image_features.size())
        probs = logits_per_image.softmax(dim=-1).cpu().numpy()

    
    # Find the index of the top probability
    index = probs.argmax() # squeeze the tensor
    print("*****", index)

    # Find the corresponding image name
    image_name = listofimages[index]

    # Print the result
    print(f"The closest image to the target image is: {image_name}")

    image_name = str(image_name).split("_")[-1].replace('.jpg','')

    print(f"The closest uiD to the target image is: {image_name}")

    return image_name
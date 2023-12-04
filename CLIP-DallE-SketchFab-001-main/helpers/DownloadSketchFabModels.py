# Import requests library for making HTTP requests
import requests
# Import os library for working with files and directories
import os
import random
def query_and_download_sketchfab_models(query):
  # Define the base URL for the Sketchfab API
  base_url = 'https://api.sketchfab.com/v3'

  # Define the authorization header with the user OAuth access token
  # Replace {INSERT_USER_OAUTH_ACCESS_TOKEN} with the actual token
  auth_header = {'authorization': 'Bearer 09eeace89d7643dba63fd19da7265477'}

  # Define the parameters for the search request
  # Limit the results to 10 and sort by popularity
  # Add the downloadable parameter to filter by download availability
  params = {'q': query, 'count': 100, 'isDownloadable': 'true'}

  # Make the search request and get the response object
  search_response = requests.get(base_url + '/search', params=params, headers=auth_header)

  # Check the status code of the response
  if search_response.status_code == 200:
    # Parse the JSON response
    search_data = search_response.json()

    # Extract the UIDs of the top 10 models from the response
    # Check the download attribute of each model before adding the UID
    #uids = [result['uid'] for result in search_data['results']['models'] if result['download']['available']]
   # uids = [result['uid'] for result in search_data['results']['models'] if result['download']]
    print(search_data['results']['models'][0])
    uids = [result['uid'] for result in search_data['results']['models'] if result['isDownloadable']]    # Loop through the UIDs and request the thumbnail image and the download URL for each model
    for uid in uids:
      # Make the thumbnail request and get the image URL
      thumbnail_response = requests.get(base_url + '/models/' + uid + '?width=512', headers=auth_header)
      if thumbnail_response.status_code == 200:
        thumbnail_url = thumbnail_response.json()['thumbnails']['images'][0]['url']
      elif thumbnail_response.status_code == 302:
        thumbnail_url = thumbnail_response.headers['Location']
      else:
        print(f'Thumbnail request failed with status code {thumbnail_response.status_code}')

      # Print the image URL and the download URL for each model
      print(f'Model {uid} has a thumbnail image at:')
      print(thumbnail_url)
      # Get the download URL from the download attribute of the model
      download_url = thumbnail_response.json()['uri']
      print(f'Model {uid} can be downloaded from:')
      print(download_url)
      # Download the image and save it to a local file
      image_response = requests.get(thumbnail_url, headers=auth_header)
      if image_response.status_code == 200:
        # Create a directory named after the query if it does not exist
        if not os.path.exists(query):
          os.mkdir(query)
        # Count how many files are in the directory with the same name as the query
        file_count = len([f for f in os.listdir(query) if f.startswith(query)])
        # Generate a file name based on the query and the file count
        file_name = query + '_' + str(file_count + 1) + '_' + uid + '.jpg'
        # Join the directory and the file name to get the full path
        file_path = os.path.join(query, file_name)
        # Open the file in write binary mode and write the image content
        with open(file_path, 'wb') as image_file: # Use jpg as the extension as the thumbnails are JPEG images
          image_file.write(image_response.content) # Write the binary content of the response to the file
      else:
        print(f'Image request failed with status code {image_response.status_code}')

  else:
    # Handle the error
    print(f'Search request failed with status code {search_response.status_code}')

# get_objects("table")
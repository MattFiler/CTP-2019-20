#Thanks: https://medium.com/@muskulpesent/create-numpy-array-of-images-fecb4e514c4b

import cv2
import glob
import numpy as np
#Train data
train = []
train_labels = []
files = glob.glob ("../Builds/StreetviewRipper/Output/Images/PulledClouds/BestMatch/NormalisedSizes/*STREETVIEW_LDR.png") # your image path
for myFile in files:
    image = cv2.imread (myFile)
    train.append (image)
    train_labels.append([1., 0.])
files = glob.glob ("../Builds/StreetviewRipper/Output/Images/PulledClouds/BestMatch/NormalisedSizes/*CLOUD_MASK.png")
for myFile in files:
    image = cv2.imread (myFile)
    train.append (image)
    train_labels.append([0., 1.])
train = np.array(train,dtype='float32') #as mnist
train_labels = np.array(train_labels,dtype='float64') #as mnist
# convert (number of images x height x width x number of channels) to (number of images x (height * width *3)) 
# for example (120 * 40 * 40 * 3)-> (120 * 4800)
#train = np.reshape(train,[train.shape[0],train.shape[1]*train.shape[2]*train.shape[3]])

# save numpy array as .npy formats
np.save('DeepDoodle/x_data',train)
np.save('DeepDoodle/y_data',train_labels)
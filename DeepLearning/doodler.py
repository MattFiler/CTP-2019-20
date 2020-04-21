import random
import numpy as np
import cv2
from dutil import *
import struct

#User constants
device = "gpu"
model_fname = 'Model.h5'
input_w = 144
input_h = 192

#Keras
print("Loading Keras...")
import os
os.environ['THEANORC'] = "./" + device + ".theanorc"
os.environ['KERAS_BACKEND'] = "theano"
import theano
print("Theano Version: " + theano.__version__)
from keras.models import Sequential, load_model
from keras import backend as K
K.set_image_data_format('channels_first')

#Load properties
fin = open("params.bin", "rb")
model_dims = (0,0,0)
for x in range(3):
    model_dims[x] = struct.unpack('f', fin.read(4))[0]
fin.close()

#Load the model
print("Loading Model...")
model = build_model((3,192,144))
model.load_weights(model_fname)

print("Predicting...")
cur_drawing = np.zeros((1, input_h, input_w), dtype=np.uint8)
fdrawing = np.expand_dims(cur_drawing.astype(np.float32) / 255.0, axis=0)
pred = model.predict(add_pos(fdrawing), batch_size=1)[0]


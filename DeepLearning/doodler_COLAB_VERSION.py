import random
import numpy as np
import cv2

def add_pos(arr):
	s = arr.shape
	result = np.empty((s[0], s[1] + 2, s[2], s[3]), dtype=np.float32)
	result[:,:s[1],:,:] = arr
	x = np.repeat(np.expand_dims(np.arange(s[3]) / float(s[3]), axis=0), s[2], axis=0)
	y = np.repeat(np.expand_dims(np.arange(s[2]) / float(s[2]), axis=0), s[3], axis=0)
	result[:,s[1] + 0,:,:] = x
	result[:,s[1] + 1,:,:] = np.transpose(y)
	return result

#User constants
device = "gpu"
model_fname = 'Model.h5'
input_w = 144
input_h = 192

#Global variables
prev_mouse_pos = None
mouse_pressed = False
needs_update = True
cur_color_ix = 1
cur_gen = np.zeros((3, input_h, input_w), dtype=np.uint8)
cur_drawing = cv2.imread("user_drawing.png")
cur_drawing = np.reshape(cur_drawing[:, :, 0], (1, input_h, input_w))

#Keras
print("Loading Keras...")
import os
os.environ['SDL_VIDEODRIVER']='dummy'
os.environ['THEANORC'] = "./" + device + ".theanorc"
os.environ['KERAS_BACKEND'] = "theano"
import theano
print("Theano Version: " + theano.__version__)
from keras.models import Sequential, load_model
from keras import backend as K
K.set_image_data_format('channels_first')

#Load the model
print("Loading Model...")
model = load_model(model_fname)

#Predict using the model
fdrawing = np.expand_dims(cur_drawing.astype(np.float32) / 255.0, axis=0)
pred = model.predict(add_pos(fdrawing), batch_size=1)[0]
cur_gen = (pred * 255.0).astype(np.uint8)

#Output prediction
cv2.imwrite("prediction.png", np.transpose(cur_gen, (2, 1, 0)))
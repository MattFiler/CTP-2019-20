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

def auto_canny(image, sigma=0.0):
	gray = cv2.cvtColor(image, cv2.COLOR_RGB2GRAY)
	grayed = np.where(gray < 20, 255, 0)

	lower = sigma*128 + 128
	upper = 255
	edged = cv2.Canny(image, lower, upper)

	return np.maximum(edged, grayed)

def save_image(x, fname):
	img = np.transpose(x * 255, (1, 2, 0))
	img = cv2.cvtColor(img.astype(np.uint8), cv2.COLOR_RGB2BGR)
	cv2.imwrite(fname, img)
    
import os, math
os.environ['THEANORC'] = "./gpu.theanorc"
os.environ['KERAS_BACKEND'] = "theano"
import theano
from keras.layers import Input, Dense, Activation, Dropout, Flatten, Reshape, concatenate
from keras.layers.convolutional import Conv2D, Conv2DTranspose, UpSampling2D
from keras.layers.local import LocallyConnected2D
from keras.layers.pooling import MaxPooling2D
from keras.layers.noise import GaussianNoise
from keras.models import Model, Sequential, load_model
from keras.optimizers import Adam, RMSprop, SGD
from keras.regularizers import l2
from keras.utils import plot_model
from keras import backend as K
K.set_image_data_format('channels_first')

lr = 0.0008

def build_model(shape):
    model = Sequential()

    model.add(Conv2D(48, (5, 5), padding='same', input_shape=shape))
    model.add(Activation("relu"))
    model.add(MaxPooling2D(pool_size=(2,2)))
    model.add(Conv2D(96, (5, 5), padding='same'))
    model.add(Activation("relu"))
    model.add(MaxPooling2D(pool_size=(2,2)))
    model.add(Conv2D(192, (5, 5), padding='same'))
    model.add(Activation("relu"))
    model.add(MaxPooling2D(pool_size=(2,2)))
    model.add(Conv2D(192, (5, 5), padding='same'))
    model.add(Activation("relu"))
    model.add(UpSampling2D(size=(2,2)))
    model.add(Conv2D(192, (5, 5), padding='same'))
    model.add(Activation("relu"))
    model.add(UpSampling2D(size=(2,2)))
    model.add(Conv2D(96, (5, 5), padding='same'))
    model.add(Activation("relu"))
    model.add(UpSampling2D(size=(2,2)))
    model.add(Conv2D(48, (5, 5), padding='same'))
    model.add(Activation("relu"))
    model.add(Conv2D(3, (1, 1), padding='same'))
    model.add(Activation("sigmoid"))

    model.compile(optimizer=Adam(lr=lr), loss='mse')
    return model
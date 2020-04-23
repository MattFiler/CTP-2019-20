import os, random, sys
import numpy as np
import cv2
from dutil import *

NUM_IMAGES = %%IMG_COUNT%%
IMAGE_W = 144
IMAGE_H = 192
IMAGE_DIR = '../Builds/StreetviewRipper/Output/Images/'
NUM_SAMPLES = NUM_IMAGES * 2
NUM_CHANNELS = 3

def yb_resize(img):
	return cv2.resize(img, (IMAGE_W, IMAGE_H), interpolation = cv2.INTER_LINEAR)

print("Compiling...")
x_data = np.empty((NUM_SAMPLES, NUM_CHANNELS, IMAGE_H, IMAGE_W), dtype=np.uint8)
y_data = np.empty((NUM_SAMPLES, 3, IMAGE_H, IMAGE_W), dtype=np.uint8)
ix = 0
for root, subdirs, files in os.walk(IMAGE_DIR):
    for file in files:
        path = root + "\\" + file
        if not path[len(path)-12:len(path)] == ".SKY_LDR.jpg":
            continue

        img = cv2.imread(path)
        img_mask = cv2.imread(path[0:len(path)-12] + ".CLOUD_MASK.png")

        if img is None:
            assert(False)
        if len(img.shape) != 3 or img.shape[2] != 3:
            assert(False)

        img = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
        img = cv2.resize(img, (IMAGE_W, IMAGE_H), interpolation = cv2.INTER_LINEAR)
        img_mask = cv2.cvtColor(img_mask, cv2.COLOR_BGR2RGB)
        img_mask = cv2.resize(img_mask, (IMAGE_W, IMAGE_H), interpolation = cv2.INTER_LINEAR)

        y_data[ix] = np.transpose(img, (2, 0, 1))
        x_data[ix] = np.transpose(img_mask, (2, 0, 1))
        ix += 1
        y_data[ix] = np.flip(y_data[ix - 1], axis=2)
        x_data[ix] = np.flip(x_data[ix - 1], axis=2)
        ix += 1

        sys.stdout.write('\r')
        progress = ix * 100 / NUM_SAMPLES
        sys.stdout.write(str(progress) + "%")
        sys.stdout.flush()
        if ix >= NUM_SAMPLES:
            break;

assert(ix == NUM_SAMPLES) #We'll only get this if NUM_IMAGES is wrong

print("\nSaving...")
np.save('x_data.npy', x_data)
np.save('y_data.npy', y_data)
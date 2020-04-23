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

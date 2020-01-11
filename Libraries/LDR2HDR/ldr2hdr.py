import tensorflow as tf
import numpy as np

from scipy.misc import imresize, imread
from glob import glob
import os

'''import Imath'''

from ldr2hdr_loader import LDR2HDR_Loader

'''This model takes [128, 64, 128, 3] data as input'''
im_height = 64
im_width = 128
batch_size = 128


class LDR2HDR(object):

    def __init__(self, sess=None):
        self.im_height = 64
        self.im_width = 128
        self.batch_size = 128
        self.sess = sess
        if self.sess is None:
            self.sess = tf.Session(config=tf.ConfigProto(log_device_placement=False))

        self.model_path = './model_DomainAdapt'  # Try the finetune model to get best performance on thetaS images

        self.fname_model = self._locateModel(self.model_path)
        self._load()

    def _locateModel(self, path):
        matafiles = glob(os.path.join(path, '*.meta'))
        return matafiles[-1]

    def _load(self):
        '''load the model with TF backend'''
        self.model = LDR2HDR_Loader(self.sess)
        self.model.load_tf_model(self.fname_model)


def setData(fname=''):
    # load ldr image, resize it to feed the our model
    im = imresize(imread(fname), [im_height, im_width]).astype('float32') / 255.
    ims = np.repeat(np.reshape(im, [1, im_height, im_width, 3]), batch_size, 0)
    return ims


def writeHDR(arr, outfilename):
    '''write HDR image (https://gist.github.com/edouardp/3089602)'''
    f = open(outfilename, "wb")
    f.write(("#?RADIANCE\n# Made with Python & Numpy\nFORMAT=32-bit_rle_rgbe\n\n").encode())
    f.write(("-Y {0} +X {1}\n".format(im_height, im_width)).encode())
    
    brightest = np.maximum(np.maximum(arr[...,0], arr[...,1]), arr[...,2])
    mantissa = np.zeros_like(brightest)
    exponent = np.zeros_like(brightest)
    np.frexp(brightest, mantissa, exponent)
    scaled_mantissa = mantissa * 256.0 / brightest
    rgbe = np.zeros((im_height, im_width, 4), dtype=np.uint8)
    rgbe[...,0:3] = np.around(arr[...,0:3] * scaled_mantissa[...,None])
    rgbe[...,3] = np.around(exponent + 128)
    
    rgbe.flatten().tofile(f)
    f.close()


def demo():
    '''predicting the HDR from a single LDR'''
    # define the LDR2HDR model
    ldr2hdr = LDR2HDR()

    # format the input
    ims = setData(fname='./streetview.jpg')
    # prediction
    preds, _ = ldr2hdr.model.forward(ims)
    pred = np.reshape(preds[0, ...], [64, 128, 3])

    # write HDR image
    writeHDR(pred, 'streetview.hdr')


if __name__ == '__main__':
    demo()

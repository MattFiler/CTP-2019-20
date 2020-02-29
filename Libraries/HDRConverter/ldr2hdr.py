import numpy as np
import imageio

def readHDR(file):
    im = imageio.imread(file)
    return im

def writeHDR(arr, outfilename):
    im_height = arr.shape[0]
    im_width = arr.shape[1]
    
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
    
if __name__ == '__main__':
    writeHDR(readHDR('input.hdr'), 'output.hdr')

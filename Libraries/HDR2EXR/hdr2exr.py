import glob
import os
import cv2

hdr = cv2.imread("input.hdr",-1)
cv2.imwrite("output.exr",hdr)
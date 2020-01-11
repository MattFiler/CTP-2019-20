import glob
import os
import cv2

hdr = cv2.imread("input.exr",-1)
cv2.imwrite("output.hdr",hdr)
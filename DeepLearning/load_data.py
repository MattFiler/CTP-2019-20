import struct

# Read in cloudmap
fin = open("F:/Github Repos/CTP-2019-20/Output/Images/SKULUDwlJQa9o9yZot82ew_cloudmap.bin", "rb")
width = (struct.unpack('i', fin.read(4)))[0]
height = (struct.unpack('i', fin.read(4)))[0]

cloudmap = [False] * (width*height)
for x in range(width*height):
    cloudmap[x] = struct.unpack('?', fin.read(1))[0]

fin.close()

# Read in depth values
fin = open("F:/Github Repos/CTP-2019-20/Output/Images/SKULUDwlJQa9o9yZot82ew_depth.bin", "rb")
width = (struct.unpack('i', fin.read(4)))[0]
height = (struct.unpack('i', fin.read(4)))[0]

depth = [0.0] * (width*height)
for x in range(width*height):
    depth[x] = struct.unpack('f', fin.read(4))[0]

fin.close()

print("loaded cloudmap & depthmap")
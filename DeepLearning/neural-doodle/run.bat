call conda activate tensorflow_gpu
python doodle.py --style samples/orig_sky.png --content samples/hosek_sky.png --output test.png --device=cuda* --phases=4 --iterations=40
pause
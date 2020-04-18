# Matt Filer CTP (17021871)

This repo contains all project files for my dissertation: a skybox generation tool for games.


## Contents

### Main projects (by folder name)
- **StreetviewRipper**: tool for downloading Streetview images and optionally processing them
- **Raytracer**: project for producing renders of volumetric data structures
- **WebAPI**: the PHP Streetview API used for StreetviewRipper
- **DeepLearning**: deep learning projects and various tests/scripts

### Other folders
- **Builds**: built binaries for both the StreetviewRipper and Raytracer projects
- **Demo**: materials from the progress demo (poster/video)
- **Libraries**: all libraries required by StreetviewRipper and Raytracer projects
- **Tests**: test projects, typically for refining processing steps


## Set up

**To allow processing images in StreetviewRipper:**

- Download and install Anaconda (Python 3.7)
- Add Anaconda to PATH
- Download and install MATLAB
- Open Anaconda and execute:
    - `conda create -n streetviewripper pip python=3.7`
    - `conda activate streetviewripper`
    - `conda install numpy`
    - `conda install opencv`
    - `conda install pillow`
	- `conda install imageio`
    - `pip install --ignore-installed --upgrade tensorflow==1.14`
    - `pip install scipy==1.2.0`
- Open command prompt and execute:
	- `conda init cmd.exe`
    
**To be able to build the raytracer:**

- Extract `Libraries/OpenVDB/OpenVDB.zip` to `Libraries/OpenVDB`
- Download and build vcpkg
- Open command prompt inside vcpkg build folder and execute:
    - `vcpkg install openvdb:x64-windows`
    - `vcpkg integrate install`
	
**To be able to use DeepDoodle:**

- Download and install Anaconda (Python 3.7)
- Open Anaconda and execute:
    - `conda create -n ctp201920_deeplearning pip python=3.7`
    - `conda activate ctp201920_deeplearning`
    - `conda install numpy`
    - `conda install matplotlib`
    - `conda install opencv`
    - `conda install theano`
    - `conda install keras`
    - `conda install pydot`
- Download and install CUDA 9.1
- Download and install cuDNN 9.1 (v7.0.5)
	
	
## About StreetviewRipper

StreetviewRipper is a tool designed to be able to automatically generate a large dataset of cloud imagery. To use it, open Google Maps, find a decent Streetview sphere, copy the URL, and paste it into the tool's textbox.

You can choose to recurse into neighbours (this will find neighbours of the photo sphere, and keep automatically going using those), or alternatively if you want to curate the results manually, you can post multiple Streetview links into the textbox (each on a new line) and disable recursion, so the tool will work through your URLs instead of automatically picking new ones.

Optionally, image processing can be disabled - this will just download the regular Streetview LDR images without performing any processing. The set up instructions above only need to be followed if you plan on enabling image processing. If you are just downloading LDR images, the tool works standalone.

If image processing and recursion is enabled, you can set the number of neighbours to skip. This will provide you with a more diverse processed dataset, as instead of processing direct neighbours, the tool will skip a given number of neighbours before selecting a new photo sphere to process.

To stop execution, press the "stop" button. The tool will stop execution once operations on the current image have completed.

If "cut out clouds" is enabled when processing, the tool will use the generated cloud mask to automatically cut out clouds from the processed images, and filter the results by what it thinks are best/ok/worst. Worst and ok images typically contain trees or branches - the tool will do its best to isolate clouds into the "best" category. Enable this option if you are going to use the dataset for training. If you wish to retroactively cut out clouds, utilise the "CloudPuller" program in the "Tests" folder.

## Using the StreetviewRipper output to train

To train the deep learning with StreetviewRipper's output, you must first normalise the dimensions of the images you've gathered. To do this, build the "NormaliseAllImageSizes" program in the "Tests" folder, and place it in "Builds/StreetviewRipper/Output/Images/PulledClouds/BestMatch/". Run this program and it will automatically go through all the best cut out clouds and create copies that all have matching resolutions.

Once this has finished processing, edit the "gpu.theanorc" file within the "DeepLearning" folder. Change the `compiler_bindir` value to match your Visual Studio compiler directory, and the `include_path`/`library_path` to match your cuDNN/CUDA directories. Also edit "datagen.py" and change the `IMAGE_W`/`IMAGE_H` to match the width and height of your resized cloud images (respectively), and change `NUM_IMAGES` to match the number of images in your dataset (this will be half the number of images in the "NormalisedSizes" folder).

You can now run "run.bat" - this will convert your newly resized best cloud images from StreetviewRipper into a format that can be used for training, and then initialise training, using the DeepDoodle network.


## Additional notes

The C# MATLAB API can sometimes crash after a series of calls - for this reason, the branch "matlab-local-fix" was created, which automatically opens MATLAB and runs the script within the program rather than through the API. This is useful for generating a large dataset of processed images. You will need to adjust the MATLAB path in code and recompile StreetviewRipper to utilise this fix, found on the "matlab-local-fix" branch. Check that the branch is not behind master before using it (may have fallen behind, merge it to update).


## Useful links

- [Hosted Streetview Web API](http://streetview.mattfiler.co.uk)
- [Load Streetview by ID](http://streetview.mattfiler.co.uk/loadpano.php)
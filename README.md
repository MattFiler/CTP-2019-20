# Matt Filer CTP (17021871)

This repo contains all project files for my dissertation: a skybox generation tool for games.

<img align="right" src="https://i.imgur.com/QIbvKNN.png" width="40%">


## Contents

### Main projects (by folder name)
- **StreetviewRipper**: tool for downloading panoramic images and optionally processing them
- **Raytracer**: project for producing renders of volumetric data structures
- **WebAPI**: the PHP Streetview API used for StreetviewRipper
- **DeepLearning**: deep learning scripts utilising DeepDoodle

### Other folders
- **Builds**: binaries for the StreetviewRipper and Raytracer projects, plus a tool for deep learning
- **Demo**: materials from the progress demo (poster/video)
- **Libraries**: all libraries required by StreetviewRipper and Raytracer projects
- **Tests**: test and other projects, typically for refining processing steps


## Set up

**To allow processing images in StreetviewRipper:**

- Download and install Anaconda (Python 3.7)
- Add Anaconda to PATH
- Download and install MATLAB
- Open Anaconda and execute:
    - `conda create -y -n streetviewripper pip python=3.7`
    - `conda activate streetviewripper`
    - `conda install -y numpy`
    - `conda install -y opencv`
    - `conda install -y pillow`
	- `conda install -y imageio`
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
	
**To be able to use the neural network:**

- Download and install Anaconda (Python 3.7)
- Open Anaconda and execute:
    - `conda create -y -n ctp201920_deeplearning pip python=3.7`
    - `conda activate ctp201920_deeplearning`
    - `conda install -y numpy`
    - `conda install -y matplotlib`
    - `conda install -y opencv`
    - `conda install -y theano`
    - `conda install -y keras`
    - `conda install -y pydot`
	- `pip install pygame`
- Open command prompt and execute:
	- `conda init cmd.exe`
- Download and install CUDA 9.1
- Download and install cuDNN 9.1 (v7.0.5)
- An install of Visual Studio with build tools
	
	
## About data collection

The data collection tool is designed to be able to automatically generate a large dataset of cloud imagery. A build is available within the "Builds/StreetviewRipper" directory. To use it, open Google Maps, find a decent Streetview sphere, copy the URL, and paste it into the tool's textbox.

You can choose to recurse into neighbours (this will find neighbours of the photo sphere, and keep automatically going using those), or alternatively if you want to curate the results manually, you can post multiple Streetview links into the textbox (each on a new line) and disable recursion, so the tool will work through your URLs instead of automatically picking new ones.

Optionally, image processing can be disabled - this will just download the regular Streetview LDR images without performing any processing. The set up instructions above only need to be followed if you plan on enabling image processing. If you are just downloading LDR images, the tool works standalone.

If image processing and recursion is enabled, you can set the number of neighbours to skip. This will provide you with a more diverse processed dataset, as instead of processing direct neighbours, the tool will skip a given number of neighbours before selecting a new photo sphere to process.

To stop execution, press the "stop" button. The tool will stop execution once operations on the current image have completed.

## Using the neural network

A neural network based on [DeepDoodle](https://github.com/HackerPoet/DeepDoodle/) is included with the project for deep learning. A tool is available within "Builds/DeepLearning" which can configure your environment and manage training/launching the doodler program. When you first launch the tool, enter your Visual Studio VC bin directory and CUDA version directory, then press save.

If you have collected a cloud dataset using the data collection toolkit, you can press "Start Training". This will launch a command prompt window showing you progress - it will take some time to complete compilation and training based on the number of images you have collected. It is recommended that you collect at least 25 images. If you would like to adjust the number of epochs in the training step, you can edit this value within "train.py", located in the "DeepLearning" folder.

Alternatively to save training yourself, a pre-trained model is available in the "DeepLearning/Pre-trained" folder. To use it instead of training on your own data, copy it out into the root "DeepLearning" folder.

With your training complete (or pre-trained model copied) you can now press "Launch Doodler" to interact with the network in realtime.

Unfortunately it is common for DLL errors to occur when launching the "doodler" application even with the correct environment setup, and so for that reason an online hosted version is available using [Google Colab](https://colab.research.google.com/drive/14z7ubtetZLTGWncskRJEqVytSfD-_xfG#scrollTo=Xpf8BvcZViuq&forceEdit=true&sandboxMode=true). To use this online version, go into the "DeepLearning" folder and launch the "local_doodle_creator.bat" file - draw a map for the network to process. This drawing will output to "user_drawing.png", take this file, upload it to the [Google Colab server](https://colab.research.google.com/drive/14z7ubtetZLTGWncskRJEqVytSfD-_xfG#scrollTo=Xpf8BvcZViuq&forceEdit=true&sandboxMode=true) using the provided upload code block, and then run the doodler code block. This will auto download the result as a png file.


## Additional notes

The C# MATLAB API can sometimes crash after a series of calls - for this reason, the branch "matlab-local-fix" was created, which automatically opens MATLAB and runs the script within the program rather than through the API. This is useful for generating a large dataset of processed images. You will need to adjust the MATLAB path in code and recompile StreetviewRipper to utilise this fix, found on the "matlab-local-fix" branch. Check that the branch is not behind master before using it (may have fallen behind, merge it to update).


## Useful links

- [Hosted Streetview Web API](http://streetview.mattfiler.co.uk)
- [Load Streetview by ID](http://streetview.mattfiler.co.uk/loadpano.php)
- [Google Colab project](https://colab.research.google.com/drive/14z7ubtetZLTGWncskRJEqVytSfD-_xfG)
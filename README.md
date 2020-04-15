# Matt Filer CTP (17021871)

This repo contains all project files for my dissertation: a skybox generation tool for games.

## Contents

### Main projects (by folder name)
- **StreetviewRipper**: tool for downloading Streetview images and optionally processing them
- **Raytracer**: project for producing renders of volumetric data structures
- **WebAPI**: the PHP Streetview API used for StreetviewRipper
- **DeepLearning**: unfinished deep learning tests

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
    - `conda install opencv`
    - `pip install --ignore-installed --upgrade tensorflow==1.14`
    - `pip install scipy==1.2.0`
    - `conda install pillow`
	- `conda install imageio`
- Open command prompt and execute:
	- `conda init cmd.exe`
    
**To be able to build the raytracer:**

- Extract `Libraries/OpenVDB/OpenVDB.zip` to `Libraries/OpenVDB`
- Download and build vcpkg
- Open command prompt inside vcpkg build folder and execute:
    - `vcpkg install openvdb:x64-windows`
    - `vcpkg integrate install`


## Useful links

- [Hosted Streetview Web API](http://streetview.mattfiler.co.uk)
- [Load Streetview by ID](http://streetview.mattfiler.co.uk/loadpano.php)
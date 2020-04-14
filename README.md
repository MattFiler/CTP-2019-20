# CTP-2019-20

### Main projects
- **StreetviewRipper**: tool for downloading Streetview images and optionally processing them
- **Raytracer**: project for producing renders of volumetric data structures

### To set up
To allow processing images in StreetviewRipper:

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
    
To be able to build the raytracer:

- Extract `Libraries/OpenVDB/OpenVDB.zip` to `Libraries/OpenVDB`
- Download and build vcpkg
- Open command prompt inside vcpkg build folder and execute:
    - `vcpkg install openvdb:x64-windows`
    - `vcpkg integrate install`

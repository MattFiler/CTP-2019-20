function [hdrlum, ldrlum] = LoadFromID(ID)
    image = hdrread(strcat('../Output/Images/',strcat(ID,'.hdr'))); 
    ycbcr = rgb2ycbcr(image); 
    hdrlum = ycbcr(:,:,1);
    
    image = imread(strcat('../Output/Images/',strcat(ID,'_shifted.jpg')));
    ycbcr = rgb2ycbcr(image);
    ldrlum = single(ycbcr(:,:,1)) ./ 255.0;
end


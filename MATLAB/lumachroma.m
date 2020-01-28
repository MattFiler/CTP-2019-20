STREETVIEW_ID = 'xdU_R-qfflPfs8x-tTKM8g';

%Downscaled LDR
image = imread(strcat('../Output/Images/',strcat(STREETVIEW_ID,'_downscaled.jpg')));
ycbcr = rgb2ycbcr(image);
y = ycbcr(:,:,1);
figure
imshow(y);
title('Downscaled LDR Image Luma');

%Regular size LDR
image = imread(strcat('../Output/Images/',strcat(STREETVIEW_ID,'_shifted.jpg')));
ycbcr = rgb2ycbcr(image);
y = ycbcr(:,:,1);
figure
imshow(y);
title('Regular LDR Image Luma');

%Regular size HDR
image = hdrread(strcat('../Output/Images/',strcat(STREETVIEW_ID,'.hdr'))); 
ycbcr = rgb2ycbcr(image); 
y = ycbcr(:,:,1);
figure
imshow(y); 
title('Regular HDR Image Luma');

%Upscaled HDR
image = hdrread(strcat('../Output/Images/',strcat(STREETVIEW_ID,'_upscaled.hdr'))); 
ycbcr = rgb2ycbcr(image); 
y = ycbcr(:,:,1);
figure
imshow(y); 
title('Upscaled HDR Image Luma');
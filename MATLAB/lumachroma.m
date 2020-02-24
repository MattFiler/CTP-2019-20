STREETVIEW_ID = 'xdU_R-qfflPfs8x-tTKM8g';
close all;

% Load luma data from HDR image matching ID
hdrimage = hdrread(strcat('../Output/Images/',strcat(STREETVIEW_ID,'.hdr'))); 
ycbcr = rgb2ycbcr(hdrimage); 
hdrlum = ycbcr(:,:,1);

% Load luma data from LDR image matching ID
ldrimage = imread(strcat('../Output/Images/',strcat(STREETVIEW_ID,'_shifted.jpg')));
ycbcr = rgb2ycbcr(ldrimage);
ldrlum = single(ycbcr(:,:,1)) ./ 255.0;
clear ycbcr;

% Create a histogram from the luma data
hdrhist = hist(reshape(hdrlum, [size(hdrlum, 1) * size(hdrlum, 2), 1]), 100);
ldrhist = hist(reshape(ldrlum, [size(ldrlum, 1) * size(ldrlum, 2), 1]), 100);

% Bring our values down to a normalised range
hdrhist = hdrhist ./ (size(hdrlum, 1) * size(hdrlum, 2));
ldrhist = ldrhist ./ (size(ldrlum, 1) * size(ldrlum, 2));

% Try and match both distributions
hdrhistmod = zeros(1, 100);
hdrhistdiff = zeros(1, 100);
leftover = zeros(1, 100);
leftover_total = 0;
for x = 1:100
    thisLDRValue = ldrhist(1, x);
    thisHDRValue = hdrhist(1, x);
    
    leftover_total = leftover_total + thisHDRValue;
    hdrhistmod(1, x) = thisLDRValue;
    leftover_total = leftover_total - thisLDRValue;
    hdrhistdiff(1, x) = leftover_total;
    leftover(1, x) = thisHDRValue - thisLDRValue;
    
    if leftover_total <= 0
        %error("Out of entries at iteration " + c);
        break;
    end
end

% Show the original HDR/LDR images for sanity
figure;
imshow(ldrlum);
title("LDR LUM");
figure;
imshow(hdrlum);
title("HDR LUM");

% Get the highest and lowest values in the LDR image
lowest_ldr_val = 0;
highest_ldr_val = 0;
for x = 1:size(ldrlum, 1)
   for y = 1:size(ldrlum, 2)
       if ldrlum(x, y) > highest_ldr_val
          highest_ldr_val = ldrlum(x, y);
       end
       if ldrlum(x, y) < lowest_ldr_val
          lowest_ldr_val = ldrlum(x, y);
       end
   end
end

% Get the highest and lowest values in the HDR image
lowest_hdr_val = 0;
highest_hdr_val = 0;
for x = 1:size(hdrlum, 1)
   for y = 1:size(hdrlum, 2)
       if hdrlum(x, y) > highest_hdr_val
          highest_hdr_val = hdrlum(x, y);
       end
       if hdrlum(x, y) < lowest_hdr_val
          lowest_hdr_val = hdrlum(x, y);
       end
   end
end

% Try and change the values for the HDR image to match LDR
reshaped_hdr = zeros(size(hdrlum, 1), size(hdrlum, 2));
for x = 1:size(hdrlum, 1)
    for y = 1:size(hdrlum, 2)
        graph_offset = -1;
        hist_cumulative = 0;
        for i = 1:100
           hist_cumulative = hist_cumulative + hdrhist(1, i);
           if hdrlum(x, y) > hist_cumulative
               graph_offset = i;
           end
        end
        reshaped_hdr(x, y) = hdrlum(x, y) + leftover(1, graph_offset);
    end
end

% Show the new HDR image
figure;
imshow(reshaped_hdr);
title("HDR LUM RESHAPE");

% Resize the HDR image & show
%reshaped_upscaled_hdr = imresize(reshaped_hdr, [size(ldrlum, 1), size(ldrlum, 2)], 'nearest');
reshaped_upscaled_hdr = imresize(reshaped_hdr, [size(ldrlum, 1), size(ldrlum, 2)]);
figure;
imshow(reshaped_upscaled_hdr);
title("HDR LUM RESHAPE UPSCALE");

% Convert back to RGB
ycbcr = rgb2ycbcr(ldrimage); 
ycbcr(:,:,1) = reshaped_upscaled_hdr * 255; % Remove 255
ycbcr = ycbcr2rgb(ycbcr);
figure;
imshow(ycbcr);
title("HDR RGB RESHAPE UPSCALE");

% Create a histogram from the new HDR luma
reshaped_hdrhist = hist(reshape(reshaped_hdr, [size(reshaped_hdr, 1) * size(reshaped_hdr, 2), 1]), 100);
reshaped_hdrhist = reshaped_hdrhist ./ (size(reshaped_hdr, 1) * size(reshaped_hdr, 2));

% Create a histogram from the new HDR (upscaled) luma
reshaped_hdrhist_upscale = hist(reshape(reshaped_upscaled_hdr, [size(reshaped_upscaled_hdr, 1) * size(reshaped_upscaled_hdr, 2), 1]), 100);
reshaped_hdrhist_upscale = reshaped_hdrhist_upscale ./ (size(reshaped_upscaled_hdr, 1) * size(reshaped_upscaled_hdr, 2));

% Plot out the values
figure;
hold on;
title(STREETVIEW_ID, 'Interpreter', 'none');
plot(ldrhist, 'DisplayName', 'LDR Luma');
plot(hdrhist, 'DisplayName', 'HDR Luma');
%plot(hdrhistmod, 'DisplayName', 'HDR Luma Adjusted');
plot(hdrhistdiff, 'DisplayName', 'HDR Luma Diff');
plot(reshaped_hdrhist, 'DisplayName', 'HDR Altered Luma');
plot(reshaped_hdrhist_upscale, 'DisplayName', 'HDR Altered Luma Upscaled');
legend;
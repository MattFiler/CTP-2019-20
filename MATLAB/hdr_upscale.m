STREETVIEW_ID = '%STREETVIEW_ID%';
close all;

% Load luma data from HDR image
hdrimage = hdrread(strcat('../Output/Images/',strcat(STREETVIEW_ID,'.hdr'))); 
ycbcr = rgb2ycbcr(hdrimage); 
hdrlum = ycbcr(:,:,1);

% Load luma data from LDR image
ldrimage = imread(strcat('../Output/Images/',strcat(STREETVIEW_ID,'_shifted.jpg')));
ycbcr = rgb2ycbcr(ldrimage);
ldrlum = single(ycbcr(:,:,1)) ./ 255.0;
clear ycbcr;

% Create LDR/HDR histograms from the luma data
hdrlum = imresize(hdrlum, [size(ldrlum, 1), size(ldrlum, 2)]);
hdrhist = histogram(reshape(hdrlum, [size(hdrlum, 1) * size(hdrlum, 2), 1]), 100);
hdrhist_binwidth = hdrhist.BinWidth;
ldrhist = histogram(reshape(ldrlum, [size(ldrlum, 1) * size(ldrlum, 2), 1]), 100);
ldrhist_binwidth = ldrhist.BinWidth;
[hdrhist, hdrhist_centres] = hist(reshape(hdrlum, [size(hdrlum, 1) * size(hdrlum, 2), 1]), 100);
hdrhist = hdrhist ./ (size(hdrlum, 1) * size(hdrlum, 2));
[ldrhist, ldrhist_centres] = hist(reshape(ldrlum, [size(ldrlum, 1) * size(ldrlum, 2), 1]), 100);
ldrhist = ldrhist ./ (size(ldrlum, 1) * size(ldrlum, 2));    
close all;    

% Tweak LDR luma values to match the differences in histograms
reshaped_hdr = zeros(size(ldrlum, 1), size(ldrlum, 2));
for x = 1:size(ldrlum, 1)
    for y = 1:size(ldrlum, 2)
        % Find our value's index in the histogram
        graph_offset = -1;
        for i = 1:100
           graph_offset = i;
           this_bin_edge = ldrhist_centres(1, i) + ldrhist_binwidth;
           this_luma_val = ldrlum(x, y);
           if this_luma_val <= this_bin_edge
               break;
           end
        end
        % Pull the histogram mapped HDR
        reshaped_hdr(x, y) = hdrhist_centres(1, graph_offset);
    end
end

% Pull the new LDR/HDR combination back to RGB
reshaped_hdrrgb = rgb2ycbcr(ldrimage); 
reshaped_hdrrgb(:,:,1) = reshaped_hdr * 255;
reshaped_hdrrgb = ycbcr2rgb(reshaped_hdrrgb);

% Write out the HDR as HDR floating point values
reshaped_hdrrgb_out = single(single(reshaped_hdrrgb) / single(255));
hdrwrite(reshaped_hdrrgb_out, strcat('../Output/Images/',strcat(STREETVIEW_ID,'_matlab_upscale.hdr')));

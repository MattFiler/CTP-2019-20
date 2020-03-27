%
% Upscale a low-res HDR image to match its higher quality LDR version.
% Created by Matt Filer
%
function [] = hdr_upscaler()

% Load luma data from HDR image
hdrimage = hdrread('input.hdr'); 
hdrlum = (0.2126 * hdrimage(:,:,1)) + (0.7152 * hdrimage(:,:,2)) + (0.0722 * hdrimage(:,:,3));

% Load luma data from LDR image
ldrimage = imread('input.jpg');
ldrlum = single((0.2126 * ldrimage(:,:,1)) + (0.7152 * ldrimage(:,:,2)) + (0.0722 * ldrimage(:,:,3))) ./ 255.0;

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
        if ldrlum(x, y) <= 0
            ldrlum(x, y) = 0.0001; % Hacky fix to disallow zero values
        end
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
        if reshaped_hdr(x, y) <= 0
           reshaped_hdr(x, y) = 0.0001; % Hacky fix to disallow zero values
        end
    end
end

% Undo the normal LDR luma & re-do the histogram mapped luma
ldrimage_hdr = zeros(size(ldrimage));
ldrimage_hdr(:,:,1) = (single(ldrimage(:,:,1)) ./ ldrlum) .* reshaped_hdr;
ldrimage_hdr(:,:,2) = (single(ldrimage(:,:,2)) ./ ldrlum) .* reshaped_hdr;
ldrimage_hdr(:,:,3) = (single(ldrimage(:,:,3)) ./ ldrlum) .* reshaped_hdr;

% Bring into HDR float range, and "normalise" based on max of HDR
img_to_write = single(single(ldrimage_hdr) ./ single(255));
img_to_write = single(single(img_to_write) ./ single(max(max(reshaped_hdr))));

% Write out the new HDR/LDR combo
hdrwrite(img_to_write, 'output.hdr');
end
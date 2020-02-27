STREETVIEW_ID = 'UeLdC8nLokOxI9Iu4ot2bw';
%STREETVIEW_ID = 'OhnM3UKJb9e4urhWzKXDOQ';
%STREETVIEW_ID = 'xdU_R-qfflPfs8x-tTKM8g';
%STREETVIEW_ID = 'oQLPJHW-26bak8Cds5-Otw';
close all;

% Load luma data from HDR image
hdrimage = hdrread(strcat('../Output/Images/',strcat(STREETVIEW_ID,'.hdr'))); 
hdrlum = (0.2126 * hdrimage(:,:,1)) + (0.7152 * hdrimage(:,:,2)) + (0.0722 * hdrimage(:,:,3));

% Load luma data from LDR image
ldrimage = imread(strcat('../Output/Images/',strcat(STREETVIEW_ID,'_shifted.jpg')));
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

% Try and match both distributions
leftover = zeros(1, 100);
leftover_total = 0;
leftover_total_historic = zeros(1, 100);
for x = 1:100
    leftover(1, x) = hdrhist(1, x) - ldrhist(1, x);
    leftover_total = leftover_total + hdrhist(1, x);
    leftover_total = leftover_total - ldrhist(1, x);
    leftover_total_historic(1, x) = leftover_total;
    
    if leftover_total <= 0
        %error("Out of entries at iteration " + c);
        break;
    end
end

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

% Undo the normal LDR luma
ldrimage_hdr = zeros(size(ldrimage));
ldrimage_hdr(:,:,1) = single(ldrimage(:,:,1)) ./ ldrlum;
ldrimage_hdr(:,:,2) = single(ldrimage(:,:,2)) ./ ldrlum;
ldrimage_hdr(:,:,3) = single(ldrimage(:,:,3)) ./ ldrlum;

% Re-do the histogram mapped luma
ldrimage_hdr(:,:,1) = single(ldrimage(:,:,1)) .* reshaped_hdr;
ldrimage_hdr(:,:,2) = single(ldrimage(:,:,2)) .* reshaped_hdr;
ldrimage_hdr(:,:,3) = single(ldrimage(:,:,3)) .* reshaped_hdr;

% Write out the new HDR/LDR combo
hdrwrite(single(single(ldrimage_hdr) / single(255)), strcat('../Output/Images/',strcat(STREETVIEW_ID,'_matlab_upscale.hdr')));
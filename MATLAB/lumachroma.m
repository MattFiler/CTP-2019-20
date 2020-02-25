STREETVIEW_ID = 'xdU_R-qfflPfs8x-tTKM8g';
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

% Create a histogram from the new luma
reshaped_hdrhist = hist(reshape(reshaped_hdr, [size(reshaped_hdr, 1) * size(reshaped_hdr, 2), 1]), 100);
reshaped_hdrhist = reshaped_hdrhist ./ (size(reshaped_hdr, 1) * size(reshaped_hdr, 2));

% Pull the new LDR/HDR combination back to RGB
reshaped_hdrrgb = rgb2ycbcr(ldrimage); 
reshaped_hdrrgb(:,:,1) = reshaped_hdr * 255; % Remove 255
reshaped_hdrrgb = ycbcr2rgb(reshaped_hdrrgb);
figure;
imshow(reshaped_hdrrgb);
title("MAX LUMA: " + max(max(reshaped_hdr)));

% Plot out the histogram values
figure;
hold on;
title(STREETVIEW_ID, 'Interpreter', 'none');
plot(ldrhist, 'DisplayName', 'LDR Luma');
plot(hdrhist, 'DisplayName', 'HDR Luma');
plot(leftover_total_historic, 'DisplayName', 'HDR Luma Diff');
plot(reshaped_hdrhist, 'DisplayName', 'LDR/HDR Combo Luma');
legend;
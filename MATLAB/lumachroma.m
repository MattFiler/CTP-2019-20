STREETVIEW_ID = 'xdU_R-qfflPfs8x-tTKM8g';

[hdrlum, ldrlum] = LoadFromID(STREETVIEW_ID);

% Create a histogram from the luma data
hdrhist = hist(reshape(hdrlum, [64 * 128, 1]), 100);
ldrhist = hist(reshape(ldrlum, [size(ldrlum, 1) * size(ldrlum, 2), 1]), 100);

% Bring our values down to a normalised range
hdrhist = hdrhist ./ (64 * 128);
ldrhist = ldrhist ./ (size(ldrlum, 1) * size(ldrlum, 2));

% Plot out the values
clf;
plot(ldrhist, 'DisplayName', 'LDR Luma');
hold on;
plot(hdrhist, 'DisplayName', 'HDR Luma');
legend;
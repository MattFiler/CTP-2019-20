Exe file reads from and writes into "input_output_files". Don't change it.
commands:

General:

argv[1]=5   (patch_width )
argv[2]=10  (sampling length/interval)
argv[3]=100 (max number of iterations during training)
argv[4]=0   (test number- for cross validation a number of training is done)

For testing:

argv[5]=Cumulus(1)           (name of the input - stored in "Input_Output_Files", - no need to add hdr)
argv[6]=Cumulus(1)Classified (name of the output - stored in "Input_Output_Files", - no need to add hdr)

For training:
(number of images in dataset)
argv[5]= 21 (Cumulus)
argv[6]= 21 (Cirrus)
argv[7]= 0  (Clearsky)
argv[8]= 15 (Stratocumulus)
argv[9]= 0  (Cirrocumulus)
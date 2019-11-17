<?php
	//Requires "panoid" param
	if (!$_GET['panoid']) {
		echo "This API requires a Streetview ID.";
		exit;
	}
	
	//Get metafile for panoid and format it
	$metafile = json_decode(substr(file_get_contents("https://www.google.co.uk/maps/photometa/v1?authuser=0&hl=en&gl=uk&pb=!1m4!1smaps_sv.tactile!11m2!2m1!1b1!2m2!1sen!2suk!3m3!1m2!1e2!2s".$_GET['panoid']."!4m57!1e1!1e2!1e3!1e4!1e5!1e6!1e8!1e12!2m1!1e1!4m1!1i48!5m1!1e1!5m1!1e2!6m1!1e1!6m1!1e2!9m36!1m3!1e2!2b1!3e2!1m3!1e2!2b0!3e3!1m3!1e3!2b1!3e2!1m3!1e3!2b0!3e3!1m3!1e8!2b0!3e3!1m3!1e1!2b0!3e3!1m3!1e4!2b0!3e3!1m3!1e10!2b1!3e2!1m3!1e10!2b0!3e3"), 4));

	//Pull this sphere's neighbours
	$meta_final->neighbour_ids = array();
	$meta_final->neighbour_positions = array();
	$neighbour_block = $metafile[1][0][5][0][3][0];
	foreach ($neighbour_block as &$neighbour_set) {
		if ($neighbour_set[0][1] == $_GET['panoid']) continue;
		array_push($meta_final->neighbour_ids, $neighbour_set[0][1]);
		array_push($meta_final->neighbour_positions, array($neighbour_set[2][0][2], $neighbour_set[2][0][3]));
	}
	//171119: There's also seemingly rotation data here too, might be worth pulling?

	//Pull this sphere's sizes
	$meta_final->compiled_sizes = array();
	$size_block = $metafile[1][0][2][3][0];
	foreach ($size_block as &$size_set) {
		array_push($meta_final->compiled_sizes, array($size_set[0][1], $size_set[0][0]));
	}

	//Pull this sphere's tile size
	$size_block = $metafile[1][0][2][3][1];
	$meta_final->tile_size = array($size_block[1], $size_block[0]);

	//Pull this sphere's geo info
	$area_text_block = $metafile[1][0][3][2];
	$meta_final->road = $area_text_block[0][0];
	$meta_final->region = $area_text_block[1][0];
	$latlon_block = $metafile[1][0][5][0][1][0];
	$meta_final->coordinates = array($latlon_block[2], $latlon_block[3]);
	
	//Pull date
	$date_block = $metafile[1][0][6][7];
	$meta_final->this_date = array($date_block[0], $date_block[1]);
	
	//Pull previous capture dates
	$meta_final->alt_dates = array();
	$history_block = $metafile[1][0][5][0][8];
	foreach ($history_block as &$history_set) {
		array_push($meta_final->alt_dates, array($history_set[1][0], $history_set[1][1]));
	}
	//171119: Indexes are also stored here ($history_set[0]) which relate to the neighbour ID array
	
	//Encode and return
	header('Content-Type: application/json');
	echo json_encode($meta_final);
?>
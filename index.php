<?php
	//Requires "panoid" param
	if (!$_GET['panoid']) {
		echo "This API requires a Streetview ID.";
		exit;
	}
	$meta_final->id = $_GET['panoid'];
	
	//Get metafile for panoid and format it
	$metafile = json_decode(substr(file_get_contents("https://www.google.co.uk/maps/photometa/v1?authuser=0&hl=en&gl=uk&pb=!1m4!1smaps_sv.tactile!11m2!2m1!1b1!2m2!1sen!2suk!3m3!1m2!1e2!2s".$_GET['panoid']."!4m57!1e1!1e2!1e3!1e4!1e5!1e6!1e8!1e12!2m1!1e1!4m1!1i48!5m1!1e1!5m1!1e2!6m1!1e1!6m1!1e2!9m36!1m3!1e2!2b1!3e2!1m3!1e2!2b0!3e3!1m3!1e3!2b1!3e2!1m3!1e3!2b0!3e3!1m3!1e8!2b0!3e3!1m3!1e1!2b0!3e3!1m3!1e4!2b0!3e3!1m3!1e10!2b1!3e2!1m3!1e10!2b0!3e3"), 4));
	
	//Pull date
	$date_block = $metafile[1][0][6][7];
	$meta_final->date = array($date_block[0], $date_block[1]);

	//Pull this sphere's geo info
	$area_text_block = $metafile[1][0][3][2];
	$meta_final->road = $area_text_block[0][0];
	$meta_final->region = $area_text_block[1][0];
	$latlon_block = $metafile[1][0][5][0][1][0];
	$meta_final->coordinates = array($latlon_block[2], $latlon_block[3]);

	//Pull this sphere's tile size
	$size_block = $metafile[1][0][2][3][1];
	$meta_final->tile_size = array($size_block[1], $size_block[0]);

	//Pull this sphere's sizes
	$meta_final->compiled_sizes = array();
	$size_block = $metafile[1][0][2][3][0];
	for ($x = 0; $x < count($size_block); $x++) {
		array_push($meta_final->compiled_sizes, array($size_block[$x][0][1], $size_block[$x][0][0]));
	}

	//Pull previous capture dates
	$meta_final->history = array();
	$history_block = $metafile[1][0][5][0][8];
	for ($x = 0; $x < count($history_block); $x++) {
		$history->id = $history_block[$x][0];
		$history->date = array($history_block[$x][1][0], $history_block[$x][1][1]);
		array_push($meta_final->history, clone $history);
	}
	
	//Pull this sphere's relations
	$meta_final->neighbours = array();
	$neighbour_block = $metafile[1][0][5][0][3][0];
	for ($x = 0; $x < count($neighbour_block); $x++) {
		if ($neighbour_block[$x][0][1] == $_GET['panoid']) continue;
		
		//If this relation is a historical photo, update history data and don't save as neighbour
		$historic = false;
		for ($i = 0; $i < count($meta_final->history); $i++) {
			if ($meta_final->history[$i]->id == $x) {
				$meta_final->history[$i]->id = $neighbour_block[$x][0][1];
				$historic = true;
				break;
			}
		}
		if ($historic) continue;
		
		$neighbours->id = $neighbour_block[$x][0][1];
		$neighbours->coordinates = array($neighbour_block[$x][2][0][2], $neighbour_block[$x][2][0][3]);
		array_push($meta_final->neighbours, clone $neighbours);
	}
	//171119: There's also seemingly rotation data here too, might be worth pulling?
	
	//Encode and return
	header('Content-Type: application/json');
	echo json_encode($meta_final);
?>
<?php
	//Requires "panoid" param
	if (!$_GET['panoid']) {
		exit;
	}

	//Trim a string
	function trimString($wholeString, $beginFrom, $endAt, $startOffset=0, $endOffset=0) {
		$startPosition = strpos($wholeString, $beginFrom) + strlen($beginFrom) + $startOffset;
		$endPosition = strpos($wholeString, $endAt, $startPosition) - $endOffset;
		$difference = $endPosition - $startPosition;
		return substr($wholeString, $startPosition, $difference);
	}

	//Load the requested sphere's metadata
	$metafile = file_get_contents("https://www.google.co.uk/maps/photometa/v1?authuser=0&hl=en&gl=uk&pb=!1m4!1smaps_sv.tactile!11m2!2m1!1b1!2m2!1sen!2suk!3m3!1m2!1e2!2s".$_GET['panoid']."!4m57!1e1!1e2!1e3!1e4!1e5!1e6!1e8!1e12!2m1!1e1!4m1!1i48!5m1!1e1!5m1!1e2!6m1!1e1!6m1!1e2!9m36!1m3!1e2!2b1!3e2!1m3!1e2!2b0!3e3!1m3!1e3!2b1!3e2!1m3!1e3!2b0!3e3!1m3!1e8!2b0!3e3!1m3!1e1!2b0!3e3!1m3!1e4!2b0!3e3!1m3!1e10!2b1!3e2!1m3!1e10!2b0!3e3");

	//Find neighbouring spheres and grab their IDs and lat/lon
	$metafile = trimString($metafile, "[[2,", "]]]]]");
	$metafile_parts = explode("[[2,", $metafile);
	for ($i = 0; $i < count($metafile_parts); $i++) {
		$PanoID = trimString($metafile_parts[$i], '"', '"');
		$LatLon = trimString($metafile_parts[$i], '[[null,null,', '],[');
		
		if ($PanoID != $_GET['panoid']) {
			echo $PanoID . "\n";
		}
	}
?>
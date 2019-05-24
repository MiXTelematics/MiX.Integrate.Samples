<?php

//Require the Composer autoload
require __DIR__ .'/vendor/autoload.php';

//Variables
$clientName =  "";
$clientID =  "";  
$clientSecret = "";
$IDBaseUrl = "https://identity.<server>.mixtelematics.com/core";
$RestBaseUrl = "https://integrate.<server>.mixtelematics.com";
$dynamixUserName = "";
$dynamixUserPassword = "";
$organisationId = 0;

$scope = "offline_access MiX.Integrate"; //This is required for MiX Integrate
 
//Connect to the OpenID with OpendID-Connect-PHP -- for help: https://github.com/jumbojett/OpenID-Connect-PHP
use Jumbojett\OpenIDConnectClient;

//Create the OpenID Connect object
$oidc = new OpenIDConnectClient($IDBaseUrl,
                                $clientID,
                                $clientSecret);

//Set params for the OpenID Client
$oidc->providerConfigParam(array(
		'token_endpoint'=>$IDBaseUrl."/connect/token")
	);
$oidc->addScope($scope);
$oidc->setClientName($clientName);
$oidc->addAuthParam(array('username'=>$dynamixUserName));
$oidc->addAuthParam(array('password'=>$dynamixUserPassword));

//Get the bearer token
print_r("Request bearer token from: ".$IDBaseUrl."<br />");
$tokenResponse = $oidc->requestResourceOwnerToken(true);
if(is_null($tokenResponse)) {
	print_r("Bearer token request failed - exit");
	exit();
} else { 
	$token = $tokenResponse->access_token;
	print_r("<table border='1' > <tr bgcolor='lightgreen'><td>token_type</td><td>expires_in</td><td>access_token</td></tr>");
	print_r("<tr><td>".($tokenResponse->token_type)."</td><td>".($tokenResponse->expires_in)."</td><td>"."Length: ".strlen($token)."</td></tr>");
	print_r("</table>");
	print_r("<br />");
}

// Create RESTClient object -- for help: https://github.com/tcdent/php-restclient
$api = new RestClient([
    'base_url' => $RestBaseUrl, 
    'headers' => ['Authorization' => 'Bearer '.$token, 'Content-Type' => 'application/json', 'Accept' => 'application/json'], 
]);


//Make an Version API call to the MiX Integrate server
print_r("Make an Version API call to the MiX Integrate server: ".$RestBaseUrl."<br />");
$resultVersion = $api->get("/version");  //Gets the version number
if($resultVersion->info->http_code != 200){
	print_r("API call failed  with code ".($resultVersion->info->http_code)." - exit");
	exit();
} else {
	$version = $resultVersion->decode_response(); 
	print_r("<table border='1' > <tr bgcolor='lightgreen'><td>Name</td><td>Version</td></tr> <tr><td>".($version->Name)."</td><td>".($version->Version)."</td></tr> </table>");
	print_r("<br />");
}


//Make a Get organisation detail API call to the MiX Integrate server
print_r("Get organisation detail for ".($organisationId)."<br />");
$resultOrgDetail = $api->get("/api/organisationgroups/details/".$organisationId);
if($resultOrgDetail->info->http_code != 200){
	print_r("API call failed  with code ".($resultOrgDetail->info->http_code)." - exit");
	exit();
} else {
	$organisation = $resultOrgDetail->decode_response();  
	print_r("<table border='1' > <tr bgcolor='lightgreen'><td>GroupId</td><td>Name</td><td>GroupType</td><td>DisplayTimeZone</td></tr>");
	print_r("<tr><td>".($organisation->GroupId)."</td><td>".($organisation->Name)."</td><td>".($organisation->GroupType)."</td><td>".($organisation->DisplayTimeZone)."</td></tr>");
	print_r("</table>");
	print_r("<br />");
}
 
//Get all assets for an organisation
print_r("Get all assets for organisation ".($organisationId)."<br />");
$resultAssets = $api->get("/api/assets/group/".$organisationId);   
if($resultAssets->info->http_code != 200){
	print_r("API call failed  with code ".($resultAssets->info->http_code)." - exit");
	exit();
} else {
	$assets = $resultAssets->decode_response(); 
	print_r("Assets: ".(count($assets))."<br /><br />");
}

//Get latest positions
print_r("Get latest positions for assets in organisation ".($organisationId)."<br />");
$assetIds = array();
foreach($assets as $asset){
	array_push($assetIds,($asset->AssetId));
}
$resultLatestPositions = $api->post("/api/positions/assets/latest/1", json_encode($assetIds));  
if($resultLatestPositions->info->http_code != 200){
	print_r("API call failed  with code ".($resultLatestPositions->info->http_code)." - exit");
	exit();
} else {
	$positions = $resultLatestPositions->decode_response(); 
	print_r("Positions: ".(count($positions))."<br /><br />"); 
}

print_r("Assets with latest positions");
print_r("<table border='1' > <tr bgcolor='lightgreen'><td>AssetId</td><td>Description</td><td>RegistrationNumber</td></td><td>Timestamp</td><td>Latitude</td><td>Longitude</td><td>SpeedKilometresPerHour</td><td>FormattedAddress</td></tr>");
foreach($assets as $asset){
	print_r("<tr>");
	print_r("<td>".($asset->AssetId)."</td><td>".($asset->Description)."</td><td>".($asset->RegistrationNumber)."</td>");	
	foreach($positions as $position){
		if($position->AssetId == $asset->AssetId){ 	 
			if(!isset($position->SpeedKilometresPerHour)) $position->SpeedKilometresPerHour = "NULL"; 
			if(!isset($position->SpeedKilometresPerHour)) $position->SpeedKilometresPerHour = "NULL"; 
			if(!isset($position->SpeedKilometresPerHour)) $position->SpeedKilometresPerHour = "NULL"; 
			if(!isset($position->FormattedAddress)) $position->FormattedAddress = "NULL";	
			print_r("<td>".($position->Timestamp)."</td><td>".($position->Latitude)."</td><td>".($position->Longitude)."</td><td>".($position->SpeedKilometresPerHour)."</td><td>".($position->FormattedAddress)."</td>");
		}
	}
	print_r("</tr>");
}
print_r("</table>");
print_r("<br />");

?>

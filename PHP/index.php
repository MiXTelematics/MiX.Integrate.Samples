<?php

//Require the Composer autoload
require __DIR__ .'/vendor/autoload.php';

//Variables
$clientName =  "";
$clientID =  "";  
$clientSecret = ""; //
$IDBaseUrl = "https://identity.<server>.mixtelematics.com/core";
$RestBaseUrl = "https://integrate.<server>.mixtelematics.com";

$dynamixUserName = "";
$dynamixUserPassword = "";
$scope = "offline_access MiX.Integrate"; //This is required for MiX Integrate

//Connect to the OpenID with OpendID-Connect-PHP -- for help: https://github.com/jumbojett/OpenID-Connect-PHP
use OpenIDConnectClient;

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

//Get the token
$token = $oidc->requestResourceOwnerToken(true)->access_token;

// Create RESTClient object -- for help: https://github.com/tcdent/php-restclient
$api = new RestClient([
    'base_url' => $RestBaseUrl, 
    'headers' => ['Authorization' => 'Bearer '.$token, 'Content-Type' => 'application/json', 'Accept' => 'application/json'], 
]);

//Make an API call to the MiX Integrate server
$result = $api->get("/version");  //this is to get the version number

//Display the result - will need to parse
if($result->info->http_code == 200)
    print_r($result->decode_response());

//Example of an API call using PUSH with parameters to get the latest position of a vehicle.
$assetId = 0123456789012345678;
$result = $api->push("/api/positions/assets/latest/1", , json_encode([$assetId])); // Parameters have to be JSON-Encoded.

//Display the result - will need to parse
if($result->info->http_code == 200)
    print_r($result->decode_response());
?>

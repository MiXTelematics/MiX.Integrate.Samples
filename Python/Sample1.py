# Test_MiX.Integrate.Api
# Authenticate and get all assets for a group
import requests
import json
import base64  
from requests.utils import quote

IdentityUrl = "https://identity.uat.mixtelematics.com"  
ApiUrl = "https://integrate.uat.mixtelematics.com"

IdentityClientId = "$clientid$"
IdentityClientSecret = "$secret$"
IdentityUsername = "$username$"
IdentityPassword = "$password$"
IdentityScope = "offline_access+MiX.Integrate"

GroupId = $groupid$ 

print("1. Get Identity server configuration")
ConfigUrl = IdentityUrl + "/core/.well-known/openid-configuration" 
print("Request: " + ConfigUrl)
ConfigResponse = requests.get(ConfigUrl) 
print(ConfigResponse.status_code, ConfigResponse.reason)
IdServerConfig = json.loads(ConfigResponse.content)
print("Config.issuer: " + IdServerConfig["issuer"])  
print("Config.token_endpoint: " + IdServerConfig["token_endpoint"])  
IdTokenEndPoint = IdServerConfig["token_endpoint"]

print("2. Authenticate against Identity server")
auth = "Basic " + base64.b64encode(bytes(IdentityClientId + ":" + IdentityClientSecret, "utf-8")).decode('ascii')
body = "grant_type=password&username=" + quote(IdentityUsername)+ "&password=" + quote(IdentityPassword) + "&scope=" + IdentityScope
#print("Authorization: " + auth)
#print("Body: " + body)
print("Request: " + IdTokenEndPoint)
TokenResponse = requests.post(IdTokenEndPoint, data = body, headers = {"accept":"application/json", "Authorization":auth }) 
print(TokenResponse.status_code, TokenResponse.reason)
Token = json.loads(TokenResponse.content)
print("expires_in: " + str(Token["expires_in"]))
print("refresh_token: " + Token["refresh_token"])
print("token_type: " + Token["token_type"])
print("access_token: " + str(len(Token["access_token"])) + " bytes")
BearerToken = "Bearer " + Token["access_token"]

print("3. Assets.GetAllAsync")
GetAssetsForGroupUrl = ApiUrl + "/api/assets/group/" + str(GroupId)
print("Request: " + GetAssetsForGroupUrl)
#print("Authorization: " + BearerToken)
AssetsResponse = requests.get(GetAssetsForGroupUrl, headers = {"accept":"application/json", "Authorization":BearerToken })
print(AssetsResponse.status_code, AssetsResponse.reason)
Assets = json.loads(AssetsResponse.content)
print("Assets.Count: " + str(len(Assets)))
print("First Asset: AssetId:" + str(Assets[0]["AssetId"]) + ", Description:" + str(Assets[0]["Description"]) + ", SiteId:" + str(Assets[0]["SiteId"]))




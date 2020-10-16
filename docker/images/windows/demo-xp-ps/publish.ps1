$sps="http://127.0.0.1:80/";
$queue="api/publishing/jobqueue";
$url=$sps+$queue;

$headers = @{

'Content-type' = 'application/json'
}

$body = '{
"contextLanguage": "en",
"languages": [
"en"
],

"targets": [
"Internet"
],

"source": "master",
"descendants": true,
"relatedItems": false,
"itemId": "11111111-1111-1111-1111-111111111111",
"user": "automation",
"metadata": {
"Publish.Options.ClearAllCaches": "true",
"Publish.Options.Republish": "true",
"PublishType": "Full re-publish",
"DetectCloneSources": "False",
"Publish.Options.IncludeDescendants": "true",
"Publish.Options.IncludeRelatedItems": "False",
"Publish.Options.ContextLanguage": "en",
"Publish.Options.Languages": "en",
"Publish.Options.Targets": "Internet",
"Publish.Options.User": "automation",
"Publish.Options.ItemId": "{11111111-1111-1111-1111-111111111111}"
}
}'

For ($i=0; $i -le 25; $i++) {
    try {
        Invoke-RestMethod -Uri $url -Method Get -ErrorAction SilentlyContinue | Out-Null
    }
    catch {}
    Start-Sleep -Seconds 1
}

Invoke-RestMethod -Uri $url -Method Put -Headers $headers -Body $body
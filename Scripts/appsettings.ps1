$appSettings = "EmbyStat.Web\appsettings.json"
$version = "$($env:BuildVersion_MajorVersion).$($env:BuildVersion_MinorVersion).$($env:BuildVersion_PatchVersion).$($env:BuildVersion_BuildCounter)"

(GC $appSettings).Replace("0.0.0.0", "$version") | Set-Content $appSettings
(GC $appSettings).Replace("RollbarENV", "dev") | Set-Content $appSettings
(GC $appSettings).Replace("XXXXXXXXXX", "$($env:RollbarKey)") | Set-Content $appSettings
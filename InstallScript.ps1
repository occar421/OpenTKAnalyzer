$nuspec = (ls $env:APPVEYOR_BUILD_FOLDER -Recurse).Where{ $_.Extension -eq ".nuspec"} | Select -First 1
[xml]$xml = Get-Content $nuspec.FullName
$number = $xml.package.metadata.version
$stage = $xml.package.metadata.stage
if(-not [String]::IsNullOrEmpty($stage)){
    $stage = "-" + $stage
}

$nupkgVersion = $number + $stage
if(($number | Select-String "\." -AllMatches).Matches.Count -eq 2){
    $number = $number + ".0"
}
$assemblyVersion = $number + "." + $env:APPVEYOR_BUILD_NUMBER

$env:APPVEYOR_BUILD_VERSION = $assemblyVersion
$xml.package.metadata.version = $nupkgVersion


$ns = New-Object System.Xml.XmlNamespaceManager($xml.NameTable)
$ns.AddNamespace("ns", $xml.DocumentElement.NamespaceURI)
$metadataNode = $xml.SelectSingleNode('/ns:package/ns:metadata', $ns)
$stageNode = $metadataNode.SelectSingleNode('ns:stage', $ns)
$metadataNode.RemoveChild($stageNode) | Out-Null

$xml.Save($nuspec.FullName)
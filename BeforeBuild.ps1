$nuspec = (ls $env:APPVEYOR_BUILD_FOLDER -Recurse).Where{ $_.Extension -eq ".nuspec"} | Select -First 1
[xml]$xml = Get-Content $nuspec.FullName
$number = $xml.package.metadata.version
$stage = $xml.package.metadata.stage
if(-not [String]::IsNullOrEmpty($stage)){
    $stage = "-" + $stage
}

$nupkgVersion = $number + $stage
$assemblyVersion = $nupkgVersion + "__" + $env:APPVEYOR_BUILD_NUMBER

$env:APPVEYOR_BUILD_VERSION = $assemblyVersion

$xml.package.metadata.version = $nupkgVersion


$ns = New-Object System.Xml.XmlNamespaceManager($xml.NameTable)
$ns.AddNamespace("ns", $xml.DocumentElement.NamespaceURI)
$metadataNode = $xml.SelectSingleNode('/ns:package/ns:metadata', $ns)
$stageNode = $metadataNode.SelectSingleNode('ns:stage', $ns)
$metadataNode.RemoveChild($stageNode) | Out-Null

$xml.Save($nuspec.FullName)

#rd "C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\Extensions\Amazon Web Services LLC" /s /q
Remove-Item "C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\Extensions\Amazon Web Services LLC" -Recurse -Force
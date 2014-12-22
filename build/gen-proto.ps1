$protogenEx=gci .\packages -Filter ProtoGen.exe -recurse
$protogen="$($protogenEx.FullName)"

Write-Host "Generating Proto Classes.."

$args = @('--proto_path=.\gauge-proto', '-output_directory=.\Lib', '--include_imports', '.\gauge-proto\api.proto', '.\gauge-proto\messages.proto')
&$protogen $args

Write-Host "Done!"
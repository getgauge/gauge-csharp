$protogenEx=gci .\packages -Filter ProtoGen.exe -recurse
$protogen="$($protogenEx.FullName)"

Write-Host "Generating Proto Classes.."

$args = @('--proto_path=.\gauge-proto', '-output_directory=.\Runner', '--include_imports', '.\gauge-proto\api.proto', '.\gauge-proto\messages.proto', '-namespace=Gauge.Messages')
&$protogen $args

Write-Host "Done!"git st

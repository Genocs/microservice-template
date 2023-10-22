# This script is cross-platform, supporting all OSes that PowerShell Core/7 runs on.

$currentDirectory = Get-Location
$rootDirectory = git rev-parse --show-toplevel

# use relative path to the template folder
$templateFolder = ''
# $templateFolder = 'src/template/'


$webApiDirectory = Join-Path -Path $rootDirectory -ChildPath $templateFolder'src/WebApi'
$infrastructurePrj = Join-Path -Path $rootDirectory -ChildPath $templateFolder'src/Infrastructure/Infrastructure.csproj'

Write-Host "Make sure you have run the Genocs.WebApi project. `n"
Write-Host "Press any key to continue... `n"
$null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown');

Set-Location -Path $webApiDirectory
Write-Host "WebApi Directory is $webApiDirectory `n"

<# Run command #>
dotnet build -t:NSwag $infrastructurePrj

Set-Location -Path $currentDirectory
Write-Host -NoNewLine 'NSwag Regenerated. Press any key to continue...';
$null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown');

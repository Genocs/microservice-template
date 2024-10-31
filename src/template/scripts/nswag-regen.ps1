# This script is cross-platform, supporting all OSes that PowerShell Core/7 runs on.

$currentDirectory = Get-Location
$rootDirectory = git rev-parse --show-toplevel

# use relative path to the template folder
$baseFolder = 'src'
# $templateFolder = 'src/template/'


$infrastructurePrj = Join-Path -Path $rootDirectory -ChildPath $baseFolder'/Infrastructure/Infrastructure.csproj'

Write-Host "Make sure you have run the WebApi project is up and running. `n"
Write-Host "Press any key to continue... `n"
$null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown');

<# Run command #>
dotnet build -t:NSwag $infrastructurePrj

Set-Location -Path $currentDirectory
Write-Host -NoNewLine 'NSwag Regenerated. Press any key to continue...';
$null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown');

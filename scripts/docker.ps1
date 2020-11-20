param(
    $RootDir       = "$PSScriptRoot/..",
    $Tag           = "latest"
)

$RootPath = Resolve-Path $RootDir

# build docker images
Write-Host "******************************************************************************"
Write-Host "Starting: Docker"
Write-Host "******************************************************************************"
Write-Host "=============================================================================="

Write-Host ""

docker build . -f $RootPath/src/Admin/Dockerfile -t mbbacr.azurecr.io/whatisthecurrentsprint/admin:$Tag

Write-Host ""

docker build . -f $RootPath/src/Web/Dockerfile -t mbbacr.azurecr.io/whatisthecurrentsprint/web:$Tag

Write-Host ""

docker build . -f $RootPath/src/FunctionApp/Dockerfile -t mbbacr.azurecr.io/whatisthecurrentsprint/functionapp:$Tag

Write-Host ""

docker push mbbacr.azurecr.io/whatisthecurrentsprint/admin

Write-Host ""

docker push mbbacr.azurecr.io/whatisthecurrentsprint/web

Write-Host ""

docker push mbbacr.azurecr.io/whatisthecurrentsprint/functionapp

Write-Host ""

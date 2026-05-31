@echo off
echo Iniciando Painel Web Dashboard SGPST Enterprise...
echo Disponivel em http://localhost:5017 ou https://localhost:7260
dotnet run --project src/SGPST.Presentation.Web/SGPST.Presentation.Web.csproj
pause

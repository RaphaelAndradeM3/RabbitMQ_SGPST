@echo off
echo Iniciando RabbitMQ via Docker...
docker-compose up -d
echo.
echo =====================================================
echo RabbitMQ disponivel em:
echo - Mensageria: localhost:5672
echo - Painel (WEB): http://localhost:15672 (guest/guest)
echo =====================================================
echo.
echo DICA: Se as aplicacoes falharem na primeira tentativa, 
echo elas tentarao reconectar automaticamente enquanto o 
echo container termina de subir.
echo.
pause

@echo off
echo Iniciando RabbitMQ via Docker...
docker-compose up -d
echo.
echo RabbitMQ disponivel em:
echo - Mensageria: localhost:5672
echo - Painel de Controle: http://localhost:15672 (guest/guest)
echo.
pause

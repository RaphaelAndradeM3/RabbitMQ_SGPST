@echo off
echo Iniciando ambiente SGPST Enterprise (PostgreSQL + RabbitMQ)...
docker compose up -d
echo.
echo Status dos servicos:
docker compose ps
echo.
echo Painel do RabbitMQ disponivel em http://localhost:15672 (guest/guest)
pause

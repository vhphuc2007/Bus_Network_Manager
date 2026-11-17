@echo off
chcp 65001 >nul
cd /d "%~dp0"

echo =====================================================================
echo  KHOI DONG BUSBOOKING O CHE DO RELEASE (GIAO DIEN THUONG MAI THAT)
echo =====================================================================

echo [1/2] Dang khoi dong BusBooking.Server (Release Mode)...
start "BusBooking Server (Release)" cmd /k "dotnet run --project BusBooking.Server -c Release --launch-profile http"

echo [2/2] Dang cho Server san sang va mo BusBooking.Client (Release Mode)...
timeout /t 4 /nobreak >nul
start "BusBooking Client (Release)" cmd /c "dotnet run --project BusBooking.Client -c Release"

exit /b 0

@echo ���ڰ�װWindowService
@Set Path=C:\Windows\Microsoft.NET\Framework\v4.0.30319;
@Set svn_dir=%~dp0
installutil %svn_dir%Dorado.VWS.ClientHost.exe

@echo ��������ServiceWindowService
C:\WINDOWS\system32\net.exe start "Dorado.VWS.ClientHost"

pause
@echo �ɹ���
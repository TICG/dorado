@ECHO OFF

REM ======================================================================
REM
REM Batch File -- Created with manual
REM
REM NAME: 
REM
REM AUTHOR:  , 
REM DATE  : 2011-08-04
REM
REM COMMENT: 
REM
REM ======================================================================

TITLE XML����ӳ�������ɹ��� Email:zhangjiangsong@vancl.com
MODE CON: COLS=60 LINES=30
REM COLOR 2e
COLOR 4f

:REPEAT
CLS
ECHO. 
ECHO===============XML����ӳ�������ɹ���==============
ECHO. 
:INPUT
SET xsd=xsd.exe
SET configpath=
SET configdir=
SET configname=
SET configext=
SET xsdpath=
SET cspath=
ECHO Ӣ���ǣ���ʼ�������������ļ��ɣ�Xml or Config����
ECHO. 
SET /p configpath=
:APPLY
IF NOT DEFINED configpath (GOTO INVALID)
IF DEFINED configpath (GOTO CREATE) 
:OPENDIR
start %configdir%
GOTO SUCCESS
:CREATE
CALL :INIT %configpath%
IF %configext% EQU .config (GOTO COPYXML)
IF %configext% EQU .xml (GOTO CREATEXSD)
GOTO NONSUPPORT
:CREATEXSD
ECHO. 
ECHO ׼�����ɼܹ��ļ� ������
ECHO. 
xsd.exe %configpath% /outputdir:%configdir%
if not exist %xsdpath%(
goto CREATEXSDFAIL
)
ECHO. 
GOTO CREATECS
:CREATECS
REM Color 69
ECHO �ܹ��ļ����ɳɹ�������׼���������ö����ļ�������
ECHO. 
xsd.exe %xsdpath% /c /outputdir:%configdir%
ECHO. 
GOTO XSD
:INVALID 
ECHO Ӣ���ǣ����������������ļ���
ECHO. 
GOTO SELECT
:FAIL
REM Color 60
ECHO. 
ECHO ��Ǹ���������ö���ʧ��!
ECHO. 
GOTO SELECT
:NONSUPPORT
ECHO. 
ECHO ��Ǹ�����ṩ�Ĳ�����Ч�������ļ�!
ECHO. 
GOTO SELECT
:CREATEXSDFAIL
REM Color 60
ECHO. 
ECHO ��Ǹ������XML�ܹ�ʧ��!
ECHO. 
GOTO SELECT
:CREATECSFAIL
REM Color 60
ECHO ��Ǹ���������ö����ļ�ʧ��!
ECHO. 
GOTO SELECT
:SUCCESS
REM Color 4f
ECHO ��ϲ���ҵ�Ӣ�ۣ��󹦸��!
ECHO. 
GOTO SELECT
:COPYXML
if exist %configname%() else(
copy %configpath% %configname%
)
set configpath=%configname%
GOTO CREATEXSD
:SELECT
SET choice=
SET /p choice=Are you ready��[Y/N]:

IF NOT DEFINED choice (GOTO SELECT)
IF %choice% EQU Y (GOTO REPEAT) 
IF %choice% EQU y (GOTO REPEAT) 
IF %choice% EQU N (GOTO END)
IF %choice% EQU n (GOTO END)  
GOTO SELECT
:DELXSD
ECHO ׼������ܹ��ļ�������
ECHO. 
del %xsdpath%
ECHO �ܹ��ļ�������ϣ�
ECHO. 
GOTO OPENDIR
:XSD
SET del=
SET /p del=Ҫɾ���ܹ��ļ���[Y/N]:
ECHO. 
IF NOT DEFINED del (GOTO XSD)
IF %del% EQU Y (GOTO DELXSD) 
IF %del% EQU y (GOTO DELXSD) 
IF %del% NEQ Y (GOTO OPENDIR)
IF %del% NEQ y (GOTO OPENDIR)  
GOTO SELECT
:END
PAUSE
:INIT
set configdir=%~dp1 
set configname=%~dp1%~n1.xml
set configext=%~x1
set xsdpath=%~dp1%~n1.xsd 
set csdpath=%~dp1%~n1.cs

























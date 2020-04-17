rem
rem  Copyright (c) 2000, 2020, Oracle and/or its affiliates.
rem
rem  Licensed under the Universal Permissive License v 1.0 as shown at
rem  http://oss.oracle.com/licenses/upl.
rem

@echo off

rem
rem This script sets all environment variables necessary to build Oracle
rem Coherence for .NET.
rem
rem Command line:
rem     cfgbuild [-reset]
rem
rem This script is responsible for the following environment variables:
rem
rem     DEV_ROOT     e.g. c:\dev\main.net
rem     ANT_HOME     e.g. c:\dev\main.net\tools\internal\ant
rem     SHFBROOT     e.g. c:\dev\main.net\tools\internal\shfb
rem     CLASSPATH    e.g.
rem     PATH         e.g. %NANT_HOME%\bin;%ANT_HOME%\bin;%PATH%
rem
rem     _ANT_HOME    saved ANT_HOME
rem     _SHFBROOT    saved SHFBROOT
rem     _DXROOT      saved DXROOT
rem     _CLASSPATH   saved CLASSPATH
rem     _PATH        saved PATH
rem

rem
rem Reset the build environment if the "-reset" flag was passed
rem
if "%1"=="-reset" (
  if "%DEV_ROOT%"=="" (
    echo Build environment already reset.
    goto exit
  )

  set "ANT_HOME=%_ANT_HOME%"
  set "SHFBROOT=%_SHFBROOT%"
  set "DXROOT=%_DXROOT%"
  set "CLASSPATH=%_CLASSPATH%"
  set "PATH=%_PATH%"

  set DEV_ROOT=
  set _ANT_HOME=
  set _SHFBROOT=
  set _DXROOT=
  set _CLASSPATH=
  set _PATH=

  echo Build environment reset.
  goto exit
)

rem
rem Determine the root of the dev tree
rem
if not "%DEV_ROOT%"=="" (
  echo Build environment already set.
  goto exit
)
for %%i in ("%~dp0..") do @set DEV_ROOT=%%~fni
echo DEV_ROOT  = %DEV_ROOT%

rem
rem Save the PATH environment variable
rem
set "_PATH=%PATH%"

rem
rem Add PuTTY to the path (necessary for ssh/scp)
rem
set "PATH=%DEV_ROOT%\tools\internal\putty\bin;%PATH%"

rem
rem Set the ANT_HOME environment variable
rem
set "_ANT_HOME=%ANT_HOME%"
set "ANT_HOME=%DEV_ROOT%\tools\internal\ant"
set "PATH=%ANT_HOME%\bin;%PATH%"

rem
rem Add .NET framework 4.0 and 2.0 to the path (necessary for SHFB and compression)
rem
set "PATH=%WINDIR%\Microsoft.NET\Framework\V4.0.30319;%PATH%;%WINDIR%\Microsoft.NET\Framework\v2.0.50727"

rem
rem Set the SHFBROOT and DXROOT environment variables
rem
set "_SHFBROOT=%SHFBROOT%"
set "SHFBROOT=%DEV_ROOT%\tools\internal\shfb"

rem
rem Set the CLASSPATH environment variable
rem
set "_CLASSPATH=%CLASSPATH%"
set CLASSPATH=

:: --------------------------------------------------------------------------------------------
:: Set JAVA_HOME
:: --------------------------------------------------------------------------------------------
if defined JAVA_HOME (
  :: Strip quotes from JAVA_HOME environment variable if present
  set JAVA_HOME=%JAVA_HOME:"=%
) else (
 echo JAVA_HOME not set.
rem goto exit_err
)

:: --------------------------------------------------------------------------------------------
:: Display settings
:: --------------------------------------------------------------------------------------------

echo JAVA_HOME = %JAVA_HOME%
"%JAVA_HOME%\bin\java" -version
echo CLASSPATH = %CLASSPATH%
echo ANT_HOME  = %ANT_HOME%
echo SHFBROOT  = %SHFBROOT%
echo DXROOT    = %DXROOT%
echo PATH      = %PATH%

echo Build environment set.

:exit
exit /b 0

:: --------------------------------------------------------------------------------------------
:: Exit with error.  Note the comspec command needs to be at the end of the file to work
:: --------------------------------------------------------------------------------------------
:exit_err
@%COMSPEC% /C exit 1 >nul
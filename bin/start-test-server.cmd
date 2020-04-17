rem
rem  Copyright (c) 2000, 2020, Oracle and/or its affiliates.
rem
rem  Licensed under the Universal Permissive License v 1.0 as shown at
rem  http://oss.oracle.com/licenses/upl.
rem

@echo off
@
@rem This will start a test DefaultCacheServer JVM.
@
setlocal

if "%1" == "" echo Usage: start-test-server.bat [port] & goto end

:config
call cfgbuild.cmd

@rem Specify the JVM heap size
set MEMORY=128m

set "JAVA_EXEC=%JAVA_HOME%\bin\java"
set JAVA_OPTS=-Xms%MEMORY% -Xmx%MEMORY%
set /a port11=%2+2
set /a port12=%2+3

set SYS_OPTS=-Dcoherence.log=..\build\DefaultCacheServer-%1.log -Dtangosol.coherence.localhost=127.0.0.1 -Dcoherence.wka=127.0.0.1 -Dcoherence.ttl=0 -Dtangosol.coherence.proxy.port=%1 -Dtangosol.coherence.proxy.port2=%2 -Dtangosol.coherence.localport=%3 -Dtangosol.coherence.proxy.port4=%4 -Dtangosol.coherence.proxy.port5=%5 -Dtangosol.coherence.proxy.port6=%6 -Dtangosol.coherence.proxy.port9=%9 -Dtangosol.coherence.proxy.port9=%9 -Dtangosol.coherence.proxy.port11=%port11% -Dtangosol.coherence.proxy.port12=%port12% -Dtangosol.coherence.localport.adjust=false -Dcoherence.cacheconfig=server-cache-config.xml -Dcoherence.management=all -Dcoherence.cluster=DotNetTest

:launch
@echo on
"%JAVA_EXEC%" -server -showversion %JAVA_OPTS% %SYS_OPTS% -cp "%DEV_ROOT%\tests\Coherence.Tests\config;%DEV_ROOT%\tools\cluster-control\lib\cohTests.jar;%DEV_ROOT%\tools\cluster-control\lib\cluster-control.jar;%DEV_ROOT%\lib\java\coherence.jar" com.tangosol.net.DefaultCacheServer
@echo off
goto end

:end
endlocal

@echo on
exit

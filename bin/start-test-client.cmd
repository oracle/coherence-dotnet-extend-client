rem
rem  Copyright (c) 2000, 2020, Oracle and/or its affiliates.
rem
rem  Licensed under the Universal Permissive License v 1.0 as shown at
rem  http://oss.oracle.com/licenses/upl.
rem

@echo off
@
@rem This will start a test CacheFactory JVM.
@
setlocal

:config
call cfgbuild.cmd

@rem Specify the JVM heap size
set MEMORY=32m

set "JAVA_EXEC=%JAVA_HOME%\bin\java"
set JAVA_OPTS=-Xms%MEMORY% -Xmx%MEMORY%
set SYS_OPTS=-Dtangosol.coherence.localhost=127.0.0.1 -Dcoherence.wka=127.0.0.1 -Dcoherence.ttl=0 -Dcoherence.cacheconfig=server-cache-config.xml -Dtangosol.coherence.distributed.localstorage=false -Dcoherence.cluster=DotNetTest

:launch
@echo on
"%JAVA_EXEC%" -server -showversion %JAVA_OPTS% %SYS_OPTS% -cp "%DEV_ROOT%\tests\Coherence.Tests\config;%DEV_ROOT%\tools\cluster-control\lib\cohTests.jar;%DEV_ROOT%\lib\java\coherence.jar" com.tangosol.net.CacheFactory
@echo off
goto end

:end
endlocal

@echo on
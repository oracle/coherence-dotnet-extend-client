rem
rem  Copyright (c) 2000, 2020, Oracle and/or its affiliates.
rem
rem  Licensed under the Universal Permissive License v 1.0 as shown at
rem  http://oss.oracle.com/licenses/upl.
rem

@echo off
@
@rem This will stop a test DefaultCacheServer JVM.
@
setlocal

:config
call cfgbuild.cmd

set "JAVA_EXEC=%JAVA_HOME%\bin\java"
set SYS_OPTS=-Dcoherence.log=..\build\stop.log -Dtangosol.coherence.localhost=127.0.0.1 -Dcoherence.wka=127.0.0.1 -Dcoherence.ttl=0 -Dcoherence.cacheconfig=cluster-control-config.xml -Dtangosol.coherence.distributed.localstorage=false -Dcoherence.cluster=DotNetTest

:stop
@echo on
"%JAVA_EXEC%" -server %SYS_OPTS% -cp "%DEV_ROOT%\tests\Coherence.Tests\config;%DEV_ROOT%\tools\cluster-control\lib\cluster-control.jar;%DEV_ROOT%\lib\java\coherence.jar" com.tangosol.tests.net.cache.ClusterControl stop
@echo off
goto end

:end
endlocal

@echo on
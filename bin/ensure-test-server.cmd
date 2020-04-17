rem
rem  Copyright (c) 2000, 2020, Oracle and/or its affiliates.
rem
rem  Licensed under the Universal Permissive License v 1.0 as shown at
rem  http://oss.oracle.com/licenses/upl.
rem

@echo off
@
@rem This will spawn a test DefaultCacheServer JVM and wait for it to start.
@
setlocal

:config
call cfgbuild.cmd

set "JAVA_EXEC=%JAVA_HOME%\bin\java"
set SYS_OPTS=-Dcoherence.log=..\build\ensure.log -Dtangosol.coherence.localhost=127.0.0.1 -Dcoherence.wka=127.0.0.1 -Dcoherence.ttl=0 -Dcoherence.cacheconfig=cluster-control-config.xml -Dtangosol.coherence.distributed.localstorage=false -Dcoherence.cluster=DotNetTest

:spawn
start "DefaultCacheServer-9099" /I start-test-server.cmd 9099 9040 8040 9098 9490 9600 9700 9800 9900
@rem to support docker, only run one server for now
@rem start "DefaultCacheServer-9100" /I start-test-server.cmd 9100 9050 8050 9198 9590 9601 9701 9801 9901

:ensure
@echo on
"%JAVA_EXEC%" -server %SYS_OPTS%  -cp "%DEV_ROOT%\tests\Coherence.Tests\config;%DEV_ROOT%\tools\cluster-control\lib\cluster-control.jar;%DEV_ROOT%\lib\java\coherence.jar" com.tangosol.tests.net.cache.ClusterControl ensure 2
@echo off
goto end

:end
endlocal

@echo on
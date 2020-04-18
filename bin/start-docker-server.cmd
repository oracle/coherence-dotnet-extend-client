rem
rem  Copyright (c) 2000, 2020, Oracle and/or its affiliates.
rem
rem  Licensed under the Universal Permissive License v 1.0 as shown at
rem  http://oss.oracle.com/licenses/upl.
rem

@echo off
@
@rem This will start a test DefaultCacheServer docker container
@
setlocal

if "%1" == "" echo Usage: start-docker-server.cmd [port] & goto end

:config
call cfgdocker.cmd

:launch
@echo on

set /a port11=%2+2
set /a port12=%2+3
docker run --name DefaultCacheServer-%1 --network docker-network --rm -p %1:%1 -p %2:%2 -p %3:%3 -p %4:%4 -p %5:%5 -p %6:%6 -p %9:%9 -p %port11%:%port11% -p %port12%:%port12% -e CLASSPATH=/u01/oracle/oracle_home/coherence/ext/lib/cohTests.jar -v %DEV_ROOT%\tests\Coherence.Tests\Config:/u01/oracle/oracle_home/coherence/ext/conf -v %DEV_ROOT%\tools\cluster-control\lib:/u01/oracle/oracle_home/coherence/ext/lib -e PROPS="-Dcoherence.log=..\build\DefaultCacheServer-%1.log -Dcoherence.localhost=DefaultCacheServer-%1 -Dcoherence.management.http=none -Dcoherence.metrics.http.enabled=false -Dcoherence.wka=DefaultCacheServer-9099 -Dtangosol.coherence.proxy.address=0.0.0.0 -Dcoherence.ttl=0 -Dtangosol.coherence.proxy.port=%1 -Dtangosol.coherence.proxy.port2=%2 -Dtangosol.coherence.localport=%3 -Dtangosol.coherence.proxy.port4=%4 -Dtangosol.coherence.proxy.port5=%5 -Dtangosol.coherence.proxy.port6=%6 -Dtangosol.coherence.proxy.port9=%9 -Dtangosol.coherence.proxy.port11=%port11% -Dtangosol.coherence.proxy.port12=%port12% -Dtangosol.coherence.localport.adjust=false -Dcoherence.cacheconfig=server-cache-config.xml -Dcoherence.management=all -Dcoherence.cluster=DotNetTest" container-registry.oracle.com/middleware/coherence:latest

@echo off
goto end

:end
endlocal

@echo on
exit

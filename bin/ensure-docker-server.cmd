rem
rem  Copyright (c) 2000, 2020, Oracle and/or its affiliates.
rem
rem  Licensed under the Universal Permissive License v 1.0 as shown at
rem  http://oss.oracle.com/licenses/upl.
rem

@echo off
@
@rem This will spawn a test DefaultCacheServer docker container and wait for it to start.
@
setlocal

:config
call cfgdocker.cmd

:spawn
start "DefaultCacheServer-9099" /I start-docker-server.cmd 9099 9040 8040 9098 9490 9600 9700 9800 9900
@rem start "DefaultCacheServer-9100" /I start-docker-server.cmd 9100 9050 8050 9198 9590 9601 9701 9801 9901

:ensure
@echo on
@rem ensure two Coherence containers are running

:startDocker1
for /F "Tokens=1" %%I in ('docker inspect -f {{.State.Running}} DefaultCacheServer-9099') Do Set StrStatus=%%I
if "%StrStatus%" neq "true" (
    timeout 5
    goto startDocker1
)

echo off
:startServer1
docker logs DefaultCacheServer-9099 > DefaultCacheServer-9099.log 2>&1
for /F "tokens=*" %%i in ('findstr /C:"Started DefaultCacheServer..." DefaultCacheServer-9099.log') do goto end
timeout 5
goto startServer1

:end
del DefaultCacheServer-9099.log
endlocal

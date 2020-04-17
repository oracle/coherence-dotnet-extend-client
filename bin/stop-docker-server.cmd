rem
rem  Copyright (c) 2000, 2020, Oracle and/or its affiliates.
rem
rem  Licensed under the Universal Permissive License v 1.0 as shown at
rem  http://oss.oracle.com/licenses/upl.
rem

@echo off
@
@rem This will stop a test DefaultCacheServer docker container.
@
setlocal

:config
call cfgbuild.cmd

:stop
@echo on
docker stop DefaultCacheServer-9099
@rem docker stop DefaultCacheServer-9100
@echo off
goto end

:end
endlocal

@echo on
#!/bin/bash
#
#  Copyright (c) 2000, 2020, Oracle and/or its affiliates.
#
#  Licensed under the Universal Permissive License v 1.0 as shown at
#  http://oss.oracle.com/licenses/upl.
#
# This script sets all environment variables necessary to submit RQ tasks for Oracle
# Coherence for .NET on supported Unix platforms.
#
# Command line:
#     . ./cfglocal.sh [-reset]
#
# This script is responsible for the following environment variables:
#
#     DEV_ROOT     e.g. /home/jhowes/dev/main.cpp
#     PATH         e.g. $ANT_HOME/bin:$JAVA_HOME/bin:$CC_HOME/bin:$PATH
#
#     _PATH        saved PATH
#

#
# Reset the build environment if the "-reset" flag was passed
#
if [ "$1" = "-reset" ]; then

  if [ -z $DEV_ROOT ]; then
    echo Build environment already reset.
    return 0
  fi

  if [ -z "$_PATH" ]; then
    unset PATH
  else
    export PATH=$_PATH
  fi

  unset DEV_ROOT
  unset _PATH

  echo Build environment reset.
  return 0
fi

#
# Determine the root of the dev tree
#
if [ ! -z $DEV_ROOT ]; then
  echo Build environment already set.
  return 0
fi
cd $SCRIPTS_DIR/..
DEV_ROOT=`pwd`
cd - > /dev/null

#
# Back up environment variables
#
_PATH=$PATH

#
# Add the RQ executables to the PATH environment variable
#
PATH=$PATH:$DEV_ROOT/tools/internal/wls/infra

#
# Export and echo environment variables
#
echo Environment variables:
export DEV_ROOT
echo "DEV_ROOT  = $DEV_ROOT"
export PATH
echo "PATH      = $PATH"
echo
echo Build environment set

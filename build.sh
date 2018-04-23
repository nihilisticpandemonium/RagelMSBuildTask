#!/usr/bin/env bash

set -eu

CYAN='\033[0;36m'
NC='\033[0m'

__exec() {
    local cmd=${1:0}
    shift
    echo -e "${CYAN} > $cmd $@${NC}"
    $cmd $@
}

rm -r artifacts/ ||:
rm -r Source/RagelMSBuildTask/obj/ ||:

__exec dotnet restore ./Source/RagelMSBuildTask/
__exec dotnet pack -c Release ./Source/RagelMSBuildTask/

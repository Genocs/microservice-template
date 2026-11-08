#!/bin/bash
MYGET_ENV=""
case "$TRAVIS_BRANCH" in
  "develop")
    MYGET_ENV="-dev"
    ;;
esac

dotnet build ../src/WebApi -c Release
dotnet build ../src/Worker -c Release
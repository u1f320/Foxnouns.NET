#!/bin/sh
(git rev-parse HEAD &&
  git describe --tags --always --long &&
  if test -z "$(git ls-files --exclude-standard --modified --deleted --others)"; then echo clean; else echo dirty; fi) > ../.version
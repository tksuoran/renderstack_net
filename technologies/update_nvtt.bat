::
::
::

@echo off
Setlocal EnableDelayedExpansion

set ROOT=%CD%
set SVN="C:\Program Files\SlikSvn\bin\svn.exe"

%SVN% export http://nvidia-texture-tools.googlecode.com/svn/branches/2.0/ nvidia-texture-tools-read-only
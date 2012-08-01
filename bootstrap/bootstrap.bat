@echo off
set include=%FASM_HOME%\include\
%FASM_HOME%\fasm.exe "%~dp0bootstrap32.asm"
%FASM_HOME%\fasm.exe "%~dp0bootstrap64.asm"

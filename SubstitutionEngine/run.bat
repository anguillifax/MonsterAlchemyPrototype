@echo off
set executable=".\Source\bin\Debug\netcoreapp3.1\Substitution.exe"
%executable% Attributes.txt Rules.txt -v < test.txt
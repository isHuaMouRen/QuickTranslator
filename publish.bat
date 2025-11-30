@echo off

dotnet publish "QuickTranslator\QuickTranslator.csproj" -c Release -o "publish\bin"
dotnet publish "ExecuteShell\ExecuteShell.csproj" -c Release -o "publish"

# Pack libs

```
dotnet pack -c Release --include-symbols
```

# Publish libs

```
dotnet nuget push .\src\BlazorTransitionGroup\bin\Release\BlazorTransitionGroup.x.x.x.nupkg -k [APIKEY] -s https://www.nuget.org/
```
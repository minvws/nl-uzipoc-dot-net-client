dotnet publish src/UziClientPoc.csproj -o publish/Tools/UziClientPoc
tar -czvf publish/UziClientPoc.tar.gz publish/Tools/UziClientPoc
rm -r publish/Tools/UziClientPoc
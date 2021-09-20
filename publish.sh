dotnet publish src/UziClientPoc.csproj -o publish/UziClientPoc
tar -czvf publish/UziClientPoc.tar.gz publish/UziClientPoc
rm -r publish/UziClientPoc
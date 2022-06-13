# UziPoc .NET 6 client example

This client provides an example how to connect to the https://github.com/minvws/nl-uzipoc-max OIDC service.

## Requirements
The .NET 6 demo uses the IdentiyServer4 dependency to require login on all resources except index.html

## Registration
To use this client a RSA certificate needs to be provided to the UziPoc OIDC service. The matching key and provided certificated needs to be configured in appsettings.json.

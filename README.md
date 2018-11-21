# AutoProxy &middot; [![NuGet](https://img.shields.io/nuget/vpre/AutoProxy.AspNetCore.svg?style=flat-square)](https://www.nuget.org/packages/AutoProxy.AspNetCore) [![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg?style=flat-square)](http://makeapullrequest.com) [![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](https://github.com/trenoncourt/AutoProxy/blob/master/LICENSE) [![Donate](	https://img.shields.io/beerpay/hashdog/scrapfy-chrome-extension.svg?style=flat-square)](https://www.paypal.me/trenoncourt/5)
> Proxy another api in 30 seconds from configuration.

AutoProxy is a drop-in microservice for proxying another api from configuration with headers forwarding, auth, cors, impersonation and more...

You can also use AutoProxy as a middleware in your aspnet core app.

## Installation
Choose between middleware or microservice

### Middleware
```Powershell
Install-Package AutoProxy.AspNetCore
```

### Microservice
Download the [last release](https://github.com/trenoncourt/AutoProxy/releases), drop it to your server and that's it!

## Configuration
All the configuration can be made in environment variable or appsettings.json file

Just add simple json or environment based configuration like this:
```json
"AutoProxy": {
  "Path": "proxy",
  "Proxies": [
    {
      "Name": "DuckDuck", // Required
      "BaseUrl": "https://duckduckgo.com/", // Required
      "Path": "proxy1",
      "WhiteList": "https://duckduckgo.com/api/(.*)", // Regex capable
      "Auth": {
        "Type": "Ntlm", // (Ntlm, Bearer or BasicToNtlm)
        "UseImpersonation": false,
        "User": "Jhon",
        "Password": "Doe",
        "Domain": "DoeCorp",
        "LogPassword": true // To log password with serilog
      },
      "Headers": {
        "Remove": [ "Origin", "Host" ],
        "Replace": [
          {
            "Key":  "Host",
            "Value": "duckduckgo.com"
          }
        ]
      }
    }
  ]
}
```

You can add multiple proxies with different path, the path for the first proxy is not required.
 
## Api configuration
### Cors
```json
"Cors": {
  "Enabled": true, // default is false
  "Methods": "...", // default is *
  "Origins": "...", // default is *
  "Headers": "...", // default is *
}
```

### Host
Currently only chose to use IIS integration
```json
"Host": {
  "UseIis": true
}
```

### Kestrel
Kestrel options, see [ms doc](https://docs.microsoft.com/fr-fr/dotnet/api/microsoft.aspnetcore.server.kestrel.core.kestrelserveroptions) for usage
```json
"Kestrel": {
  "AddServerHeader": false
}
```

### Serilog
storfiler use Serilog for logging, see [serilog-settings-configuration](https://github.com/serilog/serilog-settings-configuration) for usage
```json
"Serilog": {
  "MinimumLevel": {
    "Default": "Debug",
    "Override": {
      "Microsoft": "Warning",
      "System": "Warning"
    }
  },
  "WriteTo": [
    { "Name": "Console" }
  ],
  "Enrich": ["FromLogContext"]
}
```

# AutoProxy
> Proxy another api in 30 seconds

AutoProxy is a drop-in microservice for proxying another api from configuration with headers forwarding, auth, cors, impersonation and more...

Just add simple json or environment based configuration like this:
```json
{
  "BaseUrl": "http://192.168.1.42",
  "Cors": {
    "Enabled": true
  },
  "Auth": {
    "Type": "ntlm",
    "UseImpersonation": true
  }
}
```
 And drop the folder on your server. It can be drop in IIS, nginx and more.

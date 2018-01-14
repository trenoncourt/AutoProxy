# AutoProxy
> Proxy another api in 30 seconds from configuration

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
 
## Deployment
Download the [last release]() and place it on your server and add your configuration.

You can download previous versions [here]()

## Configuration
All the configuration can be made in environment variable or appsettings.json file :

| Name              	| Description                                   | Type        | Default value |
| --------------------- | --------------------------------------------- | ----------- |--------------:|
| **Logging**       	| Log settings, see aspnet core doc    			| Object      |               |
| BaseUrl\*     		| Base url to proxy			                    | String 	  | 	          |
| WhiteList     		| List of endpoint to accept (regex capable)	| String 	  | 	          |
| **Cors**          	| Cors settings                                 | Object      |               |
| Cors.Enabled      	| Define if cors are enabled                    | Boolean     | false         |
| Cors.Methods      	| Add specific methods to the policy            | String      | *             |
| Cors.Origins      	| Add specific origins to the policy            | String      | *             |
| Cors.Headers      	| Add specific headers to the policy            | String      | *             |
| Cors.Credentials  	| Add credentials to the policy      		    | String      | true          |
| **Auth**          	| Auth settings                                 | Object      |               |
| Auth.Type         	| (bearer or ntlm)			                    | String      |               |
| Auth.User         	| The user name for ntlm auth                   | String      |               |
| Auth.Password     	| The password for ntlm auth                    | String      |               |
| Auth.Domain        	| The domain for ntlm auth                      | String      |               |
| Auth.UseImpersonation | True to Forward ntlm token 				    | Boolean     | false         |
| Auth.Token		    | Token for bearer auth 					    | String      |               |
| **Server**          	| Server settings                               | Object      |               |
| Server.UseIIS		    | Enable iisIntegration		                    | Boolean     | false         |
| **Headers**          	| Headers settings                              | Object      |               |
| Headers.Remove		| List of headers disable in forwarding         | Array       | 	          |

Exemple of appsettings.json
```json
{
  "Logging": {
    "IncludeScopes": false,
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "WhiteList": "http://192.168.1.42/api/(.*)",
  "BaseUrl": "http://192.168.1.42",
  "Cors": {
    "Enabled": true
  },
  "Auth": {
    "Type": "ntlm",
    "UseImpersonation": true
  },
  "Server": {
    "UseIIS": true
  },
  "Headers": {
    "Remove": [
      "Host"
    ]
  }
}
```

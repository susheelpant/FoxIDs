﻿# Deployment

Deploy FoxIDs in your Azure tenant as your own private cloud. 
FoxIDs is deployed in a resource group e.g., named `FoxIDs` where you need to be `Owner` or `Contributor` and `User Access Administrator` on either subscription level or resource group level.

[![Deploy to Azure](https://aka.ms/deploytoazurebutton)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FITfoxtec%2FFoxIDs%2Fmaster%2Fazuredeploy.json)

The Azure deployment include:

- Two App Services one for FoxIDs and one for the FoxIDs Control (Client and API). Both App Services is hosted in the same App Service plan and the App Services has both a production and test slot. 
- FoxIDs is deployed to the two App Services test slots from the `master` branch with Kudu. Updates is initiated manually in the App Services test slots. Deployment updates is automatically promoted from the test slots to the production slots. It is possible to change the automatically promoted to manually initiated.
- Key vault. Secrets are placed in Key vault.
- Cosmos DB.
- Redis cache.
- Application Insights.

### Send emails with Sendgrid or SMTP
FoxIDs supports sending emails with SendGrid and SMTP as [email provider](email).

### First login and admin users
After successfully deployment open [FoxIDs Control Client](control.md#foxids-control-client) on `https://foxidscontrolxxxxxxxxxx.azurewebsites.net` (the app service starting with foxidscontrol...) which brings you to the master tenant.

> The default admin user is: `admin@foxids.com` with password: `FirstAccess!` (you are required to change the password on first login)

![FoxIDs Control Client - Master tenant](images/master-tenant2.png)

Create more admin users with a valid email addresses and grant the users the admin `role` with the value `foxids:tenant.admin`.

![FoxIDs Control Client - Master tenant admin user](images/master-tenant-admin-user.png)

> You should generally not change the parties configuration or add applications in the master tenant, unless you are sure about what you are doing.

### Troubleshooting deployent errors

**Key Vault soft deleted**
If you have deleted a previous deployment the Key Vault is only soft deleted and sill exist with the same name for some months. 
In this case you can experience getting a 'ConflictError' with the error message 'Exist soft deleted vault with the same name.'.

The solution is to delete (purge) the old Key Vault, which will release the name.

## Upload risk passwords

You can read the number of risk passwords uploaded to FoxIDs in [FoxIDs Control Client](control.md#foxids-control-client) master tenant on the Risk Passwords tap. And you can test if a password is okay or has appeared in breaches.

You can upload risk passwords with the FoxIDs seed tool. The seed tool is a console application. 

> The seed tool code can be [downloaded](https://github.com/ITfoxtec/FoxIDs/tree/master/tools/FoxIDs.SeedTool) and need to be compiled to run.

Download the `SHA-1` pwned passwords `ordered by prevalence` from [haveibeenpwned.com/passwords](https://haveibeenpwned.com/Passwords).

> Be aware that it takes some time to upload all risk passwords. This step can be omitted and postponed to later.  

The risk passwords are uploaded as bulk which has a higher consumption. Please make sure to adjust the Cosmos DB provisioned throughput (e.g. to 20000 RU/s or higher) temporarily. 
The throughput can be adjusted in Azure Cosmos DB --> Data Explorer --> Scale & Settings.

### Configure the seed tool

The seed tool is configured in the `appsettings.json` file.

> Access to upload risk passwords is granted in the `master` tenant.

Create a seed tool OAuth 2.0 client in the [FoxIDs Control Client](control.md#foxids-control-client):

1. Login to the `master` track and select the Parties tab
2. Create a OAuth 2.0 down-party, click `OAuth 2.0 - Client Credentials Grant`.
3. Set the client id to `foxids_seed`.
4. Remember the client secret.
5. In the resource and scopes section. Grant the sample seed client access to the FoxIDs Control API resource `foxids_control_api` with the scope `foxids:master`.
6. Click show advanced settings. 
7. In the issue claims section. Add a claim with the name `role` and the value `foxids:tenant.admin`. This will granted the client the administrator role. 

The seed tool client is thereby granted access to update to the master tenant.

![FoxIDs Control Client - seed tool client](images/upload-risk-passwords-seed-client.png)

Add the FoxIDs and FoxIDs Control API endpoints and client secret to the seed tool configuration. 

```json
"SeedSettings": {
    "FoxIDsEndpoint": "https://foxidsxxxx.azurewebsites.net",
    "FoxIDsControlEndpoint": "https://foxidscontrolxxxx.azurewebsites.net",
    "ClientSecret": "xxx",
    ...
}
```

### Run the seed tool

Run the seed tool executable SeedTool.exe or run the seed tool directly from Visual Studio. 

* Click 'p' to start uploading risk passwords  

The risk password upload will take a while.

## Add sample configuration to a track

It is possible to run the sample applications after they are configured in a FoxIDs track. The sample configuration can be added with the [sample seed tool](samples.md#configure-samples-in-foxids-track).

## Custom primary domains

The FoxIDs service and FoxIDs Control sites primary domains can be customized. 

> Important: change the primary domain before adding tenants.

- FoxIDs service default domain is `https://foxidsxxxx.azurewebsites.net` which can be changed to a custom primary domain like e.g., `https://somedomain.com` or `https://auth.somedomain.com`  
- FoxIDs Control default domain is `https://foxidscontrolxxxx.azurewebsites.net` which can be changed to a custom primary domain like e.g., `https://control.somedomain.com` or `https://foxidscontrol.somedomain.com`

The FoxIDs site support one primary domain and multiple [custom domains](custom-domain.md) which are connected to tenants, where the FoxIDs Control site only support one primary domain.

Configure new primary custom domains:

1) Login to [FoxIDs Control Client](control.md#foxids-control-client) using the default/old primary domain. Select the `Parties` tab and under `Down-parties` select click `OpenID Connect - foxids_control_client` and click `Show advanced settings`.

   - Add the FoxIDs Control sites new primary custom domain to the `Allow CORS origins` list without a trailing slash.
   - Add the FoxIDs Control Client sites new primary custom domain login and logout redirect URIs to the `Redirect URIs` list including the trailing `/master/authentication/login_callback` and `/master/authentication/logout_callback`.

   > If you have added tenants before changing the primary domain, the `OpenID Connect - foxids_control_client` configuration have to be done in each tenant.

2) The custom primary domains is configured on each App Service or by using a [reverse proxy](reverse-proxy.md). 
Depending on the reverse proxy your are using you might be required to also configure the domains on each App Service:

   - If configured on App Services: add the custom primary domains in Azure portal on the FoxIDs App Service and the FoxIDs Control App Service production slot under the `Custom domains` tab by clicking the `Add custom domain` link.
   - If configured on reverse proxy: the custom primary domains are exposed through the [reverse proxy](reverse-proxy.md).

3) Then configure the FoxIDs service sites new primary custom domains in the FoxIDs App Service under the `Configuration` tab and `Applications settings` sub tab: 

   - The setting `Settings:FoxIDsEndpoint` is changed to the FoxIDs service sites new primary custom domain.

4) And configure the FoxIDs service and FoxIDs Control sites new primary custom domains in the FoxIDs Control App Service under the `Configuration` tab and `Applications settings` sub tab: 

   - The setting `Settings:FoxIDsEndpoint` is changed to the FoxIDs service sites new primary custom domain.
   - The setting `Settings:FoxIDsControlEndpoint` is changed to the FoxIDs Control sites new primary custom domain.

> You can create a `main` tenant and add the custom primary domain used on the FoxIDs service as a [custom domain](custom-domain.md) to remove the tenant element from the URL.

## Reverse proxy
It is recommended to place both the FoxIDs Azure App service and the FoxIDs Control Azure App service behind a [reverse proxy](reverse-proxy.md). 

## Specify default page

An alternative default page can be configured for the FoxIDs site using the `Settings:WebsiteUrl` setting. If configured a full URL is required like e.g., `https://www.foxidsxxxx.com`.

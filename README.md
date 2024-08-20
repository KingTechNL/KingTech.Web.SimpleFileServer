[![.NET build pipeline](https://github.com/KingTechNL/KingTech.Web.SimpleFileServer/actions/workflows/dotnet_build.yml/badge.svg)](https://github.com/KingTechNL/KingTech.Web.SimpleFileServer/actions/workflows/dotnet_build.yml)
[![Docker Image Version (latest semver)](https://img.shields.io/docker/v/kingtechnl/simplefileserver?label=docker&sort=semver)](https://hub.docker.com/repository/docker/kingtechnl/simplefileserver)
[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/KingTech.Web.SimpleFileServer.Abstract)](https://www.nuget.org/packages/KingTech.Web.SimpleFileServer.Abstract/)

# Kingtech.Web.SimpleFileServer

The SimpleFileServer is, as the name suggests, a simple file server that provides access to files via a REST API.
Using a plugin system, the SimpleFileServer can be configured and extended with different sources. For example: The filesystem, or an sFTP server.
Files can also be transformed using plugins. Images for example, can automatically be resized when a 'thumbnail' is requested. This way we dont need to store multiple sizes of the same image on the file system. In order to create your own plugins, the [SimpleFileServer NuGet](https://www.nuget.org/packages/KingTech.Web.SimpleFileServer.Abstract/) package can be used.

# Deployment
The SimpleFileServer can be deployed as a docker container. An example docker-compose file is provided in the repository.
Additionally the project can be run as executable (.exe for windows, .dll for linux).


# Usage
The SimpleFileServer consists of 3 parts:
	- The main service, loading several plugins and exposing files over an HTTP(S) connection.
	- The file source plugins, enabling the main service to expose files from different locations (e.g. the file system, an sFTP server or cloud service like onedrive).
	- The transformer plugins, allowing server-side transformations to be applied to the retrieved files (e.g. image resizing).
When setting the `ENABLE_SWAGGER=true` environment variable, a swagger interface will be exposed on `[host]/swagger`. This interface represents an interactive OpenAPI documentation of the different endpoints available on the SimpleFileServer.

## File Sources
File sources are used to fetch files from different locations, e.g. the filesystem, an sFTP server or cloud service like onedrive. 
In order to add a new file source, simply implement the IFileSource interface and place the .dll files in the plugin directory.

## Transformers
Transformers are used to modify files if needed. Multiple transformers can be applied. 
In order to add one or more new Transformers, simply implement the ITransformer interface and place the .dll files in the plugin directory.

Transformers are typically triggered by specifying the `transform` query parameter in the http request. 
Transformers that match this parameter will automatically be triggered, multiple transformers can be specified.
Additional query parameters are passed to all transformers such that they may act upon them. 
For example: When using the `transform=resize` parameter to trigger the ImageResizeTransformer, an additional `width` and `height` parameter can be passed.

> https://localhost:32772/myimage.png?transform=resize&width=150

## Basic plugins
The SimpleFileServer comes with the following preinstalled default plugins:

### FileSystemFileSource
FileSource plugin that uses the standard filesystem to access files. 

| Setting | Default | Description |
| -- | -- | -- |
| Enabled | false | Whether or not this file source should be enabled. |
| BaseDirectory | "/files" | The base directory files are stored. Note that when deploying as docker, this points to a directory in the docker container. |
| IsReadOnly | false | Whether or not the source can be written to (e.g. files can be uploaded). |

### ImageResizeTransformer
This transformer resizes images server-side. 
This can be used to limit the amount of data that is sent to the client to, for example, load thumbnails based on larger images.

To trigger this transformer, the `transform=resize` query parameter can be passed in the http request.
Additionally, a `width`, `height`, and `resizemode` query parameter can be specified. These are used to define the target width and height of the resized image, 
and to specify what to do if the original aspect ratio does not match that of the resulting image. Available resize modes are:

- *crop*: (default) Crops the resized image to fit the bounds of its container.
- *max*: Constrains the resized image to fit the bounds of its container maintaining the original aspect ratio.
- *min*: Resizes the image until the shortest side reaches the set given dimension. Upscaling is disabled in this mode and the original image will be returned if attempted.
- *pad*: Pads the resized image to fit the bounds of its container. If only one dimension is passed, will maintain the original aspect ratio.
- *boxpad*: Pads the image to fit the bound of the container without resizing the original source. When downscaling, performs the same functionality as Pad.
- *stretch*: Stretches the resized image to fit the bounds of its container.

Additionally, the 'transform=thumb' query parameter can be passed. This automatically resizes the image based on the passed settings.

| Setting | Default | Description |
| -- | -- | -- |
| ThumbnailWidth | 100 | The width of the resulting image. |
| ThumbnailHeight | 100 | The height of the resulting image. |
| KeepThumbnailAspectRatio | true | Whether or not to keep the image aspect ration intact. |

## OneDrive FileSource plugin
There is an experimental OneDrive plugin available for the SimpleFileServer.
This plugin allows SimpleFileServer to use OneDrive as a file source. In order for the SimpleFileServer to access a OneDrive drive, a new App Registration needs to be made on portal.azure.com.

Due to limitations in the Microsoft Graph API, it seems like this only works for 'onedrive for business' licenses. 
All attempts on getting access to a personal onedrive have failed.

| Setting | Default | Description |
| -- | -- | -- |
| Enabled | false | Whether or not this file source should be enabled. |
| UseItemId | true | Whether to use the OneDrive Item ID's, or the file/folder names. Note that using the file/folder names may cause a loss of performance. |
| DriveId | "me" | The DriveID to use. Default Drive ID 'me' refers to the clients personal drive. |
| ClientSecretCredentials.ClientId | "" | The ID of the Client (app registration) that will be used for authentication. |
| ClientSecretCredentials.TenantId | "" | The Tenant the drive is part of. |
| ClientSecretCredentials.ClientSecret | "" | The Secret that will be used for authentication. |


## GitHub FileSource plugin
GitHub can be used as a FileSource via an optional plugin.
This plugin allows SimpleFileServer to get files from a specified GitHub repository via either the GitHub API or the plain Git protocol.

The GitHub API has a request limit of 60 per hour and therefor is only usable for very low traffic file.
The plain Git mode is more usable for high traffic file servers, it will clone the Git repository locally and periodically update it (once a minute by default).

| Setting | Default | Description |
| -- | -- | -- |
| Enabled | false | Whether or not this file source should be enabled. |
| Owner | "" | The owner of the GitHub repository. |
| Repository | "" | The repository to checkout. |
| Mode | Api | The mode of this source (Api / Git). |
| Branch | default | The branch to checkout, if not set this plugin will use the default branch of the chosen repository. |
| LocalDirectory | "" | If using plain git: The local directory to clone the git repository in. |
| CheckInterval | 00:01:00 | If using plain git: The minimum time SimpleFileServer wait until checking if the local repository is up to date. |
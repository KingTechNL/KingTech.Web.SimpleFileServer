[![.NET build pipeline](https://github.com/GVDmeijde/KingTech.Web.SimpleFileServer/actions/workflows/dotnet_build.yml/badge.svg)](https://github.com/GVDmeijde/KingTech.Web.FrontDesk/actions/workflows/dotnet_build.yml)

# Kingtech.Web.SimpleFileServer

The SimpleFileServer is, as the name suggests, a simple file server that provides access to files via a REST API.
Using a plugin system, the SimpleFileServer can be configured and extended with different sources. For example: The filesystem, or an sFTP server.
Files can also be transformed using plugins. Images for example, can automatically be resized when a 'thumbnail' is requested. This way we dont need to store multiple sizes of the same image on the file system.

# Deployment
The SimpleFileServer can be deployed as a docker container. An example docker-compose file is provided in the repository.
Additionally the project can be run as executable (.exe for windows, .dll for linux).


# Usage

## File Sources
File sources are used to fetch files from different locations, e.g. the filesystem, an sFTP server or cloud service. In order to add a new file source, simply implement the IFileSource interface and place the .dll files in the plugin directory.

## Transformers
Transformers are used to modify files if needed. Multiple transformers can be applied. In order to add one or more new Transformers, simply implement the ITransformer interface and place the .dll files in the plugin directory.

## Basic plugins
The SimpleFileServer comes with the following preinstalled default plugins:

### FileSystemFileSource
FileSource plugin that uses the standard filesystem to access files. 

| Setting | Default | Description |
| -- | -- | -- |
| Name | | The name of the transformer to load. |
| BaseDirectory | "/files" | The base directory files are stored. Note that when deploying as docker, this points to a directory in the docker container. |

### ImageResizeTransformer
This transformer resizes images based on the passed settings.

| Setting | Default | Description |
| -- | -- | -- |
| Name | | The name of the transformer to load. |
| PostFix | "_thumb" | If the requested filename ends with this string, this transformer will be applied. |
| TargetWidth | 100 | The width of the resulting image. |
| TargetHeight | 100 | The height of the resulting image. |
| KeepAspectRatio | true | Whether or not to keep the image aspect ration intact. |
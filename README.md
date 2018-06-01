# Readme

## Installation

### Prerequisites

This project was built with .NET Core and made to be cross platform. More information on installing this prerequisite can be found [on the Microsoft website](https://www.microsoft.com/net/). You will also require libgdiplus. If you are running Linux, building may require the installation of libunwind8.

Using homebrew on MacOS this can be installed via Homebrew with

```bash
brew update
brew tap caskroom/cask
brew cask install dotnet
brew install mono-libgdiplus
```

## Build

Inside the folder run "dotnet build" to create debug executables, or "dotnet run" to run the executables. In order to create portable exectuables you need to run "dotnet publish" and it will create the executables for your particular host operating system under the "$project/bin/$platform" folder.

In order to create something more portable it is advised to build the program in this manner:

```bash
dotnet publish lolresearchbot.csproj -c Release -r ubuntu-x64
```

There are other runtime identifiers if you are building for a different platform other than 64-bit Ubuntu. Please refer to the NET Core documentation for them.


After it is built, the bot expects a _configuration.json file with the following layout (replacing X with your appropriate API key for the appropriate service). The configuration file must be placed either in the directory containing the executable or if you're using the Docker deploy method, the file is expected to be placed at /data/_configuration.json.
Notably the cache size is in MB.

```JSON
{
  "tokens": 
    {
      "discord": "X",
      "SMMRY": "X",
      "LeagueofLegends": "X"
    },
    "systemOptions": 
    {
      "imageFolder": "/tmp/images",
      "cacheSize": 500
    },
    "LeagueofLegendsOptions": 
    {
      "RateLimitPer10S": "X",
      "RateLimitPer10M": "X"
    }
}
  ```


## Deploy on Docker

Once you have published the application, go to the published application location (the folder just above $app/publish)and run the following commands to deploy (remove sudo if docker daemon is not running as root)

```bash
sudo docker build -t ImageName .
```

```bash
sudo docker run -d -t ImageName
```

### Some Docker Usage Commands

The following command will stop and remove all docker containers using the image

```bash
sudo docker rm $(sudo docker stop $(sudo docker ps -a -q --filter ancestor=leaguebot.dock --format="{{.ID}}"))
```

To stop the container without removing it

```bash
sudo docker stop CONTAINER.ID
```

To list all the docker containers

```bash
sudo docker ps
```

## Usage

Once this is done, the bot can be run by either running the executable directly or utilizing Docker run. The bot takes in the commands listed throughout the Modules folder via Discord either through a direct message to the bot or with an "@mention command arguments" message in a channel. "@bot help" will list available commands.

To run as a Docker container:

```bash
sudo docker run -v /local/location/_configuration.json:/data/_configuration.json -d -t ImageName
```

## Log Location

If the bot is running in a Docker container the logs will be located in "/data/logs/". If it is not running in a Docker container the log will be located in the folder containing the bot executable.
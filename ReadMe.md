[![Build Status](https://idealens.visualstudio.com/Slaveoftime/_apis/build/status/Slaveoftime-Web-Artifact-Tetris?branchName=master)](https://idealens.visualstudio.com/Slaveoftime/_build/latest?definitionId=13&branchName=master)

# Tetris @slaveoftime

This is a free and opensource little game based on fable & react (❤)

[It is live @ here 👈](https://tetris.slaveoftime.fun)

## Dev env setup

1. Download and install dotnet sdk 3.1: <https://dotnet.microsoft.com/download/dotnet-core/3.1>
2. Download and install nodejs: https://nodejs.org/en/download/
3. Open cmd run: `npm install yarn -g`
4. Open cmd run: `dotnet tool install paket -g`
5. Open cmd run: `dotnet tool install fake-cli -g`

## Icons

To add new icons go to `https://icomoon.io/app/#/select` with `src\Client.Core.Web\public\icomoon\selection.json`
By this we can reduce font size

## Start to dev

`fake build -t RunClientWeb`

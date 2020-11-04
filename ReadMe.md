![Bundle and deploy](https://github.com/albertwoo/Tetris/workflows/Bundle%20and%20deploy%20to%20Tencent/badge.svg)

# Tetris @slaveoftime

This is a free and opensource little game based on fable & react (❤)

[It is live @ here 👈](https://tetris.slaveoftime.fun)

## Dev env setup

1. Download and install dotnet sdk 5.0: <https://dotnet.microsoft.com/download/dotnet/5.0>
2. Download and install nodejs: https://nodejs.org/en/download/
3. Open cmd run: `npm install yarn -g`
4. Open cmd run: `dotnet tool install paket -g`
5. Open cmd run: `dotnet tool install fake-cli -g`

## Icons

To add new icons go to `https://icomoon.io/app/#/select` with `src\Client.Core.Web\public\icomoon\selection.json`

By this we can reduce font size

## Start to dev for front end

`fake build -t RunClientWeb`

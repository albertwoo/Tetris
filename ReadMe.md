## Fable simple template

### Dev env setup
	1. Download and install dotnet sdk 3.1: https://dotnet.microsoft.com/download/dotnet-core/3.1
	2. Download and install nodejs: https://nodejs.org/en/download/
	3. Open cmd run: `npm install yarn -g`
	4. Open cmd run: `dotnet tool install paket -g`
	5. Open cmd run: `dotnet tool install fake-cli -g`
   
### Icons
	To add new icons go to `https://icomoon.io/app/#/select` with `src\Client.Core.Web\public\icomoon\selection.json`
	By this we can reduce font size 

### Start to work
`fake build -t RunClientWeb`
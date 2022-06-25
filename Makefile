.PHONY: build run test

build:
	@dotnet build

run:
	@dotnet run --project Kotoba.CLI

test:
	@dotnet test

import:
	@dotnet run --project Importer.CLI -- import ./source-data ./data

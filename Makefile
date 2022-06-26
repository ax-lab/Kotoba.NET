.PHONY: build run test import watch

build:
	@dotnet build

run:
	@dotnet run --project Kotoba.CLI

test:
	@dotnet test

import:
	@dotnet run --project Importer.CLI -- import ./data

watch:
	@dotnet watch run --project Kotoba.Web

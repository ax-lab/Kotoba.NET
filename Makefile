.PHONY: build run test import watch publish

build:
	@dotnet build

run:
	@dotnet run --project Kotoba.Web

test:
	@dotnet test

import:
	@dotnet run --project Importer.CLI -- import ./data

watch:
	@dotnet watch run --no-hot-reload --project Kotoba.Web

publish:
	@dotnet publish

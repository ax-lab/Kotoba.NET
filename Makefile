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
	@DOTNET_WATCH_RESTART_ON_RUDE_EDIT=1 dotnet watch run --project Kotoba.Web

publish:
	@dotnet publish

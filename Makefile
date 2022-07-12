.PHONY: build run test import watch publish check clear-data

build:
	@dotnet build

run:
	@dotnet run --project Kotoba.Web

test:
	@dotnet test --nologo -v quiet
	@npm --prefix Kotoba.Web/App test

import: clear-data
	@dotnet run --project Importer.CLI -- import ./data

watch:
	@DOTNET_WATCH_RESTART_ON_RUDE_EDIT=1 dotnet watch run --project Kotoba.Web

publish:
	@dotnet publish

check:
	@npm --prefix Kotoba.Web/App run check

clear-data:
	rm -f data/entries.db

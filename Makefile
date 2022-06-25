.PHONY: build run test

build:
	@dotnet build

run:
	@dotnet run --project Kotoba.CLI

test:
	@dotnet test

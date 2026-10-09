# CombatExtender build helpers. Paths live in Directory.Build.props;
# GAME_DIR / MOD_DIR here are optional one-off overrides.

DOTNET  ?= dotnet
PROJECT := CombatExtender.csproj
CONFIG  ?= Debug

MSBUILD_ARGS := -c $(CONFIG) -nologo
ifdef GAME_DIR
MSBUILD_ARGS += -p:GameDir="$(GAME_DIR)"
endif
ifdef MOD_DIR
MSBUILD_ARGS += -p:ModInstallDir="$(MOD_DIR)"
endif

.DEFAULT_GOAL := build
.PHONY: help restore build release deploy deploy-release clean rebuild

help:
	@echo "Targets:"
	@echo "  make build            Build ($(CONFIG))"
	@echo "  make release          Build Release"
	@echo "  make deploy           Build, then copy DLL to ModInstallDir"
	@echo "  make deploy-release   Same, using Release"
	@echo "  make clean            Remove build output"
	@echo "  make rebuild          Clean + build"
	@echo "Variables: CONFIG=Debug|Release  GAME_DIR=...  MOD_DIR=..."

restore:
	$(DOTNET) restore $(PROJECT)

build:
	$(DOTNET) build $(PROJECT) $(MSBUILD_ARGS)

release:
	$(MAKE) build CONFIG=Release

deploy: build
	$(DOTNET) msbuild $(PROJECT) -t:DeployMod -p:Configuration=$(CONFIG) $(filter -p:%,$(MSBUILD_ARGS)) -nologo -v:m

deploy-release:
	$(MAKE) deploy CONFIG=Release

clean:
	$(DOTNET) clean $(PROJECT) -nologo -v:m
	rm -rf bin obj

rebuild: clean build

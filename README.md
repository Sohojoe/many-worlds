# Many-Worlds for ML Agents

[![license badge](https://img.shields.io/badge/license-Apache--2.0-green.svg)](LICENSE)

**Many-Worlds:** Speed up your training by spawning multiple instances of your ML-Agents environment within each Unity instance.

## Features

- Many Worlds mode - each environment spawns in a unique physics scene.
- Single World mode - all environments are spawned in a single world/physics scense. This is more performant but means your ml-agents environment must be robust to spawning in different locations.
- --num-spawn-envs - specify the number of environments to spawn from the command line.
- --spawn-env - specify which environment to use from the command (Support for many ML-Agent environments in a single executable.)

## Usage

Spawn 10 Unity training instances, each with 32 instances of the environment.

``` bash
mlagents-learn config.yaml --env="envs/ManyWorlds" --num-envs=10 --run-id=10x32worlds-001 --no-graphics --env-args --spawn-env=SingleWorld --num-spawn-envs=32
```

## How to add to your ML-Agents

1. Reference Many-Worlds from your manifest

Open ```Packages.json``` and add:
``` bash
    "com.joebooth.many-worlds": "https://github.com/sohojoe/many-worlds.git"
```

2. 
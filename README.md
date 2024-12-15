# Renode NRF52840 Flash support

 [![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)

This repository contains support files for simulating the internal flash of the NRF52840 SoC with [Renode](https://github.com/renode/renode).

The peripherals that are contained in this repository are:
- [Non-volatile memory controller](https://docs.nordicsemi.com/bundle/ps_nrf52840/page/nvmc.html#register) (NVCM).
- [Factory information configuration registers](https://docs.nordicsemi.com/bundle/ps_nrf52840/page/ficr.html) (FICR).


## Usage

## CLI (Monitor) /Renode script (.resc)

```
include @https://raw.githubusercontent.com/vChavezB/renode-nrf52840_flash/refs/hea
ds/main/nrf52840_nvcm.cs
```
```
mach add "my_machine"
```
```
machine LoadPlatformDescription @platforms/cpus/nrf52840.repl
```
```
machine LoadPlatformDescription @https://raw.githubusercontent.com/vChavezB/renode
-nrf52840_flash/refs/heads/main/nrf52840_flash.repl
```

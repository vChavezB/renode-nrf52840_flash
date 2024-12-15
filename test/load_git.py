#!/usr/bin/env -S python3 -m bpython -i
from pyrenode3.wrappers import Emulation, Monitor
repo = "https://raw.githubusercontent.com/vChavezB/renode-nrf52840_flash/refs/heads/main"
e = Emulation()
m = Monitor()
res = m.Parse(f"include @{repo}/nrf52840_nvcm.cs")
device = e.add_mach("nrf")
device.load_repl("platforms/cpus/nrf52840.repl")
device.load_repl(f"{repo}/nrf52840_flash.repl")
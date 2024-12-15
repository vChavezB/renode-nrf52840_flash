#!/usr/bin/env -S python3 -m bpython -i
from pyrenode3.wrappers import Emulation, Monitor
import os

script_dir = os.path.dirname(os.path.realpath(__file__))
renode_files = os.path.join(script_dir,"..")

e = Emulation()
m = Monitor()
res = m.Parse(f"include @{renode_files}/nrf52840_nvcm.cs")
device = e.add_mach("nrf")
device.load_repl("platforms/cpus/nrf52840.repl")
device.load_repl(f"{renode_files}/nrf52840_flash.repl")



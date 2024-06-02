import os
import argparse
from pathlib import Path
import shutil

# Ensure file location is current directory
os.chdir(os.path.dirname(os.path.abspath(__file__)))

TARGET_ASSEMBLY = 'PushToMeowMod.dll'
BIN_PATH = 'bin/Debug/net48/'
PLUGINS_PATH = 'RainWorld_Data/StreamingAssets/mods/pushtomeow/plugins/'

# Paths to search for Rain World installs
DRIVES = [
    'C:/', 'D:/', 'E:/', 'F:/',
    'G:/', 'H:/', 'I:/', 'J:/'
]
FOLDERS = [
    'Program Files (x86)/Steam/steamapps/common/Rain World/',
    'Program Files/Steam/steamapps/common/Rain World/',
    'SteamLibrary/steamapps/common/Rain World/'
]
INSTALL_PATHS = []

for drive in DRIVES:
    for folder in FOLDERS:
        INSTALL_PATHS.append(os.path.join(drive, folder))

# Dependencies to fetch from Rain World
DEPS_PATH = 'Dependencies/'
DEPENDENCIES = [
    'BepInEx/core/BepInEx.dll',
    'BepInEx/plugins/HOOKS-Assembly-CSharp.dll',
    'BepInEx/core/MonoMod.RuntimeDetour.dll',
    'BepInEx/core/MonoMod.Utils.dll',
    'BepInEx/utils/PUBLIC-Assembly-CSharp.dll',
    'RainWorld_Data/Managed/UnityEngine.dll',
    'RainWorld_Data/Managed/UnityEngine.CoreModule.dll',
    'RainWorld_Data/Managed/UnityEngine.InputLegacyModule.dll',
]


def get_install_path():
    # Search for Rain World install path
    for path in INSTALL_PATHS:
        if os.path.exists(os.path.join(path, 'RainWorld.exe')):
            return path


def fetch_dependencies():
    install_path = get_install_path()

    if install_path is not None:
        print('Found Rain World install path:', install_path)
    else:
        print('Couldn\'t find Rain World install path. Skipping...')

    if install_path is not None:
        for dependency in DEPENDENCIES:
            src = os.path.join(install_path, dependency)
            dest = os.path.join(DEPS_PATH, Path(dependency).name)
            print('Copying:', src, '->', dest)
            shutil.copyfile(src, dest)
        print('Copied Rain World dependencies!')


def install():
    install_path = get_install_path()

    if install_path is not None:
        print('Found Rain World install path:', install_path)
    else:
        print('Couldn\'t find Rain World install path. Skipping...')
        return

    if install_path is not None:
        src = os.path.join(BIN_PATH, TARGET_ASSEMBLY)
        dest = os.path.join(install_path, PLUGINS_PATH, TARGET_ASSEMBLY)
        print('Copying:', src, '->', dest)
        os.makedirs(os.path.dirname(dest))
        shutil.copyfile(src, dest)
        print('Installed assembly in Rain World plugins.')


def parse_args():
    parser = argparse.ArgumentParser(
        prog='PushToMeow Utils Script',
        description='Provides utilities for working with PushToMeow'
    )
    parser.add_argument('action', choices=['fetch-dependencies', 'install'])
    return parser.parse_args()


if __name__ == '__main__':
    args = parse_args()

    if args.action == 'fetch-dependencies':
        fetch_dependencies()
    elif args.action == 'install':
        install()
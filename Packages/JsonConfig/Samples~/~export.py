# -*- coding: utf-8 -*-
import sys
import os, re
import json
import traceback
import subprocess

def init_defalut_config(syspath, sysargv):
    directory = syspath.replace('\\', '/')
    
    projectPath = None
    while True:
        index = str.rfind(directory, '/')
        if index == -1:
            break
        directory = directory[:index]
        if os.path.isfile(directory + '/ProjectSettings/ProjectSettings.asset'):
            projectPath = directory
            break
        
    if projectPath == None:
        raise Exception(u'没有找到 project 配置')

    projectPath = projectPath + '/'

    exportFile = None
    packageName = 'com.fqdev.json-config'

    with open(projectPath + 'Packages/packages-lock.json', 'r') as f:
        dependencies = json.load(f)['dependencies']
    if packageName in dependencies:
        jsonconfig = dependencies[packageName]
        if 'hash' in jsonconfig:
            hash = jsonconfig['hash']
            exportFile = 'Library/PackageCache/%s@%s' % (packageName, hash[0:10])
        
    if exportFile == None:
        exportFile = 'Packages/JsonConfig'

    argvs = ''
    for i in range(1,len(sysargv)):
        argvs += sysargv[i]

    subprocess.call("python %s%s/Tools~/export.py %s" % (projectPath, exportFile, argvs), shell=True)

# ==================================================               
def end_exit():
    # print("--------------------------------------\n")
    # subprocess.call("pause", shell=True)
    os._exit(0)

def main():
    reload(sys)
    sys.setdefaultencoding('utf-8')
    try:
        sysargv = sys.argv
        sysargv = init_defalut_config(sys.path[0], sysargv)
    except Exception as e:
        # print('[=======error=======]:', e)
        sys.stderr.write("\nException: {}\n".format(e))
        traceback.print_exc()

if __name__ == "__main__":
    main()
    end_exit()
# ==================================================
# 结束

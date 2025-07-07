#!/usr/bin/env python
# -*- coding: utf-8 -*-
import sys
import os
import traceback
import subprocess

def install(package):
    subprocess.check_call([sys.executable, "-m", "pip", "install", "--user", package])
install("openpyxl")

import config
import helper_util as utilHelper
import helper_file as fileHelper
import manager_table as tableMgr
import manager_config as configMgr
# ==================================================

# ==================================================     
def init_defalut_config(syspath, sysargv):
    if len(sysargv) == 1 or not sysargv[1].startswith('--path'):
        #读取工程下的DataCenterSettings.asset
        syspath = str.replace(syspath, '\\', '/')
        paths = str.split(syspath, '/')
        settings_path = None
        directory = syspath
        while True:
            index = str.rfind(directory, '/')
            if index == -1:
                break
            directory = directory[:index]
            if os.path.isfile(directory + '/ProjectSettings/ProjectSettings.asset'):
                settings_path = directory + '/Assets/Editor/JsonConfigSettings.asset'
                break
            
        if settings_path == None:
            raise Exception(u'not find JsonConfigSettings.asset')
            
        androidSheetPath = None
        androidJsonPath = None
        iosSheetPath = None
        iosJsonPath = None
        
        with open(settings_path, 'r') as f:
            for line in f:
                if '_headerStartRow' in line:
                    config.start_row_num = int(str.split(line, ':')[1].strip())
                elif '_outputScriptFolder' in line:
                    config.cls_path = directory + str.split(line, ':')[1].strip()
                    
                if '_defaultSheetFolder' in line:
                    androidSheetPath = directory + str.split(line, ':')[1].strip()
                elif '_outputJsonFolder' in line:
                    androidJsonPath = directory + str.split(line, ':')[1].strip()
                if '_defaultIosSheetFolder' in line:
                    iosSheetPath = directory + str.split(line, ':')[1].strip()
                elif '_outputIosJsonFolder' in line:
                    iosJsonPath = directory + str.split(line, ':')[1].strip()
        
        config.sheet_path = androidSheetPath
        config.json_path = androidJsonPath
        if len(sysargv) > 1 and 'ios' in sysargv[1].lower():
            config.sheet_path = iosSheetPath
            config.json_path = iosJsonPath
        
    else:
        paths = sysargv[1].split('#')
        config.start_row_num = int(paths[1])
        config.sheet_path = paths[2]
        config.json_path = paths[3]
        config.cls_path = paths[4]
        list.remove(sysargv, sysargv[1])
    
    if config.sheet_path[len(config.sheet_path) - 1] == '/':
        config.sheet_path = config.sheet_path[:len(config.sheet_path) - 1]
    # if config.json_path[len(config.json_path) - 1] == '/':
    #     config.json_path = config.json_path[:len(config.json_path) - 1]
    # if conifg.cls_path[len(config.cls_path) - 1] == '/':
    #     config.cls_path = config.cls_path[:len(conficonfig.cls_path) - 1]
        
    config.replenish_path_start = ''
    paths = config.json_path.split('/Resources/')
    if len(paths) != 1:
        config.replenish_path_start = paths[1]
        
    print ('==path:', config.sheet_path, config.json_path, config.cls_path, config.replenish_path_start)
    
    if config.sheet_path is None or config.json_path is None or config.cls_path is None:
        raise Exception(u'没有找到 表文件夹 或 输出文件夹')
    
    print ('==sysargv:', sysargv)
    print ('--------------------------------------')

    return sysargv

def excel_table_common(sysargv):
    # bSaveCfg, ayAllPaths = utilHelper.GetPaths(sysargv)
    ayAllPaths, bSaveCfg = utilHelper.FindPathByArgv(sysargv, config.suffixs)
    data_cfg_files = []
    
    for file_xlsx in ayAllPaths:
        sfiles = os.path.split(file_xlsx)
        swrite_name = os.path.splitext(sfiles[1])[0].replace(config.ignore_cpp, '').replace(config.ignore_load, '')
        swrite_path = sfiles[0].replace(config.sheet_path, '') + '/'
        if swrite_path[0] == '/':
            swrite_path = swrite_path[1:]
        
        signore = sfiles[1].startswith(config.ignore_cpp) and 'ignore_cpp' or 'none'
        if signore == 'none':
            signore = sfiles[1].startswith(config.ignore_load) and 'ignore_load' or 'none'
            
        workbook = fileHelper.ParseData(file_xlsx)
        print("swrite_path", swrite_path)
        signore, typeFullNames = tableMgr.Write(workbook, swrite_name, swrite_path, signore)
            
        if signore == 'none':
            if typeFullNames is None:
                data_cfg_files.append({'write_name':swrite_name, 'typeFullName':None, 'write_path':swrite_path})
            else:    
                for full_name in typeFullNames:
                    data_cfg_files.append({'write_name':swrite_name, 'typeFullName':full_name, 'write_path':swrite_path})
    
    if bSaveCfg and len(data_cfg_files) > 0:
        configMgr.SaveCfg(data_cfg_files)
        
# ==================================================    
def window_platform():
    return os.name != 'posix'
# ==================================================               
if __name__ == "__main__":
    reload(sys)
    sys.setdefaultencoding('utf-8')
    # print "platform: ", sys.platform, "osname: ", os.name
    try:
        sysargv = sys.argv
        sysargv = init_defalut_config(sys.path[0], sysargv)
        excel_table_common(sysargv)
    except Exception as e:
        # print('[=======error=======]:', e)
        # print(traceback.format_exc())
        sys.stderr.write("\nException: {}\n".format(e))
        traceback.print_exc()
    finally:
        print("--------------------------------------\n")
        if window_platform():
            subprocess.call("pause", shell=True)
        os._exit(0)
# ==================================================

# ==================================================

# 结束

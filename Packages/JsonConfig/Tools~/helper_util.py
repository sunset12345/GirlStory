#!/usr/bin/env python
# -*- coding: utf-8 -*-
import json
import os
import config

# def GetPaths(self, sysargv):
#     # sysargv = ['/Volumes/MacintoshHD/UnityPackages/AllPackages/_ConfigBase/export.py', 'MacintoshHD:UnityPackages:AllPackages:_ConfigBase:Json:']
#     bSaveCfg = len(sysargv) == 1
#     if bSaveCfg:
#         paths = self._all_path(syspath, gsuffixs)
#     else:
#         paths, bSaveCfg = self._argv_path(syspath, sysargv, gsuffixs)
            
#     # print ('======================\n', syspath)
#     # print (paths)
#     # print ('======================')
#     return bSaveCfg, paths

def _all_path(dirname, _suffixs):
    files = []  # 所有的文件
    # maindir 当前主目录 subdir 当前主目录下的所有目录 file_name_list 当前主目录下的所有文件
    for maindir, subdir, file_name_list in os.walk(dirname, False):
        if config.filters in maindir:
            continue
        for filename in file_name_list:
            if config.filters in filename:
                continue
            # print('>>path', maindir, filename)
            apath = os.path.join(maindir, filename).replace('\\', '/')  # 合并成一个完整路径
            if apath.endswith(_suffixs):
                files.append(apath)
    
    # file_name_list = os.listdir(dirname)
    # for filename in file_name_list:
    #     if config.filters in filename:
    #         continue
    #     apath = os.path.join(dirname, filename)  # 合并成一个完整路径
    #     if _suffix in apath:
    #         files.append(apath)
    return files

def FindPathByArgv(sysargv, _suffixs):
    # syspath = gsheet_path.replace('\\', '/')
    files = []
    if len(sysargv) == 1 or sysargv[1].replace('\\', '/') == config.sheet_path + '/':
        files.extend(_all_path(config.sheet_path, _suffixs))
        return files, True
        
    bSaveCfg = False
    for i in range(1, len(sysargv)):
        fname = sysargv[i].replace('\\', '/')
        # print('argv_path', fname, syspath)
        if os.path.isdir(fname):
            files.extend(_all_path(fname, _suffixs))
            if i == 1 and len(fname.replace(config.sheet_path + '/', '').split('/')) == 2:
                bSaveCfg = True
            continue
        
        if os.path.isfile(fname) and fname.endswith(_suffixs):
            files.append(fname)
            continue
        
    return files, bSaveCfg

# def _argv_path_get_real_file(self, syspath, fname):
#     if window_platform():
#         fname = fname.replace('\\', '/')
#         return fname
    
#     if fname.startswith(syspath):
#         return fname
#     # print('get_real_file', fname)
    
#     return fname
#     # syspath_st = syspath.split('/')
#     # word = syspath_st[len(syspath_st) - 1]
#     # index = str.index(fname, word)
#     # return syspath + fname[index + len(word):]


def json_dumps_sorted(data, **kwargs):
    sorted_keys = kwargs.get('sorted_keys', tuple())
    if not sorted_keys:
        return json.dumps(data)
    else:
        out_list = []
        for element in data:
            element_list = []
        for key in sorted_keys:
            if key in element:
                element_list.append(json.dumps({key: element[key]}))
        out_list.append('{undefined{undefined{}}}'.format(','.join((s[1:-1] for s in element_list))))
        return '[{}]'.format(','.join(out_list))
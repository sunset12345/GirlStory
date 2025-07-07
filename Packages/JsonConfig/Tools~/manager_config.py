#!/usr/bin/env python
# -*- coding: utf-8 -*-

import sys
import config
import helper_file as fileHelper

def SaveCfg(cfgs):
    _createBaseConfigLoad()
    _createCfgManager(cfgs)
    
file_ConfigManager_Loader = None
def _createBaseConfigLoad():
    global file_ConfigManager_Loader
    if file_ConfigManager_Loader == None:
        file_ConfigManager_Loader = fileHelper.ReadFile(sys.path[0] + '/ConfigManagerLoader.cs')
#   sinclude = '''
# public static partial class ConfigManager {
#     public static T LoadTableJson<T>(string file)
#     {
#         var res = UnityEngine.Resources.Load<UnityEngine.TextAsset>(file);
#         if (res) return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(res.text);
#         UnityEngine.Debug.LogWarning($"load text error file:{file}");
#         return default;
#     }
#     public static T LoadConstJson<T>(string file)
#     {
#         var res = UnityEngine.Resources.Load<UnityEngine.TextAsset>(file);
#         if (res) return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(res.text);
#         UnityEngine.Debug.LogWarning($"load text error file:{file}");
#         return default;
#     }
# }
# '''
    fileHelper.WriteFile('ConfigManagerLoader.cs', config.cls_path, file_ConfigManager_Loader, True)
        
def _createCfgManager(cfgs):
    sparam1 = ''
    sparam2 = ''
    sparam3 = ''

    cfgs.sort(key=_func_sort_file)

    for cfg in cfgs:
        sparam1 += _funcDefinition(cfg)
        sparam2 += _funcLoadJson(cfg)
        sparam3 += _funcClearInfo(cfg)

    sinclude = '''
using System;
using System.Collections.Generic;
using UnityEngine;
public static partial class ConfigManager {
%s
    public static void Initialize() {
        Clear();
%s
    }
    public static void Clear() {
%s
    }
}
''' % (sparam1, sparam2, sparam3)

    fileHelper.WriteFile('ConfigManager.cs', config.cls_path, sinclude)
    
# ----------------------------#
def _func_sort_file(elem):
    return elem['write_name']
# ----------------------------#
def _funcDefinition(cfg):
    if cfg['write_name'].endswith('const'):
        return ''
    return '\tpublic static %s %s = new %s();\n' % (cfg['typeFullName'], cfg['write_name'], cfg['typeFullName'])

def _funcLoadJson(cfg):
    if cfg['write_name'].endswith('const'):
        return '''
        %s.Deserialize();
    ''' % (cfg['write_name'])
    return '''
        %s = %sLoader.Deserialize();
	''' % (cfg['write_name'], cfg['write_name'])

def _funcClearInfo(cfg):
    if cfg['typeFullName'] is None:
        return ''
    if not cfg['typeFullName'].startswith('Dictionary') and not cfg['typeFullName'].startswith('List'):
        return ''
    return '''
        %s.Clear();
    ''' % (cfg['write_name'])
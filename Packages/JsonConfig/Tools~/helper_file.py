#!/usr/bin/env python
# -*- coding: utf-8 -*-
from genericpath import isfile
import json
import os
import config

import helper_parse_xlsx as xlsx
import helper_parse_xml as xml
import helper_parse_csv as csv

def ParseData(file):
    print("#######excel   ", file)

    data = None
    if file.endswith(config.suffix_xlsx):
        data = xlsx.Parse(file)
    elif file.endswith(config.suffix_xml):
        data = xml.Parse(file)
    elif file.endswith(config.suffix_csv):
        data = csv.Parse(file)
    return data

    
def WriteFile(s_file, s_path, s_body, is_have = False):
    # print ('write_file', s_path, config.filters)
    if config.filters in s_file:
        return
    spath = s_path.replace('\\', '/')
    # print ('write_file', s_path, s_file)
    folder = os.path.exists(spath)
    if not folder: #判断是否存在文件夹如果不存在则创建为文件夹
        os.makedirs(spath)

    sfile = spath + s_file
    if is_have and os.path.isfile(sfile):
        return
    with open(sfile, 'w') as f:
        f.write(s_body)
        
def WriteJson(s_file, s_path, s_body, b_format = False):
    # print('====writeJson', s_file, config.json_path, s_path)
    s_body = json_dumps(s_body, b_format)
    WriteFile(s_file, s_path, s_body)
    
def ReadFile(s_file):
    with open(s_file, 'r') as f:
        return f.read()
    
def json_dumps(s_body, b_format = False):
    if b_format:
        s_body = json.dumps(s_body , sort_keys=True, ensure_ascii = False, indent=0, separators=(',', ':'))
    else:
        s_body = json.dumps(s_body , sort_keys=True, ensure_ascii = False, separators=(',', ':'))
    return s_body

#类描述
class_description_table = None
def GetClassDescriptionTable(classname, checkerror = True):
    global class_description_table
    if class_description_table is None:
        path = config.sheet_path + '/' + config.class_description
        workbook = None
        if os.path.isfile(path + '.csv'):
            workbook = csv.Parse(path + '.csv')
        elif os.path.isfile(path + '.xlsx'):
            workbook = xlsx.Parse(path + '.xlsx')
        else:
            workbook = xml.Parse(path + '.xml')
            
        if workbook is None:
            raise Exception(u'not find class description file! (%s)' % (path))
        
        class_description_table = {}
        ayRows = list(workbook.sheets[0].Rows)
        for i in range(0, len(ayRows)):
            cells = ayRows[i]
            key = cells[0].value
            if key is None:
                continue
            key = key.replace(config.filters, '')
            contents = []
            for j in range(1, len(cells)):
                if cells[j].value is None:
                    continue
                temp = cells[j].value.split(':')
                if temp[1] in contents:
                    raise Exception(u'%skey repetition:%s' % (key, temp[1]))
                contents.append([temp[0], temp[1]])
            class_description_table[key] = contents
    
    if classname not in class_description_table:
        if checkerror:
            raise Exception(u'class description not find (%s) class!' % (classname))
        else:
            return None
    
    return class_description_table[classname]

#类继承关系描述
Inheritance_description_table = None
def GetInheritanceDescriptionTable(classname):
    global Inheritance_description_table
    if Inheritance_description_table is None:
        path = config.sheet_path + '/' + config.inheritance_description
        workbook = None
        if os.path.isfile(path + '.csv'):
            workbook = csv.Parse(path + '.csv')
        elif os.path.isfile(path + '.xlsx'):
            workbook = xlsx.Parse(path + '.xlsx')
        else:
            workbook = xml.Parse(path + '.xml')
            
        if workbook is None:
            raise Exception(u'not find inheritance description file! (%s)' % (path))
        
        Inheritance_description_table = {}
        ayRows = list(workbook.sheets[0].Rows)
        for i in range(0, len(ayRows)):
            cells = ayRows[i]
            key = cells[0].value
            if key is None:
                continue
            key = key.replace(config.filters, '')
            contents = []
            for j in range(1, len(cells)):
                if cells[j].value is None:
                    continue
                if cells[j].value in contents:
                    raise Exception(u'%skey repetition:%s' % (key, cells[j].value))
                contents.append(cells[j].value)
            Inheritance_description_table[key] = contents
    
    if classname not in Inheritance_description_table:
        raise Exception(u'inheritance description not find (%s) class!' % (classname))
    
    return Inheritance_description_table[classname]